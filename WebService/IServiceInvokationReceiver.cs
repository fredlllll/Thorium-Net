using Newtonsoft.Json.Linq;

namespace Thorium.Net
{
    public delegate InvokationResult InvokationHandler(IServiceInvokationReceiver sender, string routine, JToken arg);

    public interface IServiceInvokationReceiver
    {
        event InvokationHandler InvokationReceived;

        void Start();
        void Stop();
    }
}
