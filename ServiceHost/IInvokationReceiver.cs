using Newtonsoft.Json.Linq;

namespace Thorium.Net.ServiceHost
{
    public delegate InvokationResult InvokationHandler(IInvokationReceiver sender, string routine, JToken arg);

    public interface IInvokationReceiver
    {
        event InvokationHandler InvokationReceived;

        Config.Config Configuration { get;}

        void Start();
        void Stop();
    }
}
