namespace Saye.Contracts
{
    /// <summary>
    /// A promise-like contract that is fulfilled when a fulfilling condition is met, or rejected when a rejecting condition is met (or both are met).
    /// </summary>
    public class Contract : ReadOnlyWatchable<ContractState>, IContract
    {
        private readonly ICondition fulfilling;
        public ICondition Fulfilling => fulfilling;

        private readonly ICondition rejecting;
        public ICondition Rejecting => rejecting;

        public Contract(ICondition fulfilling) : this(fulfilling, Condition.Never) { }

        public Contract(ICondition fulfilling, ICondition rejecting)
            : base(rejecting.CurrentState
                  ? ContractState.Rejected
                  : fulfilling.CurrentState
                    ? ContractState.Fulfilled
                    : ContractState.Pending)
        {
            this.fulfilling = fulfilling;
            this.rejecting = rejecting;
            Watched += HandleWatched;
        }

        private void HandleWatched(object sender, WatchedEventArgs e)
        {
            if (e.IsWatched)
            {
                // Subscribe to both conditions, with rejection taking precedence.
                Rejecting.State += HandleRejectingState;
                Fulfilling.State += HandleFulfillingState;
            }
            else
            {
                Rejecting.State -= HandleRejectingState;
                Fulfilling.State -= HandleFulfillingState;
            }
        }

        private void HandleFulfillingState(object sender, StateEventArgs<bool> e)
        {
            if (e.CurrentState)
            {
                // Automatically unsubscribe from the conditions if the contract is fulfilled.
                Fulfilling.State -= HandleFulfillingState;
                Rejecting.State -= HandleRejectingState;

                // Fulfill the contract if it has not already been rejected.
                if (CurrentState == ContractState.Pending)
                {
                    CurrentState = ContractState.Fulfilled;
                }
            }
        }

        private void HandleRejectingState(object sender, StateEventArgs<bool> e)
        {
            if (e.CurrentState)
            {
                // Automatically unsubscribe from the conditions if the contract is rejected.
                Rejecting.State -= HandleRejectingState;
                Fulfilling.State -= HandleFulfillingState;

                // Reject the contract if it has not already been fulfilled.
                if (CurrentState == ContractState.Pending)
                {
                    CurrentState = ContractState.Rejected;
                }
            }
        }
    }

    /// <summary>
    /// Represents a promise-like contract that is fulfilled when a fulfilling condition is met, or rejected when a rejecting condition is met (or both are met).
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
    /// The status of a contract, indicating whether it is pending, fulfilled, or rejected.
    /// </summary>
    public enum ContractState
    {
        Pending,
        Fulfilled,
        Rejected,
    }
}