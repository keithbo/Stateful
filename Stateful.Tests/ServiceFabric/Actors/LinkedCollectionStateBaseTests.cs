namespace Stateful.ServiceFabric.Actors
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using global::ServiceFabric.Mocks;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Moq;
    using Stateful.ServiceFabric.Actors.Internals;
    using Xunit;

    public class LinkedCollectionStateBaseTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();

            new TestLinkedCollectionState(stateManager, keyMock.Object);
            Assert.Throws<ArgumentNullException>(() => new TestLinkedCollectionState(null, keyMock.Object));
            Assert.Throws<ArgumentNullException>(() => new TestLinkedCollectionState(stateManager, null));
        }

        [Fact]
        public async Task HasStateAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new TestLinkedCollectionState(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            Assert.False(await state.HasStateAsync(cts.Token), "State was expected to not exist");

            await stateManager.AddStateAsync("TestName", new LinkedManifest(), cts.Token);

            Assert.True(await state.HasStateAsync(cts.Token), "State was expected to exist");
        }

        [Fact]
        public async Task DeleteStateAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new TestLinkedCollectionState(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            await stateManager.AddStateAsync("TestName", new LinkedManifest
            {
                Count = 1,
                First = 0,
                Last = 0,
                Next = 1
            }, cts.Token);
            await stateManager.AddStateAsync("TestName:0", new LinkedNode<TestState>
            {
                Value = new TestState()
            }, cts.Token);

            await state.DeleteStateAsync(cts.Token);

            Assert.False(await stateManager.ContainsStateAsync("TestName", cts.Token), "State was expected to be deleted");
            Assert.False(await stateManager.ContainsStateAsync("TestName:0", cts.Token), "State was expected to be deleted");
        }

        [Fact]
        public async Task CountAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var state = new TestLinkedCollectionState(stateManager, keyMock.Object);

            var cts = new CancellationTokenSource(1000);

            Assert.Equal(0, await state.CountAsync(cts.Token));

            await stateManager.AddStateAsync("TestName", new LinkedManifest
            {
                Count = 0,
                Next = 0
            }, cts.Token);
            Assert.Equal(0, await state.CountAsync(cts.Token));

            await stateManager.SetStateAsync("TestName", new LinkedManifest
            {
                Count = 1,
                Next = 1
            }, cts.Token);
            Assert.Equal(1, await state.CountAsync(cts.Token));
        }

        [Fact]
        public async Task ContainsAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var cts = new CancellationTokenSource(1000);

            var state = new TestLinkedCollectionState(stateManager, keyMock.Object);

            var searchMock = new Mock<Predicate<TestState>>();
            searchMock.Setup(x => x(It.IsAny<TestState>()))
                .Returns<TestState>(s => s.Value == "A");

            Assert.False(await state.ContainsAsync(searchMock.Object, cts.Token));

            await stateManager.AddStateAsync("TestName", new LinkedManifest
            {
                Count = 0,
                Next = 0
            }, cts.Token);
            Assert.False(await state.ContainsAsync(searchMock.Object, cts.Token));

            await stateManager.SetStateAsync("TestName", new LinkedManifest
            {
                Count = 1,
                First = 0,
                Last = 0,
                Next = 1
            }, cts.Token);
            await stateManager.AddStateAsync("TestName:0", new LinkedNode<TestState>
            {
                Value = new TestState
                {
                    Value = "B"
                }
            }, cts.Token);
            Assert.False(await state.ContainsAsync(searchMock.Object, cts.Token));

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
                Value = new TestState
                {
                    Value = "B"
                }
            }, cts.Token);
            await stateManager.AddStateAsync("TestName:1", new LinkedNode<TestState>
            {
                Previous = 0,
                Value = new TestState
                {
                    Value = "A"
                }
            }, cts.Token);
            Assert.True(await state.ContainsAsync(searchMock.Object, cts.Token));
        }

        [Fact]
        public async Task InsertFirstAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var cts = new CancellationTokenSource(1000);

            await stateManager.SetStateAsync("TestName", new LinkedManifest
            {
                Count = 1,
                First = 0,
                Last = 0,
                Next = 1
            }, cts.Token);
            await stateManager.AddStateAsync("TestName:0", new LinkedNode<TestState>
            {
                Value = new TestState
                {
                    Value = "A"
                }
            }, cts.Token);

            var state = new TestLinkedCollectionState(stateManager, keyMock.Object);

            var values = new List<TestState>
            {
                new TestState
                {
                    Value = "I1"
                },
                new TestState
                {
                    Value = "I2"
                },
            };

            await state.TestInsertFirstAsync(values, cts.Token);

            var manifest = await stateManager.GetStateAsync<LinkedManifest>("TestName", cts.Token);
            Assert.Equal(3, manifest.Count);
            Assert.Equal(1, manifest.First);
            Assert.Equal(0, manifest.Last);
            Assert.Equal(3, manifest.Next);
            var value1 = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:1", cts.Token);
            Assert.True(value1.HasValue);
            Assert.Null(value1.Value.Previous);
            Assert.Equal(2, value1.Value.Next);
            Assert.NotNull(value1.Value.Value);
            Assert.Equal("I1", value1.Value.Value.Value);
            var value2 = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:2", cts.Token);
            Assert.True(value2.HasValue);
            Assert.Equal(1, value2.Value.Previous);
            Assert.Equal(0, value2.Value.Next);
            Assert.NotNull(value2.Value.Value);
            Assert.Equal("I2", value2.Value.Value.Value);
            var value0 = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:0", cts.Token);
            Assert.True(value0.HasValue);
            Assert.Equal(2, value0.Value.Previous);
            Assert.Null(value0.Value.Next);
            Assert.NotNull(value0.Value.Value);
            Assert.Equal("A", value0.Value.Value.Value);
        }

        [Fact]
        public async Task InsertLastAsyncTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var cts = new CancellationTokenSource(1000);

            await stateManager.SetStateAsync("TestName", new LinkedManifest
            {
                Count = 1,
                First = 0,
                Last = 0,
                Next = 1
            }, cts.Token);
            await stateManager.AddStateAsync("TestName:0", new LinkedNode<TestState>
            {
                Value = new TestState
                {
                    Value = "A"
                }
            }, cts.Token);

            var state = new TestLinkedCollectionState(stateManager, keyMock.Object);

            var values = new List<TestState>
            {
                new TestState
                {
                    Value = "I1"
                },
                new TestState
                {
                    Value = "I2"
                },
            };

            await state.TestInsertLastAsync(values, cts.Token);

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
            Assert.Equal("I1", value1.Value.Value.Value);
            var value2 = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:2", cts.Token);
            Assert.True(value2.HasValue);
            Assert.Equal(1, value2.Value.Previous);
            Assert.Null(value2.Value.Next);
            Assert.NotNull(value2.Value.Value);
            Assert.Equal("I2", value2.Value.Value.Value);
        }

        [Fact]
        public async Task InsertAtAsync_StartTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var cts = new CancellationTokenSource(1000);

            await stateManager.SetStateAsync("TestName", new LinkedManifest
            {
                Count = 2,
                First = 0,
                Last = 1,
                Next = 2
            }, cts.Token);
            await stateManager.AddStateAsync("TestName:0", new LinkedNode<TestState>
            {
                Next = 1,
                Value = new TestState
                {
                    Value = "A"
                }
            }, cts.Token);
            await stateManager.AddStateAsync("TestName:1", new LinkedNode<TestState>
            {
                Previous = 0,
                Value = new TestState
                {
                    Value = "B"
                }
            }, cts.Token);

            var state = new TestLinkedCollectionState(stateManager, keyMock.Object);

            var values = new List<TestState>
            {
                new TestState
                {
                    Value = "I1"
                },
                new TestState
                {
                    Value = "I2"
                },
            };

            await state.TestInsertAtAsync(0, values, cts.Token);

            var manifest = await stateManager.GetStateAsync<LinkedManifest>("TestName", cts.Token);
            Assert.Equal(4, manifest.Count);
            Assert.Equal(2, manifest.First);
            Assert.Equal(1, manifest.Last);
            Assert.Equal(4, manifest.Next);
            var value0 = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:2", cts.Token);
            Assert.True(value0.HasValue);
            Assert.Null(value0.Value.Previous);
            Assert.Equal(3, value0.Value.Next);
            Assert.NotNull(value0.Value.Value);
            Assert.Equal("I1", value0.Value.Value.Value);
            var value1 = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:3", cts.Token);
            Assert.True(value1.HasValue);
            Assert.Equal(2, value1.Value.Previous);
            Assert.Equal(0, value1.Value.Next);
            Assert.NotNull(value1.Value.Value);
            Assert.Equal("I2", value1.Value.Value.Value);
            var value2 = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:0", cts.Token);
            Assert.True(value2.HasValue);
            Assert.Equal(3, value2.Value.Previous);
            Assert.Equal(1, value2.Value.Next);
            Assert.NotNull(value2.Value.Value);
            Assert.Equal("A", value2.Value.Value.Value);
            var value3 = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:1", cts.Token);
            Assert.True(value3.HasValue);
            Assert.Equal(0, value3.Value.Previous);
            Assert.Null(value3.Value.Next);
            Assert.NotNull(value3.Value.Value);
            Assert.Equal("B", value3.Value.Value.Value);
        }

        [Fact]
        public async Task InsertAtAsync_BetweenTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var cts = new CancellationTokenSource(1000);

            await stateManager.SetStateAsync("TestName", new LinkedManifest
            {
                Count = 2,
                First = 0,
                Last = 1,
                Next = 2
            }, cts.Token);
            await stateManager.AddStateAsync("TestName:0", new LinkedNode<TestState>
            {
                Next = 1,
                Value = new TestState
                {
                    Value = "A"
                }
            }, cts.Token);
            await stateManager.AddStateAsync("TestName:1", new LinkedNode<TestState>
            {
                Previous = 0,
                Value = new TestState
                {
                    Value = "B"
                }
            }, cts.Token);

            var state = new TestLinkedCollectionState(stateManager, keyMock.Object);

            var values = new List<TestState>
            {
                new TestState
                {
                    Value = "I1"
                },
                new TestState
                {
                    Value = "I2"
                },
            };

            await state.TestInsertAtAsync(1, values, cts.Token);

            var manifest = await stateManager.GetStateAsync<LinkedManifest>("TestName", cts.Token);
            Assert.Equal(4, manifest.Count);
            Assert.Equal(0, manifest.First);
            Assert.Equal(1, manifest.Last);
            Assert.Equal(4, manifest.Next);
            var value0 = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:0", cts.Token);
            Assert.True(value0.HasValue);
            Assert.Null(value0.Value.Previous);
            Assert.Equal(2, value0.Value.Next);
            Assert.NotNull(value0.Value.Value);
            Assert.Equal("A", value0.Value.Value.Value);
            var value1 = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:2", cts.Token);
            Assert.True(value1.HasValue);
            Assert.Equal(0, value1.Value.Previous);
            Assert.Equal(3, value1.Value.Next);
            Assert.NotNull(value1.Value.Value);
            Assert.Equal("I1", value1.Value.Value.Value);
            var value2 = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:3", cts.Token);
            Assert.True(value2.HasValue);
            Assert.Equal(2, value2.Value.Previous);
            Assert.Equal(1, value2.Value.Next);
            Assert.NotNull(value2.Value.Value);
            Assert.Equal("I2", value2.Value.Value.Value);
            var value3 = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:1", cts.Token);
            Assert.True(value3.HasValue);
            Assert.Equal(3, value3.Value.Previous);
            Assert.Null(value3.Value.Next);
            Assert.NotNull(value3.Value.Value);
            Assert.Equal("B", value3.Value.Value.Value);
        }

        [Fact]
        public async Task InsertAtAsync_EndTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

            var cts = new CancellationTokenSource(1000);

            await stateManager.SetStateAsync("TestName", new LinkedManifest
            {
                Count = 2,
                First = 0,
                Last = 1,
                Next = 2
            }, cts.Token);
            await stateManager.AddStateAsync("TestName:0", new LinkedNode<TestState>
            {
                Next = 1,
                Value = new TestState
                {
                    Value = "A"
                }
            }, cts.Token);
            await stateManager.AddStateAsync("TestName:1", new LinkedNode<TestState>
            {
                Previous = 0,
                Value = new TestState
                {
                    Value = "B"
                }
            }, cts.Token);

            var state = new TestLinkedCollectionState(stateManager, keyMock.Object);

            var values = new List<TestState>
            {
                new TestState
                {
                    Value = "I1"
                },
                new TestState
                {
                    Value = "I2"
                },
            };

            await state.TestInsertAtAsync(2, values, cts.Token);

            var manifest = await stateManager.GetStateAsync<LinkedManifest>("TestName", cts.Token);
            Assert.Equal(4, manifest.Count);
            Assert.Equal(0, manifest.First);
            Assert.Equal(3, manifest.Last);
            Assert.Equal(4, manifest.Next);
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
            Assert.Equal(3, value2.Value.Next);
            Assert.NotNull(value2.Value.Value);
            Assert.Equal("I1", value2.Value.Value.Value);
            var value3 = await stateManager.TryGetStateAsync<LinkedNode<TestState>>("TestName:3", cts.Token);
            Assert.True(value3.HasValue);
            Assert.Equal(2, value3.Value.Previous);
            Assert.Null(value3.Value.Next);
            Assert.NotNull(value3.Value.Value);
            Assert.Equal("I2", value3.Value.Value.Value);
        }

        [Fact]
        public async Task EnumeratorTest()
        {
            var stateManager = new MockActorStateManager();
            var keyMock = new Mock<IStateKey>();
            keyMock.Setup(x => x.ToString()).Returns("TestName");

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
            await stateManager.AddStateAsync("TestName:0", new LinkedNode<TestState>
            {
                Next = 1,
                Value = values[0]
            }, cts.Token);
            await stateManager.AddStateAsync("TestName:1", new LinkedNode<TestState>
            {
                Previous = 0,
                Value = values[1]
            }, cts.Token);

            var state = new TestLinkedCollectionState(stateManager, keyMock.Object);

            var extractedValues = new List<TestState>();
            var enumerator = state.GetAsyncEnumerator();
            Assert.Null(enumerator.Current);

            while (await enumerator.MoveNextAsync(cts.Token))
            {
                Assert.NotNull(enumerator.Current);
                extractedValues.Add(enumerator.Current);
            }

            Assert.Equal(values, extractedValues);

            enumerator.Reset();

            Assert.Null(enumerator.Current);
        }
    }

    public class TestLinkedCollectionState : LinkedCollectionStateBase<TestState>
    {
        public TestLinkedCollectionState(IActorStateManager stateManager, IStateKey key)
            : base(stateManager, key)
        {
        }

        public Task TestInsertFirstAsync(IEnumerable<TestState> values, CancellationToken cancellationToken)
        {
            return InsertFirstAsync(values, cancellationToken);
        }

        public Task TestInsertAtAsync(long index, IEnumerable<TestState> values, CancellationToken cancellationToken)
        {
            return InsertAtAsync(index, values, cancellationToken);
        }

        public Task TestInsertLastAsync(IEnumerable<TestState> values, CancellationToken cancellationToken)
        {
            return InsertLastAsync(values, cancellationToken);
        }
    }
}