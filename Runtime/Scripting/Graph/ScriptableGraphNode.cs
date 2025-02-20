using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System.Reflection;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEditor.Search;

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
                var input = field.GetCustomAttribute<NodeInputAttribute>();
                if (input != null)
                {
                    CreateInputPort(field, input);
                }

                var output = field.GetCustomAttribute<NodeOutputAttribute>();
                if (output != null)
                {
                    CreateOutputPort(field, output);
                }

                var slot = field.GetCustomAttribute<NodeSlotAttribute>();
                if (slot != null)
                {
                    CreateSlot(field);
                }
            }

            RefreshPorts();
            RefreshExpandedState();
        }

        private void CreateInputPort(FieldInfo field, NodeInputAttribute attribute)
        {
            var port = Port.Create<Edge>(
                orientation: Orientation.Horizontal,
                direction: Direction.Input,
                capacity: attribute.Capacity,
                type: field.FieldType);
            port.portName = field.Name;
            port.name = field.Name;

            port.AddManipulator(new EdgeConnector<Edge>(new ScriptableGraphEdgeConnectorListener()));

            inputContainer.Add(port);
        }

        private void CreateOutputPort(FieldInfo field, NodeOutputAttribute attribute)
        {
            var port = Port.Create<Edge>(
                orientation: Orientation.Horizontal,
                direction: Direction.Output,
                capacity: attribute.Capacity,
                type: field.FieldType);
            port.portName = field.Name;
            port.name = field.Name;

            port.AddManipulator(new EdgeConnector<Edge>(new ScriptableGraphEdgeConnectorListener()));

            outputContainer.Add(port);
        }

        private void CreateSlot(FieldInfo field)
        {
            var slot = new ObjectField()
            {
                objectType = field.FieldType,
                bindingPath = field.Name,
                searchContext = SearchService.CreateContext("Assets"),
            };
            slot.name = field.Name;
            slot.RegisterValueChangedCallback((changeEvent) => field.SetValue(this, changeEvent.newValue));
            extensionContainer.Add(slot);
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
