using System;
using System.Collections.Generic;
using System.Linq;

namespace Contracts
{
    /// <summary>
    /// A career in which contracts are issued through the progression of other contracts.
    /// </summary>
    public class Career : ICareer
    {
        private readonly HashSet<ICareerProgression> pending;
        public IEnumerable<IContract> Pending => pending.Select(progression => progression.Contract);

        public event EventHandler<IssuedEventArgs> Issued;

        public event EventHandler<RetiredEventArgs> Retired;

        public Career()
        {
            pending = new();
        }

        public void Issue(ICareerProgression progression)
        {
            pending.Add(progression);
            Issued?.Invoke(this, new IssuedEventArgs(progression.Contract));
            progression.StateUpdated += IssueNext;
        }

        public void Issue(IContract contract)
        {
            Issue(new CareerProgression(contract));
        }

        private void IssueNext(object sender, StateEventArgs<IEnumerable<ICareerProgression>> e)
        {
            var progression = (ICareerProgression)sender;
            if (progression.Contract.State != ContractState.Pending)
            {
                // Remove the no longer pending progression.
                pending.Remove(progression);
                progression.StateUpdated -= IssueNext;

                var nextProgressions = e.State.ToList();
                if (nextProgressions.Count == 0 && pending.Count == 0)
                {
                    // There are no more contracts pending progression, so raise the retired event.
                    Retired?.Invoke(this, new RetiredEventArgs());
                }
                else
                {
                    // Issue the next progressions.
                    foreach (var nextProgression in nextProgressions)
                    {
                        Issue(nextProgression);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents a career in which contracts are issued through the progression of other contracts.
    /// </summary>
    public interface ICareer
    {
        /// <summary>
        /// The contracts currently pending progression.
        /// </summary>
        public IEnumerable<IContract> Pending { get; }

        /// <summary>
        /// Raised when a contract is issued.
        /// </summary>
        event EventHandler<IssuedEventArgs> Issued;

        /// <summary>
        /// Raised when there are no more contracts pending progression.
        /// </summary>
        event EventHandler<RetiredEventArgs> Retired;

        /// <summary>
        /// Issue a contract with a route of progression.
        /// </summary>
        /// <param name="progression">The contract to issue with its route of progression.</param>
        void Issue(ICareerProgression progression);

        /// <summary>
        /// Issue a contract without a route of progression.
        /// </summary>
        /// <param name="contract">The contract to issue.</param>
        void Issue(IContract contract);
    }

    /// <summary>
    /// Raised when a contract is issued through a career.
    /// </summary>
    public class IssuedEventArgs : EventArgs
    {
        public IContract Contract { get; }

        public IssuedEventArgs(IContract contract)
        {
            Contract = contract;
        }
    }

    /// <summary>
    /// Raised when there are no more contracts pending progression.
    /// </summary>
    public class RetiredEventArgs : EventArgs {}
}
