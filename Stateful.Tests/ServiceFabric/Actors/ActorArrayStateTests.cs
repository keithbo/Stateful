namespace Stateful.ServiceFabric.Actors
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::ServiceFabric.Mocks;
    using Moq;
    using Xunit;

    public class ActorArrayStateTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();

            new ActorArrayState<TestState>(stateManager, keyMock.Object, 1);
            Assert.Throws<ArgumentNullException>(() => new ActorArrayState<TestState>(null, keyMock.Object, 1));
            Assert.Throws<ArgumentNullException>(() => new ActorArrayState<TestState>(stateManager, null, 1));
            Assert.Throws<ArgumentException>(() => new ActorArrayState<TestState>(stateManager, keyMock.Object, -1));
            Assert.Throws<ArgumentException>(() => new ActorArrayState<TestState>(stateManager, keyMock.Object, 0));
        }

        [Fact]
        public async Task HasStateAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorArrayState<TestState>(stateManager, keyMock.Object, 1);

            var cts = new CancellationTokenSource(1000);

            Assert.False(await state.HasStateAsync(cts.Token));

            await stateManager.SetStateAsync("TestName", 1L, cts.Token);

            Assert.True(await state.HasStateAsync(cts.Token));
        }

        [Fact]
        public async Task DeleteStateAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorArrayState<TestState>(stateManager, keyMock.Object, 1);

            var cts = new CancellationTokenSource(1000);
            await stateManager.SetStateAsync("TestName", 1L, cts.Token);
            await stateManager.SetStateAsync("TestName:0", new TestState(), cts.Token);

            await state.DeleteStateAsync(cts.Token);

            Assert.False(await stateManager.ContainsStateAsync("TestName", cts.Token));
            Assert.False(await stateManager.ContainsStateAsync("TestName:0", cts.Token));
        }

        [Fact]
        public async Task CountAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorArrayState<TestState>(stateManager, keyMock.Object, 1);

            var cts = new CancellationTokenSource(1000);

            Assert.Equal(1, await state.CountAsync(cts.Token));
            Assert.Equal(1, await stateManager.GetStateAsync<long>("TestName", cts.Token));
        }

        [Fact]
        public async Task ContainsAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorArrayState<TestState>(stateManager, keyMock.Object, 1);

            var cts = new CancellationTokenSource(1000);

            var i = 0;
            Assert.False(await state.ContainsAsync(s => i++ == 1, cts.Token));

            await stateManager.SetStateAsync("TestName:0", new TestState { Value = "A" }, cts.Token);

            Assert.True(await state.ContainsAsync(s => s.Value == "A", cts.Token));
            i = 0;
            Assert.False(await state.ContainsAsync(s => i++ == 2, cts.Token));
        }

        [Fact]
        public async Task GetAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorArrayState<TestState>(stateManager, keyMock.Object, 1);

            var cts = new CancellationTokenSource(1000);

            await Assert.ThrowsAsync<IndexOutOfRangeException>(() => state.GetAsync(-1, cts.Token));
            await Assert.ThrowsAsync<IndexOutOfRangeException>(() => state.GetAsync(1, cts.Token));

            Assert.Null(await state.GetAsync(0, cts.Token));

            await stateManager.SetStateAsync("TestName:0", new TestState(), cts.Token);
            Assert.NotNull(await state.GetAsync(0, cts.Token));
        }

        [Fact]
        public async Task SetAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorArrayState<TestState>(stateManager, keyMock.Object, 1);

            var cts = new CancellationTokenSource(1000);

            await Assert.ThrowsAsync<IndexOutOfRangeException>(() => state.SetAsync(-1, new TestState(), cts.Token));
            await Assert.ThrowsAsync<IndexOutOfRangeException>(() => state.SetAsync(1, new TestState(), cts.Token));

            Assert.False(await stateManager.ContainsStateAsync("TestName:-1", cts.Token));
            Assert.False(await stateManager.ContainsStateAsync("TestName:1", cts.Token));

            var value = new TestState { Value = "A" };
            await state.SetAsync(0, value, cts.Token);

            Assert.Equal(value, await stateManager.GetStateAsync<TestState>("TestName:0", cts.Token));
        }
    }
}