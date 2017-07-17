namespace InProcessServices.Serialization
{
    public interface ISerializeInProcessObjects
    {
        object Copy(object obj);
    }
}