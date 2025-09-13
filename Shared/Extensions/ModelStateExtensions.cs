using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace rozetochka_api.Shared.Extensions
{
  
    public static class ModelStateExtensions
    {

        // Собирает ошибки ModelState в Dictionary для отправки на front-end, если !ModelState.IsValid
        public static Dictionary<string, List<string>> ToErrorDictionary(this ModelStateDictionary modelState)
        {
            return modelState
                .Where(ms => ms.Value?.Errors.Any() == true)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                );
        }
    }
}
