namespace WebReckrytingSystem.Models
{
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

        // Новый метод для безопасного создания ошибок
        public static ServiceResult SafeError(string message, Exception? ex = null)
        {
            // Логируем ошибку для разработчика
            if (ex != null)
            {
                Console.WriteLine($"ServiceResult Error: {ex.Message}");
            }

            return new ServiceResult { IsSuccess = false, Message = message };
        }
    }
}