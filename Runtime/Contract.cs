using System;

namespace Saye.Contracts
{
    /// <summary>
    /// A promise-like contract that responds to obligation and violation conditions.
    /// </summary>
    public class Contract : IContract, IDisposable
    {
        /// <summary>
        /// Create a contract based on obligation and violation conditions.
        /// </summary>
        public static IContract Observe(ICondition obligation, ICondition violation)
        {
            return new Contract(obligation, violation);
        }

        /// <summary>
        /// Create a contract based on an obligation condition with no violation condition.
        /// </summary>
        public static IContract Observe(ICondition obligation)
        {
            return Observe(obligation, Condition.Never);
        }

        private ICondition obligation;
        public IReadOnlyCondition Obligation => obligation;

        private ICondition violation;
        public IReadOnlyCondition Violation => violation;

        public ContractStatus Status { get; private set; } = ContractStatus.Pending;

        public event EventHandler<ContractStatusEventArgs> OnFulfilled;

        public event EventHandler<ContractStatusEventArgs> OnBreached;

        private Contract(ICondition obligation, ICondition violation)
        {
            this.obligation = obligation;
            this.violation = violation;
        }

        public void Bind()
        {
            if (Status != ContractStatus.Pending)
            {
                return;
            }

            obligation.Bind();
            violation.Bind();

            obligation.OnSatisfied += Fulfill;
            violation.OnSatisfied += Breach;

            if (violation.Satisfied)
            {
                Breach();
            }
            else if (obligation.Satisfied)
            {
                Fulfill();
            }
        }

        public void Unbind()
        {
            obligation.OnSatisfied -= Fulfill;
            violation.OnSatisfied -= Breach;

            obligation.Unbind();
            violation.Unbind();
        }

        private void Fulfill()
        {
            Status = ContractStatus.Fulfilled;
            Unbind();
            OnFulfilled?.Invoke(this, new ContractStatusEventArgs(Status));
        }

        private void Fulfill(object sender, EventArgs e)
        {
            Fulfill();
        }

        private void Breach()
        {
            Status = ContractStatus.Breached;
            Unbind();
            OnBreached?.Invoke(this, new ContractStatusEventArgs(Status));
        }

        private void Breach(object sender, EventArgs e)
        {
            Breach();
        }

        public void Dispose()
        {
            Unbind();
        }
    }
}