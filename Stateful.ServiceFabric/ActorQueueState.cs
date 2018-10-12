namespace Stateful.ServiceFabric
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Stateful.ServiceFabric.Internals;

    public class ActorQueueState<T> : LinkedCollectionStateBase<T>, IQueueState<T>
    {
        public ActorQueueState(IActorStateManager stateManager, string name)
            : base(stateManager, name)
        {
        }

        /// <inheritdoc />
        public async Task<ConditionalValue<T>> TryPeekAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedNodeManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return new ConditionalValue<T>();
            }

            var manifest = manifestResult.Value;
            if (!manifest.First.HasValue)
            {
                return new ConditionalValue<T>();
            }

            var key = IndexToKey(manifest.First.Value);
            var node = await StateManager.TryGetStateAsync<LinkedNode<T>>(key, cancellationToken);
            return node.HasValue ? new ConditionalValue<T>(node.Value.Value) : new ConditionalValue<T>();
        }

        /// <inheritdoc />
        public async Task EnqueueAsync(T value, CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedNodeManifest>(Name, cancellationToken);
            var manifest = manifestResult.HasValue ? manifestResult.Value : new LinkedNodeManifest();
            manifest.Count++;

            var newIndex = NextIndex(manifest);
            var newKey = IndexToKey(newIndex);
            var newNode = new LinkedNode<T>
            {
                Value = value,
                Previous = manifest.Last
            };
            manifest.Last = newIndex;
            if (!manifest.First.HasValue)
            {
                manifest.First = newIndex;
            }

            if (newNode.Previous.HasValue)
            {
                var lastKey = IndexToKey(newNode.Previous.Value);
                var lastNode = await StateManager.GetStateAsync<LinkedNode<T>>(lastKey, cancellationToken);
                lastNode.Next = newIndex;
                await StateManager.SetStateAsync(lastKey, lastNode, cancellationToken);
            }

            await StateManager.AddStateAsync(newKey, newNode, cancellationToken);
            await StateManager.SetStateAsync(Name, manifest, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<ConditionalValue<T>> TryDequeueAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedNodeManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return new ConditionalValue<T>();
            }

            var manifest = manifestResult.Value;
            if (!manifest.First.HasValue)
            {
                return new ConditionalValue<T>();
            }

            var key = IndexToKey(manifest.First.Value);
            var nodeResult = await StateManager.TryGetStateAsync<LinkedNode<T>>(key, cancellationToken);
            if (!nodeResult.HasValue)
            {
                return new ConditionalValue<T>();
            }

            await RemoveCoreAsync(manifest, null, null, key, nodeResult.Value, cancellationToken);

            return new ConditionalValue<T>(nodeResult.Value.Value);
        }
    }
}