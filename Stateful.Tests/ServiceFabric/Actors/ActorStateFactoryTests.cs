namespace Stateful.ServiceFabric.Actors
{
    using System.Collections.Generic;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Moq;
    using Stateful.ServiceFabric.Actors.Configuration;
    using Xunit;

    public class ActorStateFactoryTests
    {
        [Fact]
        public void CreateTransactionTest()
        {
            var stateManagerMock = new Mock<IActorStateManager>();

            var factory = new ActorStateFactory(stateManagerMock.Object, new List<IActorStateActivator>());

            var unit = factory.CreateTransaction();

            Assert.IsType<ActorStateUnit>(unit);
        }
    }
}