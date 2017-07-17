using System;
using System.Collections.Generic;
using InProcessServices;
using InProcessServices.Serialization;
using Newtonsoft.Json;

namespace InProcessServices_Example
{
    public class MemoryIsolationTestServiceClass : IMemoryIsolationTest
    {
        [JsonProperty]
        private object state = null;
        [JsonProperty]
        private IMemoryIsolationTest dependency;

        public int ReturnInteger10()
        {
            return 10;
        }

        public void SetStateValue(object stateValue)
        {
            this.state = stateValue ?? throw new ArgumentNullException(nameof(stateValue));
        }

        public object GetStateValue()
        {
            return this.state;
        }

        public void SetDependency(IMemoryIsolationTest dependency)
        {
            this.dependency = dependency;
        }

        public void UpdateDependency()
        {
            dependency.SetStateValue("something");
        }

        public IMemoryIsolationTest GetDependency()
        {
            return dependency;
        }
    }

    public class MemoryIsolationTestProxy : ServiceProxy<MemoryIsolationTestServiceClass>, IMemoryIsolationTest
    {
        public MemoryIsolationTestProxy(MemoryIsolationTestServiceClass service, ISerializeInProcessObjects serializer)
            : base(service, serializer)
        {
            this.service = service;
        }

        public int ReturnInteger10()
        {
            return service.ReturnInteger10();
        }

        public void SetStateValue(object stateValue)
        {
            ProxyMethodCall("SetStateValue", stateValue);
        }

        public object GetStateValue()
        {
            return ProxyMethodCall("GetStateValue");
        }

        public void SetDependency(IMemoryIsolationTest dependency)
        {
            ProxyMethodCall("SetDependency", dependency);
        }

        public void UpdateDependency()
        {
            service.UpdateDependency();
        }

        public IMemoryIsolationTest GetDependency()
        {
            return ProxyMethodCall<IMemoryIsolationTest>("GetDependency");
        }
    }

    [InProcessService(ProxyType = typeof(MemoryIsolationTestProxy))]
    public interface IMemoryIsolationTest
    {
        int ReturnInteger10();
        void SetStateValue(object stateValue);
        object GetStateValue();
        void SetDependency(IMemoryIsolationTest dependency);
        void UpdateDependency();
        IMemoryIsolationTest GetDependency();
    }
}