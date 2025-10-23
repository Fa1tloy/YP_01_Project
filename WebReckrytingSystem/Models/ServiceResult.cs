namespace WebReckrytingSystem.Models
{
    public class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? Data { get; set; }

        public string? ErrorType { get; set; } // "DuplicateEmail", "ValidationError"

        public static ServiceResult Success(string message, User? data = null)
        {
            return new ServiceResult { IsSuccess = true, Message = message, Data = data };
        }

        public static ServiceResult Error(string message)
        {
            return new ServiceResult { IsSuccess = false, Message = message };
        }
    }
}