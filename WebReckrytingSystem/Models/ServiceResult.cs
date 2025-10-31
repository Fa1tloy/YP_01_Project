// Models/ServiceResult.cs
namespace WebReckrytingSystem.Models
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ServiceResult<T> Success(string message, T? data = default)
        {
            return new ServiceResult<T> { IsSuccess = true, Message = message, Data = data };
        }

        public static ServiceResult<T> Error(string message)
        {
            return new ServiceResult<T> { IsSuccess = false, Message = message };
        }
    }

    // Для обратной совместимости оставляем старый ServiceResult
    public class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? Data { get; set; }

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