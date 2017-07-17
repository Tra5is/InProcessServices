using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;

namespace InProcessServices.Autofac
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterInProcService<TService>(this ContainerBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            //register the service itself. This will provide an instance of the service which the proxy will call into
            IRegistrationBuilder<TService, ConcreteReflectionActivatorData, SingleRegistrationStyle> rb = RegistrationBuilder.ForType<TService>();
            rb.RegistrationData.DeferredCallback = builder.RegisterCallback((Action<IComponentRegistry>)(cr => RegistrationBuilder.RegisterSingleComponent<TService, ConcreteReflectionActivatorData, SingleRegistrationStyle>(cr, rb)));
            
            //get the proxy class from the interface's service attribute
            var serviceInterfaces = typeof(TService)
                .GetInterfaces()
                .Where(i => i.GetTypeInfo().GetCustomAttribute<InProcessServiceAttribute>() != null);

            //Build in-proc proxy for each service interface and add that to the container builder
            foreach (var serviceInterface in serviceInterfaces)
            {
                //TODO: generate the proxy automatically instead of pulling type from attribute

                var attr = serviceInterface.GetTypeInfo().GetCustomAttribute<InProcessServiceAttribute>();
                var serviceProxyType = attr.ProxyType;
                IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> rbProxy = RegistrationBuilder.ForType(serviceProxyType);
                rbProxy.RegistrationData.DeferredCallback = builder.RegisterCallback((Action<IComponentRegistry>)(cr => RegistrationBuilder.RegisterSingleComponent<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>(cr, rbProxy)));
                rbProxy.AsImplementedInterfaces();
            }
        }
    }
}
