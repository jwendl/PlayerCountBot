namespace PlayerCountBot.Clients.Rust.Responses
{
    public class ServerInformation
    {
        public string Hostname { get; set; } = default!;

        public int MaxPlayers { get; set; }

        public int Players { get; set; }

        public int Queued { get; set; }

        public int Joining { get; set; }

        public int EntityCount { get; set; }

        public string GameTime { get; set; } = default!;

        public int Uptime { get; set; }

        public string Map { get; set; } = default!;

        public double Framerate { get; set; }

        public int Memory { get; set; }

        public int Collections { get; set; }

        public int NetworkIn { get; set; }

        public int NetworkOut { get; set; }

        public bool Restarting { get; set; }

        public string SaveCreatedTime { get; set; } = default!;
    }
}
