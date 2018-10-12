namespace Stateful.ServiceFabric
{
    using System;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class ActorStateFactory : IStateFactory
    {
        private readonly IActorStateManager _stateManager;

        public ActorStateFactory(IActorStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        /// <inheritdoc />
        public IObjectState<T> CreateObjectState<T>(string name)
        {
            return new ActorObjectState<T>(_stateManager, name);
        }

        /// <inheritdoc />
        public IListState<T> CreateListState<T>(string name)
        {
            return new ActorListState<T>(_stateManager, name);
        }

        /// <inheritdoc />
        public IDictionaryState<TKey, TValue> CreateDictionaryState<TKey, TValue>(string name) where TKey : IEquatable<TKey>, IComparable<TKey>
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IArrayState<T> CreateArrayState<T>(string name, long length)
        {
            return new ActorArrayState<T>(_stateManager, name, length);
        }

        /// <inheritdoc />
        public IQueueState<T> CreateQueueState<T>(string name)
        {
            return new ActorQueueState<T>(_stateManager, name);
        }

        /// <inheritdoc />
        public IStackState<T> CreateStackState<T>(string name)
        {
            throw new NotImplementedException();
        }
    }
}