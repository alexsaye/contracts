using System;
using System.Collections.Generic;

namespace Saye.Contracts
{
    /// <summary>
    /// Provides noticeable state with events which immediately push on subscription.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    public class Notice<T> : ReadOnlyNotice<T>, INotice<T>
    {
        public new T CurrentState
        {
            get => base.CurrentState;
            set => base.CurrentState = value;
        }

        public Notice(T initialState) : base(initialState) { }

        public Notice() : base() { }
    }

    /// <summary>
    /// Provides read-only noticeable state with events which immediately push on subscription.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    public class ReadOnlyNotice<T> : IReadOnlyNotice<T>
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
                if (!noticers.Contains(value))
                {
                    // Add and subscribe the noticer.
                    noticers.Add(value);
                    state += value;

                    // If this is the first noticer, push an noticed event.
                    if (noticers.Count == 1)
                    {
                        noticed?.Invoke(this, new NoticedEventArgs(true));

                        // Check that the noticer didn't stop noticing due to the above event.
                        if (!noticers.Contains(value))
                        {
                            return;
                        }
                    }

                    // Push the state to the noticer.
                    value(this, new StateEventArgs<T>(CurrentState));
                }
            }
            remove
            {
                // There is no need to remove a subscriber that is not noticing.
                if (noticers.Contains(value))
                {
                    // Remove and unsubscribe the noticer.
                    noticers.Remove(value);
                    state -= value;

                    // If this is the last noticer, push a not noticed event.
                    if (noticers.Count == 0)
                    {
                        noticed?.Invoke(this, new NoticedEventArgs(false));
                    }
                }
            }
        }

        private HashSet<EventHandler<StateEventArgs<T>>> noticers = new();
        public bool IsNoticed => noticers.Count > 0;

        private event EventHandler<NoticedEventArgs> noticed;
        public event EventHandler<NoticedEventArgs> Noticed
        {
            add
            {
                noticed += value;

                // Push whether the state is being noticed to the subscriber.
                value(this, new NoticedEventArgs(IsNoticed));
            }
            remove
            {
                noticed -= value;
            }
        }

        public ReadOnlyNotice(T initialState)
        {
            currentState = initialState;
        }

        public ReadOnlyNotice() : this(default) { }
    }

    /// <summary>
    /// Provides noticeable state.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    public interface INotice<T> : IReadOnlyNotice<T>
    {
        new T CurrentState { get; set; }
    }

    /// <summary>
    /// Provides read-only noticeable state.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    public interface IReadOnlyNotice<T>
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
        /// Whether the state is being noticed.
        /// </summary>
        bool IsNoticed { get; }

        /// <summary>
        /// Raised to push whether the state is being noticed.
        /// </summary>
        event EventHandler<NoticedEventArgs> Noticed;
    }

    /// <summary>
    /// Raised to push whether the state of a notice is being noticed.
    /// </summary>
    public class NoticedEventArgs : EventArgs
    {
        public bool IsNoticed { get; }

        public NoticedEventArgs(bool noticed)
        {
            IsNoticed = noticed;
        }
    }

    /// <summary>
    /// Raised to push the current state of a notice.
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
