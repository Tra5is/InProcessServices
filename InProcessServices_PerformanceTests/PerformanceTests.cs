using System;
using System.Diagnostics;
using Autofac;
using InProcessServices.Autofac;
using InProcessServices.Serialization.Json.NET;
using InProcessServices.Serialization.MessagePack;
using InProcessServices_Example;
using Xunit;
using Xunit.Abstractions;

namespace InProcessServices_PerformanceTests
{
    public class PerformanceTests
    {
        private ITestOutputHelper output;

        public PerformanceTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void UsingAutofac_UsingJsonDotNet_CallMethodThatReturnsInt1MTimes()
        {
            using (var container = SetupTestContainer<JsonObjectCopier>())
            {
                long loopCount = 1000000;
                var serviceProxy = ResolveService<ISeparateThreadTest>(container);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                for (long i = 0; i < loopCount; i++)
                {
                    serviceProxy.GetThreadId();
                }
                WriteTookMessages(stopwatch, loopCount);
            }
        }

        [Fact]
        public void UsingAutofac_UsingMessagePack_CallMethodThatReturnsInt1MTimes()
        {
            using (var container = SetupTestContainer<MessagePackObjectCopier>())
            {
                long loopCount = 1000000;
                var serviceProxy = ResolveService<ISeparateThreadTest>(container);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                for (long i = 0; i < loopCount; i++)
                {
                    serviceProxy.GetThreadId();
                }
                WriteTookMessages(stopwatch, loopCount);
            }
        }

        [Fact]
        public void UsingAutofac_UsingJsonDotNet_CallMethodThatReturnsObject100kTimes()
        {
            using (var container = SetupTestContainer<JsonObjectCopier>())
            {
                long loopCount = 100000;
                var serviceProxy = ResolveService<ISeparateThreadTest>(container);

                var data = CreateDataObject();

                var stopwatch = new Stopwatch();
                
                for (long i = 0; i < loopCount; i++)
                {
                    stopwatch.Start();
                    var retVal = serviceProxy.ReturnsValue(data);
                    stopwatch.Stop();

                    Assert.Equal(data.Property1, retVal.Property1);
                    Assert.Equal(data.Property2, retVal.Property2);
                    Assert.Equal(data.Property3.Property1, retVal.Property3.Property1);
                }
                WriteTookMessages(stopwatch, loopCount);
            }
        }

        [Fact]
        public void UsingAutofac_UsingMessagePack_CallMethodThatReturnsObject100kTimes()
        {
            using (var container = SetupTestContainer<JsonObjectCopier>())
            {
                long loopCount = 100000;
                var serviceProxy = ResolveService<ISeparateThreadTest>(container);

                var data = CreateDataObject();

                var stopwatch = new Stopwatch();

                for (long i = 0; i < loopCount; i++)
                {
                    stopwatch.Start();
                    var retVal = serviceProxy.ReturnsValue(data);
                    stopwatch.Stop();

                    Assert.Equal(data.Property1, retVal.Property1);
                    Assert.Equal(data.Property2, retVal.Property2);
                    Assert.Equal(data.Property3.Property1, retVal.Property3.Property1);
                }
                WriteTookMessages(stopwatch, loopCount);
            }
        }

        private IDataObject CreateDataObject()
        {
            return new DataObject()
            {
                Property1 = "This is a string value",
                Property2 = new byte[]
                {
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 0,
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 0,
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 0,
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 0,
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 0,
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 0,
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 0,
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 0,
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 0,
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 0,
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 0,
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 0,
                },
                Property3 = new DataObject() {Property1 = "This is a referenced instance of DataObject"}
            };
        }

        private void WriteTookMessages(Stopwatch stopwatch, long loopCount)
        {
            stopwatch.Stop();
            
            output.WriteLine("Total Took: {0}ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks);
            output.WriteLine("Per iteration took: {0}ms, {1}ticks", stopwatch.ElapsedMilliseconds / loopCount, stopwatch.ElapsedTicks/loopCount);
        }

        private TService ResolveService<TService>(IContainer container)
        {
            return container.Resolve<TService>();
        }

        private IContainer SetupTestContainer<TSerializer>()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInProcService<MemoryIsolationTestServiceClass>();
            builder.RegisterInProcService<SeparateThreadTestServiceClass>();
            builder.RegisterType<TSerializer>()
                .AsImplementedInterfaces()
                .SingleInstance();

            var container = builder.Build();
            return container;
        }
    }
}
