using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Thorium.Net.ServiceHost
{
    public abstract class ServiceInvokationReceiver : IServiceInvokationReceiver
    {
        public Config.Config Configuration { get; protected set; }

        public event InvokationHandler InvokationReceived;

        protected InvokationResult RaiseInvokationReceived(string routine, JToken arg)
        {
            return InvokationReceived?.Invoke(this, routine, arg);
        }

        public ServiceInvokationReceiver(Config.Config configuration)
        {
            Configuration = configuration;
        }

        public abstract void Start();
        public abstract void Stop();
    }
}
