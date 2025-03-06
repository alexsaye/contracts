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
        public const string SubconditionsPortName = "Subconditions";
        public const string SatisfiedPortName = "Satisfied";
        public const string DissatisfiedPortName = "Dissatisfied";

        public ScriptableCondition Condition => (ScriptableCondition)serializedObject.targetObject;
        private SerializedObject serializedObject;

        private readonly EnumField modeField;
        private readonly ObservablePort subconditionsPort;
        private readonly ObservablePort satisfiedPort;

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
            subconditionsPort = ObservablePort.Create<Edge>(SubconditionsPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(IConditionNode));
            subconditionsPort.Connected += HandleSubconditionConnected;
            subconditionsPort.Disconnected += HandleSubconditionDisconnected;
            inputContainer.Add(subconditionsPort);

            // Add an output port for if the composition is satisfied.
            satisfiedPort = ObservablePort.Create<Edge>(SatisfiedPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(IConditionNode));
            outputContainer.Add(satisfiedPort);
        }

        private void HandleSubconditionConnected(object sender, PortConnectionEventArgs e)
        {
            var condition = (IConditionNode)e.Edge.output.node;
            var subconditionsProperty = serializedObject.FindProperty("subconditions");
            var index = subconditionsProperty.arraySize;
            subconditionsProperty.InsertArrayElementAtIndex(index);
            subconditionsProperty.GetArrayElementAtIndex(index).objectReferenceValue = condition.Condition;
            serializedObject.ApplyModifiedProperties();
            Debug.Log($"Composite {modeField.value} condition connected to{condition.Condition} subcondition.");
        }

        private void HandleSubconditionDisconnected(object sender, PortConnectionEventArgs e)
        {
            var subconditionsProperty = serializedObject.FindProperty("subconditions");
            subconditionsProperty.DeleteArrayElementAtIndex(subconditionsProperty.arraySize - 1);
            serializedObject.ApplyModifiedProperties();
            Debug.Log($"Composite {modeField.value} condition subcondition disconnected.");
        }

        public override ScriptableGraphNodeModel Save()
        {
            var model = base.Save();
            model.Asset = Condition;
            return model;
        }

        public override void Load(ScriptableGraphNodeModel model)
        {
            base.Load(model);
            CompositeScriptableCondition condition;
            if (model != null && model.Asset is CompositeScriptableCondition loadedCondition)
            {
                condition = loadedCondition;
            }
            else
            {
                condition = ScriptableObject.CreateInstance<CompositeScriptableCondition>();
            }
            serializedObject = new SerializedObject(condition);
            mainContainer.Bind(serializedObject);
        }
    }
}
