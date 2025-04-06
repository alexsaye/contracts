using Contracts.Scripting;
using SimpleGraph.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Editor.Scripting
{
    [SimpleGraphNodeCapabilities(~Capabilities.Resizable)]
    [SimpleGraphNodeMenu(typeof(ContractGraph), "Condition")]
    [SimpleGraphNodeView(typeof(ConditionNode))]
    public class ConditionNodeView : SimpleGraphNodeView
    {
        private static readonly IReadOnlyDictionary<string, Type> selectableBuilders = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type != typeof(ConditionNode) && type != typeof(CompositeConditionNode))
                .Where(type => typeof(IConditionBuilder).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                .ToDictionary(type => type.Name, type => type);

        private static string FormatBuilderName(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return "Select a type...";
            }

            var words = Regex.Split(key, @"(?=[A-Z])");
            var wordsToJoin = words[words.Length - 1].Equals("Condition")
                ? words.Take(words.Length - 1)
                : words;
            return string.Join(" ", wordsToJoin);
        }

        private readonly List<VisualElement> builderElements = new();
        private readonly DropdownField typeField;
        private readonly Port outputSatisfiedPort;
        private readonly EventCallback<ChangeEvent<string>> typeFieldCallback;

        public ConditionNodeView() : base()
        {
            title = "Condition";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.6f));

            // Add an output port for when the condition is satisfied.
            outputSatisfiedPort = CreatePort<IConditionBuilder>("", Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            outputContainer.Add(outputSatisfiedPort);

            // Add a dropdown type field to select a condition builder type.
            typeField = new(selectableBuilders.Keys.ToList(), -1, FormatBuilderName, FormatBuilderName);
            inputContainer.Add(typeField);
        }

        protected override void RenderModel(SerializedProperty serializedNode)
        {
            // Change the condition builder to the selected type when the type field changes.
            if (typeFieldCallback != null)
            {
                typeField.UnregisterCallback(typeFieldCallback);
            }
            typeField.RegisterValueChangedCallback((e) => ReplaceBuilder(serializedNode, selectableBuilders[e.newValue]));

            // Show the appropriate choice in the type field for the condition builder (or lack thereof).
            var serializedBuilder = serializedNode.FindPropertyRelative(nameof(ConditionNode.Builder));
            var builder = serializedBuilder.managedReferenceValue;
            typeField.index = builder != null ? typeField.choices.IndexOf(builder.GetType().Name) : -1;

            // Render the builder (or just clear if there isn't one).
            RenderBuilder(serializedNode);
        }

        private void ReplaceBuilder(SerializedProperty serializedNode, Type type)
        {
            var serializedBuilder = serializedNode.FindPropertyRelative(nameof(ConditionNode.Builder));
            serializedBuilder.managedReferenceValue = Activator.CreateInstance(type);
            serializedNode.serializedObject.ApplyModifiedProperties();
            RenderBuilder(serializedNode);
            RefreshExpandedState();
            Debug.Log($"Replaced builder with {type.Name}");
        }

        private void RenderBuilder(SerializedProperty serializedNode)
        {
            // Clear any elements which were rendered for the previous condition builder.
            foreach (var element in builderElements)
            {
                extensionContainer.Remove(element);
            }
            builderElements.Clear();

            // Get the condition builder from the model.
            var serializedBuilder = serializedNode.FindPropertyRelative(nameof(ConditionNode.Builder));

            // Check whether the condition builder has any visible serialized properties.
            var serializedProperty = serializedBuilder.Copy();
            if (!serializedProperty.hasVisibleChildren || !serializedProperty.NextVisible(true))
            {
                return;
            }

            // Create new bound property fields for all top level visible serialized properties of the condition builder.
            extensionContainer.Unbind();
            do
            {
                var propertyField = new PropertyField(serializedProperty);
                extensionContainer.Add(propertyField);
                builderElements.Add(propertyField);
                Debug.Log($"Rendered property {serializedProperty.displayName}");
            } while (serializedProperty.NextVisible(false) && serializedProperty.depth > serializedBuilder.depth);
            extensionContainer.Bind(serializedNode.serializedObject);
        }
    }
}
