using PlayerCountBot.Clients.Rust.Commands.Responses;

namespace PlayerCountBot.Clients.Rust.Commands.Base
{
    public abstract class BaseCommand
        : BasePackage, IDisposable
    {
        public bool Completed { get; private set; }

        public string Name { get { return "WebRcon"; } }

        public virtual void Complete(ServerResponse response)
        {
            if (Completed) return;

            Completed = true;
        }

        public abstract void Dispose();

        public BaseCommand(string message)
            : base(message)
        {

        }
    }
}
