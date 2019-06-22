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

    public class ActorQueueStateTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();

            new ActorQueueState<TestState>(stateManager, keyMock.Object);
            Assert.Throws<ArgumentNullException>(() => new ActorQueueState<TestState>(null, keyMock.Object));
            Assert.Throws<ArgumentNullException>(() => new ActorQueueState<TestState>(stateManager, null));
        }

        [Fact]
        public async Task EnqueueAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorQueueState<TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            await state.EnqueueAsync(new TestState { Value = "A" }, cts.Token);
            await state.EnqueueAsync(new TestState { Value = "B" }, cts.Token);
            await state.EnqueueAsync(new TestState { Value = "C" }, cts.Token);

            var manifest = await stateManager.GetStateAsync<LinkedManifest>("TestName", cts.Token);
            Assert.Equal(3, manifest.Count);
            Assert.Equal(0, manifest.First);
            Assert.Equal(2, manifest.Last);
            Assert.Equal(3, manifest.Next);
            var value0 = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:0", cts.Token);
            Assert.True(value0.HasValue);
            Assert.Null(value0.Value.Previous);
            Assert.Equal(1, value0.Value.Next);
            Assert.NotNull(value0.Value.Value);
            Assert.Equal("A", value0.Value.Value.Value);
            var value1 = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:1", cts.Token);
            Assert.True(value1.HasValue);
            Assert.Equal(0, value1.Value.Previous);
            Assert.Equal(2, value1.Value.Next);
            Assert.NotNull(value1.Value.Value);
            Assert.Equal("B", value1.Value.Value.Value);
            var value2 = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:2", cts.Token);
            Assert.True(value2.HasValue);
            Assert.Equal(1, value2.Value.Previous);
            Assert.Null(value2.Value.Next);
            Assert.NotNull(value2.Value.Value);
            Assert.Equal("C", value2.Value.Value.Value);
        }

        [Fact]
        public async Task TryPeekAsync_When_NoStateTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorQueueState<TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            var result = await state.TryPeekAsync(cts.Token);
            Assert.False(result.HasValue);
        }

        [Fact]
        public async Task TryPeekAsync_When_NoValuesTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorQueueState<TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            await stateManager.SetStateAsync("TestName", new LinkedManifest
            {
                Count = 0,
                First = null,
                Last = null,
                Next = 0
            }, cts.Token);

            var result = await state.TryPeekAsync(cts.Token);
            Assert.False(result.HasValue);
        }

        [Fact]
        public async Task TryPeekAsync_SingleValueTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorQueueState<TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            var values = new List<TestState>
            {
                new TestState
                {
                    Value = "A"
                },
            };

            await stateManager.SetStateAsync("TestName", new LinkedManifest
            {
                Count = 1,
                First = 0,
                Last = 0,
                Next = 1
            }, cts.Token);
            await stateManager.SetStateAsync("TestName:0", new LinkedNode<TestState>
            {
                Value = values[0]
            }, cts.Token);

            var result = await state.TryPeekAsync(cts.Token);
            Assert.True(result.HasValue);
            Assert.Equal("A", result.Value.Value);
        }

        [Fact]
        public async Task TryPeekAsync_MultipleValuesTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorQueueState<TestState>(stateManager, keyMock.Object);

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

            var result = await state.TryPeekAsync(cts.Token);
            Assert.True(result.HasValue);
            Assert.Equal("A", result.Value.Value);
        }

        [Fact]
        public async Task TryDequeueAsync_When_NoStateTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorQueueState<TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            var result = await state.TryDequeueAsync(cts.Token);
            Assert.False(result.HasValue);
        }

        [Fact]
        public async Task TryDequeueAsync_When_NoValuesTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorQueueState<TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            await stateManager.SetStateAsync("TestName", new LinkedManifest
            {
                Count = 0,
                First = null,
                Last = null,
                Next = 0
            }, cts.Token);

            var result = await state.TryDequeueAsync(cts.Token);
            Assert.False(result.HasValue);
        }

        [Fact]
        public async Task TryDequeueAsync_SingleValueTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorQueueState<TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            var values = new List<TestState>
            {
                new TestState
                {
                    Value = "A"
                },
            };

            await stateManager.SetStateAsync("TestName", new LinkedManifest
            {
                Count = 1,
                First = 0,
                Last = 0,
                Next = 1
            }, cts.Token);
            await stateManager.SetStateAsync("TestName:0", new LinkedNode<TestState>
            {
                Value = values[0]
            }, cts.Token);

            var result = await state.TryDequeueAsync(cts.Token);
            Assert.True(result.HasValue);
            Assert.Equal("A", result.Value.Value);
            Assert.False(await stateManager.ContainsStateAsync("TestName:0", cts.Token));

            var manifest = await stateManager.GetStateAsync<LinkedManifest>("TestName", cts.Token);
            Assert.NotNull(manifest);
            Assert.Equal(0, manifest.Count);
            Assert.Null(manifest.First);
            Assert.Null(manifest.Last);
            Assert.Equal(0, manifest.Next);
        }

        [Fact]
        public async Task TryDequeueAsync_MultipleValuesTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorQueueState<TestState>(stateManager, keyMock.Object);

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
            await stateManager.SetStateAsync("TestName:1", new LinkedNode<TestState>
            {
                Previous = 0,
                Value = values[1]
            }, cts.Token);

            var result = await state.TryDequeueAsync(cts.Token);
            Assert.True(result.HasValue);
            Assert.Equal("A", result.Value.Value);
            Assert.False(await stateManager.ContainsStateAsync("TestName:0", cts.Token));
            Assert.True(await stateManager.ContainsStateAsync("TestName:1", cts.Token));

            var manifest = await stateManager.GetStateAsync<LinkedManifest>("TestName", cts.Token);
            Assert.NotNull(manifest);
            Assert.Equal(1, manifest.Count);
            Assert.Equal(1, manifest.First);
            Assert.Equal(1, manifest.Last);
            Assert.Equal(2, manifest.Next);

            result = await state.TryDequeueAsync(cts.Token);
            Assert.True(result.HasValue);
            Assert.Equal("B", result.Value.Value);
            Assert.False(await stateManager.ContainsStateAsync("TestName:0", cts.Token));
            Assert.False(await stateManager.ContainsStateAsync("TestName:1", cts.Token));

            manifest = await stateManager.GetStateAsync<LinkedManifest>("TestName", cts.Token);
            Assert.NotNull(manifest);
            Assert.Equal(0, manifest.Count);
            Assert.Null(manifest.First);
            Assert.Null(manifest.Last);
            Assert.Equal(0, manifest.Next);
        }

        [Fact]
        public async Task RoundTripTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorQueueState<TestState>(stateManager, keyMock.Object);

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
                },
                new TestState
                {
                    Value = "C"
                }
            };

            foreach (var value in values)
            {
                await state.EnqueueAsync(value, cts.Token);
            }

            foreach (var value in values)
            {
                var result = await state.TryDequeueAsync(cts.Token);
                Assert.True(result.HasValue);
                Assert.Equal(value, result.Value);
            }
        }
    }
}