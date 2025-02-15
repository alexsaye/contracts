using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saye.Contracts
{
    /// <summary>
    /// Routes of career progression via a contract.
    /// </summary>
    public class CareerProgression : Observable<IEnumerable<ICareerProgression>>, ICareerProgression
    {
        public IContract Contract { get; }

        public IEnumerable<ICareerProgression> NextOnResolve { get; }

        public IEnumerable<ICareerProgression> NextOnReject { get; }

        public CareerProgression(IContract contract, IEnumerable<ICareerProgression> nextOnResolve, IEnumerable<ICareerProgression> nextOnReject)
        {
            Contract = contract;
            NextOnResolve = nextOnResolve;
            NextOnResolve = nextOnReject;
            Observed += HandleObserved;
        }

        public CareerProgression(IContract contract, IEnumerable<ICareerProgression> nextOnResolve) : this(contract, nextOnResolve, Enumerable.Empty<ICareerProgression>()) { }

        public CareerProgression(IContract contract) : this(contract, Enumerable.Empty<ICareerProgression>(), Enumerable.Empty<ICareerProgression>()) { }

        private void HandleObserved(object sender, ObservableObservedEventArgs e)
        {
            if (e.IsObserved)
            {
                Contract.State += HandleContractState;
            }
            else
            {
                Contract.State -= HandleContractState;
            }
        }

        private void HandleContractState(object sender, ObservableStateEventArgs<ContractState> e)
        {
            if (e.State == ContractState.Fulfilled)
            {
                CurrentState = NextOnResolve;
            }
            else if (e.State == ContractState.Rejected)
            {
                CurrentState = NextOnReject;
            }
        }
    }

    /// <summary>
    /// Represents routes of career progression via a contract.
    /// </summary>
    public interface ICareerProgression : IObservable<IEnumerable<ICareerProgression>>
    {
        /// <summary>
        /// The contract for this progression.
        /// </summary>
        IContract Contract { get; }

        /// <summary>
        /// The next progressions when the contract is resolved.
        /// </summary>
        public IEnumerable<ICareerProgression> NextOnResolve { get; }

        /// <summary>
        /// The next progressions when the contract is rejected.
        /// </summary>
        public IEnumerable<ICareerProgression> NextOnReject { get; }
    }
}
