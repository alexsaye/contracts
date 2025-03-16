using SimpleGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    public class CareerGraph : SimpleGraphBehaviour
    {
        /// <summary>
        /// Raised when a career progression is built.
        /// </summary>
        public UnityEvent<BuiltCareerProgressionEventArgs> BuiltCareerProgression;

        protected override SimpleGraphModel CreateDefaultModel()
        {
            var startNode = new CareerStartNode() { Position = new Rect(-300f, 0f, 0f, 0f) };
            var progressionNode = new CareerProgressionNode();

            // Start.Hired -> Progression.Issued
            var startEdge = new SimpleGraphEdge(
                startNode,
                progressionNode,
                CareerStartNodeView.OutputHiredPortName,
                CareerProgressionNodeView.InputIssuedPortName);

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

        public IEnumerable<ICareerProgression> Build(UnityEvent updated)
        {
            var startNode = GetNodes<CareerStartNode>().First();
            var hiredEdges = GetOutputEdges(startNode, CareerStartNodeView.OutputHiredPortName);
            var hiredNodes = hiredEdges.Select(edge => GetNodeReceivingInput(edge));
            var progressions = new List<ICareerProgression>();
            foreach (var node in hiredNodes)
            {
                if (node is CareerProgressionNode progressionNode)
                {
                    var progression = BuildProgressionRecursively(progressionNode, updated);
                    progressions.Add(progression);
                }
            }
            return progressions;
        }

        private ICareerProgression BuildProgressionRecursively(CareerProgressionNode progressionNode, UnityEvent updated)
        {
            Debug.Log($"Building {progressionNode}...");
            IContract contract;
            var contractGraph = progressionNode.Contract;
            if (contractGraph != null)
            {
                contract = contractGraph.Build(updated);
            }
            else
            {
                Debug.LogWarning("Redundant progression node has no contract and will always progress.");
                contract = new Contract(Condition.Always);
            }

            var fulfilledEdges = GetOutputEdges(progressionNode, CareerProgressionNodeView.OutputFulfilledPortName);
            var fulfilledNodes = fulfilledEdges.Select(edge => GetNodeReceivingInput(edge) as CareerProgressionNode).ToList();

            var rejectedEdges = GetOutputEdges(progressionNode, CareerProgressionNodeView.OutputRejectedPortName);
            var rejectedNodes = rejectedEdges.Select(edge => GetNodeReceivingInput(edge) as CareerProgressionNode).ToList();

            // Set up enumerations to recursively build the next progressions.
            var nextOnFulfilled = fulfilledNodes.Select(node => BuildProgressionRecursively(node, updated));
            var nextOnRejected = rejectedNodes.Select(node => BuildProgressionRecursively(node, updated));

            // Create the progression.
            var progression = new CareerProgression(contract, nextOnFulfilled, nextOnRejected);
            BuiltCareerProgression.Invoke(new BuiltCareerProgressionEventArgs(progression, progressionNode));
            return progression;
        }
    }

    /// <summary>
    /// Raised when a career progression is built.
    /// </summary>
    public class BuiltCareerProgressionEventArgs : EventArgs
    {
        /// <summary>
        /// The progression that was built.
        /// </summary>
        public ICareerProgression Progression { get; }

        /// <summary>
        /// The node from which the progression was built.
        /// </summary>
        public CareerProgressionNode Node { get; }

        public BuiltCareerProgressionEventArgs(ICareerProgression progression, CareerProgressionNode node)
        {
            Progression = progression;
            Node = node;
        }
    }
}
