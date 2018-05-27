using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Thorium.Net.ServiceHost
{
    public interface IInvokationHandler
    {
        bool HasRoutine(string routine);
        JToken HandleInvokation(string routine, JToken arg);
    }
}
