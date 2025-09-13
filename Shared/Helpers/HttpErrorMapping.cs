namespace rozetochka_api.Shared.Helpers
{
    public static class HttpErrorMapping
    {
        public static (int status, string phrase) Get(string? code) => code switch
        {
            // общие
            "VALIDATION_ERROR" => (422, "Unprocessable Entity"),
            "CREATE_FAILED" => (400, "Bad Request"),
            "DATABASE_ERROR" => (500, "Internal Server Error"),

            // user
            "EMAIL_TAKEN" => (409, "Conflict"),
            "USERNAME_TAKEN" => (409, "Conflict"),
            "INVALID_EMAIL" => (422, "Unprocessable Entity"),
            "INVALID_USERNAME" => (422, "Unprocessable Entity"),
            "INVALID_PASSWORD" => (422, "Unprocessable Entity"),
            "PASSWORDS_DONT_MATCH" => (422, "Unprocessable Entity"),
            "INVALID_CREDENTIALS" => (401, "Unauthorized"),

            // category / product
            "SLUG_EXISTS" => (409, "Conflict"),
            "PARENT_NOT_FOUND" => (400, "Bad Request"),
            "CATEGORY_NOT_FOUND" => (400, "Bad Request"),
            "FOREIGN_KEY_VIOLATION" => (400, "Bad Request"),
            "OWNER_NOT_FOUND" => (400, "Bad Request"),
            "NULL_VIOLATION" => (400, "Bad Request"),

            _ => (400, "Bad Request")
        };
    }
}
