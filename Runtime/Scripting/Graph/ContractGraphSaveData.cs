using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace ContractGraph
{
    [Serializable]
    public class NodeSaveData
    {
        public readonly string Guid;
        public readonly string Type;
        public readonly Rect Position;
        
        public NodeSaveData(ContractGraphNode node)
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
            ContractGraphNode outputNode = edge.output.node as ContractGraphNode;
            ContractGraphNode inputNode = edge.input.node as ContractGraphNode;

            OutputNodeGuid = outputNode.Guid;
            OutputPortName = edge.output.portName;
            InputNodeGuid = inputNode.Guid;
            InputPortName = edge.input.portName;
        }
    }
}
