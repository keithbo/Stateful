namespace Stateful.ServiceFabric
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Stateful.ServiceFabric.Internals;

    public class ActorListState<T> : LinkedCollectionStateBase<T>, IListState<T>
    {
        public ActorListState(IActorStateManager stateManager, string name)
            : base(stateManager, name)
        {
        }

        /// <inheritdoc />
        public async Task AddAsync(T value, CancellationToken cancellationToken)
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
        public async Task AddRangeAsync(IEnumerable<T> values, CancellationToken cancellationToken)
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedNodeManifest>(Name, cancellationToken);
            var manifest = manifestResult.HasValue ? manifestResult.Value : new LinkedNodeManifest();

            LinkedNode<T> previousNode = null;
            string previousKey = null;

            foreach (var value in values)
            {
                if (previousNode == null && manifest.Last.HasValue)
                {
                    previousKey = IndexToKey(manifest.Last.Value);
                    previousNode = await StateManager.GetStateAsync<LinkedNode<T>>(previousKey, cancellationToken);
                }

                var newIndex = NextIndex(manifest);
                var newKey = IndexToKey(newIndex);
                var newNode = new LinkedNode<T>
                {
                    Value = value,
                    Previous = manifest.Last
                };

                manifest.Count++;
                manifest.Last = newIndex;
                if (!manifest.First.HasValue)
                {
                    manifest.First = newIndex;
                }

                if (previousNode != null)
                {
                    previousNode.Next = newIndex;
                    await StateManager.SetStateAsync(previousKey, previousNode, cancellationToken);
                }

                previousKey = newKey;
                previousNode = newNode;
            }

            if (previousNode != null)
            {
                await StateManager.SetStateAsync(previousKey, previousNode, cancellationToken);
            }
            await StateManager.SetStateAsync(Name, manifest, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new List<T>();
            var manifestResult = await StateManager.TryGetStateAsync<LinkedNodeManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return result;
            }

            var manifest = manifestResult.Value;
            LinkedNode<T> currentNode;
            for (var internalIndex = manifest.First; internalIndex.HasValue; internalIndex = currentNode.Next)
            {
                currentNode = await StateManager.GetStateAsync<LinkedNode<T>>(IndexToKey(internalIndex.Value), cancellationToken);
                result.Add(currentNode.Value);
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<ConditionalValue<T>> TryGetAsync(long index, CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedNodeManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return new ConditionalValue<T>();
            }

            var manifest = manifestResult.Value;
            if (index < 0 || index >= manifest.Count)
            {
                return new ConditionalValue<T>();
            }

            long i = 0;
            var (foundKey, foundNode) = await FindNodeAsync(manifest.First, (key, node) => i++ == index, cancellationToken);

            return foundNode != null ? new ConditionalValue<T>(foundNode.Value) : new ConditionalValue<T>();
        }

        /// <inheritdoc />
        public async Task<ConditionalValue<T>> TryFindAsync(Predicate<T> match, CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedNodeManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return new ConditionalValue<T>();
            }

            var manifest = manifestResult.Value;
            var (foundKey, foundNode) = await FindNodeAsync(manifest.First, (key, node) => match(node.Value), cancellationToken);

            return foundNode != null ? new ConditionalValue<T>(foundNode.Value) : new ConditionalValue<T>();
        }

        /// <inheritdoc />
        public async Task InsertAsync(long index, T value, CancellationToken cancellationToken = default(CancellationToken))
        {
            await InsertCoreAsync(index, value, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveAtAsync(long index, CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedNodeManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                throw new IndexOutOfRangeException("List is empty");
            }

            var manifest = manifestResult.Value;
            if (index < 0 || index >= manifest.Count)
            {
                throw new IndexOutOfRangeException($"Index {index} must be in range [0, {manifest.Count})");
            }

            long i = 0;
            await RemoveAsync(value => i++ == index, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveAsync(Predicate<T> match, CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedNodeManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return;
            }

            var manifest = manifestResult.Value;
            string previousKey = null;
            LinkedNode<T> previousNode = null;
            var (foundKey, foundNode) = await FindNodeAsync(manifest.First, (key, node) =>
            {
                var success = match(node.Value);
                if (!success)
                {
                    previousKey = key;
                    previousNode = node;
                }

                return success;
            }, cancellationToken);

            if (foundNode != null)
            {
                await RemoveCoreAsync(manifest, previousKey, previousNode, foundKey, foundNode, cancellationToken);
            }
        }
    }
}