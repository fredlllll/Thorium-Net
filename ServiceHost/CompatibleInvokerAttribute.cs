using System;

namespace Thorium.Net.ServiceHost
{
    /// <summary>
    /// indicates what type of invoker is compatible with this invokation receiver
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class CompatibleInvokerAttribute : Attribute
    {
        public Type Type { get; }

        public CompatibleInvokerAttribute(Type type)
        {
            Type = type;
        }
    }
}
