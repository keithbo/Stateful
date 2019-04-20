namespace Stateful.ServiceFabric.Actors.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class ServiceFabricActorStateConfiguration : IServiceFabricActorStateConfiguration
    {
        public IActorStateManager StateManager { get; set; }

        private readonly List<IActorStateActivator> _activations = new List<IActorStateActivator>();

        public void AddStateActivator(IActorStateActivator activator)
        {
            _activations.Add(activator);
        }


        /// <inheritdoc />
        public IStateFactory Build()
        {
            return new ActorStateFactory(StateManager, _activations);
        }
    }
}