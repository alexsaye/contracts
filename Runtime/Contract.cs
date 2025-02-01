using System;

namespace Saye.Contracts
{
    /// <summary>
    /// A promise-like contract that responds to obligation and violation conditions.
    /// </summary>
    public class Contract : IContract
    {
        private ICondition obligation;
        public ICondition Obligation => obligation;

        private ICondition violation;
        public ICondition Violation => violation;

        public ContractStatus Status { get; private set; }

        public event EventHandler OnFulfilled;

        public event EventHandler OnBreached;

        public Contract(ICondition obligation, ICondition violation)
        {
            this.obligation = obligation;
            this.violation = violation;
        }

        public Contract(ICondition obligation) : this(obligation, Condition.Never) { }

        public Contract(Contract contract) : this(contract.obligation, contract.violation) { }

        public void Bind()
        {
            if (Status != ContractStatus.Pending)
            {
                return;
            }

            obligation.Bind();
            violation.Bind();

            obligation.OnSatisfied += ObligationHandler;
            violation.OnSatisfied += ViolationHandler;

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
            obligation.OnSatisfied -= ObligationHandler;
            violation.OnSatisfied -= ViolationHandler;

            obligation.Unbind();
            violation.Unbind();
        }

        private void ObligationHandler(object sender, EventArgs e)
        {
            Fulfill();
        }

        private void ViolationHandler(object sender, EventArgs e)
        {
            Breach();
        }

        private void Fulfill()
        {
            Status = ContractStatus.Fulfilled;
            Unbind();
            OnFulfilled?.Invoke(this, EventArgs.Empty);
        }

        private void Breach()
        {
            Status = ContractStatus.Breached;
            Unbind();
            OnBreached?.Invoke(this, EventArgs.Empty);
        }
    }
}
