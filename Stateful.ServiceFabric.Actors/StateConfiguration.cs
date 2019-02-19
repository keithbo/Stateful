namespace Stateful.ServiceFabric.Actors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class StateConfiguration : IStateConfiguration
    {
        private readonly IActorStateManager _stateManager;

        private readonly IDictionary<IStateKey, StateActivator> _activations = new Dictionary<IStateKey, StateActivator>();

        public StateConfiguration(IActorStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        /// <inheritdoc />
        public IStateConfiguration Object<TValue>(IStateKey key)
        {
            _activations.Add(key, new StateActivator((stateManager, stateKey) => new ActorObjectState<TValue>(stateManager, stateKey)));
            return this;
        }

        /// <inheritdoc />
        public IStateConfiguration List<TValue>(IStateKey key)
        {
            _activations.Add(key, new StateActivator((stateManager, stateKey) => new ActorListState<TValue>(stateManager, stateKey)));
            return this;
        }

        /// <inheritdoc />
        public IStateConfiguration Dictionary<TKey, TValue>(IStateKey key) where TKey : IEquatable<TKey>, IComparable<TKey>
        {
            _activations.Add(key, new StateActivator((stateManager, stateKey) => new ActorDictionaryState<TKey, TValue>(stateManager, stateKey)));
            return this;
        }

        /// <inheritdoc />
        public IStateConfiguration Array<TValue>(IStateKey key, long length)
        {
            _activations.Add(key, new StateActivator((stateManager, stateKey) => new ActorArrayState<TValue>(stateManager, stateKey, length)));
            return this;
        }

        /// <inheritdoc />
        public IStateConfiguration Queue<TValue>(IStateKey key)
        {
            _activations.Add(key, new StateActivator((stateManager, stateKey) => new ActorQueueState<TValue>(stateManager, stateKey)));
            return this;
        }

        /// <inheritdoc />
        public IStateConfiguration Stack<TValue>(IStateKey key)
        {
            _activations.Add(key, new StateActivator((stateManager, stateKey) => new ActorStackState<TValue>(stateManager, stateKey)));
            return this;
        }

        /// <inheritdoc />
        public IStateFactory Build()
        {
            return new ActorStateFactory(() => _stateManager, _activations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }
    }
}