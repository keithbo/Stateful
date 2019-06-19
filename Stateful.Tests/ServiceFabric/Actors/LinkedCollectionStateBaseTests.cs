namespace Stateful.ServiceFabric.Actors
{
    using System;
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
            await stateManager.AddStateAsync("TestName:0", new LinkedNode<TestLinkedState>
            {
                Value = new TestLinkedState()
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

            var state = new TestLinkedCollectionState(stateManager, keyMock.Object);

            var searchMock = new Mock<Predicate<TestLinkedState>>();
            searchMock.Setup(x => x(It.IsAny<TestLinkedState>()))
                .Returns<TestLinkedState>(s => s.Value == "A");

            var cts = new CancellationTokenSource(1000);

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
            await stateManager.AddStateAsync("TestName:0", new LinkedNode<TestLinkedState>
            {
                Value = new TestLinkedState
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
            await stateManager.SetStateAsync("TestName:0", new LinkedNode<TestLinkedState>
            {
                Next = 1,
                Value = new TestLinkedState
                {
                    Value = "B"
                }
            }, cts.Token);
            await stateManager.AddStateAsync("TestName:1", new LinkedNode<TestLinkedState>
            {
                Previous = 0,
                Value = new TestLinkedState
                {
                    Value = "A"
                }
            }, cts.Token);
            Assert.True(await state.ContainsAsync(searchMock.Object, cts.Token));
        }
    }

    public class TestLinkedCollectionState : LinkedCollectionStateBase<TestLinkedState>
    {
        public TestLinkedCollectionState(IActorStateManager stateManager, IStateKey key)
            : base(stateManager, key)
        {
        }
    }

    public class TestLinkedState
    {
        public string Value { get; set; }
    }
}