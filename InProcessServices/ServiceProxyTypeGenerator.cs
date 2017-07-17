using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;


namespace InProcessServices
{
    public class ServiceProxyTypeGenerator
    {
        public static Type GenerateForService<TService>()
        {
            var typeSignature = "InProcessServiceProxies";
            var an = new AssemblyName(typeSignature);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(typeSignature + ".dll"), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("ProxiesModule");
            TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                null);

            var serviceInterfaces = typeof(TService)
                .GetInterfaces()
                .Where(i => i.GetTypeInfo().GetCustomAttribute<InProcessServiceAttribute>() != null);

            //Add constructor to accept an instance of the type to call and a serializer/deserializer

            foreach (var serviceInterface in serviceInterfaces)
            {
                tb.AddInterfaceImplementation(serviceInterface.GetType());

                foreach (var method in serviceInterface.GetMethods())
                {
                    BuildProxyMethod(method, tb);
                }
            }

            return tb.GetType();
        }

        public static void BuildProxyMethod(MethodInfo method, TypeBuilder typeBuilder)
        {
            Type[] param_types = method.GetParameters()
                .Select(p => p.GetType())
                .ToArray();
            MethodBuilder mb = typeBuilder.DefineMethod(
                method.Name,
                method.Attributes);

            Type toReturn = method.ReturnType;

            mb.SetReturnType(toReturn);
            mb.SetParameters(param_types);

            typeBuilder.DefineMethodOverride(mb, method);
        }
    }
}