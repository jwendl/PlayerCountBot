namespace PlayerCountBot.Clients.Minecraft.Models
{
    public class MinecraftStatusResponse
    {
        public int CurrentPlayers { get; set; } = 0;

        public int MaximumPlayers { get; set; } = 0;
    }
}
