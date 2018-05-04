using System;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Thorium.Net.ServiceHost
{
    public class HttpServiceInvoker : IServiceInvoker
    {
        private WebClient wc = new WebClient();
        private string host;
        private int port;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host">for example localhost</param>
        /// <param name="port">for example 80</param>
        public HttpServiceInvoker(string host, int port = 80)
        {
            this.host = host;
            this.port = port;
        }

        public HttpServiceInvoker(Config.Config config)
        {
            dynamic c = config;
            port = c.Port;
            host = c.Host;
        }

        private string ToB64(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }

        public JToken Invoke(string routine, JToken arg)
        {
            UriBuilder ub = new UriBuilder("http", host, port)
            {
                Query = "routine=" + ToB64(routine) + "&arg=" + ToB64(arg.ToString(Newtonsoft.Json.Formatting.None))
            };
            string retval = wc.DownloadString(ub.Uri);
            JObject response = JObject.Parse(retval);//should catch parse exception here in case the service returns crap
            switch(response.Get<string>("status"))
            {
                case "success":
                    return response["returnValue"];
                case "exception":
                    throw new Exception("Exception when invoking: " + response.Get<string>("exception"));
                default:
                    throw new Exception("service response did not have a valid state: " + retval);
            }
        }
    }
}
