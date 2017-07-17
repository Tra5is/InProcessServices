using System;

namespace InProcessServices
{
    public class InProcessServiceAttribute : Attribute
    {
        public Type ProxyType { get; set; }
    }
}