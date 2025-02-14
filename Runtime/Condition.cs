using System;
using System.Collections.Generic;
using System.Linq;

namespace Saye.Contracts
{
    /// <summary>
    /// A condition that is satisfied or dissatisfied when asserted.
    /// </summary>
    public class Condition : ICondition, IDisposable
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
            return Composite(() => subconditions.Any(subcondition => subcondition.Satisfied), subconditions);
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
            return Composite(() => subconditions.All(subcondition => subcondition.Satisfied), subconditions);
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
                        subcondition.OnSatisfied += condition.Assert;
                        subcondition.OnDissatisfied += condition.Assert;
                    }
                },
                unbind: condition =>
                {
                    foreach (var subcondition in subconditions)
                    {
                        subcondition.OnSatisfied -= condition.Assert;
                        subcondition.OnDissatisfied -= condition.Assert;
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

        private event EventHandler<ConditionEventArgs> onSatisfied;
        public event EventHandler<ConditionEventArgs> OnSatisfied
        {
            add => Subscribe(ref onSatisfied, value, true);
            remove => Unsubscribe(ref onSatisfied, value);
        }

        private event EventHandler<ConditionEventArgs> onDissatisfied;
        public event EventHandler<ConditionEventArgs> OnDissatisfied
        {
            add => Subscribe(ref onDissatisfied, value, false);
            remove => Unsubscribe(ref onDissatisfied, value);
        }

        public bool Satisfied { get; private set; }

        public bool Reacting { get; private set; }

        private readonly Func<bool> assert;
        private readonly Action<ICondition> bind;
        private readonly Action<ICondition> unbind;

        private Condition(Func<bool> assert, Action<ICondition> bind, Action<ICondition> unbind)
        {
            this.assert = assert;
            this.bind = bind;
            this.unbind = unbind;
            Satisfied = assert();
            Reacting = false;
        }

        private void Subscribe(ref EventHandler<ConditionEventArgs> onEvent, EventHandler<ConditionEventArgs> handler, bool expected)
        {
            // The condition now has a subscriber, so it needs to react to its binding.
            EnableReaction();

            // Subscribe for future assertions.
            onEvent += handler;

            if (Satisfied == expected)
            {

                // Immediately invoke the expected status.
                handler(this, new ConditionEventArgs(Satisfied));
            }
        }

        private void Unsubscribe(ref EventHandler<ConditionEventArgs> onEvent, EventHandler<ConditionEventArgs> handler)
        {
            onEvent -= handler;
            if (onSatisfied == null && onDissatisfied == null)
            {
                // The condition no longer has subscribers, so it is unnecessary for it to react to its binding.
                DisableReaction();
            }
        }

        private void EnableReaction()
        {
            if (!Reacting)
            {
                Reacting = true;
                bind(this);

                // Assert in case satisfaction has changed since the condition was last reacting.
                Assert();
            }
        }

        private void DisableReaction()
        {
            if (Reacting)
            {
                Reacting = false;
                unbind(this);
            }
        }

        public void Assert()
        {
            var satisfied = assert();
            if (Satisfied != satisfied)
            {
                Satisfied = satisfied;
                if (Satisfied)
                {
                    onSatisfied?.Invoke(this, new ConditionEventArgs(satisfied));
                }
                else
                {
                    onDissatisfied?.Invoke(this, new ConditionEventArgs(satisfied));
                }
            }
        }

        public void Assert(object sender, EventArgs e)
        {
            Assert();
        }

        public void Dispose()
        {
            DisableReaction();
        }
    }
}