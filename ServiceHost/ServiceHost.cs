using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NLog;
using Thorium.Config;
using Thorium.Reflection;

namespace Thorium.Net.ServiceHost
{
    public class ServiceHost
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private bool started = false;

        private readonly List<IInvokationHandler> invokationHandlers = new List<IInvokationHandler>();
        private readonly List<IInvokationReceiver> invokationReceivers = new List<IInvokationReceiver>();
        public IReadOnlyList<IInvokationReceiver> InvokationReceivers { get { return invokationReceivers; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configName"><see cref="ConfigFile.GetConfig"/></param>
        public ServiceHost(string configName = null)
        {
            if(configName != null)
            {
                var config = ConfigFile.GetConfig(configName);
                if(config.TryGet<JArray>("invokationReceivers", out JArray invokationReceivers))
                {
                    foreach(var val in invokationReceivers)
                    {
                        if(val is JObject jo && jo.Get("load", false))
                        {
                            var receiverType = jo.Get<string>("type");

                            Type type = ReflectionHelper.GetType(receiverType);
                            if(type == null)
                            {
                                logger.Warn("Couldn't find type: " + receiverType);
                                continue;
                            }
                            ConstructorInfo ci = null;
                            ci = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);
                            if(ci != null)
                            {
                                AddInvokationReceiverFromConstructor(ci, new object[] { jo.Get<string>("config") });
                            }
                            else
                            {
                                ci = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                                if(ci == null)
                                {
                                    logger.Warn("Couldn't find constructor for " + type.AssemblyQualifiedName + ". Provide either a default constructor or one that takes a string argument");
                                    continue;
                                }
                                AddInvokationReceiverFromConstructor(ci, new object[0]);
                            }
                        }
                    }
                }

                if(config.TryGet<JArray>("invokationHandlers", out JArray routineHandlers))
                {
                    foreach(var val in routineHandlers)
                    {
                        if(val is JObject jo && jo.Get("load", false))
                        {
                            var handlerType = jo.Get<string>("type");
                            Type type = ReflectionHelper.GetType(handlerType);

                            if(type == null)
                            {
                                logger.Warn("Couldn't find type: " + handlerType);
                                continue;
                            }

                            ConstructorInfo ci = null;
                            ci = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);
                            if(ci != null)
                            {
                                AddInvokationHandlerFromConstructor(ci, new object[] { jo.Get<string>("config") });
                            }
                            else
                            {
                                ci = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                                if(ci == null)
                                {
                                    logger.Warn("Couldn't find constructor for " + type.AssemblyQualifiedName + ". Provide either a default constructor or one that takes a string argument");
                                    continue;
                                }
                                AddInvokationHandlerFromConstructor(ci, new object[0]);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// creates an instance if possible and adds it. will not add on error and only write log information
        /// </summary>
        /// <param name="ci">the constructor to invoke</param>
        /// <param name="args">the arguments</param>
        private void AddInvokationReceiverFromConstructor(ConstructorInfo ci, object[] args)
        {
            try
            {
                object obj = ci.Invoke(args);
                if(obj is IInvokationReceiver sir)
                {
                    RegisterInvokationReceiver(sir);
                }
                else
                {
                    logger.Warn(ci.ReflectedType.AssemblyQualifiedName + " Is not a " + nameof(IInvokationReceiver));
                }
            }
            catch(TargetInvocationException ex)
            {
                logger.Error("Error thrown when creating service invokation receiver: " + ci.ReflectedType.AssemblyQualifiedName);
                logger.Error(ex);
            }
        }

        /// <summary>
        /// creates an instance if possible and adds it. will not add on error and only write log information
        /// </summary>
        /// <param name="ci">the constructor to invoke</param>
        /// <param name="args">the arguments</param>
        private void AddInvokationHandlerFromConstructor(ConstructorInfo ci, object[] args)
        {
            try
            {
                object obj = ci.Invoke(args);
                if(obj is IInvokationHandler ih)
                {
                    RegisterInvokationHandler(ih);
                }
                else
                {
                    logger.Warn(ci.ReflectedType.AssemblyQualifiedName + " Is not a " + nameof(IInvokationHandler));
                }
            }
            catch(TargetInvocationException ex)
            {
                logger.Error("Error thrown when creating service invokation handler: " + ci.ReflectedType.AssemblyQualifiedName);
                logger.Error(ex);
            }
        }

        /// <summary>
        /// will throw an <see cref="InvalidOperationException"/> if the servive host has been started
        /// </summary>
        private void RequireNotStarted()
        {
            if(started)
            {
                throw new InvalidOperationException("Can't register things after start");
            }
        }

        public void RegisterInvokationHandler(IInvokationHandler handler)
        {
            invokationHandlers.Add(handler);
        }

        public void RegisterInvokationReceiver(IInvokationReceiver si)
        {
            invokationReceivers.Add(si);
        }

        public void Start()
        {
            if(started)
            {
                throw new InvalidOperationException("Can't start more than once");
            }

            foreach(var si in invokationReceivers)
            {
                si.InvokationReceived += HandleInvokationReceived;
                si.Start();
            }
        }

        public void Stop()
        {
            if(!started)
            {
                throw new InvalidOperationException("Can't stop before starting");
            }

            foreach(var si in invokationReceivers)
            {
                si.InvokationReceived -= HandleInvokationReceived;
                si.Stop();
            }
        }

        private InvokationResult HandleInvokationReceived(IInvokationReceiver sender, string routine, JToken arg)
        {
            for(int i = 0; i < invokationHandlers.Count; i++)
            {
                var handler = invokationHandlers[i];
                if(handler.HasRoutine(routine))
                {

                    try
                    {
                        JToken retval = handler.HandleInvokation(routine, arg);
                        return new InvokationResult() { ReturnValue = retval };
                    }
                    catch(Exception ex)
                    {
                        return new InvokationResult() { Exception = ex };
                    }

                }
            }
            return new InvokationResult() { Exception = new Exception("No routine named '" + routine + "' registered") };
        }
    }
}
