using SimpleGraph;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting
{
    [NodeMenu("Composite")]
    [NodeContext(typeof(ContractGraph))]
    [NodeCapabilities(~Capabilities.Resizable)]
    public class CompositeConditionNode : SimpleGraphNode
    {
        public const string InputSubconditionsPortName = "Subconditions";
        public const string OutputSatisfiedPortName = "Satisfied";

        private readonly EnumField modeField;
        private readonly Port inputSubconditionsPort;
        private readonly Port outputSatisfiedPort;

        private CompositeConditionBuilder value;

        public CompositeConditionNode() : base()
        {
            title = "Composite";

            // Add an enum dropdown for selecting the composite mode.
            modeField = new();
            inputContainer.Add(modeField);

            // Add an input port for the subconditions.
            inputSubconditionsPort = SimpleGraphUtils.CreatePort<ConditionBuilder>(InputSubconditionsPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputSubconditionsPort);

            // Add an output port for if the composition is satisfied.
            outputSatisfiedPort = SimpleGraphUtils.CreatePort<ConditionBuilder>(OutputSatisfiedPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            outputContainer.Add(outputSatisfiedPort);
        }

        protected override void RenderModel()
        {
            modeField.bindingPath = SerializedNodeModel
                .FindPropertyRelative("value")
                .FindPropertyRelative("mode")
                .propertyPath;
            inputContainer.Bind(SerializedNodeModel.serializedObject);
        }

        public override object GetDefaultValue()
        {
            return new CompositeConditionBuilder();
        }
    }
}
