using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("Composite")]
    [NodeContext(typeof(ContractGraph))]
    [NodeCapabilities(~Capabilities.Resizable)]
    public class CompositeConditionNode : ScriptableGraphNode, IConditionNode
    {
        public const string SubconditionsPortName = "Subconditions";
        public const string SatisfiedPortName = "Satisfied";
        public const string DissatisfiedPortName = "Dissatisfied";

        public ScriptableCondition Condition => condition;

        private CompositeScriptableCondition condition;

        private readonly EnumField modeField;
        private readonly ObservablePort subconditionsPort;
        private readonly ObservablePort satisfiedPort;
        private readonly ObservablePort dissatisfiedPort;

        public CompositeConditionNode() : base()
        {
            title = "Composite";

            condition = ScriptableObject.CreateInstance<CompositeScriptableCondition>();

            // Add an enum dropdown for selecting the composite mode.
            modeField = new EnumField(CompositeScriptableCondition.CompositeMode.All);
            inputContainer.Add(modeField);

            // Add an input port for the subconditions.
            subconditionsPort = ObservablePort.Create<Edge>(SubconditionsPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ScriptableCondition));
            inputContainer.Add(subconditionsPort);

            subconditionsPort.Connected += HandlePortConnection;
            subconditionsPort.Disconnected += HandlePortDisconnection;

            // Add an output port for if the condition is satisfied.
            satisfiedPort = ObservablePort.Create<Edge>(SatisfiedPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCondition));
            outputContainer.Add(satisfiedPort);

            // Add an output port for if the condition is dissatisfied.
            dissatisfiedPort = ObservablePort.Create<Edge>(DissatisfiedPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCondition));
            outputContainer.Add(dissatisfiedPort);
        }

        private void HandlePortConnection(object sender, PortConnectionEventArgs e)
        {

        }

        private void HandlePortDisconnection(object sender, PortConnectionEventArgs e)
        {

        }

        public override ScriptableGraphNodeModel Save()
        {
            var model = base.Save();
            model.Asset = condition;
            return model;
        }

        public override void Load(ScriptableGraphNodeModel model)
        {
            base.Load(model);
            condition = (CompositeScriptableCondition)model.Asset;
        }
    }
}
