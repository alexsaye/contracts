using SimpleGraph;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Contracts.Scripting
{
    public class ContractGraph : SimpleGraphBehaviour
    {
        /// <summary>
        /// Raised when a condition is built.
        /// </summary>
        public UnityEvent<BuiltConditionEventArgs> BuiltCondition;

        /// <summary>
        /// Raised when a contract is built.
        /// </summary>
        public UnityEvent<BuiltContractEventArgs> BuiltContract;

        protected override SimpleGraphModel CreateDefaultModel()
        {
            var contractNode = new ContractNode() { Position = new Rect(300f, 0f, 0f, 0f) };
            var conditionNode = new ConditionNode() { Position = new Rect(-300f, 0f, 0f, 0f) };

            // Condition.Satisfied -> Contract.Fulfilled
            var satisfiedEdge = new SimpleGraphEdge(
                conditionNode,
                contractNode,
                ConditionNodeView.OutputSatisfiedPortName,
                ContractNodeView.InputFulfilledPortName);

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

        public IContract Build(UnityEvent updated)
        {
            Debug.Log("Building contract...");

            var contractNode = GetNodes<ContractNode>().First();

            ICondition fulfillingCondition;
            var fulfillingEdge = GetInputEdges(contractNode, ContractNodeView.InputFulfilledPortName).FirstOrDefault();
            if (fulfillingEdge != null)
            {
                var fulfillingNode = GetNodeProvidingOutput(fulfillingEdge);
                fulfillingCondition = BuildConditionRecursively(fulfillingNode, updated);
            }
            else
            {
                fulfillingCondition = Condition.Never;
            }

            ICondition rejectingCondition;
            var rejectingEdge = GetInputEdges(contractNode, ContractNodeView.InputRejectedPortName).FirstOrDefault();
            if (rejectingEdge != null)
            {
                var rejectingNode = GetNodeProvidingOutput(rejectingEdge);
                rejectingCondition = BuildConditionRecursively(rejectingNode, updated);
            }
            else
            {
                rejectingCondition = Condition.Never;
            }

            var contract = new Contract(fulfillingCondition, rejectingCondition);
            return contract;
        }

        private ICondition BuildConditionRecursively(SimpleGraphNode node, UnityEvent updated)
        {
            if (node is ConditionNode conditionNode)
            {
                Debug.Log($"Building {conditionNode}...");
                if (conditionNode.Builder == null)
                {
                    Debug.LogWarning("Redundant condition node has no condition builder and will always be satisfied.");
                    return Condition.Always;
                }

                var condition = conditionNode.Builder.Build(updated);
                BuiltCondition.Invoke(new BuiltConditionEventArgs(condition, conditionNode));
                return condition;
            }
            
            if (node is CompositeConditionNode compositeConditionNode)
            {
                Debug.Log($"Building {compositeConditionNode}...");
                var subconditions = GetInputEdges(compositeConditionNode, CompositeConditionNodeView.InputSubconditionsPortName)
                    .Select(edge => GetNodeProvidingOutput(edge))
                    .Select(subconditionNode => BuildConditionRecursively(subconditionNode, updated))
                    .ToList();

                return compositeConditionNode.Mode switch
                {
                    CompositeConditionNode.CompositeMode.All => Condition.All(subconditions),
                    CompositeConditionNode.CompositeMode.Any => Condition.Any(subconditions),
                    _ => throw new InvalidOperationException($"Unknown composite mode: {compositeConditionNode.Mode}"),
                };
            }

            throw new InvalidOperationException($"Failed to build {node}");
        }
    }

    /// <summary>
    /// Raised when a non-composite condition is built.
    /// </summary>
    public class BuiltConditionEventArgs : EventArgs
    {
        /// <summary>
        /// The condition that was built.
        /// </summary>
        public ICondition Condition { get; }

        /// <summary>
        /// The node from which the condition was built.
        /// </summary>
        public ConditionNode Node { get; }

        public BuiltConditionEventArgs(ICondition condition, ConditionNode node)
        {
            Condition = condition;
            Node = node;
        }
    }

    /// <summary>
    /// Raised when a contract is built.
    /// </summary>
    public class BuiltContractEventArgs : EventArgs
    {
        /// <summary>
        /// The contract that was built.
        /// </summary>
        public IContract Contract { get; }

        /// <summary>
        /// The node from which the contract was built.
        /// </summary>
        public ContractNode Node { get; }

        public BuiltContractEventArgs(IContract contract, ContractNode node)
        {
            Contract = contract;
            Node = node;
        }
    }
}
