using System.Globalization;

// Three distinct outcomes the caller must tell apart:
//   Valid    -> params passed every check; safe to hand to /news.
//   LlmError -> Claude returned its "error" envelope (couldn't translate);
//               show LlmMessage to the user. This is expected, not a bug.
//   Invalid  -> the JSON deserialized but failed a semantic check; a real
//               problem (bad model output). Inspect Errors.
public enum QueryOutcome { Valid, LlmError, Invalid }

public class ValidationResult
{
    public QueryOutcome Outcome { get; init; }
    public List<string> Errors { get; init; } = new();
    public string? LlmMessage { get; init; }   // set on LlmError
    public string? Endpoint { get; init; }     // validated endpoint, on Valid
    public NewsApiParams? Params { get; init; } // the validated params, on Valid
}

public static class NewsApiValidator
{
    // Envelope terms are our own contract — match loosely.
    private static readonly StringComparer Loose = StringComparer.OrdinalIgnoreCase;

    // NewsAPI value sets use NewsAPI's EXACT casing and are matched
    // case-sensitively, so anything that passes is guaranteed acceptable
    // to NewsAPI (it rejects e.g. "EN" or "publishedat").
    private static readonly HashSet<string> Languages = new()
        { "ar","de","en","es","fr","he","it","nl","no","pt","ru","sv","ud","zh" };

    private static readonly HashSet<string> Countries = new()
        { "ae","ar","at","au","be","bg","br","ca","ch","cn","co","cu","cz","de","eg",
          "fr","gb","gr","hk","hu","id","ie","il","in","it","jp","kr","lt","lv","ma",
          "mx","my","ng","nl","no","nz","ph","pl","pt","ro","rs","ru","sa","se","sg",
          "si","sk","th","tr","tw","ua","us","ve","za" };

    private static readonly HashSet<string> Categories = new()
        { "business","entertainment","general","health","science","sports","technology" };

    private static readonly HashSet<string> SortByValues = new()
        { "relevancy","popularity","publishedAt" };

    private static readonly HashSet<string> SearchInFields = new()
        { "title","description","content" };

    public static ValidationResult Validate(LlmQueryResult? result)
    {
        if (result is null)
            return Fail("Response did not deserialize into a query object (not valid JSON).");

        // --- Envelope: Claude's own error branch short-circuits here ---
        if (Loose.Equals(result.Status, "error"))
            return new ValidationResult
            {
                Outcome = QueryOutcome.LlmError,
                LlmMessage = result.Message ?? "The model reported it could not translate the request."
            };

        var errors = new List<string>();

        if (!Loose.Equals(result.Status, "success"))
            errors.Add($"status must be \"success\" or \"error\", got \"{result.Status ?? "null"}\".");

        var endpoint = result.Endpoint;
        bool isEverything = Loose.Equals(endpoint, "everything");
        bool isHeadlines  = Loose.Equals(endpoint, "top-headlines");
        if (!isEverything && !isHeadlines)
            errors.Add($"endpoint must be \"everything\" or \"top-headlines\", got \"{endpoint ?? "null"}\".");

        var p = result.Params;
        if (p is null)
        {
            errors.Add("params object is missing on a success response.");
            return Fail(errors);   // nothing more to check without params
        }

        // --- Value whitelists (only when the field is present; null = unset) ---
        if (p.Language is not null && !Languages.Contains(p.Language))
            errors.Add($"language \"{p.Language}\" is not a NewsAPI-supported code.");

        if (p.SortBy is not null && !SortByValues.Contains(p.SortBy))
            errors.Add($"sortBy \"{p.SortBy}\" must be relevancy, popularity, or publishedAt.");

        if (p.Category is not null && !Categories.Contains(p.Category))
            errors.Add($"category \"{p.Category}\" is not a valid NewsAPI category.");

        if (p.Country is not null && !Countries.Contains(p.Country))
            errors.Add($"country \"{p.Country}\" is not a NewsAPI-supported code.");

        if (p.SearchIn is not null)
            foreach (var field in p.SearchIn.Split(',',
                         StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                if (!SearchInFields.Contains(field))
                    errors.Add($"searchIn contains \"{field}\"; allowed: title, description, content.");

        // --- Ranges ---
        if (p.PageSize is int ps && (ps < 1 || ps > 100))
            errors.Add($"pageSize {ps} is out of range (1–100).");
        if (p.Page is int pg && pg < 1)
            errors.Add($"page {pg} must be 1 or greater.");

        // --- Dates: must parse as yyyy-MM-dd (InvariantCulture, so a Swedish
        //     locale doesn't reinterpret them), and from must not exceed to ---
        var from = ParseDate(p.From, "from", errors);
        var to   = ParseDate(p.To,   "to",   errors);
        if (from is { } f && to is { } t && f > t)
            errors.Add("from date is later than to date.");

        // --- Endpoint-appropriateness (a valid name on the wrong endpoint is invalid) ---
        if (isHeadlines)
        {
            if (p.Language is not null)       errors.Add("language is not valid on top-headlines (use everything to filter by language).");
            if (p.SearchIn is not null)       errors.Add("searchIn is not valid on top-headlines.");
            if (p.Domains is not null)        errors.Add("domains is not valid on top-headlines.");
            if (p.ExcludeDomains is not null) errors.Add("excludeDomains is not valid on top-headlines.");
            if (p.From is not null)           errors.Add("from is not valid on top-headlines.");
            if (p.To is not null)             errors.Add("to is not valid on top-headlines.");
            if (p.SortBy is not null)         errors.Add("sortBy is not valid on top-headlines.");

            // Mutual exclusion: sources cannot combine with country or category.
            bool hasSources = !string.IsNullOrWhiteSpace(p.Sources);
            if (hasSources && (p.Country is not null || p.Category is not null))
                errors.Add("top-headlines cannot combine sources with country or category.");
        }
        if (isEverything)
        {
            if (p.Country is not null)  errors.Add("country is only valid on top-headlines.");
            if (p.Category is not null) errors.Add("category is only valid on top-headlines.");
        }

        // --- Must carry at least one usable search criterion ---
        // (NewsAPI rejects a query with only filters like language and no q/sources.)
        bool hasCriterion =
            !string.IsNullOrWhiteSpace(p.Q) ||
            !string.IsNullOrWhiteSpace(p.Sources) ||
            !string.IsNullOrWhiteSpace(p.Domains) ||
            (isHeadlines && (p.Country is not null || p.Category is not null));
        if (!hasCriterion)
            errors.Add("query has no usable criterion (need at least q, sources, domains, or country/category).");

        // --- q length cap ---
        if (p.Q is { Length: > 500 })
            errors.Add($"q is {p.Q.Length} characters; NewsAPI's limit is 500.");

        if (errors.Count > 0)
            return Fail(errors);

        return new ValidationResult
        {
            Outcome  = QueryOutcome.Valid,
            Endpoint = endpoint,
            Params   = p
        };
    }

    private static DateTime? ParseDate(string? value, string name, List<string> errors)
    {
        if (value is null) return null;
        if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                   DateTimeStyles.None, out var d))
            return d;
        errors.Add($"{name} \"{value}\" is not a valid yyyy-MM-dd date.");
        return null;
    }

    private static ValidationResult Fail(string error) =>
        new() { Outcome = QueryOutcome.Invalid, Errors = { error } };

    private static ValidationResult Fail(List<string> errors) =>
        new() { Outcome = QueryOutcome.Invalid, Errors = errors };
}
