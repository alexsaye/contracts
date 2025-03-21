using SimpleGraph;
using System;
using UnityEngine;

namespace Contracts.Scripting
{
    [Serializable]
    public class ContractNode : SimpleGraphNode, IContractBuilder
    {
        public IConditionBuilder Fulfilling { get; set; }
        public IConditionBuilder Rejecting { get; set; }

        public IContract Build()
        {
            Debug.Log($"Building {this}...");

            Debug.Log("Building fulfilling condition...");
            var fulfillingCondition = Fulfilling != null
                ? Fulfilling.Build()
                : Condition.Never;

            Debug.Log("Building rejecting condition...");
            var rejectingCondition = Rejecting != null
                ? Rejecting.Build()
                : Condition.Never;

            var contract = new Contract(fulfillingCondition, rejectingCondition);
            return contract;
        }
    }
}
