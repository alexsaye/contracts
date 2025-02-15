using System;
using System.Collections.Generic;
using System.Linq;

namespace Saye.Contracts
{
    /// <summary>
    /// A condition that can be either satisfied or dissatisfied.
    /// </summary>
    public class Condition : Observable<bool>, ICondition
    {
        /// <summary>
        /// Create a condition based on a given assertion, with explicitly managed binding.
        /// </summary>
        /// <returns></returns>
        public static ICondition When(Func<bool> assert, Action<ICondition> bind, Action<ICondition> unbind)
        {
            return new Condition(assert, bind, unbind);
        }

        /// <summary>
        /// Create a composite condition based on subconditions, which is satisfied when any are satisfied and dissatisfied when all are dissatisfied.
        /// </summary>
        public static ICondition Any(IEnumerable<ICondition> subconditions)
        {
            return Composite(() => subconditions.Any(subcondition => subcondition.CurrentState), subconditions);
        }

        /// <summary>
        /// Create a composite condition based on subconditions, which is satisfied when any are satisfied and dissatisfied when all are dissatisfied.
        /// </summary>
        public static ICondition Any(params ICondition[] subconditions)
        {
            return Any((IEnumerable<ICondition>)subconditions);
        }

        /// <summary>
        /// Create a composite condition based on subconditions, which is satisfied when all are satisfied and dissatisfied when any are dissatisfied.
        /// </summary>
        public static ICondition All(IEnumerable<ICondition> subconditions)
        {
            return Composite(() => subconditions.All(subcondition => subcondition.CurrentState), subconditions);
        }

        /// <summary>
        /// Create a composite condition based on subconditions, which is satisfied when all are satisfied and dissatisfied when any are dissatisfied.
        /// </summary>
        public static ICondition All(params ICondition[] subconditions)
        {
            return All((IEnumerable<ICondition>)subconditions);
        }

        /// <summary>
        /// Create a composite condition based on subconditions.
        /// </summary>
        public static ICondition Composite(Func<bool> assert, IEnumerable<ICondition> subconditions)
        {
            return new Condition(
                assert,
                bind: condition =>
                {
                    foreach (var subcondition in subconditions)
                    {
                        subcondition.State += condition.Update;
                    }
                },
                unbind: condition =>
                {
                    foreach (var subcondition in subconditions)
                    {
                        subcondition.State -= condition.Update;
                    }
                }
            );
        }

        /// <summary>
        /// A condition which is never satisfied.
        /// </summary>
        public static readonly ICondition Never = When(() => false, Unbound, Unbound);

        /// <summary>
        /// A condition which is always satisfied.
        /// </summary>
        public static readonly ICondition Always = When(() => true, Unbound, Unbound);

        private static void Unbound(ICondition _) { }

        private readonly Func<bool> assert;
        private readonly Action<ICondition> bind;
        private readonly Action<ICondition> unbind;

        private Condition(Func<bool> assert, Action<ICondition> bind, Action<ICondition> unbind)
        {
            this.assert = assert;
            this.bind = bind;
            this.unbind = unbind;
            Observed += HandleObserved;

            // Determine the initial state.
            Update();
        }

        private void HandleObserved(object sender, ObservableObservedEventArgs e)
        {
            if (e.IsObserved)
            {
                // Bind to allow automatic updates while observed.
                bind(this);

                // Update in case the externally asserted state has changed since the condition was last observed.
                Update();
            }
            else
            {
                // Unbind to prevent unnecessary automatic updates while not observed.
                unbind(this);
            }
        }

        public void Update()
        {
            CurrentState = assert();
        }

        public void Update(object sender, EventArgs e)
        {
            Update();
        }
    }

    /// <summary>
    /// Represents a condition that can be either satisfied or dissatisfied.
    /// </summary>
    public interface ICondition : IObservable<bool>
    {
        /// <summary>
        /// Updates the condition's state.
        /// </summary>
        void Update();

        /// <summary>
        /// Updates the condition's state (for use as an event handler).
        /// </summary>
        void Update(object sender, EventArgs e);
    }
}