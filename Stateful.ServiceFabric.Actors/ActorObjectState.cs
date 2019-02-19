namespace Stateful.ServiceFabric.Actors
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class ActorObjectState<T> : IObjectState<T>
    {
        private readonly IActorStateManager _stateManager;
        private readonly string _name;

        public IStateKey Key { get; }

        public ActorObjectState(IActorStateManager stateManager, IStateKey key)
        {
            _stateManager = stateManager;
            Key = key;
            _name = key.Name;
        }

        public Task<bool> HasStateAsync(CancellationToken cancellationToken)
        {
            return _stateManager.ContainsStateAsync(_name, cancellationToken);
        }

        public Task DeleteStateAsync(CancellationToken cancellationToken)
        {
            return _stateManager.TryRemoveStateAsync(_name, cancellationToken);
        }

        public async Task<ConditionalValue<T>> TryGetAsync(CancellationToken cancellationToken)
        {
            var value = await _stateManager.TryGetStateAsync<T>(_name, cancellationToken);
            return new ConditionalValue<T>(value.HasValue, value.Value);
        }

        public async Task<T> SetAsync(T value, CancellationToken cancellationToken)
        {
            await _stateManager.SetStateAsync(_name, value, cancellationToken);
            return value;
        }
    }
}