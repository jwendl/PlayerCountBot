namespace PlayerCountBot.Clients.Rust.Commands.Messages
{
    public class ChatMessage
    {
        public int Channel { get; set; }
        public string? Message { get; set; }
        public long UserId { get; set; }
        public string? Username { get; set; }
        public string? Color { get; set; }
        public long Time { get; set; }
    }
}
