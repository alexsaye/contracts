using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor;
using SimpleGraph.Editor;
using Contracts.Scripting;

namespace Contracts.Editor.Scripting
{
    [SimpleGraphNodeCapabilities(~Capabilities.Resizable)]
    [SimpleGraphNodeMenu(typeof(ContractGraph), "Composite")]
    [SimpleGraphNodeView(typeof(CompositeConditionNode))]
    public class CompositeConditionNodeView : SimpleGraphNodeView
    {
        private readonly EnumField modeField;
        private readonly Port inputSubconditionsPort;
        private readonly Port outputSatisfiedPort;

        public CompositeConditionNodeView() : base()
        {
            title = "Composite";

            // Add an input port for the subconditions.
            inputSubconditionsPort = CreatePort<IConditionBuilder>("Subconditions", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputSubconditionsPort);

            // Add an output port for if the composition is satisfied.
            outputSatisfiedPort = CreatePort<IConditionBuilder>("Satisfied", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            outputContainer.Add(outputSatisfiedPort);

            // Add an enum dropdown for selecting the composite mode.
            modeField = new();
            extensionContainer.Add(modeField);
            RefreshExpandedState();
        }

        protected override void RenderModel(SerializedProperty serializedNode)
        {
            var serializedMode = serializedNode.FindPropertyRelative(nameof(CompositeConditionNode.Mode));
            modeField.bindingPath = serializedMode.propertyPath;
            extensionContainer.Bind(serializedNode.serializedObject);
        }
    }
}
