using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Thorium.Config;

namespace Thorium.Net.ServiceHost
{
    [CompatibleInvoker(typeof(HttpServiceInvoker))]
    public class HttpServiceInvokationReceiver : ServiceInvokationReceiver
    {
        HttpListener listener;

        private int port;

        public HttpServiceInvokationReceiver(Config.Config config) : base(config)
        {
            dynamic c = config;
            port = c.Port;
        }

        public HttpServiceInvokationReceiver(string configName) : this(ConfigFile.GetConfig(configName)) { }

        public override void Start()
        {
            listener = new HttpListener();

            listener.Prefixes.Add(string.Format("http://*:{0}/", port));

            listener.Start();
            listener.BeginGetContext(GetContext, null);
        }

        public override void Stop()
        {
            listener.Stop();
            listener = null;
        }

        private string FromB64(string str)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(str));
        }

        private void GetContext(IAsyncResult res)
        {
            var context = listener.EndGetContext(res);

            try
            {
                string routine = FromB64(context.Request.QueryString["routine"]);
                string arg = "null";
                try
                {
                    arg = FromB64(context.Request.QueryString["arg"]);
                }
                catch { }
                var result = RaiseInvokationReceived(routine, JToken.Parse(arg));

                context.Response.ContentType = "application/json";

                JObject responseObject = new JObject();
                if(result.Exception != null)
                {
                    responseObject["status"] = "exception";
                    responseObject["exception"] = result.Exception.ToString();
                }
                else
                {
                    responseObject["status"] = "success";
                    responseObject["returnValue"] = result.ReturnValue;
                }

                using(StreamWriter sw = new StreamWriter(context.Response.OutputStream))
                {
                    sw.Write(responseObject.ToString(Newtonsoft.Json.Formatting.None));
                }
            }
            catch(Exception ex)
            {
                //meh, log?
                using(StreamWriter sw = new StreamWriter(context.Response.OutputStream))
                {
                    sw.Write("Exception occured while executing request: " + ex.ToString());
                }
            }

            context.Response.Close();

            listener.BeginGetContext(GetContext, null); //TODO: can this be at the start of the handler too? does it matter?
        }
    }
}
