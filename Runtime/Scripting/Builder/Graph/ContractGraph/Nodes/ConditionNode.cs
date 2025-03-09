using SimpleGraph;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting
{
    [NodeMenu("Condition")]
    [NodeContext(typeof(ContractGraph))]
    [NodeCapabilities(~Capabilities.Resizable)]
    public class ConditionNode : SimpleGraphViewNode<ConditionBuilder>
    {
        public const string OutputSatisfiedPortName = "Satisfied";

        private static readonly IReadOnlyDictionary<string, System.Type> conditionTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(ConditionBuilder)) && !type.IsAbstract)
                .Where(type => type != typeof(CompositeConditionBuilder))
                .ToDictionary(type => type.Name, type => type);

        private readonly List<VisualElement> conditionElements = new();
        private readonly DropdownField typeField;
        private readonly ObservablePort outputSatisfiedPort;

        public ConditionNode() : base()
        {
            title = "Condition";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.6f));

            // Add a dropdown field to select a condition type.
            typeField = new(conditionTypes.Keys.ToList(), -1, FormatConditionName, FormatConditionName);
            inputContainer.Add(typeField);

            // If the type field is given a valid type, create a new condition instance of that type and clear the asset field.
            // TODO: maybe actually remove the node and replace with a new one?
            typeField.RegisterValueChangedCallback((e) =>
            {
                ObjectReference = CreateObjectReference();
                Reconnect();
                RefreshPorts();
                RefreshExpandedState();
            });

            // Add an output port for when the condition is satisfied.
            outputSatisfiedPort = ObservablePort.Create<Edge>(OutputSatisfiedPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(ConditionBuilder));
            outputContainer.Add(outputSatisfiedPort);
        }

        private string FormatConditionName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "Select a type...";
            }

            // Remove "ScriptableCondition" or "Condition" from the end of the name.
            var words = Regex.Split(name, @"(?=[A-Z])");
            var wordsToJoin = words[words.Length - 1].Equals("Condition")
                ? words[words.Length - 2].Equals("Scriptable")
                    ? words.Take(words.Length - 2)
                    : words.Take(words.Length - 1)
                : words;
            return string.Join(" ", wordsToJoin);
        }

        /// <summary>
        /// Disconnects then reconnects ports, so that all connections treat this as a newly connected node
        /// </summary>
        private void Reconnect()
        {
            // Reconnect the satisfied port to propagate the condition change.
            foreach (var edge in outputSatisfiedPort.connections)
            {
                edge.input.Disconnect(edge);
                edge.input.Connect(edge);
            }
        }

        protected override ConditionBuilder CreateObjectReference()
        {
            var type = typeField.index >= 0 ? conditionTypes[typeField.value] : conditionTypes.Values.First();
            return (ConditionBuilder)ScriptableObject.CreateInstance(type);
        }

        protected override void RenderObjectReference()
        {
            // Clear any existing elements.
            foreach (var element in conditionElements)
            {
                element.RemoveFromHierarchy();
            }
            conditionElements.Clear();

            // Set up the loaded condition's elements.
            if (ObjectReference is ConditionBuilder condition)
            {
                // Show the condition type in the type field.
                typeField.index = conditionTypes.Keys.ToList().IndexOf(condition.GetType().Name);

                // Iterate over all the visible serialized properties of the condition.
                var iterator = SerializedObject.GetIterator();
                iterator.NextVisible(true);
                while (iterator.NextVisible(false))
                {
                    // Create an extension field for this property so that it can be customised within the graph.
                    var field = iterator.CreateFieldElement();
                    extensionContainer.Add(field);
                    conditionElements.Add(field);
                }
                extensionContainer.Bind(SerializedObject);
            }
            else
            {
                // Clear the type field if there is no condition.
                typeField.index = -1;
            }
        }
    }
}
