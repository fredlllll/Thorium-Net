using Newtonsoft.Json.Linq;

namespace Thorium.Net.ServiceHost
{
    public interface IInvokationHandler
    {
        bool HasRoutine(string routine);
        JToken HandleInvokation(string routine, JToken arg);
    }
}
