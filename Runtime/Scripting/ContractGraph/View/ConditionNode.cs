using SimpleGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Contracts.Scripting
{
    [NodeCapabilities(~Capabilities.Resizable)]
    [NodeMenu("Condition")]
    [NodeView(typeof(ConditionNodeModel))]
    public class ConditionNode : SimpleGraphNode
    {
        public const string OutputSatisfiedPortName = "Satisfied";

        private static readonly IReadOnlyDictionary<string, Type> selectableBuilders = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IConditionBuilder).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                .ToDictionary(type => type.Name, type => type);

        private static string FormatBuilderName(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return "Select a type...";
            }

            var words = Regex.Split(key, @"(?=[A-Z])");
            var wordsToJoin = words[words.Length - 1].Equals("Builder")
                ? words[words.Length - 2].Equals("Condition")
                    ? words.Take(words.Length - 2)
                    : words.Take(words.Length - 1)
                : words;
            return string.Join(" ", wordsToJoin);
        }

        private readonly List<VisualElement> renderedElements = new();
        private readonly DropdownField typeField;
        private readonly Port outputSatisfiedPort;
        private readonly EventCallback<ChangeEvent<string>> typeFieldCallback;

        public ConditionNode() : base()
        {
            title = "Condition";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.6f));

            // Add an output port for when the condition is satisfied.
            outputSatisfiedPort = SimpleGraphUtils.CreatePort<IConditionBuilder>(OutputSatisfiedPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            outputContainer.Add(outputSatisfiedPort);

            // Add a dropdown type field to select a condition builder type.
            typeField = new(selectableBuilders.Keys.ToList(), -1, FormatBuilderName, FormatBuilderName);
            inputContainer.Add(typeField);
        }

        public override SimpleGraphNodeModel CreateDefaultModel()
        {
            return new ConditionNodeModel();
        }

        protected override void RenderModel(SerializedProperty serializedNodeModel)
        {
            // Change the condition builder to the selected type when the type field changes.
            if (typeFieldCallback != null)
            {
                typeField.UnregisterCallback(typeFieldCallback);
            }
            typeField.RegisterValueChangedCallback((e) => ReplaceBuilder(serializedNodeModel, selectableBuilders[e.newValue]));

            // Show the appropriate choice in the type field for the condition builder (or lack thereof).
            var serializedBuilder = serializedNodeModel.FindPropertyRelative(nameof(ConditionNodeModel.Builder));
            var builder = serializedBuilder.managedReferenceValue;
            typeField.index = builder != null ? typeField.choices.IndexOf(builder.GetType().Name) : -1;

            // Render the builder (or just clear if there isn't one).
            RenderBuilder(serializedNodeModel);
        }

        private void ReplaceBuilder(SerializedProperty serializedNodeModel, Type type)
        {
            var serializedBuilder = serializedNodeModel.FindPropertyRelative(nameof(ConditionNodeModel.Builder));
            serializedBuilder.managedReferenceValue = Activator.CreateInstance(type);
            serializedNodeModel.serializedObject.ApplyModifiedProperties();
            RenderBuilder(serializedNodeModel);
            RefreshPorts();
            RefreshExpandedState();
        }

        private void RenderBuilder(SerializedProperty serializedNodeModel)
        {
            // Clear any elements which were rendered for the previous condition builder.
            foreach (var element in renderedElements)
            {
                element.RemoveFromHierarchy();
            }
            renderedElements.Clear();

            // Get the condition builder from the model.
            var serializedBuilder = serializedNodeModel.FindPropertyRelative(nameof(ConditionNodeModel.Builder));

            // Check whether the condition builder has any visible serialized properties.
            var serializedProperty = serializedBuilder.Copy();
            if (!serializedProperty.hasVisibleChildren || !serializedProperty.NextVisible(true))
            {
                return;
            }

            // Create bound property fields for all top level visible serialized properties of the condition builder.
            do
            {
                var propertyField = new PropertyField(serializedProperty);
                extensionContainer.Add(propertyField);
                renderedElements.Add(propertyField);
            } while (serializedProperty.NextVisible(false) && serializedProperty.depth > serializedBuilder.depth);
        }
    }
}
