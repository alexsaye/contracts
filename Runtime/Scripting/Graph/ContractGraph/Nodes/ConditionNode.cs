using Codice.Client.Common.GameUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("Condition")]
    [NodeContext(typeof(ContractGraph))]
    [NodeCapabilities(~Capabilities.Resizable)]
    public class ConditionNode : ScriptableGraphNode
    {
        private static readonly IReadOnlyDictionary<string, Type> conditionTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(ScriptableCondition)) && !type.IsAbstract)
                .ToDictionary(type => type.Name, type => type);

        [NodeOutput(Port.Capacity.Single)]
        public bool Satisfied;

        [NodeOutput(Port.Capacity.Single)]
        public bool Dissatisfied;

        private ScriptableCondition condition;
        private readonly List<VisualElement> conditionElements = new();

        public ConditionNode() : base("Condition", new Color(0.3f, 0.3f, 0.6f))
        {
            // Add a dropdown field to select a condition type.
            var typeField = new DropdownField("Type", conditionTypes.Keys.ToList(), -1, FormatConditionName, FormatConditionName);
            inputContainer.Add(typeField);

            // Add an object field to select a condition asset.
            var assetField = new ObjectField("Asset")
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
            });

            // If the asset field is given a valid asset, set the condition instance to the selected asset and clear the type field.
            assetField.RegisterValueChangedCallback((e) =>
            {
                if (e.newValue != null)
                {
                    typeField.index = -1;
                    condition = (ScriptableCondition)e.newValue;
                    RefreshConditionElements();
                }
            });
        }

        private string FormatConditionName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return "Select a type...";
            }
            return (typeName.EndsWith("Condition") ? typeName.Substring(0, typeName.Length - "Condition".Length) : typeName).CamelToTitle();
        }

        private void RefreshConditionElements()
        {
            // Clear existing condition elements.
            foreach (var element in conditionElements)
            {
                element.RemoveFromHierarchy();
            }
            conditionElements.Clear();

            // Create new elements if there is a condition assigned.
            if (condition != null)
            {
                if (condition is AllCondition || condition is AnyCondition)
                {
                    var input = Port.Create<Edge>(
                        orientation: Orientation.Horizontal,
                        direction: Direction.Input,
                        capacity: Port.Capacity.Multi,
                        type: typeof(bool));
                    input.portName = "Subconditions";
                    inputContainer.Add(input);
                    conditionElements.Add(input);
                }
                else
                {
                    // Get all public fields and fields marked with SerializeField.
                    var fields = condition
                        .GetType()
                        .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(field => field.IsPublic || field.GetCustomAttribute<SerializeField>() != null);

                    // Create input elements for each field.
                    foreach (var field in fields)
                    {
                        var input = CreateFieldInput(field, condition);
                        extensionContainer.Add(input);
                        conditionElements.Add(input);
                    }
                }
            }
            RefreshExpandedState();
        }

        public override NodeSaveData Save()
        {
            var nodeSave = base.Save();
            nodeSave.Item = condition;
            return nodeSave;
        }

        public override void Load(NodeSaveData nodeSave)
        {
            base.Load(nodeSave);
            condition = (ScriptableCondition)nodeSave.Item;
            RefreshConditionElements();
        }
    }
}
