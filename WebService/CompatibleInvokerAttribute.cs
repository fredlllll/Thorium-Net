using System;

namespace Thorium.Net.ServiceHost
{
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
