namespace Stateful.ServiceFabric.Actors
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Stateful.ServiceFabric.Actors.Internals;

    public class ActorDictionaryState<TKey, TValue> : IDictionaryState<TKey, TValue> where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        protected const string BucketIndexFormat = "{0}:{1:X}";
        protected const string KeyIndexFormat = "{0}:{1:X}:{2:X}";
        protected const string ValueIndexFormat = "{0}:{1:X}:{2:X}:v";

        protected string Name => Key.ToString();

        protected IActorStateManager StateManager { get; }

        public IStateKey Key { get; }

        public ActorDictionaryState(IActorStateManager stateManager, IStateKey key)
        {
            StateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        /// <inheritdoc />
        public Task<bool> HasStateAsync(CancellationToken cancellationToken)
        {
            return StateManager.ContainsStateAsync(Name, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteStateAsync(CancellationToken cancellationToken)
        {
            var manifestResult = await StateManager.TryGetStateAsync<HashManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return;
            }

            var manifest = manifestResult.Value;
            HashBucket currentBucket;
            for (var bucketIndex = manifest.Head; bucketIndex.HasValue; bucketIndex = currentBucket.Next)
            {
                var bucketName = IndexToBucket(bucketIndex.Value);
                currentBucket = await StateManager.GetStateAsync<HashBucket>(bucketName, cancellationToken);

                HashKeyNode<TKey> currentKeyNode;
                for (var keyIndex = currentBucket.Head; keyIndex.HasValue; keyIndex = currentKeyNode.Next)
                {
                    var keyName = IndexToKey(bucketIndex.Value, keyIndex.Value);
                    var valueName = IndexToValue(bucketIndex.Value, keyIndex.Value);

                    currentKeyNode = await StateManager.GetStateAsync<HashKeyNode<TKey>>(keyName, cancellationToken);

                    await StateManager.RemoveStateAsync(keyName, cancellationToken);
                    await StateManager.RemoveStateAsync(valueName, cancellationToken);
                }

                await StateManager.RemoveStateAsync(bucketName, cancellationToken);
            }

            await StateManager.RemoveStateAsync(Name, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<long> CountAsync(CancellationToken cancellationToken)
        {
            var manifest = await StateManager.TryGetStateAsync<HashManifest>(Name, cancellationToken);
            return manifest.HasValue ? manifest.Value.Count : 0;
        }

        /// <inheritdoc />
        public async Task AddAsync(TKey key, TValue value, CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await StateManager.TryGetStateAsync<HashManifest>(Name, cancellationToken);
            var manifest = manifestResult.HasValue ? manifestResult.Value : new HashManifest();

            var bucketIndex = (long)key.GetHashCode();
            var bucketName = IndexToBucket(bucketIndex);

            var bucketResult = await StateManager.TryGetStateAsync<HashBucket>(bucketName, cancellationToken);
            var bucket = bucketResult.HasValue ? bucketResult.Value : new HashBucket { HashCode = bucketIndex, Previous = manifest.Tail };

            long? lastKeyIndex = null;
            string lastKeyName = null;
            HashKeyNode<TKey> lastKeyNode = null;
            if (bucketResult.HasValue)
            {
                for (var keyIndex = bucket.Head; keyIndex.HasValue; keyIndex = lastKeyNode.Next)
                {
                    lastKeyIndex = keyIndex;

                    lastKeyName = IndexToKey(bucketIndex, keyIndex.Value);

                    lastKeyNode = await StateManager.GetStateAsync<HashKeyNode<TKey>>(lastKeyName, cancellationToken);

                    if (EqualityComparer<TKey>.Default.Equals(key, lastKeyNode.Key))
                    {
                        throw new ArgumentException("Key already exists");
                    }
                }
            }
            else
            {
                if (manifest.Tail.HasValue)
                {
                    var lastBucketName = IndexToBucket(manifest.Tail.Value);
                    var lastBucket = await StateManager.GetStateAsync<HashBucket>(lastBucketName, cancellationToken);
                    lastBucket.Next = bucketIndex;
                    await StateManager.SetStateAsync(lastBucketName, lastBucket, cancellationToken);
                }

                manifest.Tail = bucketIndex;
                manifest.Head = manifest.Head ?? bucketIndex;
            }

            manifest.Count++;

            var newKeyIndex = NextIndex(manifest);
            var newKeyName = IndexToKey(bucketIndex, newKeyIndex);
            var newKeyNode = new HashKeyNode<TKey>
            {
                Key = key,
                Previous = lastKeyIndex
            };
            await StateManager.AddStateAsync(newKeyName, newKeyNode, cancellationToken);

            var newValueName = IndexToValue(bucketIndex, newKeyIndex);
            await StateManager.AddStateAsync(newValueName, value, cancellationToken);

            if (lastKeyIndex.HasValue)
            {
                lastKeyNode.Next = newKeyIndex;
                await StateManager.SetStateAsync(lastKeyName, lastKeyNode, cancellationToken);
            }

            bucket.Tail = newKeyIndex;
            if (!bucket.Head.HasValue)
            {
                bucket.Head = newKeyIndex;
            }

            await StateManager.SetStateAsync(bucketName, bucket, cancellationToken);

            await StateManager.SetStateAsync(Name, manifest, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> ContainsAsync(Predicate<KeyValuePair<TKey, TValue>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await StateManager.TryGetStateAsync<HashManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return false;
            }

            var found = false;
            var manifest = manifestResult.Value;
            HashBucket currentBucket;
            for (var bucketIndex = manifest.Head; !found && bucketIndex.HasValue; bucketIndex = currentBucket.Next)
            {
                var bucketName = IndexToBucket(bucketIndex.Value);
                currentBucket = await StateManager.GetStateAsync<HashBucket>(bucketName, cancellationToken);

                HashKeyNode<TKey> currentKeyNode;
                for (var keyIndex = currentBucket.Head; !found && keyIndex.HasValue; keyIndex = currentKeyNode.Next)
                {
                    var keyName = IndexToKey(bucketIndex.Value, keyIndex.Value);
                    var valueName = IndexToValue(bucketIndex.Value, keyIndex.Value);

                    currentKeyNode = await StateManager.GetStateAsync<HashKeyNode<TKey>>(keyName, cancellationToken);
                    var value = await StateManager.GetStateAsync<TValue>(valueName, cancellationToken);

                    found = predicate(new KeyValuePair<TKey, TValue>(currentKeyNode.Key, value));
                }
            }

            return found;
        }

        /// <inheritdoc />
        public async Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken = default(CancellationToken))
        {
            var bucketIndex = (long)key.GetHashCode();
            var bucketName = IndexToBucket(bucketIndex);

            var bucketResult = await StateManager.TryGetStateAsync<HashBucket>(bucketName, cancellationToken);
            if (!bucketResult.HasValue)
            {
                return false;
            }

            var found = false;
            HashKeyNode<TKey> currentKeyNode;
            for (var keyIndex = bucketResult.Value.Head; !found && keyIndex.HasValue; keyIndex = currentKeyNode.Next)
            {
                var keyName = IndexToKey(bucketIndex, keyIndex.Value);

                currentKeyNode = await StateManager.GetStateAsync<HashKeyNode<TKey>>(keyName, cancellationToken);

                found = EqualityComparer<TKey>.Default.Equals(key, currentKeyNode.Key);
            }

            return found;
        }

        /// <inheritdoc />
        public async Task<ConditionalValue<TValue>> TryGetValueAsync(TKey key, CancellationToken cancellationToken = default(CancellationToken))
        {
            var bucketIndex = (long)key.GetHashCode();
            var bucketName = IndexToBucket(bucketIndex);

            var bucketResult = await StateManager.TryGetStateAsync<HashBucket>(bucketName, cancellationToken);
            if (!bucketResult.HasValue)
            {
                return new ConditionalValue<TValue>();
            }

            var found = false;
            var value = default(TValue);
            HashKeyNode<TKey> currentKeyNode;
            for (var keyIndex = bucketResult.Value.Head; !found && keyIndex.HasValue; keyIndex = currentKeyNode.Next)
            {
                var keyName = IndexToKey(bucketIndex, keyIndex.Value);

                currentKeyNode = await StateManager.GetStateAsync<HashKeyNode<TKey>>(keyName, cancellationToken);

                found = EqualityComparer<TKey>.Default.Equals(key, currentKeyNode.Key);
                if (found)
                {
                    value = await StateManager.GetStateAsync<TValue>(IndexToValue(bucketIndex, keyIndex.Value), cancellationToken);
                }
            }

            return found ? new ConditionalValue<TValue>(value) : new ConditionalValue<TValue>();
        }

        /// <inheritdoc />
        public async Task<bool> RemoveAsync(TKey key, CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await StateManager.TryGetStateAsync<HashManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return false;
            }

            var manifest = manifestResult.Value;

            var bucketIndex = (long)key.GetHashCode();
            var bucketName = IndexToBucket(bucketIndex);

            var bucketResult = await StateManager.TryGetStateAsync<HashBucket>(bucketName, cancellationToken);
            if (!bucketResult.HasValue)
            {
                return false;
            }

            var bucket = bucketResult.Value;

            var found = false;
            long currentKeyIndex = 0;
            string currentKeyName = null;
            HashKeyNode<TKey> currentKeyNode = null;
            for (var keyIndex = bucket.Head; !found && keyIndex.HasValue; keyIndex = currentKeyNode.Next)
            {
                currentKeyIndex = keyIndex.Value;
                currentKeyName = IndexToKey(bucketIndex, currentKeyIndex);
                currentKeyNode = await StateManager.GetStateAsync<HashKeyNode<TKey>>(currentKeyName, cancellationToken);

                found = EqualityComparer<TKey>.Default.Equals(key, currentKeyNode.Key);
            }

            if (!found)
            {
                return false;
            }

            await PruneNodeAsync(bucketIndex, currentKeyIndex, currentKeyName, currentKeyNode, cancellationToken);

            var bucketChanged = false;
            if (bucket.Head == currentKeyIndex)
            {
                bucket.Head = currentKeyNode.Next;
                bucketChanged = true;
            }

            if (bucket.Tail == currentKeyIndex)
            {
                bucket.Tail = currentKeyNode.Previous;
                bucketChanged = true;
            }

            if (bucketChanged && !await PruneBucketAsync(manifest, bucketIndex, bucketName, bucket, cancellationToken))
            {
                await StateManager.SetStateAsync(bucketName, bucket, cancellationToken);
            }

            manifest.Count--;
            await StateManager.SetStateAsync(Name, manifest, cancellationToken);

            return true;
        }

        /// <summary>
        /// Removes a key/value node.
        /// Condenses any surrounding nodes if necessary
        /// </summary>
        internal async Task PruneNodeAsync(long bucketIndex, long currentKeyIndex, string currentKeyName, HashKeyNode<TKey> currentKeyNode, CancellationToken cancellationToken)
        {
            var currentValueName = IndexToValue(bucketIndex, currentKeyIndex);

            await StateManager.RemoveStateAsync(currentKeyName, cancellationToken);
            await StateManager.RemoveStateAsync(currentValueName, cancellationToken);

            if (currentKeyNode.Previous.HasValue)
            {
                var previousKeyName = IndexToKey(bucketIndex, currentKeyNode.Previous.Value);
                var previousNode = await StateManager.GetStateAsync<HashKeyNode<TKey>>(previousKeyName, cancellationToken);
                previousNode.Next = currentKeyNode.Next;
                await StateManager.SetStateAsync(previousKeyName, previousNode, cancellationToken);
            }

            if (currentKeyNode.Next.HasValue)
            {
                var nextKeyName = IndexToKey(bucketIndex, currentKeyNode.Next.Value);
                var nextNode = await StateManager.GetStateAsync<HashKeyNode<TKey>>(nextKeyName, cancellationToken);
                nextNode.Previous = currentKeyNode.Previous;
                await StateManager.SetStateAsync(nextKeyName, nextNode, cancellationToken);
            }
        }

        /// <summary>
        /// Conditionally removes a bucket as long as it is empty of key/value pairs.
        /// Condenses any surrounding buckets if necessary
        /// </summary>
        internal async Task<bool> PruneBucketAsync(HashManifest manifest, long bucketIndex, string bucketName, HashBucket bucket, CancellationToken cancellationToken)
        {
            if (bucket.Head.HasValue || bucket.Tail.HasValue)
            {
                return false;
            }

            await StateManager.RemoveStateAsync(bucketName, cancellationToken);

            if (bucket.Previous.HasValue)
            {
                var previousBucketName = IndexToBucket(bucket.Previous.Value);
                var previousBucket = await StateManager.GetStateAsync<HashBucket>(previousBucketName, cancellationToken);
                previousBucket.Next = bucket.Next;
                await StateManager.SetStateAsync(previousBucketName, previousBucket, cancellationToken);
            }

            if (bucket.Next.HasValue)
            {
                var nextBucketName = IndexToBucket(bucket.Next.Value);
                var nextBucket = await StateManager.GetStateAsync<HashBucket>(nextBucketName, cancellationToken);
                nextBucket.Previous = bucket.Previous;
                await StateManager.SetStateAsync(nextBucketName, nextBucket, cancellationToken);
            }

            if (manifest.Head == bucketIndex)
            {
                manifest.Head = bucket.Next;
            }

            if (manifest.Tail == bucketIndex)
            {
                manifest.Tail = bucket.Previous;
            }

            return true;
        }

        /// <inheritdoc />
        public IAsyncEnumerator<KeyValuePair<TKey, TValue>> GetAsyncEnumerator()
        {
            return new AsyncEnumerator(this);
        }

        protected static long NextIndex(HashManifest manifest)
        {
            return manifest.Next++;
        }

        protected string IndexToBucket(long bucket)
        {
            return string.Format(BucketIndexFormat, Name, bucket);
        }

        protected string IndexToKey(long bucket, long index)
        {
            return string.Format(KeyIndexFormat, Name, bucket, index);
        }

        protected string IndexToValue(long bucket, long index)
        {
            return string.Format(ValueIndexFormat, Name, bucket, index);
        }

        private class AsyncEnumerator : IAsyncEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly ActorDictionaryState<TKey, TValue> _source;
            private long _currentBucketIndex;
            private long? _nextBucketIndex;
            private long? _nextKeyIndex;

            internal AsyncEnumerator(ActorDictionaryState<TKey, TValue> source)
            {
                _source = source;
            }

            /// <inheritdoc />
            public void Dispose()
            {
            }

            /// <inheritdoc />
            public KeyValuePair<TKey, TValue> Current { get; private set; }

            /// <inheritdoc />
            public async Task<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                var stateManager = _source.StateManager;

                if (!_nextKeyIndex.HasValue)
                {
                    if (!_nextBucketIndex.HasValue)
                    {
                        var manifestResult = await stateManager.TryGetStateAsync<HashManifest>(_source.Name, cancellationToken);
                        if (!manifestResult.HasValue)
                        {
                            return false;
                        }

                        _nextBucketIndex = manifestResult.Value.Head;
                    }

                    HashBucket currentBucket;
                    for (; _nextBucketIndex.HasValue && !_nextKeyIndex.HasValue; _nextBucketIndex = currentBucket.Next)
                    {
                        _currentBucketIndex = _nextBucketIndex.Value;
                        var bucketName = _source.IndexToBucket(_currentBucketIndex);
                        currentBucket = await stateManager.GetStateAsync<HashBucket>(bucketName, cancellationToken);

                        _nextKeyIndex = currentBucket.Head;
                        _nextBucketIndex = currentBucket.Next;
                    }
                }

                if (_nextKeyIndex.HasValue)
                {
                    var keyName = _source.IndexToKey(_currentBucketIndex, _nextKeyIndex.Value);
                    var valueName = _source.IndexToValue(_currentBucketIndex, _nextKeyIndex.Value);
                    var keyNode = await stateManager.GetStateAsync<HashKeyNode<TKey>>(keyName, cancellationToken);
                    var value = await stateManager.GetStateAsync<TValue>(valueName, cancellationToken);

                    Current = new KeyValuePair<TKey, TValue>(keyNode.Key, value);

                    _nextKeyIndex = keyNode.Next;
                    return true;
                }

                return false;
            }

            /// <inheritdoc />
            public void Reset()
            {
                Current = new KeyValuePair<TKey, TValue>();
                _nextKeyIndex = null;
                _nextBucketIndex = null;
            }
        }
    }
}