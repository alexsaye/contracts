using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting.Graph
{
    [CreateAssetMenu(fileName = "New Contract Graph", menuName = "Contracts/Graph/Contract Graph")]
    public class ContractGraph : ScriptableGraph
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
                ConditionNode.SatisfiedPortName,
                ContractNode.FulfillPortName);

            // Set the default model.
            Model = new ScriptableGraphModel(
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
            Debug.Log($"Building contract from graph {name}...");

            // Find the contract node.
            var contractNode = Model.Nodes.First((node) => node.IsType(typeof(ContractNode)));

            // Build the contract from the node asset, which will cascade through and build its conditions.
            var contractAsset = (ScriptableContract)contractNode.Asset;
            var contract = contractAsset.Build(updated);

            Debug.Log($"Built contract from graph {name}");
            return contract;
        }
    }
}
