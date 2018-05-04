using Newtonsoft.Json.Linq;

namespace Thorium.Net.ServiceHost
{
    public delegate InvokationResult InvokationHandler(IServiceInvokationReceiver sender, string routine, JToken arg);

    public interface IServiceInvokationReceiver
    {
        event InvokationHandler InvokationReceived;

        Config.Config Configuration { get;}

        void Start();
        void Stop();
    }
}
