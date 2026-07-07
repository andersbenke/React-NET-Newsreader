/*
 * LLM Generated newsapi data classes
 * */


using System.Text.Json.Serialization;

// The envelope Claude returns:
//   {"status":"success","endpoint":"everything","params":{...}}
//   {"status":"error","message":"..."}
public class LlmQueryResult
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }      // "success" | "error"

    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }    // "everything" | "top-headlines"

    [JsonPropertyName("message")]
    public string? Message { get; set; }     // populated only on error

    [JsonPropertyName("params")]
    public NewsApiParams? Params { get; set; }
}

public class NewsApiParams
{
    // ---- Valid on BOTH endpoints ----
    [JsonPropertyName("q")]
    public string? Q { get; set; }

    [JsonPropertyName("sources")]
    public string? Sources { get; set; }

	// title,description,content
	[JsonPropertyName("searchIn")]
    public string? SearchIn { get; set; }

    [JsonPropertyName("domains")]
    public string? Domains { get; set; }

    [JsonPropertyName("excludeDomains")]
    public string? ExcludeDomains { get; set; }


	/*
     * Both dates From and To are per "2026-07-01" format.
     * */
	[JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("to")]
    public string? To { get; set; }


	/* These are the lang codes possible (I think):
     * ar de en es fr he it nl no pt ru sv ud zh
     */
	[JsonPropertyName("language")]
    public string? Language { get; set; }

	// relevancy | popularity | publishedAt
	[JsonPropertyName("sortBy")]
    public string? SortBy { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

	// business,  entertainment,  general,
    // health, science,  sports,  technology
	[JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("pageSize")]
    public int? PageSize { get; set; }

    [JsonPropertyName("page")]
    public int? Page { get; set; }
}