namespace rozetochka_api.Shared
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; }
        public string? ErrorMessage { get; }     // Пример: "Email already taken"
        public string? ErrorCode { get; }       // Пример: "EMAIL_TAKEN"
        public T? Data { get; }

        private ServiceResult(bool isSuccess, T? data, string? errorMessage, string? errorCode = null)
        {
            IsSuccess = isSuccess;
            Data = data;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }

        public static ServiceResult<T> Ok(T data)
        {
            return new ServiceResult<T>(true, data, null);
        }

        public static ServiceResult<T> Fail(string errorMessage, string? errorCode = null)
        {
            return new ServiceResult<T>(false, default, errorMessage, errorCode);     // default - значение по умолчанию для типа T (null для reference types, 0/false для value types)
        }
    }
}
