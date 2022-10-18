namespace PlayerCountBot.Clients.Rust.Commands.Messages
{
    public class ConsoleMessage
    {
        public ConsoleMessage(string message, MessageType type)
        {
            Message = message;
            Type = type;
        }

        public string Message { get; private set; }

        public MessageType Type { get; private set; }

        public enum MessageType
        {
            Generic,
            Log,
            Error,
            Warning
        }
    }
}
