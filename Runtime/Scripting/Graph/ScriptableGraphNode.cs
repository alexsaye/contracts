using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System.Reflection;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Contracts.Scripting.Graph
{
    public abstract class ScriptableGraphNode : Node
    {
        public string Guid { get; set; }

        private Dictionary<string, Port> inputPorts = new();
        private Dictionary<string, Port> outputPorts = new();

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

            port.AddManipulator(new EdgeConnector<Edge>(new ScriptableGraphEdgeConnectorListener()));

            inputPorts.Add(field.Name, port);
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

            port.AddManipulator(new EdgeConnector<Edge>(new ScriptableGraphEdgeConnectorListener()));

            outputPorts.Add(field.Name, port);
            outputContainer.Add(port);
        }

        public Port GetInputPort(string name)
        {
            return inputPorts[name];
        }

        public Port GetOutputPort(string name)
        {
            return outputPorts[name];
        }
    }
}
