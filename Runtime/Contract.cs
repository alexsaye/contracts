using System;

namespace Saye.Contracts
{
    /// <summary>
    /// A promise-like contract that is resolved or rejected based on conditions.
    /// </summary>
    public class Contract : IContract, IDisposable
    {
        private ICondition resolving;
        public IReadOnlyCondition Resolving => resolving;

        private ICondition rejecting;
        public IReadOnlyCondition Rejecting => rejecting;

        public ContractStatus Status { get; private set; } = ContractStatus.Pending;

        public event EventHandler<ContractStatusEventArgs> OnResolved;

        public event EventHandler<ContractStatusEventArgs> OnRejected;

        public Contract(ICondition resolving) : this(resolving, Condition.Never) { }

        public Contract(ICondition resolving, ICondition rejecting)
        {
            this.resolving = resolving;
            this.rejecting = rejecting;
        }

        public void Bind()
        {
            if (Status != ContractStatus.Pending)
            {
                return;
            }

            resolving.Bind();
            rejecting.Bind();

            resolving.OnSatisfied += Resolve;
            rejecting.OnSatisfied += Reject;

            if (rejecting.Satisfied)
            {
                Reject();
            }
            else if (resolving.Satisfied)
            {
                Resolve();
            }
        }

        public void Unbind()
        {
            resolving.OnSatisfied -= Resolve;
            rejecting.OnSatisfied -= Reject;

            resolving.Unbind();
            rejecting.Unbind();
        }

        private void Resolve()
        {
            Status = ContractStatus.Resolved;
            Unbind();
            OnResolved?.Invoke(this, new ContractStatusEventArgs(Status));
        }

        private void Resolve(object sender, EventArgs e)
        {
            Resolve();
        }

        private void Reject()
        {
            Status = ContractStatus.Rejected;
            Unbind();
            OnRejected?.Invoke(this, new ContractStatusEventArgs(Status));
        }

        private void Reject(object sender, EventArgs e)
        {
            Reject();
        }

        public void Dispose()
        {
            Unbind();
        }
    }
}