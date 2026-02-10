namespace Api.Middlewares
{
    public class ErrorResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public string? StackTrace { get; set; }
    }

}
