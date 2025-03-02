using UnityEditor.Experimental.GraphView;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("Any")]
    [NodeContext(typeof(ContractGraph))]
    [NodeCapabilities(~Capabilities.Resizable)]
    public class AnyNode : ScriptableGraphNode
    {
        private CompositeScriptableCondition condition;
        public CompositeScriptableCondition Condition => condition;

        private readonly ObservablePort subconditionsPort;
        private readonly ObservablePort satisfiedPort;
        private readonly ObservablePort dissatisfiedPort;

        public AnyNode() : base()
        {
            title = "Any";

            condition = CompositeScriptableCondition.CreateInstance(CompositeScriptableCondition.CompositeType.Any);

            // Add an input port for the subconditions.
            subconditionsPort = ObservablePort.Create<Edge>("Subconditions", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ScriptableCondition));
            inputContainer.Add(subconditionsPort);

            // Add an output port for if the condition is satisfied.
            satisfiedPort = ObservablePort.Create<Edge>("Satisfied", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCondition));
            outputContainer.Add(satisfiedPort);

            // Add an output port for if the condition is dissatisfied.
            dissatisfiedPort = ObservablePort.Create<Edge>("Dissatisfied", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCondition));
            outputContainer.Add(dissatisfiedPort);
        }
    }
}
