namespace MuniBot.Client.Entities
{
    public class Response<T>
    {
        public T Data { get; set; }
        public bool IsSuccess { get; set; }
        public int error_number { get; set; }
        public string error_message { get; set; }
    }
}
