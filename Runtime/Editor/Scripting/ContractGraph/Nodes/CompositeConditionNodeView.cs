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
using SimpleGraph.Editor;

namespace Contracts.Scripting
{
    [SimpleGraphNodeCapabilities(~Capabilities.Resizable)]
    [SimpleGraphNodeMenu("Composite")]
    [SimpleGraphNodeModel(typeof(CompositeConditionNodeModel))]
    public class CompositeConditionNodeView : SimpleGraphNodeView
    {
        public const string InputSubconditionsPortName = "Subconditions";
        public const string OutputSatisfiedPortName = "Satisfied";

        private readonly EnumField modeField;
        private readonly Port inputSubconditionsPort;
        private readonly Port outputSatisfiedPort;

        public CompositeConditionNodeView() : base()
        {
            title = "Composite";

            // Add an input port for the subconditions.
            inputSubconditionsPort = CreatePort<IConditionBuilder>(InputSubconditionsPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputSubconditionsPort);

            // Add an output port for if the composition is satisfied.
            outputSatisfiedPort = CreatePort<IConditionBuilder>(OutputSatisfiedPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            outputContainer.Add(outputSatisfiedPort);

            // Add an enum dropdown for selecting the composite mode.
            modeField = new();
            extensionContainer.Add(modeField);
            RefreshExpandedState();
        }

        protected override void RenderModel(SerializedProperty serializedNodeModel)
        {
            var serializedMode = serializedNodeModel.FindPropertyRelative(nameof(CompositeConditionNodeModel.Mode));
            modeField.bindingPath = serializedMode.propertyPath;
            extensionContainer.Bind(serializedNodeModel.serializedObject);
        }
    }
}
