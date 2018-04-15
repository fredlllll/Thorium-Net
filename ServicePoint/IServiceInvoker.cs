using Newtonsoft.Json.Linq;

namespace Thorium.Net
{
    public interface IServiceInvoker
    {
        JToken Invoke(string routine, JToken arg);
    }
}
