using System.Collections.Generic;
using System.Linq;

namespace Saye.Contracts
{
    /// <summary>
    /// A route of career progression via a contract.
    /// </summary>
    public class CareerProgression : ReadOnlyWatchable<IEnumerable<ICareerProgression>>, ICareerProgression
    {
        public IContract Contract { get; }

        private IEnumerable<ICareerProgression> nextOnFulfilled;

        private IEnumerable<ICareerProgression> nextOnRejected;

        public CareerProgression(IContract contract, IEnumerable<ICareerProgression> nextOnFulfilled, IEnumerable<ICareerProgression> nextOnRejected)
            : base(contract.CurrentState switch
            {
                ContractState.Fulfilled => nextOnFulfilled,
                ContractState.Rejected => nextOnRejected,
                _ => Enumerable.Empty<ICareerProgression>()
            })
        {
            Contract = contract;
            this.nextOnFulfilled = nextOnFulfilled;
            this.nextOnRejected = nextOnRejected;
            Watched += HandleWatched;
        }

        public CareerProgression(IContract contract)
            : this(contract, Enumerable.Empty<ICareerProgression>(), Enumerable.Empty<ICareerProgression>()) { }

        private void HandleWatched(object sender, WatchedEventArgs e)
        {
            if (e.IsWatched)
            {
                Contract.State += HandleContractState;
            }
            else
            {
                Contract.State -= HandleContractState;
            }
        }

        private void HandleContractState(object sender, StateEventArgs<ContractState> e)
        {
            // If the contract is no longer pending, fully enumerate the next progressions for the current state.
            if (e.CurrentState == ContractState.Fulfilled)
            {
                CurrentState = nextOnFulfilled.ToList();
            }
            else if (e.CurrentState == ContractState.Rejected)
            {
                CurrentState = nextOnRejected.ToList();
            }
        }
    }

    /// <summary>
    /// Represents a route of career progression via a contract.
    /// </summary>
    public interface ICareerProgression : IReadOnlyWatchable<IEnumerable<ICareerProgression>>
    {
        /// <summary>
        /// The contract for this progression.
        /// </summary>
        IContract Contract { get; }
    }
}
