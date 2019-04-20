namespace Stateful.ServiceFabric.Actors.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class ServiceFabricActorStateConfiguration : IServiceFabricActorStateConfiguration
    {
        public IActorStateManager StateManager { get; set; }

        private readonly IDictionary<IStateKey, IActorStateActivator> _activations = new Dictionary<IStateKey, IActorStateActivator>();

        public ServiceFabricActorStateConfiguration()
        {
        }

        public void AddStateActivator(IStateKey key, IActorStateActivator activator)
        {
            _activations.Add(key, activator);
        }


        /// <inheritdoc />
        public IStateFactory Build()
        {
            return new ActorStateFactory(() => StateManager, _activations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }
    }
}