namespace Stateful.ServiceFabric.Actors
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Moq;
    using Xunit;

    public class ActorStateUnitTests
    {
        [Fact]
        public async Task DisposeTest()
        {
            var stateManagerMock = new Mock<IActorStateManager>();
            var factoryMock = new Mock<Func<IActorStateManager, IStateKey, IState>>();

            var unit = new ActorStateUnit(stateManagerMock.Object, factoryMock.Object);

            unit.Dispose();

            Assert.Throws<ObjectDisposedException>(() => unit.Get<IState>(new StateKey("Test")));
            await Assert.ThrowsAsync<ObjectDisposedException>(() => unit.CommitAsync());
            Assert.Throws<ObjectDisposedException>(() => unit.Abort());
        }

        [Fact]
        public void GetTest()
        {
            var expectedKey = new StateKey("Test");
            var stateMock = new Mock<IListState<string>>();

            var stateManagerMock = new Mock<IActorStateManager>();
            var factoryMock = new Mock<Func<IActorStateManager, IStateKey, IState>>();
            factoryMock.Setup(x => x(stateManagerMock.Object, expectedKey))
                .Returns<IActorStateManager, IStateKey>((sm, sk) => stateMock.Object)
                .Verifiable();

            var unit = new ActorStateUnit(stateManagerMock.Object, factoryMock.Object);

            var resultState = unit.Get<IListState<string>>(new StateKey("Test"));
            Assert.Same(stateMock.Object, resultState);

            factoryMock.Verify();
        }

        [Fact]
        public async Task CommitTest()
        {
            var stateManagerMock = new Mock<IActorStateManager>();
            var factoryMock = new Mock<Func<IActorStateManager, IStateKey, IState>>();

            var unit = new ActorStateUnit(stateManagerMock.Object, factoryMock.Object);

            await unit.CommitAsync();

            Assert.Throws<ObjectDisposedException>(() => unit.Get<IListState<string>>(new StateKey("Test")));
        }
    }
}
