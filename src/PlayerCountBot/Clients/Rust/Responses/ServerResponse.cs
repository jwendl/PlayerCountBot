namespace PlayerCountBot.Clients.Rust.Commands.Responses
{
    public class ServerResponse
    {
        public string Type { get; set; } = default!;

        public string Stacktrace { get; set; } = default!;

        public int Identifier { get; set; }

        public string Message { get; set; } = default!;
    }
}
