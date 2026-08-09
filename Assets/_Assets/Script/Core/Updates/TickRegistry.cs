using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LilyOfValley.Core.Updates
{
    public sealed class TickRegistry<T> where T : class
    {
        #region Field

        private const int DefaultCapacity = 64;

        private const int PendingCapacity = 16;

        private const int NoChunkLimit = 0;

        private const int MinChunkSize = 1;

        private const int FirstIndex = 0;

        private const int NotFound = -1;

        private const int EmptyCount = 0;

        private const float NoElapsedTime = 0f;

        private readonly Action<T, float> _invoke;

        private readonly List<T> _items;

        private readonly List<float> _elapsedTimes;

        private readonly HashSet<T> _itemSet;

        private readonly List<T> _pendingAdd = new(PendingCapacity);

        private readonly HashSet<T> _pendingRemove = new(PendingCapacity);

        private int _chunkSize = NoChunkLimit;

        private int _chunkStart;

        private bool _isTicking;

        #endregion

        #region Property

        public int Count => this._items.Count;

        public int ChunkSize
        {
            get => this._chunkSize;
            set => this._chunkSize = value < MinChunkSize ? NoChunkLimit : value;
        }

        #endregion

        #region Registration

        public TickRegistry(Action<T, float> invoke, int capacity = DefaultCapacity)
        {
            this._invoke = invoke ?? throw new ArgumentNullException(nameof(invoke));
            this._items = new List<T>(capacity);
            this._elapsedTimes = new List<float>(capacity);
            this._itemSet = new HashSet<T>(capacity);
        }

        public bool Register(T item)
        {
            if (item == null) return false;
            if (!this._isTicking) return RegisterImmediate(item);
            if (this._itemSet.Contains(item)) return this._pendingRemove.Remove(item);
            if (this._pendingAdd.Contains(item)) return false;

            this._pendingAdd.Add(item);

            return true;
        }

        public bool Unregister(T item)
        {
            if (item == null) return false;
            if (!this._isTicking) return UnregisterImmediate(item);
            if (this._pendingAdd.Remove(item)) return true;
            if (!this._itemSet.Contains(item)) return false;

            return this._pendingRemove.Add(item);
        }

        public void Clear()
        {
            this._items.Clear();
            this._elapsedTimes.Clear();
            this._itemSet.Clear();
            this._pendingAdd.Clear();
            this._pendingRemove.Clear();
            this._chunkStart = FirstIndex;
            this._isTicking = false;
        }

        #endregion

        #region Ticking

        public void Tick(float deltaTime)
        {
            this._isTicking = true;

            try
            {
                FlushPending();
                Accumulate(deltaTime);
                Invoke();
                FlushPending();
            }
            finally
            {
                this._isTicking = false;
            }
        }

        private void Accumulate(float deltaTime)
        {
            for (var i = 0; i < this._elapsedTimes.Count; i++)
            {
                this._elapsedTimes[i] += deltaTime;
            }
        }

        private void Invoke()
        {
            var count = this._items.Count;
            if (count == EmptyCount) return;

            if (this._chunkStart >= count) this._chunkStart = FirstIndex;

            var endIndex = this._chunkSize == NoChunkLimit ? count : Math.Min(this._chunkStart + this._chunkSize, count);

            for (var i = this._chunkStart; i < endIndex && i < this._items.Count; i++)
            {
                var item = this._items[i];

                if (IsDestroyed(item))
                {
                    this._pendingRemove.Add(item);
                    continue;
                }

                if (IsPendingRemoval(item)) continue;

                var elapsed = this._elapsedTimes[i];
                this._elapsedTimes[i] = NoElapsedTime;

                try
                {
                    this._invoke(item, elapsed);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    this._pendingRemove.Add(item);
                }
            }

            this._chunkStart = endIndex >= count ? FirstIndex : endIndex;
        }

        #endregion

        #region Method

        private bool RegisterImmediate(T item)
        {
            if (!this._itemSet.Add(item)) return false;

            this._items.Add(item);
            this._elapsedTimes.Add(NoElapsedTime);

            return true;
        }

        private bool UnregisterImmediate(T item)
        {
            if (!this._itemSet.Remove(item)) return false;

            var index = this._items.IndexOf(item);
            if (index == NotFound) return false;

            this._items.RemoveAt(index);
            this._elapsedTimes.RemoveAt(index);

            if (index < this._chunkStart) this._chunkStart--;

            return true;
        }

        private void FlushPending()
        {
            for (var i = 0; i < this._pendingAdd.Count; i++)
            {
                RegisterImmediate(this._pendingAdd[i]);
            }

            this._pendingAdd.Clear();

            if (this._pendingRemove.Count == EmptyCount) return;

            foreach (var item in this._pendingRemove)
            {
                UnregisterImmediate(item);
            }

            this._pendingRemove.Clear();
        }

        private bool IsPendingRemoval(T item)
        {
            if (this._pendingRemove.Count == EmptyCount) return false;

            return this._pendingRemove.Contains(item);
        }

        private static bool IsDestroyed(T item)
        {
            if (item == null) return true;

            return item is Object unityObject && unityObject == null;
        }

        #endregion
    }
}
