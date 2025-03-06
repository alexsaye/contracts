using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            UnityEngine.Debug.Log("DONE!");
            var progression = (ICareerProgression)sender;
            if (progression.Contract.State != ContractState.Pending)
            {
                // Remove the no longer pending progression and issue its next progressions.
                pending.Remove(progression);
                progression.StateUpdated -= IssueNext;
                foreach (var next in e.State)
                {
                    UnityEngine.Debug.Log("Issuing next!");
                    Issue(next);
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
}
