using System.Collections.Generic;
using UnityEngine;

namespace SimpleGraph
{
    public abstract class SimpleGraphBehaviour : MonoBehaviour
    {
        [SerializeReference]
        public SimpleGraphModel Model;

        /// <summary>
        /// Create a default model for when the behaviour is initialised or reset.
        /// </summary>
        protected abstract SimpleGraphModel CreateDefaultModel();

        protected virtual void OnValidate()
        {
            if (Model == null)
            {
                Debug.Log("Validate: Creating default model...");
                Model = CreateDefaultModel();
            }
        }

        protected virtual void Reset()
        {
            Debug.Log("Reset: Creating default model...");
            Model = CreateDefaultModel();
        }

        protected IEnumerable<T> GetNodes<T>() where T : SimpleGraphNode
        {
            foreach (var node in Model.Nodes)
            {
                if (node is T typedNode)
                {
                    yield return typedNode;
                }
            }
        }

        protected SimpleGraphNode GetNodeReceivingInput(SimpleGraphEdge edge)
        {
            foreach (var node in Model.Nodes)
            {
                if (node.Guid == edge.InputNodeGuid)
                {
                    return node;
                }
            }
            return null;
        }

        protected SimpleGraphNode GetNodeProvidingOutput(SimpleGraphEdge edge)
        {
            foreach (var node in Model.Nodes)
            {
                if (node.Guid == edge.OutputNodeGuid)
                {
                    return node;
                }
            }
            return null;
        }

        protected IEnumerable<SimpleGraphEdge> GetInputEdges(SimpleGraphNode node, string portName)
        {
            foreach (var edge in Model.Edges)
            {
                if (edge.InputNodeGuid == node.Guid && edge.InputPortName == portName)
                {
                    yield return edge;
                }
            }
        }

        protected IEnumerable<SimpleGraphEdge> GetOutputEdges(SimpleGraphNode node, string portName)
        {
            foreach (var edge in Model.Edges)
            {
                if (edge.OutputNodeGuid == node.Guid && edge.OutputPortName == portName)
                {
                    yield return edge;
                }
            }
        }
    }
}