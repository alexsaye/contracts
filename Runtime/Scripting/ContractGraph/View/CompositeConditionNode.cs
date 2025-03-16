using SimpleGraph;
using System.Collections.Generic;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor;

namespace Contracts.Scripting
{
    [NodeCapabilities(~Capabilities.Resizable)]
    [NodeMenu("Composite")]
    [NodeView(typeof(CompositeConditionNodeModel))]
    public class CompositeConditionNode : SimpleGraphNode
    {
        public const string InputSubconditionsPortName = "Subconditions";
        public const string OutputSatisfiedPortName = "Satisfied";

        private readonly EnumField modeField;
        private readonly Port inputSubconditionsPort;
        private readonly Port outputSatisfiedPort;

        public CompositeConditionNode() : base()
        {
            title = "Composite";

            // Add an enum dropdown for selecting the composite mode.
            modeField = new();
            inputContainer.Add(modeField);

            // Add an input port for the subconditions.
            inputSubconditionsPort = SimpleGraphUtils.CreatePort<IConditionBuilder>(InputSubconditionsPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputSubconditionsPort);

            // Add an output port for if the composition is satisfied.
            outputSatisfiedPort = SimpleGraphUtils.CreatePort<IConditionBuilder>(OutputSatisfiedPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            outputContainer.Add(outputSatisfiedPort);
        }

        public override SimpleGraphNodeModel CreateDefaultModel()
        {
            return new CompositeConditionNodeModel();
        }

        protected override void RenderModel(SerializedProperty serializedNodeModel)
        {
            var serializedMode = serializedNodeModel.FindPropertyRelative(nameof(CompositeConditionNodeModel.Mode));
            modeField.bindingPath = serializedMode.propertyPath;
        }
    }
}
