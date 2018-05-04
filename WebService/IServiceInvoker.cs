using Newtonsoft.Json.Linq;

namespace Thorium.Net.ServiceHost
{
    public interface IServiceInvoker
    {
        JToken Invoke(string routine, JToken arg);
    }
}
