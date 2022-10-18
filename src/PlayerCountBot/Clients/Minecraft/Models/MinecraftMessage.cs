namespace PlayerCountBot.Clients.Minecraft.Models
{
    public readonly struct MinecraftMessage
    {
        public readonly int Length;
        public readonly int ID;
        public readonly MessageType Type;
        public readonly string Body;

        public MinecraftMessage(int length, int id, MessageType type, string body)
        {
            Length = length;
            ID = id;
            Type = type;
            Body = body;
        }
    }
}
