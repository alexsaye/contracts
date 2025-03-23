using System.Collections.Generic;

namespace Contracts
{
    /// <summary>
    /// A parameterised contract that is fulfilled when a fulfilling condition is met, or rejected when a rejecting condition is met (or both are met).
    /// </summary>
    /// <typeparam name="T">The type of data offered by the contract.</typeparam>
    public class Contract<T> : Contract, IContract<T>
    {
        public T Offer { get; }

        public Contract(Contract<T> contract)
            : this(contract.Offer, contract.Fulfilling, contract.Rejecting) { }

        public Contract(T offer, IContract contract)
            : this(offer, contract.Fulfilling, contract.Rejecting) { }

        public Contract(T offer, ICondition fulfilling)
            : base(fulfilling)
        {
            Offer = offer;
        }

        public Contract(T offer, ICondition fulfilling, ICondition rejecting)
            : base(fulfilling, rejecting)
        {
            Offer = offer;
        }
    }

    /// <summary>
    /// A contract that is fulfilled when a fulfilling condition is met, or rejected when a rejecting condition is met (or both are met).
    /// </summary>
    public class Contract : ReadOnlyWatchable<ContractState>, IContract
    {
        private readonly ICondition fulfilling;
        public ICondition Fulfilling => fulfilling;

        private readonly ICondition rejecting;
        public ICondition Rejecting => rejecting;

        public Contract(IContract other)
            : this(other.Fulfilling, other.Rejecting) { }

        public Contract(ICondition fulfilling)
            : this(fulfilling, Condition.Never) { }

        public Contract(ICondition fulfilling, ICondition rejecting)
            : base(rejecting.State
                  ? ContractState.Rejected
                  : fulfilling.State
                    ? ContractState.Fulfilled
                    : ContractState.Pending)
        {
            this.fulfilling = fulfilling;
            this.rejecting = rejecting;
            WatchedUpdated += HandleWatched;
        }

        private void HandleWatched(object sender, WatchedEventArgs e)
        {
            if (e.Watched)
            {
                // Subscribe to both conditions, with rejection taking precedence.
                Rejecting.StateUpdated += HandleRejectingState;
                Fulfilling.StateUpdated += HandleFulfillingState;
            }
            else
            {
                Rejecting.StateUpdated -= HandleRejectingState;
                Fulfilling.StateUpdated -= HandleFulfillingState;
            }
        }

        private void HandleFulfillingState(object sender, StateEventArgs<bool> e)
        {
            if (e.State)
            {
                // Automatically unsubscribe from the conditions if the contract is fulfilled.
                Fulfilling.StateUpdated -= HandleFulfillingState;
                Rejecting.StateUpdated -= HandleRejectingState;

                // Fulfill the contract if it has not already been rejected.
                if (State == ContractState.Pending)
                {
                    State = ContractState.Fulfilled;
                }
            }
        }

        private void HandleRejectingState(object sender, StateEventArgs<bool> e)
        {
            if (e.State)
            {
                // Automatically unsubscribe from the conditions if the contract is rejected.
                Rejecting.StateUpdated -= HandleRejectingState;
                Fulfilling.StateUpdated -= HandleFulfillingState;

                // Reject the contract if it has not already been fulfilled.
                if (State == ContractState.Pending)
                {
                    State = ContractState.Rejected;
                }
            }
        }
    }

    /// <summary>
    /// The status of a contract, indicating whether it is pending, fulfilled, or rejected.
    /// </summary>
    public enum ContractState
    {
        Pending,
        Fulfilled,
        Rejected,
    }

    /// <summary>
    /// Represents a parameterised contract that is fulfilled when a fulfilling condition is met, or rejected when a rejecting condition is met (or both are met).
    /// </summary>
    public interface IContract<T> : IContract
    {
        T Offer { get; }
    }

    /// <summary>
    /// Represents a contract that is fulfilled when a fulfilling condition is met, or rejected when a rejecting condition is met (or both are met).
    /// </summary>
    public interface IContract : IReadOnlyWatchable<ContractState>
    {
        /// <summary>
        /// The condition for fulfilling the contract.
        /// </summary>
        ICondition Fulfilling { get; }

        /// <summary>
        /// The condition for rejecting the contract.
        /// </summary>
        ICondition Rejecting { get; }
    }

    /// <summary>
    /// Represents a builder for a contract.
    /// </summary>
    public interface IContractBuilder
    {
        /// <summary>
        /// Builds the contract.
        /// </summary>
        IContract Build();
    }

    /// <summary>
    /// Represents builder for a contract which can be chained forwards to builders for future contracts.
    /// </summary>
    public interface IContractBuilderProgression : IContractBuilder
    {
        /// <summary>
        /// The next contract builders when the contract is fulfilled.
        /// </summary>
        IEnumerable<IContractBuilder> NextOnFulfilled { get; }

        /// <summary>
        /// The next contract builders when the contract is rejected.
        /// </summary>
        IEnumerable<IContractBuilder> NextOnRejected { get; }
    }
}