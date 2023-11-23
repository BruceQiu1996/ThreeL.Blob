namespace ThreeL.Blob.Clients.Win.Dtos
{
    public class HubMessageResponseDto<T> where T : class
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public static HubMessageResponseDto<object> GetNewDefaultSuccessInstance(string message = null, T data = null)
        {
            return new HubMessageResponseDto<object>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static HubMessageResponseDto<object> GetNewDefaultFailInstance(string message = null, T data = null)
        {
            return new HubMessageResponseDto<object>
            {
                Success = false,
                Message = message,
                Data = data
            };
        }
    }
}
