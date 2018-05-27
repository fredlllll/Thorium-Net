using Newtonsoft.Json.Linq;

namespace Thorium.Net.ServiceHost
{
    public interface IInvoker
    {
        JToken Invoke(string routine, JToken arg);
    }
}
