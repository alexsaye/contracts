
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("All")]
    [NodeContext(typeof(ContractGraph))]
    [NodeCapabilities(~Capabilities.Resizable)]
    public class AllNode : ScriptableGraphNode
    {
        private CompositeScriptableCondition condition;
        public CompositeScriptableCondition Condition => condition;

        private readonly ObservablePort subconditionsPort;
        private readonly ObservablePort satisfiedPort;
        private readonly ObservablePort dissatisfiedPort;

        public AllNode() : base()
        {
            title = "All";

            condition = CompositeScriptableCondition.CreateInstance(CompositeScriptableCondition.CompositeType.All);

            // Add an input port for the subconditions.
            subconditionsPort = ObservablePort.Create<Edge>("Subconditions", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ScriptableCondition));
            inputContainer.Add(subconditionsPort);

            // When the subconditions port is connected, add the connected condition to the composite condition.
            subconditionsPort.Connected += (sender, e) =>
            {
                if (e.Edge.input.node is ConditionNode conditionNode)
                {
                    if (e.Edge.input.portName == "Satisfied")
                    {
                        condition.ExpectSatisfied.Add(conditionNode.Condition);
                    }
                    else if (e.Edge.input.portName == "Dissatisfied")
                    {
                        condition.ExpectDissatisfied.Add(conditionNode.Condition);
                    }
                }
            };

            // When the subconditions port is disconnected, remove the connected condition from the composite condition.
            subconditionsPort.Disconnected += (sender, e) =>
            {
                if (e.Edge.input.node is ConditionNode conditionNode)
                {
                    if (e.Edge.input.portName == "Satisfied")
                    {
                        condition.ExpectSatisfied.Remove(conditionNode.Condition);
                    }
                    else if (e.Edge.input.portName == "Dissatisfied")
                    {
                        condition.ExpectDissatisfied.Remove(conditionNode.Condition);
                    }
                }
            };

            // Add an output port for if the condition is satisfied.
            satisfiedPort = ObservablePort.Create<Edge>("Satisfied", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCondition));
            outputContainer.Add(satisfiedPort);

            // Add an output port for if the condition is dissatisfied.
            dissatisfiedPort = ObservablePort.Create<Edge>("Dissatisfied", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCondition));
            outputContainer.Add(dissatisfiedPort);
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
