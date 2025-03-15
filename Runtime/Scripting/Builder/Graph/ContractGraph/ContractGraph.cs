using SimpleGraph;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    public class ContractGraph : SimpleGraphBehaviour, IBuilder<IContract>
    {
        protected override SimpleGraphModel CreateDefaultModel()
        {
            // Create the required contract node, plus one condition node for convenience.
            var spacing = 300;
            var contractNode = SimpleGraphNodeModel.Create<ContractNode>(new Rect(spacing, 0, 0, 0));
            var conditionNode = SimpleGraphNodeModel.Create<ConditionNode>(new Rect(-spacing, 0, 0, 0));

            // Link the nodes together through the satisfied port as a basic example.
            var satisfiedEdge = SimpleGraphEdgeModel.Create(
                conditionNode,
                contractNode,
                ConditionNode.OutputSatisfiedPortName,
                ContractNode.InputFulfilledPortName);

            // Set the default model.
            return SimpleGraphModel.Create(
                new SimpleGraphNodeModel[]
                {
                    contractNode,
                    conditionNode
                },
                new SimpleGraphEdgeModel[]
                {
                    satisfiedEdge
                });
        }

        public IContract Build(UnityEvent updated)
        {
            var builder = Nodes.First((node) => node.Value is IBuilder<IContract>).Value as IBuilder<IContract>;
            return builder.Build(updated);
        }
    }
}
