using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InProcessServices.Serialization.Json.NET
{
    public class JsonObjectCopier : ISerializeInProcessObjects
    {
        private JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        public object Copy(object obj)
        {
            //Todo: Check if object serializable by this serializer


            string serializedObj = JsonConvert.SerializeObject(obj, settings);
            //return JObject.Parse(output).ToObject(obj.GetType());
            return JsonConvert.DeserializeObject(serializedObj, obj.GetType(), settings);
        }
    }
}
