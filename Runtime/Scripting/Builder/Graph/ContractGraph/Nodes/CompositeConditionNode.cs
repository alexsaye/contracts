using NUnit.Framework.Constraints;
using SimpleGraph;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting
{
    [NodeMenu("Composite")]
    [NodeContext(typeof(ContractGraph))]
    [NodeCapabilities(~Capabilities.Resizable)]
    public class CompositeConditionNode : SimpleGraphViewNode<ConditionBuilder>
    {
        public const string InputSubconditionsPortName = "Subconditions";
        public const string OutputSatisfiedPortName = "Satisfied";

        public ConditionBuilder Condition => (ConditionBuilder)ObjectReference;

        private readonly EnumField modeField;
        private readonly ObservablePort inputSubconditionsPort;
        private readonly ObservablePort outputSatisfiedPort;

        public CompositeConditionNode() : base()
        {
            title = "Composite";

            // Add an enum dropdown for selecting the composite mode.
            modeField = new(CompositeConditionBuilder.CompositeMode.All)
            {
                bindingPath = "mode",
            };
            inputContainer.Add(modeField);

            // Add an input port for the subconditions.
            inputSubconditionsPort = ObservablePort.Create<Edge>(InputSubconditionsPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ConditionBuilder));
            inputSubconditionsPort.Connected += HandleSubconditionConnected;
            inputSubconditionsPort.Disconnected += HandleSubconditionDisconnected;
            inputContainer.Add(inputSubconditionsPort);

            // Add an output port for if the composition is satisfied.
            outputSatisfiedPort = ObservablePort.Create<Edge>(OutputSatisfiedPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ConditionBuilder));
            outputContainer.Add(outputSatisfiedPort);
        }

        private void HandleSubconditionConnected(object sender, PortConnectionEventArgs e)
        {
            var condition = (SimpleGraphViewNode<ConditionBuilder>)e.Edge.output.node;
            var subconditionsProperty = SerializedObject.FindProperty("subconditions");
            var index = subconditionsProperty.arraySize;
            subconditionsProperty.InsertArrayElementAtIndex(index);
            subconditionsProperty.GetArrayElementAtIndex(index).objectReferenceValue = condition.ObjectReference;
            SerializedObject.ApplyModifiedProperties();
            Debug.Log($"Composite {modeField.value} condition connected to{condition.ObjectReference} subcondition.");
        }

        private void HandleSubconditionDisconnected(object sender, PortConnectionEventArgs e)
        {
            var subconditionsProperty = SerializedObject.FindProperty("subconditions");
            subconditionsProperty.DeleteArrayElementAtIndex(subconditionsProperty.arraySize - 1);
            SerializedObject.ApplyModifiedProperties();
            Debug.Log($"Composite {modeField.value} condition subcondition disconnected.");
        }
        protected override void RenderObjectReference()
        {
            inputContainer.Bind(SerializedObject);
        }
    }
}
