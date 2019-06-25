namespace Stateful.ServiceFabric.Actors.Configuration
{
    using System.Collections.Generic;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class ActorStateFactoryConfiguration : IActorStateFactoryConfiguration
    {
        private readonly List<IActorStateActivator> _activations = new List<IActorStateActivator>();

        public IActorStateManager StateManager { get; set; }

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