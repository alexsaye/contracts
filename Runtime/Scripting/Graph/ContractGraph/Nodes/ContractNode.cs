using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(ScriptableContractGraph))]
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    public class ContractNode : ScriptableGraphNode
    {
        public const string InputFulfilledPortName = "Fulfilled";
        public const string InputRejectedPortName = "Rejected";

        public ScriptableContract Contract => (ScriptableContract)Asset;

        private readonly ObservablePort inputFulfilledPort;
        private readonly ObservablePort inputRejectedPort;

        public ContractNode() : base()
        {
            title = "Contract";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.6f, 0.3f));

            // Add an input port for the conditions which fulfil the contract.
            inputFulfilledPort = ObservablePort.Create<Edge>(InputFulfilledPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(IConditionNode));
            inputFulfilledPort.Connected += HandleFulfilledConnected;
            inputFulfilledPort.Disconnected += HandleFulfilledDisconnected;
            inputContainer.Add(inputFulfilledPort);

            // Add an input port for the conditions which reject the contract.
            inputRejectedPort = ObservablePort.Create<Edge>(InputRejectedPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(IConditionNode));
            inputRejectedPort.Connected += HandleRejectedConnected;
            inputRejectedPort.Disconnected += HandleRejectedDisconnected;
            inputContainer.Add(inputRejectedPort);
        }

        private void HandleFulfilledConnected(object sender, PortConnectionEventArgs args)
        {
            var condition = (IConditionNode)args.Edge.output.node;
            SerializedAsset.FindProperty("fulfilling").objectReferenceValue = condition.Condition;
            SerializedAsset.ApplyModifiedProperties();
            Debug.Log($"Contract fulfilled port connected to{condition.Condition} condition.");
        }

        private void HandleFulfilledDisconnected(object sender,PortConnectionEventArgs args)
        {
            SerializedAsset.FindProperty("fulfilling").objectReferenceValue = null;
            SerializedAsset.ApplyModifiedProperties();
            Debug.Log($"Contract fulfilled port disconnected.");
        }

        private void HandleRejectedConnected(object sender, PortConnectionEventArgs args)
        {
            var condition = (IConditionNode)args.Edge.output.node;
            SerializedAsset.FindProperty("rejecting").objectReferenceValue = condition.Condition;
            SerializedAsset.ApplyModifiedProperties();
            Debug.Log($"Contract rejected port connected to{condition.Condition} condition.");
        }

        private void HandleRejectedDisconnected(object sender, PortConnectionEventArgs args)
        {
            SerializedAsset.FindProperty("rejecting").objectReferenceValue = null;
            SerializedAsset.ApplyModifiedProperties();
            Debug.Log($"Contract rejected port disconnected.");
        }

        protected override ScriptableObject CreateDefaultAsset()
        {
            return ScriptableObject.CreateInstance<ScriptableContract>();
        }
    }
}
