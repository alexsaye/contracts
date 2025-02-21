using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System.Reflection;
using System;
using UnityEditor.Search;
using UnityEngine.UIElements;

namespace Contracts.Scripting.Graph
{
    public abstract class ScriptableGraphNode : Node
    {
        public string Guid { get; set; }

        public ScriptableGraphNode(string title, Color? titleBarColor = null)
        {
            this.title = title;

            if (titleBarColor.HasValue)
            {
                titleContainer.style.backgroundColor = new StyleColor(titleBarColor.Value);
            }

            var fieldInfos = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var fieldInfo in fieldInfos)
            {
                var inputAttribute = fieldInfo.GetCustomAttribute<NodeInputAttribute>();
                if (inputAttribute != null)
                {
                    var port = CreateInputPort(fieldInfo, inputAttribute);
                    inputContainer.Add(port);
                }

                var outputAttribute = fieldInfo.GetCustomAttribute<NodeOutputAttribute>();
                if (outputAttribute != null)
                {
                    var port = CreateOutputPort(fieldInfo, outputAttribute);
                    outputContainer.Add(port);
                }

                var fieldAttribute = fieldInfo.GetCustomAttribute<NodeFieldAttribute>();
                if (fieldAttribute != null)
                {
                    var field = CreateField(fieldInfo);
                    extensionContainer.Add(field);
                }
            }

            RefreshPorts();
            RefreshExpandedState();
        }

        private Port CreateInputPort(FieldInfo fieldInfo, NodeInputAttribute attribute)
        {
            var port = Port.Create<Edge>(
                orientation: Orientation.Horizontal,
                direction: Direction.Input,
                capacity: attribute.Capacity,
                type: fieldInfo.FieldType);
            port.portName = fieldInfo.Name;
            port.name = fieldInfo.Name;
            port.AddManipulator(new EdgeConnector<Edge>(new ScriptableGraphEdgeConnectorListener()));
            return port;
        }

        private Port CreateOutputPort(FieldInfo fieldInfo, NodeOutputAttribute attribute)
        {
            var port = Port.Create<Edge>(
                orientation: Orientation.Horizontal,
                direction: Direction.Output,
                capacity: attribute.Capacity,
                type: fieldInfo.FieldType);
            port.portName = fieldInfo.Name;
            port.name = fieldInfo.Name;
            port.AddManipulator(new EdgeConnector<Edge>(new ScriptableGraphEdgeConnectorListener()));
            return port;
        }

        private VisualElement CreateField(FieldInfo fieldInfo) => fieldInfo.FieldType switch
        {
            Type type when type == typeof(bool) => BindField(fieldInfo, new Toggle()),
            Type type when type == typeof(int) => BindField(fieldInfo, new IntegerField()),
            Type type when type == typeof(float) => BindField(fieldInfo, new FloatField()),
            Type type when type == typeof(double) => BindField(fieldInfo, new DoubleField()),
            Type type when type == typeof(string) => BindField(fieldInfo, new TextField()),
            Type type when type == typeof(Enum) => BindField(fieldInfo, new EnumField()),
            Type type when type == typeof(Vector2) => BindField(fieldInfo, new Vector2Field()),
            Type type when type == typeof(Vector3) => BindField(fieldInfo, new Vector3Field()),
            Type type when type == typeof(Vector4) => BindField(fieldInfo, new Vector4Field()),
            Type type when type == typeof(Color) => BindField(fieldInfo, new UnityEditor.UIElements.ColorField()),
            // Default to an ObjectField so we can at least see what weird type we're trying to bind (maybe throw here instead?)
            _ => BindField(fieldInfo, new ObjectField
            {
                objectType = fieldInfo.FieldType,
                bindingPath = fieldInfo.Name,
                searchContext = SearchService.CreateContext("Assets"),
            })
        };

        // This only exists to make the above easier because of how awkward C# makes this (maybe I'm missing something?)
        private BaseField<T> BindField<T>(FieldInfo fieldInfo, BaseField<T> field)
        {
            field.name = fieldInfo.Name;
            field.label = fieldInfo.Name;
            field.RegisterValueChangedCallback((changeEvent) => fieldInfo.SetValue(this, changeEvent.newValue));
            return field;
        }

        public Port GetInputPort(string name)
        {
            return inputContainer.Query<Port>(name);
        }

        public Port GetOutputPort(string name)
        {
            return outputContainer.Query<Port>(name);
        }

        public ObjectField GetSlot(string name)
        {
            return extensionContainer.Query<ObjectField>(name);
        }
    }
}
