using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("Condition")]
    [NodeContext(typeof(ContractGraph))]
    [NodeCapabilities(~Capabilities.Resizable)]
    public class ConditionNode : ScriptableGraphNode, IConditionNode
    {
        private static readonly IReadOnlyDictionary<string, Type> conditionTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(ScriptableCondition)) && !type.IsAbstract)
                .ToDictionary(type => type.Name, type => type);

        public ScriptableCondition Condition => condition;

        private ScriptableCondition condition;
        private SerializedObject serializedCondition;

        private readonly List<VisualElement> conditionElements = new();
        private readonly DropdownField typeField;
        private readonly UnityEditor.Search.ObjectField assetField;
        private readonly ObservablePort satisfiedPort;
        private readonly ObservablePort dissatisfiedPort;

        public ConditionNode() : base()
        {
            title = "Condition";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.6f));

            // Add a dropdown field to select a condition type.
            typeField = new(conditionTypes.Keys.ToList(), -1, FormatConditionName, FormatConditionName);
            inputContainer.Add(typeField);

            // Add an object field to select a condition asset.
            assetField = new()
            {
                objectType = typeof(ScriptableCondition),
                searchContext = SearchService.CreateContext("Assets"),
            };
            inputContainer.Add(assetField);

            // If the type field is given a valid type, create a new condition instance of that type and clear the asset field.
            typeField.RegisterValueChangedCallback((e) =>
            {
                if (!string.IsNullOrEmpty(e.newValue))
                {
                    assetField.value = null;
                    condition = (ScriptableCondition)ScriptableObject.CreateInstance(conditionTypes[e.newValue]);
                }
                RefreshConditionElements();
                ReconnectSatisfactionPorts();
            });

            // If the asset field is given a valid asset, set the condition instance to the selected asset and clear the type field.
            assetField.RegisterValueChangedCallback((e) =>
            {
                if (e.newValue != null)
                {
                    typeField.index = -1;
                    condition = (ScriptableCondition)e.newValue;
                }
                RefreshConditionElements();
                ReconnectSatisfactionPorts();
            });

            // Add an output port for if the condition is satisfied.
            satisfiedPort = ObservablePort.Create<Edge>("Satisfied", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCondition));
            outputContainer.Add(satisfiedPort);

            // Add an output port for if the condition is dissatisfied.
            dissatisfiedPort = ObservablePort.Create<Edge>("Dissatisfied", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCondition));
            outputContainer.Add(dissatisfiedPort);
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

        private void RefreshConditionElements()
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
                return;
            }

            // Serialize the condition to create property inputs with data binding.
            serializedCondition = new SerializedObject(condition);

            // Iterate over all the visible serialized properties of the condition.
            var iterator = serializedCondition.GetIterator();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false))
            {
                // Create an extension field for this property so that it can be customised within the graph.
                var field = iterator.CreateFieldElement();
                extensionContainer.Add(field);
                conditionElements.Add(field);
            }
            mainContainer.Bind(serializedCondition);

            RefreshPorts();
            RefreshExpandedState();
        }

        // TODO: this might be necessary to forward changes to the composite nodes. review
        private void ReconnectSatisfactionPorts()
        {
            // Reconnect the satisfied port to propagate the condition change.
            foreach (var edge in satisfiedPort.connections)
            {
                edge.input.Disconnect(edge);
                edge.input.Connect(edge);
            }

            // Reconnect the dissatisfied port to propagate the condition change.
            foreach (var edge in dissatisfiedPort.connections)
            {
                edge.input.Disconnect(edge);
                edge.input.Connect(edge);
            }
        }

        public override NodeSaveData Save()
        {
            var nodeSave = base.Save();
            nodeSave.Value = condition;
            return nodeSave;
        }

        public override void Load(NodeSaveData nodeSave)
        {
            base.Load(nodeSave);
            if (nodeSave.Value != null)
            {
                condition = (ScriptableCondition)nodeSave.Value;
                if (AssetDatabase.Contains(condition))
                {
                    // The condition is a separate game asset, so update the asset field.
                    assetField.value = condition;
                }
                else
                {
                    // The condition is nested within the graph, so update the type field.
                    typeField.index = conditionTypes.Keys.ToList().IndexOf(condition.GetType().Name);
                }
            }
            RefreshConditionElements();
        }
    }
}
