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
        /// Create a condition based on a given assertion, with no binding (requires manual assertion).
        /// </summary>
        public static ICondition When(Func<bool> assert)
        {
            return When(assert, Unbound, Unbound);
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
                    foreach (var subcondition in subconditions)
                    {
                        subcondition.Bind();
                    }
                },
                unbind: condition =>
                {
                    foreach (var subcondition in subconditions)
                    {
                        subcondition.OnSatisfied -= condition.Assert;
                        subcondition.OnDissatisfied -= condition.Assert;
                    }
                    foreach (var subcondition in subconditions)
                    {
                        subcondition.Unbind();
                    }
                }
            );
        }

        /// <summary>
        /// A condition which is never satisfied.
        /// </summary>
        public static readonly ICondition Never = When(() => false);

        /// <summary>
        /// A condition which is always satisfied.
        /// </summary>
        public static readonly ICondition Always = When(() => true);

        private static void Unbound(ICondition _) { }

        private readonly Func<bool> assert;
        private readonly Action<ICondition> bind;
        private readonly Action<ICondition> unbind;

        public event EventHandler<ConditionStatusEventArgs> OnSatisfied;

        public event EventHandler<ConditionStatusEventArgs> OnDissatisfied;

        public bool Satisfied { get; private set; }

        private Condition(Func<bool> assert, Action<ICondition> bind, Action<ICondition> unbind)
        {
            this.assert = assert;
            this.bind = bind;
            this.unbind = unbind;
            Satisfied = assert();
        }

        public void Assert()
        {
            var satisfied = assert();
            if (Satisfied != satisfied)
            {
                Satisfied = satisfied;
                Invoke();
            }
        }

        public void Assert(object sender, EventArgs e)
        {
            Assert();
        }

        public void Bind()
        {
            bind(this);
            Satisfied = assert();
            Invoke();
        }

        public void Unbind()
        {
            unbind(this);
        }

        private void Invoke()
        {
            var onStatus = Satisfied ? OnSatisfied : OnDissatisfied;
            onStatus?.Invoke(this, new ConditionStatusEventArgs(Satisfied));
        }

        public void Dispose()
        {
            Unbind();
        }
    }
}
