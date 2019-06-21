namespace Stateful.ServiceFabric.Actors
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using global::ServiceFabric.Mocks;
    using Moq;
    using Stateful.ServiceFabric.Actors.Internals;
    using Xunit;

    public class ActorListStateTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();

            new ActorListState<TestState>(stateManager, keyMock.Object);
            Assert.Throws<ArgumentNullException>(() => new ActorListState<TestState>(null, keyMock.Object));
            Assert.Throws<ArgumentNullException>(() => new ActorListState<TestState>(stateManager, null));
        }

        [Fact]
        public async Task TryGetAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorListState<TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            var resultList = await state.TryGetAsync(cts.Token);
            Assert.False(resultList.HasValue, "State was expected to be missing");

            var resultSingle = await state.TryGetAsync(0, cts.Token);
            Assert.False(resultSingle.HasValue, "State was expected to be missing");

            var values = new List<TestState>
            {
                new TestState
                {
                    Value = "A"
                },
                new TestState
                {
                    Value = "B"
                }
            };

            await stateManager.SetStateAsync("TestName", new LinkedManifest
            {
                Count = 2,
                First = 0,
                Last = 1,
                Next = 2
            }, cts.Token);
            await stateManager.SetStateAsync("TestName:0", new LinkedNode<TestState>
            {
                Next = 1,
                Value = values[0]
            }, cts.Token);
            await stateManager.AddStateAsync("TestName:1", new LinkedNode<TestState>
            {
                Previous = 0,
                Value = values[1]
            }, cts.Token);

            resultList = await state.TryGetAsync(cts.Token);
            Assert.True(resultList.HasValue, "State was expected to exist");
            Assert.Equal(values, resultList.Value);

            resultSingle = await state.TryGetAsync(0, cts.Token);
            Assert.True(resultSingle.HasValue, "State was expected to exist");
            Assert.Equal(values[0], resultSingle.Value);

            resultSingle = await state.TryGetAsync(1, cts.Token);
            Assert.True(resultSingle.HasValue, "State was expected to exist");
            Assert.Equal(values[1], resultSingle.Value);
        }

        [Fact]
        public async Task TryFindAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorListState<TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            var result = await state.TryFindAsync(s => true, cts.Token);
            Assert.False(result.HasValue, "State was expected to be missing");

            var values = new List<TestState>
            {
                new TestState
                {
                    Value = "A"
                },
                new TestState
                {
                    Value = "B"
                }
            };

            await stateManager.SetStateAsync("TestName", new LinkedManifest
            {
                Count = 2,
                First = 0,
                Last = 1,
                Next = 2
            }, cts.Token);
            await stateManager.SetStateAsync("TestName:0", new LinkedNode<TestState>
            {
                Next = 1,
                Value = values[0]
            }, cts.Token);
            await stateManager.AddStateAsync("TestName:1", new LinkedNode<TestState>
            {
                Previous = 0,
                Value = values[1]
            }, cts.Token);

            result = await state.TryFindAsync(s => true, cts.Token);
            Assert.True(result.HasValue, "State was expected to exist");
            Assert.Equal(values[0], result.Value);

            var i = 0;
            result = await state.TryFindAsync(s => i++ == 1, cts.Token);
            Assert.True(result.HasValue, "State was expected to exist");
            Assert.Equal(values[1], result.Value);

            i = 0;
            result = await state.TryFindAsync(s => i++ == 2, cts.Token);
            Assert.False(result.HasValue, "State was expected to be missing");
        }

        [Fact]
        public async Task RemoveAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorListState<TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            var values = new List<TestState>
            {
                new TestState
                {
                    Value = "A"
                },
                new TestState
                {
                    Value = "B"
                }
            };

            await stateManager.SetStateAsync("TestName", new LinkedManifest
            {
                Count = 2,
                First = 0,
                Last = 1,
                Next = 2
            }, cts.Token);
            await stateManager.SetStateAsync("TestName:0", new LinkedNode<TestState>
            {
                Next = 1,
                Value = values[0]
            }, cts.Token);
            await stateManager.AddStateAsync("TestName:1", new LinkedNode<TestState>
            {
                Previous = 0,
                Value = values[1]
            }, cts.Token);

            var i = 0;
            await state.RemoveAsync(s => i++ == 1, cts.Token);

            Assert.False(await stateManager.ContainsStateAsync("TestName:1", cts.Token), "State was expected to be deleted");
            var node = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:0", cts.Token);
            Assert.True(node.HasValue, "State was expected to exist");
            Assert.NotNull(node.Value.Value);
            Assert.Null(node.Value.Previous);
            Assert.Null(node.Value.Next);

            var manifest = await stateManager.TryGetStateAsync<LinkedManifest>("TestName", cts.Token);
            Assert.True(manifest.HasValue, "Manifest was expected to exist");
            Assert.Equal(1, manifest.Value.Count);
            Assert.Equal(2, manifest.Value.Next);
            Assert.Equal(0, manifest.Value.First);
            Assert.Equal(0, manifest.Value.Last);

            i = 0;
            await state.RemoveAsync(s => i++ == 0, cts.Token);

            Assert.False(await stateManager.ContainsStateAsync("TestName:0", cts.Token), "State was expected to be deleted");

            manifest = await stateManager.TryGetStateAsync<LinkedManifest>("TestName", cts.Token);
            Assert.True(manifest.HasValue, "Manifest was expected to exist");
            Assert.Equal(0, manifest.Value.Count);
            Assert.Equal(2, manifest.Value.Next);
            Assert.Null(manifest.Value.First);
            Assert.Null(manifest.Value.Last);
        }
    }
}