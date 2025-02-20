using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System.Reflection;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace ContractGraph
{
    public abstract class ContractGraphNode : Node
    {
        public string Guid { get; set; }

        private Dictionary<string, Port> inputPorts = new();
        private Dictionary<string, Port> outputPorts = new();

        public ContractGraphNode(string title, Color? titleBarColor = null)
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
        }

        private void CreateInputPort(FieldInfo field, NodeInputAttribute attribute)
        {
            var port = Port.Create<Edge>(
                orientation: Orientation.Horizontal,
                direction: Direction.Input,
                capacity: attribute.Capacity,
                type: field.FieldType);
            port.portName = attribute.Name;

            port.AddManipulator(new EdgeConnector<Edge>(new EdgeConnectorListener()));

            RefreshExpandedState();
            RefreshPorts();

            inputPorts.Add(attribute.Name, port);
            inputContainer.Add(port);
        }

        private void CreateOutputPort(FieldInfo field, NodeOutputAttribute attribute)
        {
            var port = Port.Create<Edge>(
                orientation: Orientation.Horizontal,
                direction: Direction.Output,
                capacity: attribute.Capacity,
                type: field.FieldType);
            port.portName = attribute.Name;

            port.AddManipulator(new EdgeConnector<Edge>(new EdgeConnectorListener()));

            RefreshExpandedState();
            RefreshPorts();

            outputPorts.Add(attribute.Name, port);
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
