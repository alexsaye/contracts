using System;

namespace Saye.Contracts
{
    /// <summary>
    /// A promise-like contract that can be bound and unbound.
    /// </summary>
    public interface IContract : IReadOnlyContract
    {
        /// <summary>
        /// Binds the contract to its resolve and reject conditions.
        /// </summary>
        void Bind();

        /// <summary>
        /// Unbinds the contract from its resolve and reject conditions.
        /// </summary>
        void Unbind();
    }

    /// <summary>
    /// A promise-like contract that can be resolved or rejected based on resolve and reject conditions.
    /// </summary>
    public interface IReadOnlyContract
    {
        /// <summary>
        /// The condition for resolving the contract.
        /// </summary>
        IReadOnlyCondition Resolving { get; }

        /// <summary>
        /// The condition for rejecting the contract.
        /// </summary>
        IReadOnlyCondition Rejecting { get; }

        /// <summary>
        /// The status of the contract, indicating whether it is pending, resolved, or rejected.
        /// </summary>
        ContractStatus Status { get; }

        /// <summary>
        /// Raised when the contract is resolved.
        /// </summary>
        event EventHandler<ContractStatusEventArgs> OnResolved;

        /// <summary>
        /// Raised when the contract is rejected.
        /// </summary>
        event EventHandler<ContractStatusEventArgs> OnRejected;
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

    public class ContractStatusEventArgs : EventArgs
    {
        public ContractStatus Status { get; }

        public ContractStatusEventArgs(ContractStatus status)
        {
            Status = status;
        }
    }
}