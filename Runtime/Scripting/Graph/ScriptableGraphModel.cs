using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Contracts.Scripting.Graph
{
    [Serializable]
    public class ScriptableGraphModel
    {
        [SerializeField]
        private ScriptableGraphNodeModel[] nodes;
        public IEnumerable<ScriptableGraphNodeModel> Nodes => nodes;        

        [SerializeField]
        private ScriptableGraphEdgeModel[] edges;
        public IEnumerable<ScriptableGraphEdgeModel> Edges => edges;

        public ScriptableGraphModel(IEnumerable<ScriptableGraphNodeModel> nodes, IEnumerable<ScriptableGraphEdgeModel> edges)
        {
            this.nodes = nodes.ToArray();
            this.edges = edges.ToArray();
        }
    }

    [Serializable]
    public class ScriptableGraphNodeModel
    {
        public string Type;
        public Rect Position;
        public string Guid;

        [SerializeReference]
        public ScriptableObject Asset;

        public ScriptableGraphNodeModel(Type type, Rect position, string guid, ScriptableObject asset = null)
        {
            Type = type.AssemblyQualifiedName;
            Position = position;
            Guid = guid;
            Asset = asset;
        }

        public ScriptableGraphNodeModel(Type type, Rect position, ScriptableObject asset = null)
        {
            Type = type.AssemblyQualifiedName;
            Position = position;
            Guid = System.Guid.NewGuid().ToString();
            Asset = asset;
        }

        public bool IsType(Type type)
        {
            return type.AssemblyQualifiedName == Type;
        }

        public override string ToString()
        {
            return $"{{ Type: {Type}, Position: {Position}, Guid: {Guid}, Asset: {Asset} }}";
        }
    }

    [Serializable]
    public class ScriptableGraphEdgeModel
    {
        public string OutputNodeGuid;
        public string InputNodeGuid;
        public string OutputPortName;
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
            return $"{{ Output: {OutputNodeGuid}@{OutputPortName}, Input: {InputNodeGuid}@{InputPortName} }}";
        }
    }
}
