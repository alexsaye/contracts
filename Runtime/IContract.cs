using System;

namespace Saye.Contracts
{
    /// <summary>
    /// A promise-like contract that responds to obligation and violation conditions.
    /// </summary>
    public interface IContract
    {
        /// <summary>
        /// The obligation that fulfills the contract.
        /// </summary>
        ICondition Obligation { get; }

        /// <summary>
        /// The violation that breaches the contract.
        /// </summary>
        ICondition Violation { get; }

        /// <summary>
        /// The status of the contract, indicating whether it is pending, fulfilled, or breached.
        /// </summary>
        ContractStatus Status { get; }

        /// <summary>
        /// Raised when the contract is fulfilled.
        /// </summary>
        event EventHandler OnFulfilled;

        /// <summary>
        /// Raised when the contract is breached.
        /// </summary>
        event EventHandler OnBreached;

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
    /// The status of a contract, indicating whether it is pending, fulfilled, or breached.
    /// </summary>
    public enum ContractStatus
    {
        Pending,
        Fulfilled,
        Breached,
    }
}