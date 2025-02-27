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
        public ScriptableObject Item; // TODO: maybe only a ConditionNodeSaveData and CareerProgressionNodeSaveData have an associated item, with more explicit typing.

        public NodeSaveData(Type type, Rect position, string guid, ScriptableObject item = null)
        {
            Type = type.AssemblyQualifiedName;
            Position = position;
            Guid = guid;
            Item = item;
        }

        public NodeSaveData(Type type, Rect position, ScriptableObject item = null)
        {
            Type = type.AssemblyQualifiedName;
            Position = position;
            Guid = System.Guid.NewGuid().ToString();
            Item = item;
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
