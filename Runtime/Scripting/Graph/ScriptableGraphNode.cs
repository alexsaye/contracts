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

            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var inputAttribute = field.GetCustomAttribute<NodeInputAttribute>();
                if (inputAttribute != null)
                {
                    var port = CreatePortInput(field, inputAttribute);
                    inputContainer.Add(port);
                }

                var outputAttribute = field.GetCustomAttribute<NodeOutputAttribute>();
                if (outputAttribute != null)
                {
                    var port = CreatePortOutput(field, outputAttribute);
                    outputContainer.Add(port);
                }

                var fieldAttribute = field.GetCustomAttribute<NodeFieldAttribute>();
                if (fieldAttribute != null)
                {
                    var input = CreateFieldInput(field);
                    extensionContainer.Add(input);
                }
            }

            RefreshPorts();
            RefreshExpandedState();
        }

        private Port CreatePortInput(FieldInfo field, NodeInputAttribute attribute)
        {
            var input = Port.Create<Edge>(
                orientation: Orientation.Horizontal,
                direction: Direction.Input,
                capacity: attribute.Capacity,
                type: field.FieldType);
            input.portName = field.Name;
            input.name = field.Name;
            input.AddManipulator(new EdgeConnector<Edge>(new ScriptableGraphEdgeConnectorListener()));
            return input;
        }

        private Port CreatePortOutput(FieldInfo field, NodeOutputAttribute attribute)
        {
            var output = Port.Create<Edge>(
                orientation: Orientation.Horizontal,
                direction: Direction.Output,
                capacity: attribute.Capacity,
                type: field.FieldType);
            output.portName = field.Name;
            output.name = field.Name;
            output.AddManipulator(new EdgeConnector<Edge>(new ScriptableGraphEdgeConnectorListener()));
            return output;
        }

        private VisualElement CreateFieldInput(FieldInfo field) => field.FieldType switch
        {
            Type type when type == typeof(bool) => BindFieldInput(field, new Toggle()),
            Type type when type == typeof(int) => BindFieldInput(field, new IntegerField()),
            Type type when type == typeof(float) => BindFieldInput(field, new FloatField()),
            Type type when type == typeof(string) => BindFieldInput(field, new TextField()),
            Type type when type == typeof(Enum) => BindFieldInput(field, new EnumField()),
            Type type when type == typeof(Vector2) => BindFieldInput(field, new Vector2Field()),
            Type type when type == typeof(Vector3) => BindFieldInput(field, new Vector3Field()),
            Type type when type == typeof(Vector4) => BindFieldInput(field, new Vector4Field()),
            Type type when type == typeof(Color) => BindFieldInput(field, new UnityEditor.UIElements.ColorField()),
            // Default to an ObjectField so we can at least see what weird type we're trying to bind (maybe throw here instead?)
            _ => BindFieldInput(field, new ObjectField
            {
                objectType = field.FieldType,
                bindingPath = field.Name,
                searchContext = SearchService.CreateContext("Assets"),
            })
        };

        // This only exists to make the above easier because of how awkward C# makes this (maybe I'm missing something?)
        private BaseField<T> BindFieldInput<T>(FieldInfo field, BaseField<T> input)
        {
            input.name = field.Name;
            input.label = field.Name;
            input.RegisterValueChangedCallback((changeEvent) => field.SetValue(this, changeEvent.newValue));
            return input;
        }

        public Port GetPortInput(string name)
        {
            return inputContainer.Query<Port>(name);
        }

        public Port GetPortOutput(string name)
        {
            return outputContainer.Query<Port>(name);
        }

        public VisualElement GetFieldInput(string name)
        {
            return extensionContainer.Query(name);
        }
    }
}
