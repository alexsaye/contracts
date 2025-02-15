using System;
using System.Collections.Generic;

namespace Saye.Contracts
{
    /// <summary>
    /// Provides watchable state with events which immediately push on subscription.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    public class Watchable<T> : ReadOnlyWatchable<T>, IWatchable<T>
    {
        public new T CurrentState
        {
            get => base.CurrentState;
            set => base.CurrentState = value;
        }

        public Watchable(T initialState) : base(initialState) { }

        public Watchable() : base() { }
    }

    /// <summary>
    /// Provides read-only watchable state with events which immediately push on subscription.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    public class ReadOnlyWatchable<T> : IReadOnlyWatchable<T>
    {
        private T currentState;
        public T CurrentState
        {
            get => currentState;
            protected set
            {
                // If the state has changed, push the state.
                if (!currentState.Equals(value))
                {
                    currentState = value;
                    state?.Invoke(this, new StateEventArgs<T>(currentState));
                }
            }
        }

        private event EventHandler<StateEventArgs<T>> state;
        public event EventHandler<StateEventArgs<T>> State
        {
            add
            {
                // Only allow unique subscriptions.
                if (!watchers.Contains(value))
                {
                    // Add and subscribe the watcher.
                    watchers.Add(value);
                    state += value;

                    // If this is the first watcher, push an watched event.
                    if (watchers.Count == 1)
                    {
                        watched?.Invoke(this, new WatchedEventArgs(true));

                        // Check that the watcher didn't stop watching due to the above event.
                        if (!watchers.Contains(value))
                        {
                            return;
                        }
                    }

                    // Push the state to the watcher.
                    value(this, new StateEventArgs<T>(CurrentState));
                }
            }
            remove
            {
                // There is no need to remove a subscriber that is not watching.
                if (watchers.Contains(value))
                {
                    // Remove and unsubscribe the watcher.
                    watchers.Remove(value);
                    state -= value;

                    // If this is the last watcher, push a not watched event.
                    if (watchers.Count == 0)
                    {
                        watched?.Invoke(this, new WatchedEventArgs(false));
                    }
                }
            }
        }

        private HashSet<EventHandler<StateEventArgs<T>>> watchers = new();
        public bool IsWatched => watchers.Count > 0;

        private event EventHandler<WatchedEventArgs> watched;
        public event EventHandler<WatchedEventArgs> Watched
        {
            add
            {
                watched += value;

                // Push whether the state is being watched to the subscriber.
                value(this, new WatchedEventArgs(IsWatched));
            }
            remove
            {
                watched -= value;
            }
        }

        public ReadOnlyWatchable(T initialState)
        {
            currentState = initialState;
        }

        public ReadOnlyWatchable() : this(default) { }
    }

    /// <summary>
    /// Provides watchable state.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    public interface IWatchable<T> : IReadOnlyWatchable<T>
    {
        new T CurrentState { get; set; }
    }

    /// <summary>
    /// Provides read-only watchable state.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    public interface IReadOnlyWatchable<T>
    {
        /// <summary>
        /// The current state.
        /// </summary>
        T CurrentState { get; }

        /// <summary>
        /// Raised to push the state.
        /// </summary>
        event EventHandler<StateEventArgs<T>> State;

        /// <summary>
        /// Whether the watchable state is being watched.
        /// </summary>
        bool IsWatched { get; }

        /// <summary>
        /// Raised to push whether the watchable state is being watched.
        /// </summary>
        event EventHandler<WatchedEventArgs> Watched;
    }

    /// <summary>
    /// Raised to push whether a watchable state is being watched.
    /// </summary>
    public class WatchedEventArgs : EventArgs
    {
        public bool IsWatched { get; }

        public WatchedEventArgs(bool watched)
        {
            IsWatched = watched;
        }
    }

    /// <summary>
    /// Raised to push the current state of a watchable.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    public class StateEventArgs<T> : EventArgs
    {
        public T CurrentState { get; }

        public StateEventArgs(T currentState)
        {
            CurrentState = currentState;
        }
    }
}
