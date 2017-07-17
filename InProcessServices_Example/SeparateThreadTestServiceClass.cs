using System.Threading;
using InProcessServices;
using InProcessServices.Serialization;

namespace InProcessServices_Example
{
    public class SeparateThreadTestServiceClass : ISeparateThreadTest
    {
        public int GetThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }

        public IDataObject ReturnsValue(IDataObject dataObject)
        {
            return dataObject;
        }
    }

    public interface IDataObject
    {
        string Property1 { get; set; }
        byte[] Property2 { get; set; }
        IDataObject Property3 { get; set; }
    }

    public class DataObject : IDataObject
    {
        public string Property1 { get; set; }
        public byte[] Property2 { get; set; }
        public IDataObject Property3 { get; set; }
    }

    [InProcessService(ProxyType = typeof(SeparateThreadTestProxy))]
    public interface ISeparateThreadTest
    {
        int GetThreadId();
        IDataObject ReturnsValue(IDataObject dataObject);
    }

    public class SeparateThreadTestProxy : ServiceProxy<SeparateThreadTestServiceClass>, ISeparateThreadTest
    {
        public SeparateThreadTestProxy(SeparateThreadTestServiceClass service, ISerializeInProcessObjects serializer) : base(service, serializer)
        {
        }

        public int GetThreadId()
        {
            return ProxyMethodCallOnSeparateThread<int>("GetThreadId");
        }

        public IDataObject ReturnsValue(IDataObject dataObject)
        {
            return ProxyMethodCallOnSeparateThread<IDataObject>("ReturnsValue", dataObject);
        }
    }
}