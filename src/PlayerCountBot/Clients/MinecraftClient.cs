using PlayerCountBot.Encoders;
using PlayerCountBot.Models.Minecraft;
using System.Net.Sockets;

namespace PlayerCountBot.Clients
{
    class MinecraftClient
        : IDisposable
    {
        private const int MaxMessageSize = 4110; // 4096 + 14 bytes of header data.

        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _connection;
        private int lastID = 0;

        public MinecraftClient(string host, int port)
        {
            _tcpClient = new TcpClient(host, port);
            _connection = _tcpClient.GetStream();
        }

        public void Dispose()
        {
            this.Close();
        }

        public void Close()
        {
            _connection.Close();
            _tcpClient.Close();
        }

        public bool Authenticate(string password)
        {
            return sendMessage(new MinecraftMessage(
                password.Length + MinecraftEncoder.HeaderLength,
                Interlocked.Increment(ref lastID),
                MessageType.Authenticate,
                password
            ), out _);
        }

        public bool SendCommand(string command, out MinecraftMessage responseMessage)
        {
            return sendMessage(new MinecraftMessage(
                command.Length + MinecraftEncoder.HeaderLength,
                Interlocked.Increment(ref lastID),
                MessageType.Command,
                command
            ), out responseMessage);
        }

        private bool sendMessage(MinecraftMessage req, out MinecraftMessage resp)
        {
            // Send the message.
            byte[] encoded = MinecraftEncoder.EncodeMessage(req);
            _connection.Write(encoded, 0, encoded.Length);

            // Receive the response.
            byte[] respBytes = new byte[MaxMessageSize];
            int bytesRead = _connection.Read(respBytes, 0, respBytes.Length);
            Array.Resize(ref respBytes, bytesRead);

            // Decode the response and check for errors before returning.
            resp = MinecraftEncoder.DecodeMessage(respBytes);
            if (req.ID != resp.ID) { return false; };
            return true;
        }
    }
}
