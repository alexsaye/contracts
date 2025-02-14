using System;

namespace Saye.Contracts
{
    /// <summary>
    /// A promise-like contract that is resolved or rejected based on conditions.
    /// </summary>
    public class Contract : IContract, IDisposable
    {
        private event EventHandler<ContractEventArgs> onResolved;
        public event EventHandler<ContractEventArgs> OnResolved
        {
            add => Subscribe(ref onResolved, value, ContractStatus.Resolved);
            remove => Unsubscribe(ref onResolved, value);
        }

        private event EventHandler<ContractEventArgs> onRejected;
        public event EventHandler<ContractEventArgs> OnRejected
        {
            add => Subscribe(ref onRejected, value, ContractStatus.Rejected);
            remove => Unsubscribe(ref onRejected, value);
        }

        private readonly ICondition resolving;
        public ICondition Resolving => resolving;

        private readonly ICondition rejecting;
        public ICondition Rejecting => rejecting;

        public ContractStatus Status { get; private set; }

        public bool Reacting { get; private set; }

        public Contract(ICondition resolving) : this(resolving, Condition.Never) { }

        public Contract(ICondition resolving, ICondition rejecting)
        {
            this.resolving = resolving;
            this.rejecting = rejecting;
            Status = rejecting.Satisfied ? ContractStatus.Rejected : resolving.Satisfied ? ContractStatus.Resolved : ContractStatus.Pending;
            Reacting = false;
        }

        public void Subscribe(ref EventHandler<ContractEventArgs> onEvent, EventHandler<ContractEventArgs> handler, ContractStatus expected)
        {
            // The contract now has subscribers, so it needs to react to its conditions.
            EnableReaction();

            if (Status == ContractStatus.Pending)
            {
                // Subscribe for a future resolved/rejected status.
                onEvent += handler;
            }
            else if (Status == expected)
            {
                // Immediately invoke the expected status.
                handler(this, new ContractEventArgs(Status));
            }
        }

        public void Unsubscribe(ref EventHandler<ContractEventArgs> onEvent, EventHandler<ContractEventArgs> handler)
        {
            onEvent -= handler;
            if (onResolved == null && onRejected == null)
            {
                // The contract no longer has subscribers, so it is unnecessary for it to react to its conditions.
                DisableReaction();
            }
        }

        private void Resolve(object sender, EventArgs e)
        {
            DisableReaction();
            if (Status == ContractStatus.Pending)
            {
                Status = ContractStatus.Resolved;
                onResolved?.Invoke(this, new ContractEventArgs(Status));
            }
        }

        private void Reject(object sender, EventArgs e)
        {
            DisableReaction();
            if (Status == ContractStatus.Pending)
            {
                Status = ContractStatus.Rejected;
                onRejected?.Invoke(this, new ContractEventArgs(Status));
            }
        }

        private void EnableReaction()
        {
            if (!Reacting)
            {
                Reacting = true;
                rejecting.OnSatisfied += Reject;
                resolving.OnSatisfied += Resolve;
            }
        }

        private void DisableReaction()
        {
            if (Reacting)
            {
                Reacting = false;
                rejecting.OnSatisfied -= Reject;
                resolving.OnSatisfied -= Resolve;
            }
        }

        public void Dispose()
        {
            DisableReaction();
        }
    }
}