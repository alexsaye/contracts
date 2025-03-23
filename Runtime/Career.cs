using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Contracts
{
    /// <summary>
    /// A career which issues drafted contracts.
    /// </summary>
    public class Career : ICareer
    {
        private readonly HashSet<IContractBuilder> pendingBuilders = new();
        private readonly Dictionary<IContract, IContractBuilder> pendingIssues = new();

        public event EventHandler<IssuedEventArgs> Issued;

        public Career()
        {
        }

        public Career(IContractBuilder builder)
        {
            DraftContract(builder);
        }

        public Career(IEnumerable<IContractBuilder> builders)
        {
            foreach (var builder in builders)
            {
                DraftContract(builder);
            }
        }

        public void DraftContract(IContractBuilder builder)
        {
            pendingBuilders.Add(builder);
        }

        public bool IssueContracts()
        {
            if (pendingBuilders.Count == 0)
            {
                return false;
            }

            // Drain the pending builders and start issuing their contracts.
            var issuingBuilders = new List<IContractBuilder>(pendingBuilders);
            pendingBuilders.Clear();
            foreach (var builder in issuingBuilders)
            {
                var contract = builder.Build();
                pendingIssues.Add(contract, builder);
                Issued?.Invoke(this, new IssuedEventArgs(contract));
                contract.StateUpdated += HandleContractState;
            }
            return true;
        }

        private void HandleContractState(object sender, StateEventArgs<ContractState> e)
        {
            if (e.State == ContractState.Pending)
            {
                return;
            }

            // Stop tracking the finished contract.
            var contract = (IContract)sender;
            var contractBuilder = pendingIssues[contract];
            pendingIssues.Remove(contract);
            contract.StateUpdated -= HandleContractState;

            // If the builder is a progression, draft its next builders based on the contract's state.
            if (contractBuilder is IContractBuilderProgression progression)
            {
                var nextBuilders = e.State == ContractState.Fulfilled
                    ? progression.NextOnFulfilled
                    : progression.NextOnRejected;
                foreach (var nextBuilder in nextBuilders)
                {
                    DraftContract(nextBuilder);
                }
            }
        }
    }

    public interface ICareer
    {
        /// <summary>
        /// Draft a contract to issue when prompted.
        /// </summary>
        void DraftContract(IContractBuilder builder);

        /// <summary>
        /// Issue drafted contracts.
        /// </summary>
        /// <returns>True if contracts were issued, otherwise false.</returns>
        bool IssueContracts();

        /// <summary>
        /// Raised when a contract is issued.
        /// </summary>
        event EventHandler<IssuedEventArgs> Issued;
    }

    /// <summary>
    /// Represents a builder for a career.
    /// </summary>
    public interface ICareerBuilder
    {
        ICareer Build();
    }

    public class IssuedEventArgs : EventArgs
    {
        public IContract Contract { get; }

        public IssuedEventArgs(IContract contract)
        {
            Contract = contract;
        }
    }
}
