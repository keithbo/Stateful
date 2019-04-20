namespace Stateful.ServiceFabric.Actors.Configuration
{
    using System;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class ActorStateActivator : IActorStateActivator
    {
        private readonly Func<IActorStateManager, IState> _factoryMethod;

        public IStateKey Key { get; }

        public ActorStateActivator(IStateKey key, Func<IActorStateManager, IState> factoryMethod)
        {
            Key = key;
            _factoryMethod = factoryMethod;
        }

        public IState Resolve(IActorStateManager stateManager)
        {
            return _factoryMethod(stateManager);
        }
    }
}