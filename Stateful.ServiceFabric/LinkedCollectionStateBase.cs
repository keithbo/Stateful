namespace Stateful.ServiceFabric
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Stateful.ServiceFabric.Internals;

    public abstract class LinkedCollectionStateBase<T> : ICollectionState<T>
    {
        protected const string IndexKeyFormat = "{0}:{1:X}";

        protected IActorStateManager StateManager { get; private set; }

        public string Name { get; }

        protected LinkedCollectionStateBase(IActorStateManager stateManager, string name)
        {
            StateManager = stateManager;
            Name = name;
        }

        /// <inheritdoc />
        public Task<bool> HasStateAsync(CancellationToken cancellationToken)
        {
            return StateManager.ContainsStateAsync(Name, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteStateAsync(CancellationToken cancellationToken)
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedNodeManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return;
            }

            var manifest = manifestResult.Value;
            LinkedNode<T> currentNode;
            for (var internalIndex = manifest.First; internalIndex.HasValue; internalIndex = currentNode.Next)
            {
                var key = IndexToKey(internalIndex.Value);
                currentNode = await StateManager.GetStateAsync<LinkedNode<T>>(key, cancellationToken);
                await StateManager.RemoveStateAsync(key, cancellationToken);
            }

            await StateManager.RemoveStateAsync(Name, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<long> CountAsync(CancellationToken cancellationToken)
        {
            var manifest = await StateManager.TryGetStateAsync<LinkedNodeManifest>(Name, cancellationToken);
            return manifest.HasValue ? manifest.Value.Count : 0;
        }

        /// <inheritdoc />
        public async Task<bool> ContainsAsync(Predicate<T> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedNodeManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return false;
            }

            var manifest = manifestResult.Value;
            var (foundKey, foundNode) = await FindNodeAsync(manifest.First, (key, node) => predicate(node.Value), cancellationToken);

            return foundNode != null;
        }

        /// <inheritdoc />
        public IAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return new AsyncEnumerator(this);
        }

        protected async Task InsertCoreAsync(long position, T value, CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedNodeManifest>(Name, cancellationToken);
            var manifest = manifestResult.HasValue ? manifestResult.Value : new LinkedNodeManifest();

            if (position < 0 || position > manifest.Count)
            {
                throw new IndexOutOfRangeException($"Index {position} must be in range [0, {manifest.Count}]");
            }

            long i = 0;
            string previousKey = null;
            LinkedNode<T> previousNode = null;
            var (foundKey, foundNode) = await FindNodeAsync(manifest.First, (key, node) =>
            {
                var success = i++ == position;
                if (!success)
                {
                    previousKey = key;
                    previousNode = node;
                }

                return success;
            }, cancellationToken);

            var newIndex = NextIndex(manifest);
            var newKey = IndexToKey(newIndex);
            var newNode = new LinkedNode<T>
            {
                Value = value,
                Next = previousNode?.Next ?? manifest.First,
                Previous = foundNode?.Previous ?? manifest.Last
            };

            await StateManager.AddStateAsync(newKey, newNode, cancellationToken);

            if (previousNode == null)
            {
                manifest.First = newIndex;
            }
            else
            {
                previousNode.Next = newIndex;
                await StateManager.SetStateAsync(previousKey, previousNode, cancellationToken);
            }

            if (foundNode == null)
            {
                manifest.Last = newIndex;
            }
            else
            {
                foundNode.Previous = newIndex;
                await StateManager.SetStateAsync(foundKey, foundNode, cancellationToken);
            }

            manifest.Count++;
            await StateManager.SetStateAsync(Name, manifest, cancellationToken);
        }

        protected async Task RemoveCoreAsync(LinkedNodeManifest manifest, string previousKey, LinkedNode<T> previousNode, string removeKey, LinkedNode<T> removeNode, CancellationToken cancellationToken)
        {
            if (previousNode != null)
            {
                previousNode.Next = removeNode.Next;
                await StateManager.SetStateAsync(previousKey, previousNode, cancellationToken);
            }
            else
            {
                manifest.First = removeNode.Next;
            }

            if (removeNode.Next.HasValue)
            {
                var nextKey = IndexToKey(removeNode.Next.Value);
                var nextNode = await StateManager.GetStateAsync<LinkedNode<T>>(nextKey, cancellationToken);
                nextNode.Previous = removeNode.Previous;
                await StateManager.SetStateAsync(nextKey, nextNode, cancellationToken);
            }
            else
            {
                manifest.Last = removeNode.Previous;
            }

            await StateManager.RemoveStateAsync(removeKey, cancellationToken);

            manifest.Count--;
            await StateManager.SetStateAsync(Name, manifest, cancellationToken);
        }

        protected static long NextIndex(LinkedNodeManifest manifest)
        {
            return manifest.Next++;
        }

        protected string IndexToKey(long index)
        {
            return string.Format(IndexKeyFormat, Name, index);
        }

        protected async Task<(string Key, LinkedNode<T> Node)> FindNodeAsync(long? startIndex, Func<string, LinkedNode<T>, bool> predicate, CancellationToken cancellationToken)
        {
            string currentKey = null;
            LinkedNode<T> current = null;
            var found = false;
            for (var currentIndex = startIndex; !found && currentIndex.HasValue; currentIndex = current.Next)
            {
                currentKey = IndexToKey(currentIndex.Value);
                current = await StateManager.GetStateAsync<LinkedNode<T>>(currentKey, cancellationToken);
                found = predicate(currentKey, current);
            }

            return (Key: currentKey, Node: current);
        }

        private class AsyncEnumerator : IAsyncEnumerator<T>
        {
            private readonly LinkedCollectionStateBase<T> _source;
            private LinkedNode<T> _currentNode;

            internal AsyncEnumerator(LinkedCollectionStateBase<T> source)
            {
                _source = source;
            }

            /// <inheritdoc />
            public void Dispose()
            {
            }

            /// <inheritdoc />
            public T Current => _currentNode != null ? _currentNode.Value : default(T);

            /// <inheritdoc />
            public async Task<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                var stateManager = _source.StateManager;
                if (_currentNode == null)
                {
                    var manifestResult = await stateManager.TryGetStateAsync<LinkedNodeManifest>(_source.Name, cancellationToken);
                    if (!manifestResult.HasValue)
                    {
                        return false;
                    }

                    var manifest = manifestResult.Value;
                    if (!manifest.First.HasValue)
                    {
                        return false;
                    }

                    _currentNode = await stateManager.GetStateAsync<LinkedNode<T>>(string.Format(IndexKeyFormat, _source.Name, manifest.First.Value), cancellationToken);
                }
                else
                {
                    if (_currentNode.Next.HasValue)
                    {
                        _currentNode = await stateManager.GetStateAsync<LinkedNode<T>>(string.Format(IndexKeyFormat, _source.Name, _currentNode.Next.Value), cancellationToken);
                    }
                    else
                    {
                        _currentNode = null;
                    }
                }

                return _currentNode != null;
            }

            /// <inheritdoc />
            public void Reset()
            {
                _currentNode = null;
            }
        }
    }
}