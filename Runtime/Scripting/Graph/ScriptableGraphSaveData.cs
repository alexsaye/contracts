using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System.Reflection;

namespace Contracts.Scripting.Graph
{
    [Serializable]
    public class NodeSaveData
    {
        public readonly string Type;
        public readonly string Guid;
        public readonly Rect Position;
        public readonly Dictionary<string, UnityEngine.Object> Slots;

        public NodeSaveData(ScriptableGraphNode node)
        {
            var nodeType = node.GetType();
            Guid = node.Guid;
            Type = nodeType.AssemblyQualifiedName;
            Position = node.GetPosition();
            Slots = nodeType.GetFields()
                .Where(field => field.GetCustomAttribute<NodeSlotAttribute>() != null)
                .ToDictionary(field => field.Name, field => (UnityEngine.Object)field.GetValue(node));

            foreach (var slot in Slots)
            {
                Debug.Log($"Save Slot: {slot.Key} = {slot.Value}");
            }
        }

        public NodeSaveData(Type type, Vector2 position)
        {
            Guid = System.Guid.NewGuid().ToString();
            Type = type.AssemblyQualifiedName;
            Position = new Rect(position, Vector2.zero);
            Slots = type.GetFields()
                .Where(field => field.GetCustomAttribute<NodeSlotAttribute>() != null)
                .ToDictionary(field => field.Name, field => (UnityEngine.Object)null);
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
