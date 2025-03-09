using SimpleGraph;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    public class CareerProgressionGraph : SimpleGraphScriptableObject, IBuilder<IEnumerable<ICareerProgression>>
    {
        private void Awake()
        {
            // Create the required start and end nodes by default, plus one progression node for convenience.
            var spacing = 300f;
            var careerStartNode = new ScriptableGraphNodeModel(typeof(CareerStartNode), new Rect(-spacing, 0f, 0f, 0f));
            var careerProgressionNode = new ScriptableGraphNodeModel(typeof(CareerProgressionNode), new Rect(0f, 0f, 0f, 0f));
            var careerEndNode = new ScriptableGraphNodeModel(typeof(CareerEndNode), new Rect(spacing, 0f, 0f, 0f));

            // Link the nodes together to represent a valid graph which has a path from start to end.
            var startEdge = new ScriptableGraphEdgeModel(
                careerStartNode.Guid,
                careerProgressionNode.Guid,
                CareerStartNode.OutputHiredPortName,
                CareerProgressionNode.InputIssuedPortName);

            var endEdge = new ScriptableGraphEdgeModel(
                careerProgressionNode.Guid,
                careerEndNode.Guid,
                CareerProgressionNode.OutputFulfilledPortName,
                CareerEndNode.InputRetiredPortName);

            // Set the default model.
            Model = new SimpleGraphModel(
                new ScriptableGraphNodeModel[]
                {
                    careerStartNode,
                    careerProgressionNode,
                    careerEndNode
                },
                new ScriptableGraphEdgeModel[]
                {
                    startEdge,
                    endEdge,
                });
        }

        public IEnumerable<ICareerProgression> Build(UnityEvent updated)
        {
            // Build the first progressions from the start node.
            var startNode = Model.Nodes.First((node) => node.IsType(typeof(CareerStartNode)));
            var endNode = Model.Nodes.First((node) => node.IsType(typeof(CareerEndNode)));
            var progressions = Model.Edges
                .Where((edge) => edge.OutputNodeGuid == startNode.Guid && edge.InputNodeGuid != endNode.Guid)
                .Select((edge) => Model.Nodes.First((node) => node.Guid == edge.InputNodeGuid))
                .Select((node) => (CareerProgressionBuilder)node.ObjectReference)
                .Select((progression) => progression.Build(updated))
                .ToArray();
            return progressions;
        }
    }
}
