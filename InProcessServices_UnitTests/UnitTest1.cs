using System.Threading;
using Autofac;
using FluentAssertions;
using InProcessServices.Autofac;
using InProcessServices.Serialization.Json.NET;
using InProcessServices_Example;
using Xunit;

namespace InProcessServices_UnitTests
{
    public class UnitTest1
    {
        private string initialStateValue = "Initial Value";
        
        [Fact]
        public void UsingAutoFac_UsingJsonDotNet_ClientCannotChangeMemoryInService()
        {
            using (var container = SetupTestContainer())
            {
                var fetchedService = ResolveService<IMemoryIsolationTest>(container);
                var state = new StateClass { Value = initialStateValue };
                fetchedService.SetStateValue(state);

                var clientSetValue = "Client set value, should not be shared with service";
                state.Value = clientSetValue;

                var fetchedState = fetchedService.GetStateValue();

                fetchedService.Should().NotBeOfType<MemoryIsolationTestServiceClass>();
                fetchedService.Should().BeAssignableTo<IMemoryIsolationTest>();

                fetchedState.Should().BeOfType<StateClass>();
                var fetchedStateAsStateClass = fetchedState as StateClass;
                fetchedStateAsStateClass.Value.Should().Be(initialStateValue);
            }
        }

        [Fact]
        public void UsingAutoFac_UsingJsonDotNet_ServiceCannotChangeMemoryInClient()
        {
            using (var container = SetupTestContainer())
            {
                var fetchedService = ResolveService<IMemoryIsolationTest>(container);

                var dependencyService = CreateComplexDependency();
                fetchedService.SetDependency(dependencyService);

                var fetchedDependency = fetchedService.GetDependency(); //get dependency back from service. This must be a copy of what the service has
                fetchedService.UpdateDependency(); //when service updates the dependency this must not affect the copy the client has

                fetchedService.Should().NotBeOfType<MemoryIsolationTestServiceClass>();
                fetchedService.Should().BeAssignableTo<IMemoryIsolationTest>();

                fetchedDependency.GetStateValue().Should().BeOfType<StateClass>();
            }
        }

        [Fact]
        public void UsingAutofac_UsingJsonDotNet_ServiceRunsInASeparateThread()
        {
            using (var container = SetupTestContainer())
            {
                var fetchedService = ResolveService<ISeparateThreadTest>(container);

                var currentThreadId = Thread.CurrentThread.ManagedThreadId;
                fetchedService.GetThreadId().Should().NotBe(currentThreadId);
            }
        }

        private MemoryIsolationTestServiceClass CreateComplexDependency()
        {
            var dependencyService = new MemoryIsolationTestServiceClass();
            var state = new StateClass {Value = initialStateValue};
            dependencyService.SetStateValue(state);
            return dependencyService;
        }

        private TService ResolveService<TService>(IContainer container)
        {
            return container.Resolve<TService>();
        }

        private static IContainer SetupTestContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInProcService<MemoryIsolationTestServiceClass>();
            builder.RegisterInProcService<SeparateThreadTestServiceClass>();
            builder.RegisterType<JsonObjectCopier>()
                .AsImplementedInterfaces();

            var container = builder.Build();
            return container;
        }
    }

    public class StateClass
    {
        public string Value { get; set; }
    }
}
