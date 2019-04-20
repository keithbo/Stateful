namespace Stateful.ServiceFabric.Actors.Configuration
{
    using System;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class ServiceFabricActorStateFactoryConfigurator : IServiceFabricActorStateFactoryConfigurator
    {
        private readonly IServiceFabricActorStateConfiguration _configuration;

        public ServiceFabricActorStateFactoryConfigurator(IServiceFabricActorStateConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActorStateManager StateManager
        {
            set => _configuration.StateManager = value;
        }

        /// <inheritdoc />
        public void AddObject<TValue>(IStateKey key)
        {
            _configuration.AddStateActivator(
                key,
                new ActorStateActivator((stateManager, stateKey) => new ActorObjectState<TValue>(stateManager, stateKey)));
        }

        /// <inheritdoc />
        public void AddList<TValue>(IStateKey key)
        {
            _configuration.AddStateActivator(
                key,
                new ActorStateActivator((stateManager, stateKey) => new ActorListState<TValue>(stateManager, stateKey)));
        }

        /// <inheritdoc />
        public void AddDictionary<TKey, TValue>(IStateKey key) where TKey : IEquatable<TKey>, IComparable<TKey>
        {
            _configuration.AddStateActivator(
                key,
                new ActorStateActivator((stateManager, stateKey) => new ActorDictionaryState<TKey, TValue>(stateManager, stateKey)));
        }

        /// <inheritdoc />
        public void AddArray<TValue>(IStateKey key, long length)
        {
            _configuration.AddStateActivator(
                key,
                new ActorStateActivator((stateManager, stateKey) => new ActorArrayState<TValue>(stateManager, stateKey, length)));
        }

        /// <inheritdoc />
        public void AddQueue<TValue>(IStateKey key)
        {
            _configuration.AddStateActivator(
                key,
                new ActorStateActivator((stateManager, stateKey) => new ActorQueueState<TValue>(stateManager, stateKey)));
        }

        /// <inheritdoc />
        public void AddStack<TValue>(IStateKey key)
        {
            _configuration.AddStateActivator(
                key,
                new ActorStateActivator((stateManager, stateKey) => new ActorStackState<TValue>(stateManager, stateKey)));
        }
    }
}