using System;
using System.Reflection;
using MP = MessagePack;

namespace InProcessServices.Serialization.MessagePack
{
    public class MessagePackObjectCopier : ISerializeInProcessObjects
    {
        MethodInfo DeserializeMethod = typeof(MP.MessagePackSerializer).GetMethod("Deserialize", new Type[] { typeof(byte[]) });

        public object Copy(object obj)
        {
            var bin = MP.MessagePackSerializer.Serialize(obj, MP.Resolvers.ContractlessStandardResolver.Instance);
            //MP.MessagePackSerializer.Deserialize<object>(bin);
            return CallDeserialize(bin, obj.GetType());
        }

        public object CallDeserialize(byte[] serializedObject, Type type)
        {
            MethodInfo method = DeserializeMethod.MakeGenericMethod(type);
            return method.Invoke(null, new [] { serializedObject });
        }
    }
}
