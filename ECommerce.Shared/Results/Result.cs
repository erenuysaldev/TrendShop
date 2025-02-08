namespace ECommerce.Shared.Results
{
    public class Result : IResult
    {
        public bool Success { get; }
        public string Message { get; }

        public Result(bool success, string message = null)
        {
            Success = success;
            Message = message;
        }
    }
} 