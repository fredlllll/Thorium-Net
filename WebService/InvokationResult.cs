using System;
using Newtonsoft.Json.Linq;

namespace Thorium.Net.ServiceHost
{
    public class InvokationResult
    {
        public Exception Exception { get; set; }
        public JToken ReturnValue { get; set; }
    }
}
