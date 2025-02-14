using System;

namespace Saye.Contracts
{
    /// <summary>
    /// An observable contract that can be resolved or rejected based on resolve and reject conditions.
    /// </summary>
    public interface IContract
    {
        /// <summary>
        /// The condition for resolving the contract.
        /// </summary>
        ICondition Resolving { get; }

        /// <summary>
        /// The condition for rejecting the contract.
        /// </summary>
        ICondition Rejecting { get; }

        /// <summary>
        /// The status of the contract, indicating whether it is pending, resolved, or rejected.
        /// </summary>
        ContractStatus Status { get; }

        /// <summary>
        /// Raised when the contract is resolved.
        /// </summary>
        event EventHandler<ContractEventArgs> OnResolved;

        /// <summary>
        /// Raised when the contract is rejected.
        /// </summary>
        event EventHandler<ContractEventArgs> OnRejected;
    }

    /// <summary>
    /// The status of a contract, indicating whether it is pending, resolved, or rejected.
    /// </summary>
    public enum ContractStatus
    {
        Pending,
        Resolved,
        Rejected,
    }

    public class ContractEventArgs : EventArgs
    {
        public ContractStatus Status { get; }

        public ContractEventArgs(ContractStatus status)
        {
            Status = status;
        }
    }
}