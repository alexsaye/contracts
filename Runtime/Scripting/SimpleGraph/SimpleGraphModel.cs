using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleGraph
{
    [Serializable]
    public class SimpleGraphModel
    {
        [SerializeField]
        private ScriptableGraphNodeModel[] nodes;
        public IEnumerable<ScriptableGraphNodeModel> Nodes => nodes;        

        [SerializeField]
        private ScriptableGraphEdgeModel[] edges;
        public IEnumerable<ScriptableGraphEdgeModel> Edges => edges;

        public SimpleGraphModel(IEnumerable<ScriptableGraphNodeModel> nodes, IEnumerable<ScriptableGraphEdgeModel> edges)
        {
            this.nodes = nodes.ToArray();
            this.edges = edges.ToArray();
        }
    }

    [Serializable]
    public class ScriptableGraphNodeModel
    {
        /// <summary>
        /// A unique identifier of the graph node for cross-referencing with the edge models.
        /// </summary>
        public string Guid;

        /// <summary>
        /// The type of graph node that this model represents.
        /// </summary>
        public string Type;

        /// <summary>
        /// The position of the graph node in the graph view.
        /// </summary>
        public Rect Position;

        /// <summary>
        /// The object referenced by this node.
        /// </summary>
        [SerializeReference]
        public UnityEngine.Object ObjectReference;

        public ScriptableGraphNodeModel(string guid, Type type, Rect position, UnityEngine.Object objectReference = null)
        {
            Guid = guid;
            Type = type.AssemblyQualifiedName;
            Position = position;
            ObjectReference = objectReference;
        }

        public ScriptableGraphNodeModel(Type type, Rect position, UnityEngine.Object objectReference = null)
        {
            Guid = System.Guid.NewGuid().ToString();
            Type = type.AssemblyQualifiedName;
            Position = position;
            ObjectReference = objectReference;
        }

        public ScriptableGraphNodeModel(ScriptableGraphNodeModel model)
        {
            Guid = model.Guid;
            Type = model.Type;
            Position = model.Position;
            ObjectReference = model.ObjectReference;
        }

        public bool IsType(Type type)
        {
            return type.AssemblyQualifiedName == Type;
        }

        public override string ToString()
        {
            return $"{{ Type: {Type}, Asset: {ObjectReference}, Guid: {Guid} }}";
        }
    }

    [Serializable]
    public class ScriptableGraphEdgeModel
    {
        /// <summary>
        /// The id of the node on the output side of the port.
        /// </summary>
        public string OutputNodeGuid;

        /// <summary>
        /// The id of the node on the input side of the port.
        /// </summary>
        public string InputNodeGuid;

        /// <summary>
        /// The name of the output port on the output node.
        /// </summary>
        public string OutputPortName;

        /// <summary>
        /// The name of the input port on the input node.
        /// </summary> 
        public string InputPortName;

        public ScriptableGraphEdgeModel(string outputNodeGuid, string inputNodeGuid, string outputPortName, string inputPortName)
        {
            OutputNodeGuid = outputNodeGuid;
            InputNodeGuid = inputNodeGuid;
            OutputPortName = outputPortName;
            InputPortName = inputPortName;
        }

        public override string ToString()
        {
            // return "From output to input"
            return $"{{ Output: {OutputPortName}, Input: {InputPortName} }}";
        }
    }
}
