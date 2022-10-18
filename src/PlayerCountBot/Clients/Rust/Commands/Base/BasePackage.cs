using System.Text.Json.Serialization;

namespace PlayerCountBot.Clients.Rust.Commands.Base
{
    public abstract class BasePackage
    {
        [JsonPropertyName("Identifier")]
        public int ID { get; private set; }

        [JsonPropertyName("Message")]
        public string Content { get; }

        private static int id_counter = 2;

        public BasePackage(int id, string content)
        {
            ID = id;
            Content = content;
        }

        public BasePackage(string content)
        {
            ID = ++id_counter;
            Content = content;
        }
    }
}
