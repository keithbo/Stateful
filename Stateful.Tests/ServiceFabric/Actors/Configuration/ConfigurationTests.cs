namespace Stateful.ServiceFabric.Actors.Configuration
{
    using System;
    using System.Collections.Generic;
    using global::ServiceFabric.Mocks;
    using Xunit;

    public class ConfigurationTests
    {
        [Fact]
        public void ExtensionTest()
        {
            var stateManager = new MockActorStateManager();

            Assert.Throws<ArgumentNullException>(() => State.Factory.CreateUsingServiceFabricActors(null, c => {}));
            Assert.Throws<ArgumentNullException>(() => State.Factory.CreateUsingServiceFabricActors(stateManager, null));
        }

        [Fact]
        public void NoStatesTest()
        {
            var stateManager = new MockActorStateManager();

            var f = State.Factory.CreateUsingServiceFabricActors(stateManager, c =>
            {
            });

            Assert.IsType<ActorStateFactory>(f);
            var unit = f.CreateTransaction();
            Assert.IsType<ActorStateUnit>(unit);

            Assert.Throws<KeyNotFoundException>(() => unit.Get<IObjectState<string>>(new StateKey("Test")));
        }

        [Fact]
        public void ConfigureObjectTest()
        {
            var stateManager = new MockActorStateManager();

            var f = State.Factory.CreateUsingServiceFabricActors(stateManager, c =>
            {
                c.AddObject<string>(s =>
                {
                    s.Key = new StateKey("Test");
                });
            });

            Assert.IsType<ActorStateFactory>(f);
            var unit = f.CreateTransaction();
            Assert.IsType<ActorStateUnit>(unit);

            var state = unit.Get<IObjectState<string>>(new StateKey("Test"));
            Assert.NotNull(state);
            Assert.IsType<ActorObjectState<string>>(state);
        }

        [Fact]
        public void ConfigureArrayTest()
        {
            var stateManager = new MockActorStateManager();

            var f = State.Factory.CreateUsingServiceFabricActors(stateManager, c =>
            {
                c.AddArray<string>(s =>
                {
                    s.Length = 1;
                    s.Key = new StateKey("Test");
                });
            });

            Assert.IsType<ActorStateFactory>(f);
            var unit = f.CreateTransaction();
            Assert.IsType<ActorStateUnit>(unit);

            var state = unit.Get<IArrayState<string>>(new StateKey("Test"));
            Assert.NotNull(state);
            Assert.IsType<ActorArrayState<string>>(state);
        }

        [Fact]
        public void ConfigureListTest()
        {
            var stateManager = new MockActorStateManager();

            var f = State.Factory.CreateUsingServiceFabricActors(stateManager, c =>
            {
                c.AddList<string>(s =>
                {
                    s.Key = new StateKey("Test");
                });
            });

            Assert.IsType<ActorStateFactory>(f);
            var unit = f.CreateTransaction();
            Assert.IsType<ActorStateUnit>(unit);

            var state = unit.Get<IListState<string>>(new StateKey("Test"));
            Assert.NotNull(state);
            Assert.IsType<ActorListState<string>>(state);
        }

        [Fact]
        public void ConfigureQueueTest()
        {
            var stateManager = new MockActorStateManager();

            var f = State.Factory.CreateUsingServiceFabricActors(stateManager, c =>
            {
                c.AddQueue<string>(s =>
                {
                    s.Key = new StateKey("Test");
                });
            });

            Assert.IsType<ActorStateFactory>(f);
            var unit = f.CreateTransaction();
            Assert.IsType<ActorStateUnit>(unit);

            var state = unit.Get<IQueueState<string>>(new StateKey("Test"));
            Assert.NotNull(state);
            Assert.IsType<ActorQueueState<string>>(state);
        }

        [Fact]
        public void ConfigureStackTest()
        {
            var stateManager = new MockActorStateManager();

            var f = State.Factory.CreateUsingServiceFabricActors(stateManager, c =>
            {
                c.AddStack<string>(s =>
                {
                    s.Key = new StateKey("Test");
                });
            });

            Assert.IsType<ActorStateFactory>(f);
            var unit = f.CreateTransaction();
            Assert.IsType<ActorStateUnit>(unit);

            var state = unit.Get<IStackState<string>>(new StateKey("Test"));
            Assert.NotNull(state);
            Assert.IsType<ActorStackState<string>>(state);
        }

        [Fact]
        public void ConfigureDictionaryTest()
        {
            var stateManager = new MockActorStateManager();

            var f = State.Factory.CreateUsingServiceFabricActors(stateManager, c =>
            {
                c.AddDictionary<string, string>(s =>
                {
                    s.Key = new StateKey("Test");
                });
            });

            Assert.IsType<ActorStateFactory>(f);
            var unit = f.CreateTransaction();
            Assert.IsType<ActorStateUnit>(unit);

            var state = unit.Get<IDictionaryState<string, string>>(new StateKey("Test"));
            Assert.NotNull(state);
            Assert.IsType<ActorDictionaryState<string, string>>(state);
        }
    }
}