using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Saye.Contracts.Scripting
{
    /// <summary>
    /// Issues and observes contracts until it retires.
    /// </summary>
    public class Employer : MonoBehaviour
    {
        [Tooltip("The initial scripted contracts to issue when this issuer is enabled.")]
        [SerializeField]
        private List<ScriptableContract> initialContracts;

        /// <summary>
        /// Mapping of contracts to their corresponding scripted contracts. TODO: I don't like this, but it works for now.
        /// </summary>
        private Dictionary<IContract, ScriptableContract> scriptedContractMapping;

        /// <summary>
        /// Currently pending contracts.
        /// </summary>
        private HashSet<IContract> pendingContracts;

        /// <summary>
        /// Raised when a contract is issued.
        /// </summary>
        public UnityEvent<IContract> OnIssued;

        /// <summary>
        /// Raised when a contract is resolved.
        /// </summary>
        public UnityEvent<IContract> OnResolved;

        /// <summary>
        /// Raised when a contract is rejected.
        /// </summary>
        public UnityEvent<IContract> OnRejected;

        /// <summary>
        /// Raised when a contract is revoked.
        /// </summary>
        public UnityEvent<IContract> OnRevoked;

        /// <summary>
        /// Raised when there are no more contracts.
        /// </summary>
        public UnityEvent OnRetired;

        /// <summary>
        /// Raised when the issuer updates, for updating any conditions which require per-frame assertion.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private UnityEvent OnUpdate;

        private void Awake()
        {
            // Build the scripted contracts as pending contracts.
            pendingContracts = new();
            scriptedContractMapping = new();
            foreach (var scriptedContract in initialContracts)
            {
                Debug.Log($"Building scripted contract: {scriptedContract}");
                var contract = scriptedContract.Build(OnUpdate);
                pendingContracts.Add(contract);
                scriptedContractMapping.Add(contract, scriptedContract);
            }
        }

        private void OnEnable()
        {
            foreach (var contract in pendingContracts)
            {
                Debug.Log($"Issuing pending contract: {contract}");
                OnIssued.Invoke(contract);
            }
            foreach (var contract in pendingContracts)
            {
                Bind(contract);
            }
        }

        private void OnDisable()
        {
            foreach (var contract in pendingContracts)
            {
                Debug.Log($"Revoking pending contract: {contract}");
                OnRevoked.Invoke(contract);
            }
            foreach (var issue in pendingContracts)
            {
                Unbind(issue);
            }
        }

        private void Update()
        {
            OnUpdate.Invoke();
        }

        private void Bind(IContract contract)
        {
            Debug.Log($"Binding contract: {contract}");
            contract.State += HandleContractState;
        }

        private void Unbind(IContract contract)
        {
            Debug.Log($"Unbinding contract: {contract}");
            contract.State -= HandleContractState;
        }

        private void HandleContractState(object sender, EventArgs e)
        {
            var contract = (IContract)sender;
            var consequences = scriptedContractMapping[contract].NextOnResolved;
            Debug.Log($"Handling resolved contract: {contract}");
            IssueConsequences(contract, consequences);
        }

        private void RejectedHandler(object sender, EventArgs e)
        {
            var contract = (IContract)sender;
            var consequences = scriptedContractMapping[contract].NextOnRejected;
            Debug.Log($"Handling rejected contract: {contract}");
            IssueConsequences(contract, consequences);
        }

        private void IssueConsequences(IContract contract, IEnumerable<ScriptableContract> scriptedConsequences)
        {
            // Build the scripted consequences and set up their mappings.
            var consequences = new List<IContract>();
            foreach (var scriptedConsequence in scriptedConsequences)
            {
                var consequence = scriptedConsequence.Build(OnUpdate);
                consequences.Add(consequence);
                scriptedContractMapping.Add(consequence, scriptedConsequence);
            }
            IssueConsequences(contract, consequences);
        }

        private void IssueConsequences(IContract contract, IEnumerable<IContract> consequences)
        {
            // Unbind the contract and remove its mapping.
            Unbind(contract);
            scriptedContractMapping.Remove(contract);

            // Issue and bind the consequences.
            pendingContracts.UnionWith(consequences);
            foreach (var consequence in consequences)
            {
                Debug.Log($"Issuing consequential contract: {consequence}");
                OnIssued.Invoke(consequence);
            }
            foreach (var consequence in consequences)
            {
                Bind(consequence);
            }

            // If there aren't any pending contracts, this issuer has expired.
            if (pendingContracts.Count == 0)
            {
                Debug.Log($"Expiring issuer: {this}");
                OnRetired.Invoke();
            }
        }
    }

    public class EmployerContractEventArgs
    {
        public IContract Contract { get; }

        public EmployerContractEventArgs(IContract contract)
        {
            Contract = contract;
        }
    }
}