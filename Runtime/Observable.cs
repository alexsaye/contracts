using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Saye.Contracts
{
    /// <summary>
    /// Provides observable state with events which immediately push on subscription.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    public class Observable<T> : IObservable<T>, IDisposable
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
                    state?.Invoke(this, new ObservableStateEventArgs<T>(currentState));
                }
            }
        }

        private event EventHandler<ObservableStateEventArgs<T>> state;
        public event EventHandler<ObservableStateEventArgs<T>> State
        {
            add
            {
                // Only allow unique observers.
                if (!observers.Contains(value))
                {
                    // Add and subscribe the observer.
                    observers.Add(value);
                    state += value;

                    // If this is the first observer, push an observed event.
                    if (observers.Count == 1)
                    {
                        observed?.Invoke(this, new ObservableObservedEventArgs(true));

                        // Check that the observer didn't stop observing due to the above event.
                        if (observers.Contains(value))
                        {
                            return;
                        }
                    }

                    // Push the state to the observer.
                    value(this, new ObservableStateEventArgs<T>(CurrentState));
                }
            }
            remove
            {
                // There is no need to remove a subscriber that is not observing.
                if (observers.Contains(value))
                {
                    // Remove and unsubscribe the observer.
                    observers.Remove(value);
                    state -= value;

                    // If this is the last observer, push a not observed event.
                    if (observers.Count == 0)
                    {
                        observed?.Invoke(this, new ObservableObservedEventArgs(false));
                    }
                }
            }
        }

        private HashSet<EventHandler<ObservableStateEventArgs<T>>> observers = new();
        public bool IsObserved => observers.Count > 0;

        private event EventHandler<ObservableObservedEventArgs> observed;
        public event EventHandler<ObservableObservedEventArgs> Observed
        {
            add
            {
                observed += value;

                // Push whether the state is being observed to the subscriber.
                value(this, new ObservableObservedEventArgs(IsObserved));
            }
            remove
            {
                observed -= value;
            }
        }

        public Observable(T initialState)
        {
            CurrentState = initialState;
        }

        public Observable() : this(default) { }

        public void Dispose()
        {
            foreach (var observer in observers)
            {
                State -= observer;
            }
        }
    }

    /// <summary>
    /// Provides observable state.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    public interface IObservable<T>
    {
        /// <summary>
        /// The current state.
        /// </summary>
        T CurrentState { get; }

        /// <summary>
        /// Raised to push the state.
        /// </summary>
        event EventHandler<ObservableStateEventArgs<T>> State;

        /// <summary>
        /// Whether the state is being observed.
        /// </summary>
        bool IsObserved { get; }

        /// <summary>
        /// Raised to push whether the state is being observed.
        /// </summary>
        event EventHandler<ObservableObservedEventArgs> Observed;
    }

    /// <summary>
    /// Raised to push whether the state of an observable is being observed.
    /// </summary>
    public class ObservableObservedEventArgs : EventArgs
    {
        public bool IsObserved { get; }

        public ObservableObservedEventArgs(bool observed)
        {
            IsObserved = observed;
        }
    }

    /// <summary>
    /// Raised to push the current state of an observable.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    public class ObservableStateEventArgs<T> : EventArgs
    {
        public T State { get; }

        public ObservableStateEventArgs(T state)
        {
            State = state;
        }
    }
}
