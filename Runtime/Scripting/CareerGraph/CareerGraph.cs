using SimpleGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    public class CareerGraph : SimpleGraph.SimpleGraph
    {
        protected override SimpleGraphModel CreateDefaultModel()
        {
            var startNode = new CareerStartNodeModel() { Position = new Rect(-300f, 0f, 0f, 0f) };
            var endNode = new CareerEndNodeModel() { Position = new Rect(300f, 0f, 0f, 0f) };
            var progressionNode = new CareerProgressionNodeModel();

            // Start.Hired -> Progression.Issued
            var startEdge = new SimpleGraphEdgeModel(
                startNode,
                progressionNode,
                CareerStartNodeView.OutputHiredPortName,
                CareerProgressionNodeView.InputIssuedPortName);

            // Progression.Fulfilled -> End.Retired
            var endEdge = new SimpleGraphEdgeModel(
                progressionNode,
                endNode,
                CareerProgressionNodeView.OutputFulfilledPortName,
                CareerEndNodeView.InputRetiredPortName);

            return new SimpleGraphModel()
            {
                Nodes = new SimpleGraphNodeModel[]
                {
                    startNode,
                    progressionNode,
                    endNode
                },
                Edges = new SimpleGraphEdgeModel[]
                {
                    startEdge,
                    endEdge,
                }
            };
        }

        public IEnumerable<ICareerProgression> Build(UnityEvent updated)
        {
            //// Build the first progressions from the start node.
            //var startNode = Nodes.First((node) => node.Type == typeof(CareerStartNode).AssemblyQualifiedName);
            //var endNode = Nodes.First((node) => node.Type == typeof(CareerStartNode).AssemblyQualifiedName);
            //var progressions = Edges
            //    .Where((edge) => edge.OutputNodeGuid == startNode.Guid && edge.InputNodeGuid != endNode.Guid)
            //    .Select((edge) => Nodes.First((node) => node.Guid == edge.InputNodeGuid))
            //    .Select((node) => node.Value)
            //    .Select((builder) => builder.Build(updated))
            //    .ToArray();
            //return progressions;
            return null;
        }
    }
}
