using SimpleGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Contracts.Scripting
{
    public class CareerGraph : SimpleGraphBehaviour, ICareerBuilder
    {
        private ICareerBuilder builder;

        protected override SimpleGraphModel CreateDefaultModel()
        {
            var startNode = new StartNode() { Position = new Rect(-300f, 0f, 0f, 0f) };
            var progressionNode = new ProgressionNode();

            // Start.Hired -> Progression.Issued
            var startEdge = new SimpleGraphEdge(
                startNode,
                progressionNode,
                StartNodeView.OutputHiredPortName,
                ProgressionNodeView.InputIssuedPortName);

            return new SimpleGraphModel()
            {
                Nodes = new SimpleGraphNode[]
                {
                    startNode,
                    progressionNode,
                },
                Edges = new SimpleGraphEdge[]
                {
                    startEdge,
                }
            };
        }

        private void Awake()
        {
            builder = CacheCareerBuilder();
        }

        private ICareerBuilder CacheCareerBuilder()
        {
            // Find the required start node.
            var startNode = GetNodes<StartNode>().FirstOrDefault();
            if (startNode == null)
            {
                Debug.LogError($"Failed to find the required start node in {name}");
                return null;
            }
            Debug.Log($"Caching career builder for {startNode}...");

            // Create progressions from the nodes directly connected to the output edges of the start node.
            Debug.Log("Caching contract progression builders...");
            var rootEdges = GetOutputEdges(startNode, StartNodeView.OutputHiredPortName);
            startNode.NextOnHired = CacheContractBuilderProgressions(rootEdges);

            return startNode;
        }

        private List<IContractBuilderProgression> CacheContractBuilderProgressions(IEnumerable<SimpleGraphEdge> edges)
        {
            return edges
                .Select(edge => GetNodeReceivingInput(edge))
                .Select(node => CacheContractBuilderProgression(node))
                .ToList();
        }

        private IContractBuilderProgression CacheContractBuilderProgression(SimpleGraphNode node)
        {
            Debug.Log($"Caching progression builder for {node}...");

            if (node is not ProgressionNode progressionNode)
            {
                throw new InvalidOperationException($"Failed to create a contract progression builder for {node}");
            }

            Debug.Log("Caching next on fulfilled progression builders...");
            var fulfilledEdges = GetOutputEdges(progressionNode, ProgressionNodeView.OutputFulfilledPortName);
            progressionNode.NextOnFulfilled = CacheContractBuilderProgressions(fulfilledEdges);

            Debug.Log("Caching next on rejected progression builders...");
            var rejectedEdges = GetOutputEdges(progressionNode, ProgressionNodeView.OutputRejectedPortName);
            progressionNode.NextOnRejected = CacheContractBuilderProgressions(rejectedEdges);

            return progressionNode;
        }

        public ICareer Build()
        {
            return builder.Build();
        }
    }
}
