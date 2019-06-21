namespace Stateful.ServiceFabric.Actors
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::ServiceFabric.Mocks;
    using Moq;
    using Xunit;

    public class ActorObjectStateTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();

            new ActorObjectState<TestState>(stateManager, keyMock.Object);
            Assert.Throws<ArgumentNullException>(() => new ActorObjectState<TestState>(null, keyMock.Object));
            Assert.Throws<ArgumentNullException>(() => new ActorObjectState<TestState>(stateManager, null));
        }

        [Fact]
        public async Task HasStateAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorObjectState<TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            Assert.False(await state.HasStateAsync(cts.Token), "State was expected to not exist");

            await stateManager.AddStateAsync("TestName", new TestState(), cts.Token);

            Assert.True(await state.HasStateAsync(cts.Token), "State was expected to exist");
        }

        [Fact]
        public async Task DeleteStateAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorObjectState<TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            await stateManager.AddStateAsync("TestName", new TestState(), cts.Token);

            await state.DeleteStateAsync(cts.Token);

            Assert.False(await stateManager.ContainsStateAsync("TestName", cts.Token), "State was expected to be deleted");
        }

        [Fact]
        public async Task TryGetAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorObjectState<TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            var result = await state.TryGetAsync(cts.Token);
            Assert.False(result.HasValue, "State was expected to be missing");

            await stateManager.AddStateAsync("TestName", new TestState(), cts.Token);

            result = await state.TryGetAsync(cts.Token);
            Assert.True(result.HasValue, "State was expected to be found");
        }

        [Fact]
        public async Task SetAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorObjectState<TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            var result = await state.SetAsync(new TestState(), cts.Token);
            Assert.NotNull(result);

            var check = await stateManager.TryGetStateAsync<TestState>("TestName", cts.Token);
            Assert.True(check.HasValue);
            Assert.Same(result, check.Value);
        }
    }
}