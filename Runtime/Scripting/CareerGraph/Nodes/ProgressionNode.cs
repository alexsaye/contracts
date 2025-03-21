using SimpleGraph;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Contracts.Scripting
{
    [Serializable]
    public class ProgressionNode : SimpleGraphNode, IContractBuilderProgression
    {
        [SerializeReference]
        public ContractGraph Contract;

        public IEnumerable<IContractBuilder> NextOnFulfilled { get; set; }

        public IEnumerable<IContractBuilder> NextOnRejected { get; set; }

        public IContract Build()
        {
            Debug.Log($"Building {this}...");
            if (Contract == null)
            {
                Debug.LogWarning("Redundant contract node has no contract builder and will always be fulfilled.");
                return new Contract(Condition.Always);
            }
            return Contract.Build();
        }
    }
}
