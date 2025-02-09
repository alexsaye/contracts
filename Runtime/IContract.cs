using System;

namespace Saye.Contracts
{
    /// <summary>
    /// A contract that can be bound and unbound.
    /// </summary>
    public interface IContract : IReadOnlyContract
    {
        /// <summary>
        /// Binds the contract to its obligation and violation conditions.
        /// </summary>
        void Bind();

        /// <summary>
        /// Unbinds the contract from its obligation and violation conditions, effectively suspending it.
        /// </summary>
        void Unbind();
    }

    /// <summary>
    /// A contract that can be fulfilled or breached based on obligation and violation conditions.
    /// </summary>
    public interface IReadOnlyContract
    {
        /// <summary>
        /// The obligation that fulfills the contract.
        /// </summary>
        IReadOnlyCondition Obligation { get; }

        /// <summary>
        /// The violation that breaches the contract.
        /// </summary>
        IReadOnlyCondition Violation { get; }

        /// <summary>
        /// The status of the contract, indicating whether it is pending, fulfilled, or breached.
        /// </summary>
        ContractStatus Status { get; }

        /// <summary>
        /// Raised when the contract is fulfilled.
        /// </summary>
        event EventHandler<ContractStatusEventArgs> OnFulfilled;

        /// <summary>
        /// Raised when the contract is breached.
        /// </summary>
        event EventHandler<ContractStatusEventArgs> OnBreached;
    }

    /// <summary>
    /// The status of a contract, indicating whether it is pending, fulfilled, or breached.
    /// </summary>
    public enum ContractStatus
    {
        Pending,
        Fulfilled,
        Breached,
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