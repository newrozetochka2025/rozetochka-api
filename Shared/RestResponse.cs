namespace rozetochka_api.Shared
{

    // REST Унифицированный формат ответа сервера
    public class RestResponse
    {
        public RestStatus Status { get; set; } = new();
        public RestMeta Meta { get; set; } = new();
        public object? Data { get; set; }
    }

    public class RestMeta
    {
        public string Service { get; set; } = "rozetochka-api";
        public string Method { get; set; } = "GET";
        public string Action { get; set; } = "Read";
        public string DataType { get; set; } = "null";
        public long Time { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();    // Вернёт Unix time в секундах прошедших с 01.01.1970 UTC.
        public Dictionary<string, object>? Params { get; set; } = [];      // Вернуть параметры из запроса
    }

    public class RestStatus
    {
        public bool IsOk { get; set; } = true;
        public int Code { get; set; } = 200;
        public string Phrase { get; set; } = "OK";
    }
}
