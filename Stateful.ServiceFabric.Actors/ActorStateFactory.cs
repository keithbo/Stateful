namespace Stateful.ServiceFabric.Actors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Stateful.ServiceFabric.Actors.Configuration;

    internal class ActorStateFactory : IStateFactory
    {
        private readonly IActorStateManager _stateManager;
        private readonly IDictionary<IStateKey, IActorStateActivator> _activators;

        public ActorStateFactory(IActorStateManager stateManager, IEnumerable<IActorStateActivator> activators)
        {
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _activators = activators.ToDictionary(kvp => kvp.Key);
        }

        /// <inheritdoc />
        public IUnit CreateTransaction()
        {
            return new ActorStateUnit(_stateManager, (sm, key) => _activators[key].Resolve(sm));
        }
    }
}