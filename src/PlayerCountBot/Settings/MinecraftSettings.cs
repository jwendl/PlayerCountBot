﻿using System.Net;

namespace PlayerCountBot.Settings
{
    public class MinecraftSettings
    {
        public string IpAddress { get; set; } = default!;

        public int Port { get; set; }

        public string RconPassword { get; set; } = default!;
    }
}
