using SimpleGraph;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    public class ContractGraph : SimpleGraph.SimpleGraph
    {
        protected override SimpleGraphModel CreateDefaultModel()
        {
            var contractNode = new ContractNodeModel() { Position = new Rect(300f, 0f, 0f, 0f) };
            var conditionNode = new ConditionNodeModel() { Position = new Rect(-300f, 0f, 0f, 0f) };

            // Condition.Satisfied -> Contract.Fulfilled
            var satisfiedEdge = new SimpleGraphEdgeModel(
                conditionNode,
                contractNode,
                ConditionNodeView.OutputSatisfiedPortName,
                ContractNodeView.InputFulfilledPortName);

            return new SimpleGraphModel()
            {
                Nodes = new SimpleGraphNodeModel[]
                {
                    contractNode,
                    conditionNode
                },
                Edges = new SimpleGraphEdgeModel[]
                {
                    satisfiedEdge
                }
            };
        }

        public IContract Build(UnityEvent updated)
        {
            //var builder = Nodes.First((node) => node.Value is IConditionBuilder<IContract>).Value as IConditionBuilder<IContract>;
            //return builder.Build(updated);
            return null;
        }
    }
}
