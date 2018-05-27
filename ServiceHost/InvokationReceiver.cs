using Newtonsoft.Json.Linq;

namespace Thorium.Net.ServiceHost
{
    public abstract class InvokationReceiver : IInvokationReceiver
    {
        public Config.Config Configuration { get; protected set; }

        public event InvokationHandler InvokationReceived;

        protected InvokationResult RaiseInvokationReceived(string routine, JToken arg)
        {
            return InvokationReceived?.Invoke(this, routine, arg);
        }

        public InvokationReceiver(Config.Config configuration)
        {
            Configuration = configuration;
        }

        public abstract void Start();
        public abstract void Stop();
    }
}
