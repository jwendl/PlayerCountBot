using Microsoft.Extensions.Logging;
using PlayerCountBot.Clients.Rust.Commands.Base;
using PlayerCountBot.Clients.Rust.Commands.Messages;
using PlayerCountBot.Clients.Rust.Commands.Responses;
using PlayerCountBot.Clients.Rust.Extensions;
using System.Text.Json;
using WebSocketSharp;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;

namespace PlayerCountBot.Clients.Rust
{
    public interface IRustClient
    {
        void Connect(string hostName, int port, string password);

        void Disconnect();

        void SendCommand(BaseCommand command);
    }

    public class RustClient
        : IRustClient, IDisposable
    {
        private WebSocket? _client;
        private readonly List<BaseCommand> _commands;
        private readonly ILogger<RustClient> _logger;

        public RustClient(ILogger<RustClient> logger)
        {
            _logger = logger;
            _commands = new List<BaseCommand>();

            OnConsoleMessage = (cm) => { };
            OnChatMessage = (cm) => { };
            OnConnectionChanged = (v) => { };
        }

        public void Connect(string hostName, int port, string password)
        {
            _client = new WebSocket($"ws://{hostName}:{port}/{password}");
            try
            {
                _client.Connect();
                _client.OnClose += OnClose!;
                _client.OnError += OnError!;
                _client.OnMessage += OnMessage!;
                _client.OnOpen += OnOpen!;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "[RustClient] {Message}", exception.Message);
            }
        }

        public void Disconnect()
        {
            _client?.Close();
        }

        public void SendCommand(BaseCommand baseCommand)
        {
            if (_client == null)
            {
                throw new Exception(nameof(_client));
            }

            if (baseCommand == null)
            {
                throw new ArgumentNullException(nameof(baseCommand));
            }

            string json = JsonSerializer.Serialize(baseCommand);

            _client.Send(json);

            _commands.Add(baseCommand);
        }

        public bool IsConnected
        {
            get
            {
                if (_client == null)
                {
                    throw new Exception(nameof(_client));
                }

                return _client.IsAlive;
            }
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data)) return;

            var response = JsonSerializer.Deserialize<ServerResponse>(e.Data);

            if (response == null) return;

            var command = _commands.Find(x => x.ID == response.Identifier);

            if (command != null)
            {
                if (command.Completed) return;

                command.Complete(response);
                command.Dispose();
                _commands.Remove(command);
                return;
            }

            try
            {
                if (response == null || response.Message == null || response.Message.StartsWith("[rcon]"))
                {
                    return;
                }

                switch (response.Identifier)
                {
                    case 0:
                        {
                            if (response != null && response.Type != null)
                            {
                                OnConsoleMessage?.Invoke(new ConsoleMessage(response.Message, response.Type.ToEnum<ConsoleMessage.MessageType>()));
                            }
                            break;
                        }
                    default:
                        if (response.Type == "Chat")
                        {
                            var chatMessage = JsonSerializer.Deserialize<ChatMessage>(response.Message);
                            OnChatMessage?.Invoke(chatMessage!);
                        }
                        break;
                }
            }
            catch (Exception)
            {

            }
        }

        private void OnOpen(object sender, EventArgs e)
        {
            OnConnectionChanged?.Invoke(true);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            _logger.LogError(e.Exception, "[RustClient] {Message}", e.Message);

            try
            {
                if (_client == null)
                {
                    throw new Exception(nameof(_client));
                }

                _client.Close();
                OnConnectionChanged?.Invoke(false);
            }
            catch
            {

            }
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            OnConnectionChanged?.Invoke(false);
        }

        public void Dispose()
        {
            if (_client == null)
            {
                throw new Exception(nameof(_client));
            }

            if (IsConnected)
            {
                _client.Close();
            }

            GC.SuppressFinalize(this);
        }

        public event Action<ConsoleMessage> OnConsoleMessage;
        public event Action<ChatMessage> OnChatMessage;
        public event Action<bool> OnConnectionChanged;
    }
}
