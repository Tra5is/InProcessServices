using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using InProcessServices.Serialization;

namespace InProcessServices
{
    public class ServiceProxy<TService>
    {
        protected TService service;
        protected ISerializeInProcessObjects serializer;

        public ServiceProxy(TService service, ISerializeInProcessObjects serializer)
        {
            this.service = service;
            this.serializer = serializer;
        }

        protected object ProxyMethodCall(string methodName, params object[] parameters)
        {
            return ProxyMethodCall<object>(methodName, parameters);
        }

        protected TReturn ProxyMethodCall<TReturn>(string methodName, params object[] parameters)
        {
            var proxiedParameters = new List<object>();
            foreach (var param in parameters)
            {
                proxiedParameters.Add(serializer.Copy(param));
            }

            var response = this.service.GetType().GetMethod(methodName).Invoke(this.service, proxiedParameters.ToArray());

            if (response == null)
                return default(TReturn);
            else
                return (TReturn) serializer.Copy(response);
        }

        protected object ProxyMethodCallOnSeparateThread(string methodName, params object[] parameters)
        {
            return ProxyMethodCallOnSeparateThread<object>(methodName, parameters);
        }

        protected TReturn ProxyMethodCallOnSeparateThread<TReturn>(string methodName, params object[] parameters)
        {
            return Task.Run(() => ProxyMethodCall<TReturn>(methodName, parameters)).GetAwaiter().GetResult();
        }
    }
}
