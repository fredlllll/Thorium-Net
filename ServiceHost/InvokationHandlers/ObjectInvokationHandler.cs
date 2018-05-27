using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Thorium.Net.ServiceHost.InvokationHandlers
{
    public class ObjectInvokationHandler<T> : IInvokationHandler
    {
        private readonly T obj;

        Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();

        public ObjectInvokationHandler(T obj)
        {
            this.obj = obj;
            Type type = typeof(T);

            if(!type.IsInterface)
            {
                throw new ArgumentException("Please use an interface as type argument to avoid accidentally making random methods available");
            }

            MethodInfo[] methods = type.GetMethods();

            foreach(var mi in methods)
            {
                this.methods.Add(mi.Name, mi);
            }
        }

        public JToken HandleInvokation(string routine, JToken arg)
        {
            MethodInfo mi = methods[routine];

            var parameters = mi.GetParameters();
            object[] arguments = new object[parameters.Length];
            if(parameters.Length == 1)
            {
                arguments[0] = arg.Value(parameters[0].ParameterType);
            }
            else if(parameters.Length > 1)
            {
                JObject obj = (JObject)arg;
                for(int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo p = parameters[i];
                    arguments[i] = obj.Get(p.ParameterType, p.Name);
                }
            }

            var result = mi.Invoke(this.obj, arguments);
            if(mi.ReturnType.Equals(typeof(void)))
            {
                return null;
            }
            else
            {
                return JToken.FromObject(result);
            }
        }

        public bool HasRoutine(string routine)
        {
            return methods.ContainsKey(routine);
        }
    }
}
