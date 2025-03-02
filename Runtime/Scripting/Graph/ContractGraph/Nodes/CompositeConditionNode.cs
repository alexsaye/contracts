
using System;
using UnityEditor;
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
            subconditionsPort = ObservablePort.Create<Edge>("Subconditions", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ScriptableCondition));
            inputContainer.Add(subconditionsPort);

            subconditionsPort.Connected += HandlePortConnection;
            subconditionsPort.Disconnected += HandlePortDisconnection;

            // Add an output port for if the condition is satisfied.
            satisfiedPort = ObservablePort.Create<Edge>("Satisfied", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCondition));
            outputContainer.Add(satisfiedPort);

            // Add an output port for if the condition is dissatisfied.
            dissatisfiedPort = ObservablePort.Create<Edge>("Dissatisfied", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCondition));
            outputContainer.Add(dissatisfiedPort);
        }

        private void HandlePortConnection(object sender, PortConnectionEventArgs e)
        {

        }

        private void HandlePortDisconnection(object sender, PortConnectionEventArgs e)
        {

        }

        public override NodeSaveData Save()
        {
            var nodeSave = base.Save();
            nodeSave.Value = condition;
            return nodeSave;
        }

        public override void Load(NodeSaveData saveData)
        {
            base.Load(saveData);
            condition = (CompositeScriptableCondition)saveData.Value;
        }
    }
}
