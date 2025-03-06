using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("Composite")]
    [NodeContext(typeof(ScriptableContractGraph))]
    [NodeCapabilities(~Capabilities.Resizable)]
    public class CompositeConditionNode : ScriptableGraphNode, IConditionNode
    {
        public const string InputSubconditionsPortName = "Subconditions";
        public const string OutputSatisfiedPortName = "Satisfied";

        public ScriptableCondition Condition => (ScriptableCondition)Asset;

        private readonly EnumField modeField;
        private readonly ObservablePort inputSubconditionsPort;
        private readonly ObservablePort outputSatisfiedPort;

        public CompositeConditionNode() : base()
        {
            title = "Composite";

            // Add an enum dropdown for selecting the composite mode.
            modeField = new(CompositeScriptableCondition.CompositeMode.All)
            {
                bindingPath = "mode",
            };
            inputContainer.Add(modeField);

            // Add an input port for the subconditions.
            inputSubconditionsPort = ObservablePort.Create<Edge>(InputSubconditionsPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(IConditionNode));
            inputSubconditionsPort.Connected += HandleSubconditionConnected;
            inputSubconditionsPort.Disconnected += HandleSubconditionDisconnected;
            inputContainer.Add(inputSubconditionsPort);

            // Add an output port for if the composition is satisfied.
            outputSatisfiedPort = ObservablePort.Create<Edge>(OutputSatisfiedPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(IConditionNode));
            outputContainer.Add(outputSatisfiedPort);
        }

        private void HandleSubconditionConnected(object sender, PortConnectionEventArgs e)
        {
            var condition = (IConditionNode)e.Edge.output.node;
            var subconditionsProperty = SerializedAsset.FindProperty("subconditions");
            var index = subconditionsProperty.arraySize;
            subconditionsProperty.InsertArrayElementAtIndex(index);
            subconditionsProperty.GetArrayElementAtIndex(index).objectReferenceValue = condition.Condition;
            SerializedAsset.ApplyModifiedProperties();
            Debug.Log($"Composite {modeField.value} condition connected to{condition.Condition} subcondition.");
        }

        private void HandleSubconditionDisconnected(object sender, PortConnectionEventArgs e)
        {
            var subconditionsProperty = SerializedAsset.FindProperty("subconditions");
            subconditionsProperty.DeleteArrayElementAtIndex(subconditionsProperty.arraySize - 1);
            SerializedAsset.ApplyModifiedProperties();
            Debug.Log($"Composite {modeField.value} condition subcondition disconnected.");
        }

        protected override ScriptableObject CreateDefaultAsset()
        {
            return ScriptableObject.CreateInstance<CompositeScriptableCondition>();
        }

        protected override void SetupAssetElements()
        {
            inputContainer.Bind(SerializedAsset);
        }
    }
}
