using System;
using System.Collections.Generic;
using System.Linq;

namespace Saye.Contracts.Scripting
{
    /// <summary>
    /// A career in which contracts are issued through the progression of other contracts.
    /// </summary>
    public class Career : ICareer
    {
        private readonly HashSet<ICareerProgression> pending;

        public event EventHandler<CareerIssueEventArgs> Issued;

        public Career()
        {
            pending = new();
        }

        public void Issue(ICareerProgression progression)
        {
            pending.Add(progression);
            Issued?.Invoke(this, new CareerIssueEventArgs(progression.Contract));
            progression.State += IssueNext;
        }

        public void Issue(IContract contract)
        {
            Issue(new CareerProgression(contract));
        }

        private void IssueNext(object sender, StateEventArgs<IEnumerable<ICareerProgression>> e)
        {
            var completed = (ICareerProgression)sender;
            pending.Remove(completed);
            foreach (var progression in e.CurrentState)
            {
                Issue(progression);
            }
        }
    }

    /// <summary>
    /// Represents a career in which contracts are issued through the progression of other contracts.
    /// </summary>
    public interface ICareer
    {
        /// <summary>
        /// Raised when a contract is issued.
        /// </summary>
        event EventHandler<CareerIssueEventArgs> Issued;

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
    public class CareerIssueEventArgs : EventArgs
    {
        public IContract Contract { get; }

        public CareerIssueEventArgs(IContract contract)
        {
            Contract = contract;
        }
    }
}
