using System;
using System.Collections.Generic;
using System.Linq;

namespace Saye.Contracts
{
    /// <summary>
    /// A condition that is satisfied or dissatisfied when asserted.
    /// </summary>
    public class Condition : ICondition
    {
        public static Condition When(Func<bool> assert, Action<EventHandler> bind, Action<EventHandler> unbind)
        {
            return new Condition(assert, bind, unbind);
        }

        public static Condition When(Func<bool> assert)
        {
            return When(assert, Unbound, Unbound);
        }

        private static void Unbound(EventHandler handler) { }

        public static Condition Any(IEnumerable<ICondition> conditions)
        {
            return Combine(() => conditions.Any(condition => condition.Satisfied), conditions);
        }

        public static Condition Any(params ICondition[] conditions)
        {
            return Any((IEnumerable<ICondition>)conditions);
        }

        public static Condition All(IEnumerable<ICondition> conditions)
        {
            return Combine(() => conditions.All(condition => condition.Satisfied), conditions);
        }

        public static Condition All(params ICondition[] conditions)
        {
            return All((IEnumerable<ICondition>)conditions);
        }

        private static Condition Combine(Func<bool> assert, IEnumerable<ICondition> conditions)
        {
            return new Condition(
                assert,
                (handler) =>
                {
                    foreach (var condition in conditions)
                    {
                        condition.OnSatisfied += handler;
                        condition.OnDissatisfied += handler;
                    }
                    foreach (var condition in conditions)
                    {
                        condition.Bind();
                    }
                },
                (handler) =>
                {
                    foreach (var condition in conditions)
                    {
                        condition.OnSatisfied -= handler;
                        condition.OnDissatisfied -= handler;
                    }
                    foreach (var condition in conditions)
                    {
                        condition.Unbind();
                    }
                }
            );
        }

        public static readonly Condition Never = When(() => false);

        public static readonly Condition Always = When(() => true);

        private readonly Func<bool> assert;
        private readonly Action<EventHandler> bind;
        private readonly Action<EventHandler> unbind;

        public event EventHandler OnSatisfied;
        public event EventHandler OnDissatisfied;

        private bool satisfied;
        public bool Satisfied => satisfied;

        private Condition(Func<bool> assert, Action<EventHandler> bind, Action<EventHandler> unbind)
        {
            this.assert = assert;
            this.bind = bind;
            this.unbind = unbind;
            satisfied = assert();
        }

        public void Assert()
        {
            satisfied = assert();
            Invoke();
        }

        public void AssertChanged()
        {
            var satisfied = assert();
            if (this.satisfied != satisfied)
            {
                this.satisfied = satisfied;
                Invoke();
            }
        }

        private void Invoke()
        {
            if (satisfied)
            {
                OnSatisfied?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                OnDissatisfied?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Bind()
        {
            bind(BoundEventHandler);
            Assert();
        }

        public void Unbind()
        {
            unbind(BoundEventHandler);
        }

        private void BoundEventHandler(object sender, EventArgs e)
        {
            AssertChanged();
        }
    }
}
