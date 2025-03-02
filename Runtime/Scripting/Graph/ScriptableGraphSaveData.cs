using System;
using UnityEngine;

namespace Contracts.Scripting.Graph
{
    [Serializable]
    public class NodeSaveData
    {
        public string Type;
        public Rect Position;
        public string Guid;
        public ScriptableObject Value;

        public NodeSaveData(Type type, Rect position, string guid, ScriptableObject value = null)
        {
            Type = type.AssemblyQualifiedName;
            Position = position;
            Guid = guid;
            Value = value;
        }

        public NodeSaveData(Type type, Rect position, ScriptableObject value = null)
        {
            Type = type.AssemblyQualifiedName;
            Position = position;
            Guid = System.Guid.NewGuid().ToString();
            Value = value;
        }
    }

    [Serializable]
    public class EdgeSaveData
    {
        public string OutputNodeGuid;
        public string OutputPortName;
        public string InputNodeGuid;
        public string InputPortName;

        public EdgeSaveData(string outputNodeGuid, string outputPortName, string inputNodeGuid, string inputPortName)
        {
            OutputNodeGuid = outputNodeGuid;
            OutputPortName = outputPortName;
            InputNodeGuid = inputNodeGuid;
            InputPortName = inputPortName;
        }
    }
}
