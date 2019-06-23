namespace Stateful.ServiceFabric.Actors
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::ServiceFabric.Mocks;
    using Moq;
    using Stateful.ServiceFabric.Actors.Internals;
    using Xunit;

    public class ActorDictionaryStateTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();

            new ActorDictionaryState<string, TestState>(stateManager, keyMock.Object);
            Assert.Throws<ArgumentNullException>(() => new ActorDictionaryState<string, TestState>(null, keyMock.Object));
            Assert.Throws<ArgumentNullException>(() => new ActorDictionaryState<string, TestState>(stateManager, null));
        }

        [Fact]
        public async Task HasStateAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorDictionaryState<string, TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            Assert.False(await state.HasStateAsync(cts.Token));

            await stateManager.SetStateAsync("TestName", new HashManifest(), cts.Token);

            Assert.True(await state.HasStateAsync(cts.Token));
        }

        [Fact]
        public async Task CountAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorDictionaryState<string, TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            Assert.Equal(0, await state.CountAsync(cts.Token));

            await stateManager.SetStateAsync("TestName", new HashManifest(), cts.Token);
            Assert.Equal(0, await state.CountAsync(cts.Token));

            await stateManager.SetStateAsync("TestName", new HashManifest { Count = 1, Next = 1 }, cts.Token);
            Assert.Equal(1, await state.CountAsync(cts.Token));
        }

        [Fact]
        public async Task AddAsync_SingleValueTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorDictionaryState<string, TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            var key = "A";
            var keyHashCode = (long)"A".GetHashCode();

            await state.AddAsync("A", new TestState { Value = "A" }, cts.Token);

            var manifest = await stateManager.GetStateAsync<HashManifest>("TestName", cts.Token);
            Assert.NotNull(manifest);
            Assert.Equal(1, manifest.Count);
            Assert.Equal(1, manifest.Next);
            Assert.Equal(keyHashCode, manifest.Head);
            Assert.Equal(keyHashCode, manifest.Tail);
            var bucket = await stateManager.GetStateAsync<HashBucket>($"TestName:{keyHashCode:X}", cts.Token);
            Assert.NotNull(bucket);
            Assert.Equal(0, bucket.Head);
            Assert.Equal(0, bucket.Tail);
            Assert.Null(bucket.Previous);
            Assert.Null(bucket.Next);
            Assert.Equal(keyHashCode, bucket.HashCode);
            var node = await stateManager.GetStateAsync<HashKeyNode<string>>($"TestName:{keyHashCode:X}:0", cts.Token);
            Assert.NotNull(node);
            Assert.Null(node.Previous);
            Assert.Null(node.Next);
            var value = await stateManager.GetStateAsync<TestState>($"TestName:{keyHashCode:X}:0:v", cts.Token);
            Assert.NotNull(value);
            Assert.Equal("A", value.Value);
        }

        [Fact]
        public async Task AddAsync_SecondValueTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorDictionaryState<string, TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            var firstKey = "A";
            var firstKeyHash = (long)firstKey.GetHashCode();

            await stateManager.SetStateAsync("TestName", new HashManifest
            {
                Count = 1,
                Next = 1,
                Head = firstKeyHash,
                Tail = firstKeyHash
            }, cts.Token);
            await stateManager.SetStateAsync($"TestName:{firstKeyHash:X}", new HashBucket
            {
                Head = 0,
                Tail = 0,
                HashCode = firstKeyHash
            }, cts.Token);
            await stateManager.SetStateAsync($"TestName:{firstKeyHash:X}:0", new HashKeyNode<string>
            {
                Key = firstKey
            }, cts.Token);
            await stateManager.SetStateAsync($"TestName:{firstKeyHash:X}:0:v", new TestState
            {
                Value = firstKey
            }, cts.Token);

            var secondKey = "B";
            var secondKeyHash = (long)secondKey.GetHashCode();

            await state.AddAsync(secondKey, new TestState { Value = secondKey }, cts.Token);

            var manifest = await stateManager.GetStateAsync<HashManifest>("TestName", cts.Token);
            Assert.NotNull(manifest);
            Assert.Equal(2, manifest.Count);
            Assert.Equal(2, manifest.Next);
            Assert.Equal(firstKeyHash, manifest.Head);
            Assert.Equal(secondKeyHash, manifest.Tail);
            var bucket = await stateManager.GetStateAsync<HashBucket>($"TestName:{secondKeyHash:X}", cts.Token);
            Assert.NotNull(bucket);
            Assert.Equal(1, bucket.Head);
            Assert.Equal(1, bucket.Tail);
            Assert.Equal(firstKeyHash, bucket.Previous);
            Assert.Null(bucket.Next);
            Assert.Equal(secondKeyHash, bucket.HashCode);
            var node = await stateManager.GetStateAsync<HashKeyNode<string>>($"TestName:{secondKeyHash:X}:1", cts.Token);
            Assert.NotNull(node);
            Assert.Null(node.Previous);
            Assert.Null(node.Next);
            var value = await stateManager.GetStateAsync<TestState>($"TestName:{secondKeyHash:X}:1:v", cts.Token);
            Assert.NotNull(value);
            Assert.Equal(secondKey, value.Value);
        }
    }
}