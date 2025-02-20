using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Contracts.Scripting.Graph
{
    [Serializable]
    public class NodeSaveData
    {
        public readonly string Guid;
        public readonly string Type;
        public readonly Rect Position;

        public NodeSaveData(ScriptableGraphNode node)
        {
            Guid = node.Guid;
            Type = node.GetType().AssemblyQualifiedName;
            Position = node.GetPosition();
        }
    }

    [Serializable]
    public class EdgeSaveData
    {
        public readonly string OutputNodeGuid;
        public readonly string OutputPortName;
        public readonly string InputNodeGuid;
        public readonly string InputPortName;
        
        public EdgeSaveData(Edge edge)
        {
            ScriptableGraphNode outputNode = edge.output.node as ScriptableGraphNode;
            ScriptableGraphNode inputNode = edge.input.node as ScriptableGraphNode;

            OutputNodeGuid = outputNode.Guid;
            OutputPortName = edge.output.portName;
            InputNodeGuid = inputNode.Guid;
            InputPortName = edge.input.portName;
        }
    }
}
