using SimpleGraph;
using System;
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
    public class ConditionNode : SimpleGraphNode
    {
        public const string OutputSatisfiedPortName = "Satisfied";

        private static readonly IReadOnlyDictionary<string, Type> selectableBuilders = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(ConditionBuilder)) && !type.IsAbstract)
                .Where(type => type != typeof(CompositeConditionBuilder))
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

        public ConditionNode() : base()
        {
            title = "Condition";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.6f));

            // Add an output port for when the condition is satisfied.
            outputSatisfiedPort = SimpleGraphUtils.CreatePort<ConditionBuilder>(OutputSatisfiedPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            outputContainer.Add(outputSatisfiedPort);

            // Add a dropdown type field to select a condition builder type.
            typeField = new(selectableBuilders.Keys.ToList(), -1, FormatBuilderName, FormatBuilderName);
            inputContainer.Add(typeField);

            // When the type field is changed, change the condition builder to the selected type.
            typeField.RegisterValueChangedCallback((e) => ReplaceBuilder(selectableBuilders[e.newValue]));
        }

        protected void ReplaceBuilder(Type type)
        {
            var builder = (ConditionBuilder)Activator.CreateInstance(type); ;
            Debug.Log($"Created builder: {builder}");

            SerializedNodeModel.FindPropertyRelative("value").managedReferenceValue = builder;
            SerializedNodeModel.serializedObject.ApplyModifiedProperties();
            RenderModel();
            RefreshPorts();
            RefreshExpandedState();
        }

        protected override void RenderModel()
        {
            // Clear any elements which were rendered for the previous condition builder.
            foreach (var element in renderedElements)
            {
                element.RemoveFromHierarchy();
            }
            renderedElements.Clear();

            // Get the condition builder from the model.
            var serializedValue = SerializedNodeModel.FindPropertyRelative("value");
            var builder = serializedValue.managedReferenceValue;

            // If there is no condition builder assigned, reset the type field to show the default message.
            if (builder == null)
            {
                typeField.index = -1;
                return;
            }

            // Show the appropriate choice in the type field for the condition builder.
            typeField.index = typeField.choices.IndexOf(builder.GetType().Name);

            // Check whether the condition builder has any visible serialized properties.
            var serializedProperty = serializedValue.Copy();
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
            } while (serializedProperty.NextVisible(false) && serializedProperty.depth > serializedValue.depth);
            extensionContainer.Bind(SerializedNodeModel.serializedObject);
        }
    }
}
