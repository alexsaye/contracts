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
        public string Guid { get; private set; }

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
            }
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

        protected VisualElement CreateFieldInput(FieldInfo field, object source) => field.FieldType switch
        {
            Type type when type == typeof(bool) => InitFieldInput(new Toggle(), field, source),
            Type type when type == typeof(int) => InitFieldInput(new IntegerField(), field, source),
            Type type when type == typeof(float) => InitFieldInput(new FloatField(), field, source),
            Type type when type == typeof(string) => InitFieldInput(new TextField(), field, source),
            Type type when type == typeof(Enum) => InitFieldInput(new EnumField(), field, source),
            Type type when type == typeof(Vector2) => InitFieldInput(new Vector2Field(), field, source),
            Type type when type == typeof(Vector3) => InitFieldInput(new Vector3Field(), field, source),
            Type type when type == typeof(Vector4) => InitFieldInput(new Vector4Field(), field, source),
            Type type when type == typeof(Color) => InitFieldInput(new UnityEditor.UIElements.ColorField(), field, source),
            _ => InitFieldInput(new ObjectField
            {
                objectType = field.FieldType,
                searchContext = SearchService.CreateContext("Assets"),
            }, field, source)
        };

        // TODO: I think we're misusing these and should be modifying SerializedObjects through bindingPath.
        private BaseField<T> InitFieldInput<T>(BaseField<T> input, FieldInfo field, object source)
        {
            input.name = field.Name;
            input.label = field.Name; // TODO: make it spaced with capitalization
            input.value = (T)field.GetValue(source);
            input.RegisterValueChangedCallback((evt) => field.SetValue(source, evt.newValue));
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

        public virtual NodeSaveData Save()
        {
            // We need to (well, probably should) pass a consistent guid in after making the node so cache one and pass it into all node saves from this node.
            if (Guid == null)
            {
                Guid = System.Guid.NewGuid().ToString();
            }
            return new NodeSaveData(GetType(), GetPosition(), Guid);
        }

        public virtual void Load(NodeSaveData saveData)
        {
            Guid = saveData.Guid;
        }
    }
}
