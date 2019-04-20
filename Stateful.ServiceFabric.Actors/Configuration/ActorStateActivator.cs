namespace Stateful.ServiceFabric.Actors.Configuration
{
    using System;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class ActorStateActivator : IActorStateActivator
    {
        private readonly Func<IActorStateManager, IStateKey, IState> _factoryMethod;

        public ActorStateActivator(Func<IActorStateManager, IStateKey, IState> factoryMethod)
        {
            _factoryMethod = factoryMethod;
        }

        public IState Resolve(IActorStateManager stateManager, IStateKey key)
        {
            return _factoryMethod(stateManager, key);
        }
    }
}