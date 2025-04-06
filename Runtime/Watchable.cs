using System;
using System.Collections.Generic;

namespace Contracts
{
    /// <summary>
    /// Provides watchable state with events which immediately push on subscription.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    public class Watchable<T> : ReadOnlyWatchable<T>, IWatchable<T>
    {
        public new T State
        {
            get => base.State;
            set => base.State = value;
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
        private T state;
        public T State
        {
            get => state;
            protected set
            {
                // If the state has changed, push the state.
                if (state == null && value != null || !state.Equals(value))
                {
                    state = value;
                    stateUpdated?.Invoke(this, new StateUpdatedEventArgs<T>(state));
                }
            }
        }

        private event EventHandler<StateUpdatedEventArgs<T>> stateUpdated;
        public event EventHandler<StateUpdatedEventArgs<T>> StateUpdated
        {
            add
            {
                // Only allow unique subscriptions.
                if (!watchers.Contains(value))
                {
                    // Add and subscribe the watcher.
                    watchers.Add(value);
                    stateUpdated += value;

                    // If this is the first watcher, push an watched event.
                    if (watchers.Count == 1)
                    {
                        watchedUpdated?.Invoke(this, new WatchedUpdatedEventArgs(true));

                        // Check that the watcher didn't stop watching due to the above event.
                        if (!watchers.Contains(value))
                        {
                            return;
                        }
                    }

                    // Push the state to the watcher.
                    value(this, new StateUpdatedEventArgs<T>(State));
                }
            }
            remove
            {
                // There is no need to remove a subscriber that is not watching.
                if (watchers.Contains(value))
                {
                    // Remove and unsubscribe the watcher.
                    watchers.Remove(value);
                    stateUpdated -= value;

                    // If this is the last watcher, push a not watched event.
                    if (watchers.Count == 0)
                    {
                        watchedUpdated?.Invoke(this, new WatchedUpdatedEventArgs(false));
                    }
                }
            }
        }

        private HashSet<EventHandler<StateUpdatedEventArgs<T>>> watchers = new();
        public bool Watched => watchers.Count > 0;

        private event EventHandler<WatchedUpdatedEventArgs> watchedUpdated;
        public event EventHandler<WatchedUpdatedEventArgs> WatchedUpdated
        {
            add
            {
                watchedUpdated += value;

                // Push whether the state is being watched to the subscriber.
                value(this, new WatchedUpdatedEventArgs(Watched));
            }
            remove
            {
                watchedUpdated -= value;
            }
        }

        public ReadOnlyWatchable(T initialState)
        {
            state = initialState;
        }

        public ReadOnlyWatchable() : this(default) { }
    }

    /// <summary>
    /// Provides watchable state.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    public interface IWatchable<T> : IReadOnlyWatchable<T>
    {
        new T State { get; set; }
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
        T State { get; }

        /// <summary>
        /// Raised to push the state.
        /// </summary>
        event EventHandler<StateUpdatedEventArgs<T>> StateUpdated;

        /// <summary>
        /// Whether the watchable state is being watched.
        /// </summary>
        bool Watched { get; }

        /// <summary>
        /// Raised to push whether the watchable state is being watched.
        /// </summary>
        event EventHandler<WatchedUpdatedEventArgs> WatchedUpdated;
    }

    /// <summary>
    /// Raised to push whether a watchable state is being watched.
    /// </summary>
    public class WatchedUpdatedEventArgs : EventArgs
    {
        public bool Watched { get; }

        public WatchedUpdatedEventArgs(bool watched)
        {
            Watched = watched;
        }
    }

    /// <summary>
    /// Raised to push the current state of a watchable.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    public class StateUpdatedEventArgs<T> : EventArgs
    {
        public T State { get; }

        public StateUpdatedEventArgs(T state)
        {
            State = state;
        }
    }
}
