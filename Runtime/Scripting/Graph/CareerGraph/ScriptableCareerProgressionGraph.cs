using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting.Graph
{
    [CreateAssetMenu(fileName = "New Career Progression Graph", menuName = "Contracts/Graph/Career Progression Graph")]
    public class ScriptableCareerProgressionGraph : ScriptableGraph
    {
        private void Awake()
        {
            // Create the required start and end nodes by default, plus one progression node for convenience.
            var spacing = 300f;
            var careerStartNode = new ScriptableGraphNodeModel(typeof(CareerStartNode), new Rect(-spacing, 0f, 0f, 0f));
            var careerEndNode = new ScriptableGraphNodeModel(typeof(CareerEndNode), new Rect(spacing, 0f, 0f, 0f));
            var careerProgressionNode = new ScriptableGraphNodeModel(typeof(CareerProgressionNode), new Rect(0f, 0f, 0f, 0f));

            // Link the nodes together to represent a valid graph which has a path from start to end.
            var hiredEdge = new ScriptableGraphEdgeModel(
                careerStartNode.Guid,
                careerProgressionNode.Guid,
                CareerStartNode.OutputPortName,
                CareerProgressionNode.InputIssuedPortName);

            var fulfilledEdge = new ScriptableGraphEdgeModel(
                careerProgressionNode.Guid,
                careerEndNode.Guid,
                CareerProgressionNode.OutputFulfilledPortName,
                CareerEndNode.InputPortName);

            // Set the default model.
            Model = new ScriptableGraphModel(
                new ScriptableGraphNodeModel[]
                {
                    careerStartNode,
                    careerProgressionNode,
                    careerEndNode
                },
                new ScriptableGraphEdgeModel[]
                {
                    hiredEdge,
                    fulfilledEdge,
                });
        }

        public ICollection<ICareerProgression> Build(UnityEvent updated)
        {
            // Find the progression nodes directly connected to the start node
            var startNode = Model.Nodes.First((node) => node.IsType(typeof(CareerStartNode)));
            var endNode = Model.Nodes.First((node) => node.IsType(typeof(CareerEndNode)));
            var progressionNodes = Model.Edges
                .Where((edge) => edge.OutputNodeGuid == startNode.Guid && edge.InputNodeGuid != endNode.Guid)
                .Select((edge) => Model.Nodes.First((node) => node.Guid == edge.InputNodeGuid));

            // Build the progressions from the node assets.
            var progressionAssets = progressionNodes.Select((node) => (ScriptableCareerProgression)node.Asset);
            var progressions = progressionAssets
                .Select((progression) => progression.Build(updated))
                .ToArray();

            Debug.Log($"Built {progressions.Length} career progressions from graph {name}");
            return progressions;
        }
    }
}
