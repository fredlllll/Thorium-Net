using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;
using Thorium.IO;
using Thorium.Reflection;

namespace Thorium.Net.ServiceHost.InvokationHandlers
{
    public class RoutineInvokationHandler : IInvokationHandler
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, Routine> routines = new Dictionary<string, Routine>();

        public RoutineInvokationHandler() { }

        public RoutineInvokationHandler(string configFile)
        {
            var config = Config.ConfigFile.GetConfig(configFile);

            if(config.TryGet("routines", out JArray arr))
            {
                foreach(JObject obj in arr)
                {
                    string typeName = obj.Get<string>("type");
                    string methodName = obj.Get<string>("method");
                    string name = obj.Get<string>("name", methodName);

                    Type type = ReflectionHelper.GetType(typeName);

                    MethodInfo mi = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(JToken) }, null);
                    if(mi == null)
                    {
                        logger.Warn("Couldn't find suitable method " + methodName + " that takes a JToken");
                        continue;
                    }
                    if(!mi.ReturnType.Equals(typeof(JToken)))
                    {
                        logger.Warn("The routine handler " + methodName + " has to return JToken");
                        continue;
                    }
                    RoutineHandler routineHandler = (RoutineHandler)Delegate.CreateDelegate(typeof(RoutineHandler), mi);
                    Routine routine = new Routine(name, routineHandler);
                    RegisterRoutine(routine);
                }
            }
        }

        public JToken HandleInvokation(string routine, JToken arg)
        {
            Routine r = routines[routine];
            return r.Invoke(arg);
        }

        public bool HasRoutine(string routine)
        {
            return routines.ContainsKey(routine);
        }

        public void RegisterRoutine(Routine routine)
        {
            if(routines.ContainsKey(routine.Name))
            {
                throw new ArgumentException("There is already a routine named '" + routine.Name + "' registered");
            }
            routines[routine.Name] = routine;
        }

    }
}
