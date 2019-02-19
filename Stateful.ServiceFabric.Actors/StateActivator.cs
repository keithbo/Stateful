namespace Stateful.ServiceFabric.Actors
{
    using System;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class StateActivator
    {
        private readonly Func<IActorStateManager, IStateKey, IState> _factoryMethod;

        public StateActivator(Func<IActorStateManager, IStateKey, IState> factoryMethod)
        {
            _factoryMethod = factoryMethod;
        }

        public IState Resolve(IActorStateManager stateManager, IStateKey key)
        {
            return _factoryMethod(stateManager, key);
        }
    }
}