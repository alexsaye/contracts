using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
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
            // Add a dropdown field to select the type of condition.
            var conditionField = new DropdownField("Type", conditionTypes.Keys.ToList(), -1, FormatConditionName, FormatConditionName);
            conditionField.RegisterValueChangedCallback((e) => LoadCondition(conditionTypes[e.newValue]));
            inputContainer.Add(conditionField);
        }

        private string FormatConditionName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                // TODO: how do we handle creating nodes with none?
                // maybe the context menu should supply all the types instead and theyre constructed with it so there is no none option?
                // maybe there shouldn't be a dropdown at all and we pass the type selected in the dropdown into the constructor (I think that's probably better but this works for now)
                return "None";
            }

            var words = System.Text.RegularExpressions.Regex.Split(typeName, @"(?=[A-Z])");
            var wordsToJoin = words.Take(words.Last().Equals("Condition") ? words.Length - 1 : words.Length);
            return string.Join(" ", wordsToJoin);
        }

        private void LoadCondition(Type type)
        {
            LoadCondition((ScriptableCondition)ScriptableObject.CreateInstance(type));
        }

        private void LoadCondition(ScriptableCondition condition)
        {
            // Clear the previously rendered condition's elements.
            foreach (var element in conditionElements)
            {
                element.RemoveFromHierarchy();
            }
            conditionElements.Clear();

            this.condition = condition;
            if (condition != null)
            {
                if (condition is AllCondition || condition is AnyCondition)
                {
                    // TODO: this should be its own node really
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
            LoadCondition((ScriptableCondition)nodeSave.Item);
        }
    }
}
