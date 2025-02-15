using System.Collections.Generic;
using System.Linq;

namespace Saye.Contracts
{
    /// <summary>
    /// Routes of career progression via a contract.
    /// </summary>
    public class CareerProgression : ReadOnlyNotice<IEnumerable<ICareerProgression>>, ICareerProgression
    {
        public IContract Contract { get; }

        public IEnumerable<ICareerProgression> NextOnFulfilled { get; }

        public IEnumerable<ICareerProgression> NextOnRejected { get; }

        public CareerProgression(IContract contract, IEnumerable<ICareerProgression> nextOnFulfilled, IEnumerable<ICareerProgression> nextOnRejected)
            : base(contract.CurrentState switch
            {
                ContractState.Fulfilled => nextOnFulfilled,
                ContractState.Rejected => nextOnRejected,
                _ => Enumerable.Empty<ICareerProgression>()
            })
        {
            Contract = contract;
            NextOnFulfilled = nextOnFulfilled;
            NextOnFulfilled = nextOnRejected;
            Noticed += HandleNoticed;
        }

        public CareerProgression(IContract contract, IContract nextOnFulfilled, IContract nextOnRejected) : this(contract, new[] { new CareerProgression(nextOnFulfilled) }, new[] { new CareerProgression(nextOnRejected) }) { }

        public CareerProgression(IContract contract) : this(contract, Enumerable.Empty<ICareerProgression>(), Enumerable.Empty<ICareerProgression>()) { }

        private void HandleNoticed(object sender, NoticedEventArgs e)
        {
            if (e.IsNoticed)
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
            // Update the current state if the contract is fulfilled or rejected.
            if (e.CurrentState == ContractState.Fulfilled)
            {
                CurrentState = NextOnFulfilled;
            }
            else if (e.CurrentState == ContractState.Rejected)
            {
                CurrentState = NextOnRejected;
            }
        }
    }

    /// <summary>
    /// Represents routes of career progression via a contract.
    /// </summary>
    public interface ICareerProgression : IReadOnlyNotice<IEnumerable<ICareerProgression>>
    {
        /// <summary>
        /// The contract for this progression.
        /// </summary>
        IContract Contract { get; }

        /// <summary>
        /// The next progressions when the contract is fulfilled.
        /// </summary>
        public IEnumerable<ICareerProgression> NextOnFulfilled { get; }

        /// <summary>
        /// The next progressions when the contract is rejected.
        /// </summary>
        public IEnumerable<ICareerProgression> NextOnRejected { get; }
    }
}
