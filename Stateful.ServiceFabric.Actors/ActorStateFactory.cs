namespace Stateful.ServiceFabric.Actors
{
    using System;
    using System.Collections.Generic;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Stateful.ServiceFabric.Actors.Configuration;

    internal class ActorStateFactory : IStateFactory
    {
        private readonly Func<IActorStateManager> _stateManagerFactory;
        private readonly IDictionary<IStateKey, IActorStateActivator> _activators;

        public ActorStateFactory(Func<IActorStateManager> stateManagerFactory, IDictionary<IStateKey, IActorStateActivator> activators)
        {
            _stateManagerFactory = stateManagerFactory;
            _activators = activators;
        }

        /// <inheritdoc />
        public IUnit CreateTransaction()
        {
            return new ActorStateUnit(_stateManagerFactory, (sm, key) => _activators[key].Resolve(sm, key));
        }
    }
}