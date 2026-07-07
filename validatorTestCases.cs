string[] validatorTestCases =
{
    // ================= VALID  (expect QueryOutcome.Valid) =================
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""Uganda"",""sortBy"":""publishedAt""}}",                                   // basic keyword search
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""hundar"",""language"":""sv"",""sortBy"":""publishedAt""}}",                // Swedish language filter
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""climate"",""from"":""2026-06-10"",""to"":""2026-06-20"",""sortBy"":""relevancy""}}", // valid date range
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""domains"":""bbc.co.uk""}}",                                                       // criterion via domains, no q
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""ai policy"",""searchIn"":""title,description""}}",                          // valid searchIn fields
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""tesla"",""excludeDomains"":""reddit.com"",""pageSize"":20,""page"":2}}",    // ranges within bounds
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""electric cars NOT Tesla"",""sortBy"":""popularity""}}",                     // boolean operator in q
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""sources"":""bbc-news""}}",                                                        // criterion via sources, no q
    @"{""status"":""success"",""endpoint"":""top-headlines"",""params"":{""country"":""se"",""category"":""technology""}}",                               // country + category (allowed together)
    @"{""status"":""success"",""endpoint"":""top-headlines"",""params"":{""sources"":""bbc-news""}}",                                                     // sources alone on headlines
    @"{""status"":""success"",""endpoint"":""top-headlines"",""params"":{""q"":""bitcoin""}}",                                                            // q alone on headlines

    // ================= LLM ERROR  (expect QueryOutcome.LlmError) =================
    @"{""status"":""error"",""message"":""The message is a greeting and contains no news topic.""}",                                                      // greeting
    @"{""status"":""error"",""message"":""Input is gibberish with no extractable keywords.""}",                                                           // gibberish
    @"{""status"":""error""}",                                                                                                                            // error with no message (falls back)

    // ================= INVALID  (expect QueryOutcome.Invalid) =================
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""nyheder"",""language"":""da""}}",                                           // Danish: valid-looking, unsupported
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""x"",""language"":""xx""}}",                                                 // nonsense language code
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""x"",""sortBy"":""newest""}}",                                               // bad sortBy value
    @"{""status"":""success"",""endpoint"":""top-headlines"",""params"":{""country"":""us"",""category"":""politics""}}",                                 // invalid category
    @"{""status"":""success"",""endpoint"":""top-headlines"",""params"":{""country"":""dk""}}",                                                           // Denmark: unsupported country
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""x"",""searchIn"":""title,body""}}",                                         // bad searchIn field
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""x"",""pageSize"":500}}",                                                    // pageSize over 100
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""x"",""page"":0}}",                                                          // page below 1
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""x"",""from"":""last week""}}",                                              // unparseable date
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""x"",""from"":""2026-06-20"",""to"":""2026-06-10""}}",                        // from later than to
    @"{""status"":""success"",""endpoint"":""top-headlines"",""params"":{""country"":""se"",""language"":""sv""}}",                                       // language on wrong endpoint
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""x"",""category"":""technology""}}",                                         // category on wrong endpoint
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""x"",""country"":""se""}}",                                                  // country on wrong endpoint
    @"{""status"":""success"",""endpoint"":""top-headlines"",""params"":{""sources"":""bbc-news"",""country"":""us""}}",                                  // mutual exclusion: sources + country
    @"{""status"":""success"",""endpoint"":""top-headlines"",""params"":{""sources"":""bbc-news"",""category"":""business""}}",                           // mutual exclusion: sources + category
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""language"":""sv""}}",                                                             // no usable criterion (filter only)
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""electric vehicles and battery technology news electric vehicles and battery technology news electric vehicles and battery technology news electric vehicles and battery technology news electric vehicles and battery technology news electric vehicles and battery technology news electric vehicles and battery technology news electric vehicles and battery technology news electric vehicles and battery technology news electric vehicles and battery technology news electric vehicles and battery technology news electric vehicles and battery technology news""}}", // q over 500 chars
    @"{""status"":""ok"",""endpoint"":""everything"",""params"":{""q"":""x""}}",                                                                          // NewsAPI's own status leaking in
    @"{""status"":""success"",""params"":{""q"":""x""}}",                                                                                                // missing endpoint
    @"{""status"":""success"",""endpoint"":""headlines"",""params"":{""q"":""x""}}",                                                                      // malformed endpoint value
    @"{""status"":""success"",""endpoint"":""everything""}",                                                                                              // missing params on success
    @"{""status"":""success"",""endpoint"":""top-headlines"",""params"":{""sources"":""bbc-news"",""category"":""politics"",""language"":""sv"",""country"":""zz""}}", // several errors at once
    @"null",                                                                                                                                              // deserializes to null -> Validate(null)

    // ========= DESERIALIZATION FAILURES (throw at Deserialize, BEFORE the validator) =========
    // These test the try/catch around your JsonSerializer.Deserialize call, not Validate itself.
    @"{not valid json",                                                                                                                                   // malformed JSON -> JsonException
    @"{""status"":""success"",""endpoint"":""everything"",""params"":{""q"":""x"",""pageSize"":""ten""}}",                                                // wrong type for int? -> JsonException
};
