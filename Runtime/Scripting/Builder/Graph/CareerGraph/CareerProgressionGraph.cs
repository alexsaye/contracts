using SimpleGraph;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    public class CareerProgressionGraph : SimpleGraphBehaviour, IBuilder<IEnumerable<ICareerProgression>>
    {
        protected override SimpleGraphModel CreateDefaultModel()
        {
            // Create the required start and end nodes by default, plus one progression node for convenience.
            var spacing = 300f;
            var careerStartNode = SimpleGraphNodeModel.Create<CareerStartNode>(new Rect(-spacing, 0f, 0f, 0f));
            var careerProgressionNode = SimpleGraphNodeModel.Create<CareerProgressionNode>(new Rect(0f, 0f, 0f, 0f), new CareerProgressionBuilder());
            var careerEndNode = SimpleGraphNodeModel.Create<CareerEndNode>(new Rect(spacing, 0f, 0f, 0f));

            // Link the nodes together to represent a valid graph which has a path from start to end.
            var startEdge = SimpleGraphEdgeModel.Create(
                careerStartNode,
                careerProgressionNode,
                CareerStartNode.OutputHiredPortName,
                CareerProgressionNode.InputIssuedPortName);

            var endEdge = SimpleGraphEdgeModel.Create(
                careerProgressionNode,
                careerEndNode,
                CareerProgressionNode.OutputFulfilledPortName,
                CareerEndNode.InputRetiredPortName);

            return SimpleGraphModel.Create(
                new SimpleGraphNodeModel[]
                {
                    careerStartNode,
                    careerProgressionNode,
                    careerEndNode
                },
                new SimpleGraphEdgeModel[]
                {
                    startEdge,
                    endEdge,
                });
        }

        public IEnumerable<ICareerProgression> Build(UnityEvent updated)
        {
            // Build the first progressions from the start node.
            var startNode = Nodes.First((node) => node.Type == typeof(CareerStartNode).AssemblyQualifiedName);
            var endNode = Nodes.First((node) => node.Type == typeof(CareerStartNode).AssemblyQualifiedName);
            var progressions = Edges
                .Where((edge) => edge.OutputNodeGuid == startNode.Guid && edge.InputNodeGuid != endNode.Guid)
                .Select((edge) => Nodes.First((node) => node.Guid == edge.InputNodeGuid))
                .Select((node) => node.Value as IBuilder<ICareerProgression>)
                .Select((builder) => builder.Build(updated))
                .ToArray();
            return progressions;
        }
    }
}
