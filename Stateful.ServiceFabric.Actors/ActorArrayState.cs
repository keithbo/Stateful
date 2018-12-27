namespace Stateful.ServiceFabric
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class ActorArrayState<T> : IArrayState<T>
    {
        private const string IndexKeyFormat = "{0}:{1:X}";

        private readonly IActorStateManager _stateManager;
        private bool _isLengthValidated;

        public string Name { get; }

        public long Length { get; }

        public ActorArrayState(IActorStateManager stateManager, string name, long length)
        {
            if (length < 1L)
            {
                throw new ArgumentException("Length must be non-zero", nameof(length));
            }

            _stateManager = stateManager;
            Name = name;
            Length = length;
        }

        /// <inheritdoc />
        public Task<bool> HasStateAsync(CancellationToken cancellationToken)
        {
            return _stateManager.ContainsStateAsync(Name, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteStateAsync(CancellationToken cancellationToken)
        {
            var actualLength = await _stateManager.TryGetStateAsync<long>(Name, cancellationToken);
            if (!actualLength.HasValue)
            {
                return;
            }

            await _stateManager.RemoveStateAsync(Name, cancellationToken);

            for (var i = 0L; i < actualLength.Value; i++)
            {
                await _stateManager.TryRemoveStateAsync(string.Format(IndexKeyFormat, Name, i), cancellationToken);
            }

            _isLengthValidated = false;
        }

        /// <inheritdoc />
        public async Task<long> CountAsync(CancellationToken cancellationToken)
        {
            await ValidateLengthAsync(cancellationToken);
            return Length;
        }

        /// <inheritdoc />
        public async Task<bool> ContainsAsync(Predicate<T> predicate, CancellationToken cancellationToken)
        {
            await ValidateLengthAsync(cancellationToken);

            var found = false;
            for (var i = 0L; !found && i < Length; i++)
            {
                var current = await _stateManager.TryGetStateAsync<T>(string.Format(IndexKeyFormat, Name, i), cancellationToken);
                if (current.HasValue)
                {
                    found = predicate(current.Value);
                }
            }

            return found;
        }

        /// <inheritdoc />
        public async Task<T> GetAsync(long index, CancellationToken cancellationToken)
        {
            await ValidateLengthAsync(cancellationToken);
            if (index < 0 || index >= Length)
            {
                throw new IndexOutOfRangeException($"Index {index} must be in range [0, {Length})");
            }

            var value = await _stateManager.TryGetStateAsync<T>(string.Format(IndexKeyFormat, Name, index), cancellationToken);
            return value.HasValue ? value.Value : default(T);
        }

        /// <inheritdoc />
        public async Task SetAsync(long index, T value, CancellationToken cancellationToken)
        {
            await ValidateLengthAsync(cancellationToken);
            if (index < 0 || index >= Length)
            {
                throw new IndexOutOfRangeException($"Index {index} must be in range [0, {Length})");
            }

            await _stateManager.AddOrUpdateStateAsync(string.Format(IndexKeyFormat, Name, index), value, (key, oldValue) => value, cancellationToken);
        }

        /// <inheritdoc />
        public IAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return new AsyncEnumerator(this);
        }

        private async Task ValidateLengthAsync(CancellationToken cancellationToken)
        {
            if (_isLengthValidated) return;
            var actualLength = await _stateManager.TryGetStateAsync<long>(Name, cancellationToken);
            if (actualLength.HasValue && actualLength.Value != Length)
            {
                throw new InvalidOperationException("Provided Length for ArrayState does not match Length saved into actual state");
            }

            _isLengthValidated = true;
        }

        public class AsyncEnumerator : IAsyncEnumerator<T>
        {
            private readonly ActorArrayState<T> _array;
            private long _index;

            internal AsyncEnumerator(ActorArrayState<T> array)
            {
                _array = array;
                Current = default(T);
            }

            /// <inheritdoc />
            public void Dispose()
            {
            }

            /// <inheritdoc />
            public T Current { get; private set; }

            /// <inheritdoc />
            public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
            {
                await _array.ValidateLengthAsync(cancellationToken);
                var stateManager = _array._stateManager;
                if (_index >= _array.Length)
                {
                    return false;
                }

                var valueResult = await stateManager.TryGetStateAsync<T>(string.Format(IndexKeyFormat, _array.Name, _index), cancellationToken);
                Current = valueResult.Value;
                _index++;
                return true;
            }

            /// <inheritdoc />
            public void Reset()
            {
                _index = 0;
                Current = default(T);
            }
        }
    }
}