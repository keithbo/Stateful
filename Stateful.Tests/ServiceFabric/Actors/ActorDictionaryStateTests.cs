namespace Stateful.ServiceFabric.Actors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::ServiceFabric.Mocks;
    using Microsoft.ServiceFabric.Actors.Runtime;
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

            var hashCodes = await SetupValues(stateManager, "TestName", ("A", new TestState { Value = "A" }));
            var firstKeyHash = hashCodes[0];

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

        [Fact]
        public async Task DeleteAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorDictionaryState<string, TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            var hashCodes = await SetupValues(stateManager, "TestName",
                ("A", new TestState { Value = "A1" }),
                ("A", new TestState { Value = "A2" }),
                ("B", new TestState { Value = "B1" }));
            var firstKeyHash = hashCodes[0];
            var secondKeyHash = hashCodes[1];

            await state.DeleteStateAsync(cts.Token);

            Assert.False(await stateManager.ContainsStateAsync("TestName", cts.Token));
            Assert.False(await stateManager.ContainsStateAsync($"TestName:{firstKeyHash:X}", cts.Token));
            Assert.False(await stateManager.ContainsStateAsync($"TestName:{firstKeyHash:X}:0", cts.Token));
            Assert.False(await stateManager.ContainsStateAsync($"TestName:{firstKeyHash:X}:0:v", cts.Token));
            Assert.False(await stateManager.ContainsStateAsync($"TestName:{firstKeyHash:X}:1", cts.Token));
            Assert.False(await stateManager.ContainsStateAsync($"TestName:{firstKeyHash:X}:1:v", cts.Token));
            Assert.False(await stateManager.ContainsStateAsync($"TestName:{secondKeyHash:X}", cts.Token));
            Assert.False(await stateManager.ContainsStateAsync($"TestName:{secondKeyHash:X}:2", cts.Token));
            Assert.False(await stateManager.ContainsStateAsync($"TestName:{secondKeyHash:X}:2:v", cts.Token));
        }

        [Fact]
        public async Task ContainsAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorDictionaryState<string, TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            await SetupValues(stateManager, "TestName",
                ("A", new TestState { Value = "A1" }),
                ("A", new TestState { Value = "A2" }),
                ("B", new TestState { Value = "B1" }));

            Assert.True(await state.ContainsAsync(kvp => kvp.Key == "A" && kvp.Value.Value == "A1", cts.Token));
            Assert.True(await state.ContainsAsync(kvp => kvp.Key == "A" && kvp.Value.Value == "A2", cts.Token));
            Assert.True(await state.ContainsAsync(kvp => kvp.Key == "B" && kvp.Value.Value == "B1", cts.Token));
        }

        [Fact]
        public async Task ContainsKeyAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorDictionaryState<string, TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            await SetupValues(stateManager, "TestName",
                ("A", new TestState { Value = "A1" }),
                ("A", new TestState { Value = "A2" }),
                ("B", new TestState { Value = "B1" }));

            Assert.True(await state.ContainsKeyAsync("A", cts.Token));
            Assert.True(await state.ContainsKeyAsync("B", cts.Token));
            Assert.False(await state.ContainsKeyAsync("C", cts.Token));
        }

        [Fact]
        public async Task TryGetAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new ActorDictionaryState<string, TestState>(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            await SetupValues(stateManager, "TestName", 
                ("A", new TestState { Value = "A1" }),
                ("A", new TestState { Value = "A2" }),
                ("B", new TestState { Value = "B1" }));

            var value0 = await state.TryGetValueAsync("A", cts.Token);
            Assert.True(value0.HasValue);
            Assert.Equal("A1", value0.Value.Value);
            var value1 = await state.TryGetValueAsync("B", cts.Token);
            Assert.True(value1.HasValue);
            Assert.Equal("B1", value1.Value.Value);
            var value2 = await state.TryGetValueAsync("C", cts.Token);
            Assert.False(value2.HasValue);
        }

        private static async Task<long[]> SetupValues<TKey, TValue>(IActorStateManager stateManager, string prefix, params (TKey Key, TValue Value)[] pairs)
        {
            var hashCodes = new List<long>();

            var buckets = new List<(string Key, HashBucket Bucket)>();

            var manifest = new HashManifest();

            var i = 0;
            long previousBucketKey = 0;
            HashBucket previousBucket = null;
            foreach (var grouping in pairs
                .Select(t => (Key: t.Key, HashCode: (long) t.Key.GetHashCode(), Value: t.Value))
                .GroupBy(tuple => tuple.HashCode))
            {
                hashCodes.Add(grouping.Key);
                if (!manifest.Head.HasValue)
                {
                    manifest.Head = grouping.Key;
                }

                manifest.Tail = grouping.Key;

                var bucket = new HashBucket
                {
                    HashCode = grouping.Key,
                    Head = i
                };
                if (previousBucket != null)
                {
                    previousBucket.Next = grouping.Key;
                    bucket.Previous = previousBucketKey;
                }

                previousBucketKey = grouping.Key;
                previousBucket = bucket;

                var previousI = i;
                HashKeyNode<TKey> previousKeyNode = null;
                var breakouts = grouping.Select(tuple =>
                {
                    var keyNode = new HashKeyNode<TKey>
                    {
                        Key = tuple.Key
                    };
                    if (previousKeyNode != null)
                    {
                        previousKeyNode.Next = i;
                        keyNode.Previous = previousI;
                    }

                    bucket.Tail = i;
                    previousI = i;
                    previousKeyNode = keyNode;
                    var keyKey = $"{prefix}:{grouping.Key:X}:{i}";
                    var valueKey = $"{prefix}:{grouping.Key:X}:{i}:v";

                    i++;
                    return (KeyKey: keyKey, KeyNode: keyNode, ValueKey: valueKey, Value: tuple.Value);
                }).ToList();

                foreach (var (keyKey, keyNode, valueKey, value) in breakouts)
                {
                    await stateManager.SetStateAsync(keyKey, keyNode);
                    await stateManager.SetStateAsync(valueKey, value);
                }

                buckets.Add(($"{prefix}:{grouping.Key:X}", bucket));
            }

            foreach (var tuple in buckets)
            {
                await stateManager.SetStateAsync(tuple.Key, tuple.Bucket);
            }

            manifest.Count = i;
            manifest.Next = i;

            await stateManager.SetStateAsync(prefix, manifest);

            return hashCodes.ToArray();
        }
    }
}