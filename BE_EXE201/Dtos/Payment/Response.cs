namespace BE_EXE201.Dtos.Payment
{
    public class Response
    {
        public int error { get; set; }
        public string message { get; set; }
        public object? data { get; set; }

        // Parameterless constructor
        public Response() { }

        // Constructor with three parameters
        public Response(int error, string message, object? data)
        {
            this.error = error;
            this.message = message;
            this.data = data;
        }
    }
}
