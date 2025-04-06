using SimpleGraph;
using System;
using System.Linq;
using UnityEngine;

namespace Contracts.Scripting
{
    public class ContractGraph : SimpleGraphBehaviour, IContractBuilder
    {
        private IContractBuilder builder;

        protected override SimpleGraphModel CreateDefaultModel()
        {
            var contractNode = new ContractNode() { Position = new Rect(300f, 0f, 0f, 0f) };
            var conditionNode = new ConditionNode() { Position = new Rect(-300f, 0f, 0f, 0f) };

            // Condition.Satisfied -> Contract.Fulfilled
            var satisfiedEdge = new SimpleGraphEdge(conditionNode, "Satisfied", contractNode, "Fulfil");

            return new SimpleGraphModel()
            {
                Nodes = new SimpleGraphNode[]
                {
                    contractNode,
                    conditionNode
                },
                Edges = new SimpleGraphEdge[]
                {
                    satisfiedEdge
                }
            };
        }

        private void Awake()
        {
            builder = CacheContractBuilder();
        }

        private IContractBuilder CacheContractBuilder()
        {
            // Find the required contract node.
            var contractNode = GetNodes<ContractNode>().FirstOrDefault();
            if (contractNode == null)
            {
                Debug.LogError($"Failed to find the required contract node in {name}");
                return null;
            }
            Debug.Log($"Caching contract builder for {contractNode}...");

            // Find the optional fulfilling input edge and create a condition builder for its output node.
            Debug.Log("Caching fulfilling condition builder...");
            var fulfillingEdge = GetInputEdges(contractNode, "Fulfil").FirstOrDefault();
            contractNode.Fulfilling = fulfillingEdge != null
                ? CacheConditionBuilder(GetNodeProvidingOutput(fulfillingEdge))
                : null;

            // Find the optional rejecting input edge and create a condition builder for its output node.
            Debug.Log("Caching rejecting condition builder...");
            var rejectingEdge = GetInputEdges(contractNode, "Reject").FirstOrDefault();
            contractNode.Rejecting = rejectingEdge != null
                ? CacheConditionBuilder(GetNodeProvidingOutput(rejectingEdge))
                : null;

            return contractNode;
        }

        private IConditionBuilder CacheConditionBuilder(SimpleGraphNode node)
        {
            Debug.Log($"Caching condition builder for {node}...");

            if (node is ConditionNode conditionNode)
            {
                return conditionNode;
            }

            if (node is CompositeConditionNode compositeConditionNode)
            {
                compositeConditionNode.Subconditions = GetInputEdges(compositeConditionNode, "Subconditions")
                    .Select(edge => GetNodeProvidingOutput(edge))
                    .Select(subconditionNode => CacheConditionBuilder(subconditionNode))
                    .ToList();
                return compositeConditionNode;
            }

            throw new InvalidOperationException($"Failed to create a condition builder for {node}");
        }

        public IContract Build()
        {
            return builder.Build();
        }
    }
}
