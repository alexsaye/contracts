using System.Collections.Generic;
using System.Linq;

namespace Contracts
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
            : base(contract.State switch
            {
                ContractState.Fulfilled => nextOnFulfilled,
                ContractState.Rejected => nextOnRejected,
                _ => Enumerable.Empty<ICareerProgression>()
            })
        {
            Contract = contract;
            this.nextOnFulfilled = nextOnFulfilled;
            this.nextOnRejected = nextOnRejected;
            WatchedUpdated += HandleWatched;
        }

        public CareerProgression(IContract contract)
            : this(contract, Enumerable.Empty<ICareerProgression>(), Enumerable.Empty<ICareerProgression>()) { }

        private void HandleWatched(object sender, WatchedEventArgs e)
        {
            if (e.Watched)
            {
                Contract.StateUpdated += HandleContractState;
            }
            else
            {
                Contract.StateUpdated -= HandleContractState;
            }
        }

        private void HandleContractState(object sender, StateEventArgs<ContractState> e)
        {
            // If the contract is no longer pending, fully enumerate the next progressions for the current state.
            if (e.State == ContractState.Fulfilled)
            {
                State = nextOnFulfilled.ToList();
            }
            else if (e.State == ContractState.Rejected)
            {
                State = nextOnRejected.ToList();
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
