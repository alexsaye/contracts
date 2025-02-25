using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Contracts.Scripting.Graph
{
    [Serializable]
    public class NodeSaveData
    {
        public string Type;
        public string Guid;
        public Rect Position;
        public FieldSaveData[] Fields;

        public NodeSaveData(string type, Rect position, string guid, Dictionary<string, object> fields)
        {
            Guid = guid;
            Type = type;
            Position = position;
            Fields = fields
                .Where((pair) => pair.Value != null)
                .Select((pair) => new FieldSaveData(pair.Key, pair.Value)).ToArray();
        }

        public NodeSaveData(string type, Vector2 position)
        {
            Guid = System.Guid.NewGuid().ToString();
            Type = type;
            Position = new Rect(position, Vector2.zero);
            Fields = new FieldSaveData[0];
        }
    }

    [Serializable]
    public class FieldSaveData
    {
        public string Name;
        public string TypeName;
        public UnityEngine.Object ValueObject;
        public string ValuePrimitive;
        public string ValueJson;

        public FieldSaveData(string name, object value)
        {
            Name = name;
            TypeName = value.GetType().AssemblyQualifiedName;

            if (value is UnityEngine.Object unityObject)
            {
                ValueObject = unityObject;
            }
            else if (value.GetType().IsPrimitive || value.GetType().IsEnum)
            {
                ValuePrimitive = value.ToString();
            }
            else
            {
                ValueJson = JsonUtility.ToJson(value);
            }
        }

        public object GetValue()
        {
            if (ValueObject != null)
            {
                return ValueObject;
            }

            var type = Type.GetType(TypeName);

            if (!string.IsNullOrEmpty(ValuePrimitive))
            {
                return type.GetMethod("Parse", new Type[] { typeof(string) }).Invoke(type, new object[] { ValuePrimitive });
            }

            if (!string.IsNullOrEmpty(ValueJson))
            {
                return JsonUtility.FromJson(ValueJson, type);
            }

            return null;
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
