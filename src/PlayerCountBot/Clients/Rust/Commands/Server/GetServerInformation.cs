using PlayerCountBot.Clients.Rust.Commands.Base;
using PlayerCountBot.Clients.Rust.Commands.Responses;
using PlayerCountBot.Clients.Rust.Responses;
using System.Text.Json;

namespace PlayerCountBot.Clients.Rust.Commands.Server
{
    public class GetServerInformation
        : BaseCommand
    {
        private Action<ServerInformation>? _callback;

        public GetServerInformation(Action<ServerInformation>? callback = null)
            : base("serverinfo")
        {
            _callback = callback;
        }

        public override void Complete(ServerResponse response)
        {
            base.Complete(response);

            try
            {
                var serverInfo = JsonSerializer.Deserialize<ServerInformation>(response?.Message!);

                _callback?.Invoke(serverInfo!);
            }
            catch
            {
                _callback?.Invoke(new ServerInformation());
            }
        }

        public override void Dispose()
        {
            _callback = null;

            GC.SuppressFinalize(this);
        }
    }
}
