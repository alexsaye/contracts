using SimpleGraph;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    public class ContractGraph : SimpleGraphScriptableObject, IBuilder<IContract>
    {
        private void Awake()
        {
            // Create the required contract node, plus one condition node for convenience.
            var spacing = 300;
            var contractNode = new ScriptableGraphNodeModel(typeof(ContractNode), new Rect(spacing, 0, 0, 0));
            var conditionNode = new ScriptableGraphNodeModel(typeof(ConditionNode), new Rect(-spacing, 0, 0, 0));

            // Link the nodes together through the satisfied port as a basic example.
            var satisfiedEdge = new ScriptableGraphEdgeModel(
                conditionNode.Guid,
                contractNode.Guid,
                ConditionNode.OutputSatisfiedPortName,
                ContractNode.InputFulfilledPortName);

            // Set the default model.
            Model = new SimpleGraphModel(
                new ScriptableGraphNodeModel[]
                {
                    contractNode,
                    conditionNode
                },
                new ScriptableGraphEdgeModel[]
                {
                    satisfiedEdge
                });
        }

        public IContract Build(UnityEvent updated)
        {
            // Find the contract node.
            var contractNode = Model.Nodes.First((node) => node.IsType(typeof(ContractNode)));

            // Build the contract from the node asset, which will cascade through and build its conditions.
            var contractAsset = (ContractBuilder)contractNode.ObjectReference;
            var contract = contractAsset.Build(updated);
            return contract;
        }
    }
}
