using System;

namespace Saye.Contracts.Tests
{
    public class MockSubject
    {
        public bool Satisfied { get; private set; }

        public event EventHandler Event;

        public MockSubject(bool satisfied)
        {
            Satisfied = satisfied;
        }

        public void Satisfy()
        {
            Satisfied = true;
            Event?.Invoke(this, EventArgs.Empty);
        }

        public void Dissatisfy()
        {
            Satisfied = false;
            Event?.Invoke(this, EventArgs.Empty);
        }

        public ICondition AsCondition()
        {
            return Condition.When(
                assert: () => Satisfied,
                bind: condition => Event += condition.Update,
                unbind: condition => Event -= condition.Update);
        }
    }
}
