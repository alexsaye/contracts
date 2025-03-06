using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("Condition")]
    [NodeContext(typeof(ScriptableContractGraph))]
    [NodeCapabilities(~Capabilities.Resizable)]
    public class ConditionNode : ScriptableGraphNode, IConditionNode
    {
        public const string OutputPortName = "Satisfied";

        private static readonly IReadOnlyDictionary<string, System.Type> conditionTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(ScriptableCondition)) && !type.IsAbstract)
                .Where(type => type != typeof(CompositeScriptableCondition))
                .ToDictionary(type => type.Name, type => type);

        public ScriptableCondition Condition => serializedObject != null ? (ScriptableCondition)serializedObject.targetObject : null;
        private SerializedObject serializedObject;

        private readonly List<VisualElement> conditionElements = new();
        private readonly DropdownField typeField;
        private readonly ObservablePort outputPort;

        public ConditionNode() : base()
        {
            title = "Condition";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.6f));

            // Add a dropdown field to select a condition type.
            typeField = new(conditionTypes.Keys.ToList(), -1, FormatConditionName, FormatConditionName);
            inputContainer.Add(typeField);

            // If the type field is given a valid type, create a new condition instance of that type and clear the asset field.
            typeField.RegisterValueChangedCallback((e) =>
            {
                var selectedCondition = string.IsNullOrEmpty(e.newValue) ? null : (ScriptableCondition)ScriptableObject.CreateInstance(conditionTypes[e.newValue]);
                ChangeCondition(selectedCondition);
                Reconnect();
            });

            // Add an output port for when the condition is satisfied.
            outputPort = ObservablePort.Create<Edge>(OutputPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(IConditionNode));
            outputContainer.Add(outputPort);
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

        private void ChangeCondition(ScriptableCondition condition)
        {
            // Clear existing elements.
            foreach (var element in conditionElements)
            {
                element.RemoveFromHierarchy();
            }
            conditionElements.Clear();

            // If there is no condition, nothing needs to be added.
            if (condition == null)
            {
                serializedObject = null;
                return;
            }

            // Serialize the condition for data binding.
            serializedObject = new SerializedObject(condition);
            mainContainer.Bind(serializedObject);

            // Iterate over all the visible serialized properties of the condition.
            var iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false))
            {
                // Create an extension field for this property so that it can be customised within the graph.
                var field = iterator.CreateFieldElement();
                extensionContainer.Add(field);
                conditionElements.Add(field);
            }

            RefreshPorts();
            RefreshExpandedState();
        }

        /// <summary>
        /// Disconnects then reconnects ports, so that all connections treat this as a newly connected node.
        /// TODO: maybe actually remove the node and replace with a new one?
        /// </summary>
        private void Reconnect()
        {
            // Reconnect the satisfied port to propagate the condition change.
            foreach (var edge in outputPort.connections)
            {
                edge.input.Disconnect(edge);
                edge.input.Connect(edge);
            }
        }

        public override ScriptableGraphNodeModel Save()
        {
            var model = base.Save();
            model.Asset = Condition;
            return model;
        }

        public override void Load(ScriptableGraphNodeModel model)
        {
            base.Load(model);
            if (model != null && model.Asset is ScriptableCondition loadedCondition)
            {
                typeField.index = conditionTypes.Keys.ToList().IndexOf(loadedCondition.GetType().Name);
                ChangeCondition(loadedCondition);
            }
            else
            {
                typeField.index = -1;
                ChangeCondition(null);
            }
        }
    }
}
