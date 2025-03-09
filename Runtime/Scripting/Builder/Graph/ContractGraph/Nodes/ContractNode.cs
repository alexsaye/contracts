using SimpleGraph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting
{
    [NodeContext(typeof(ContractGraph))]
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    public class ContractNode : SimpleGraphViewNode<ContractBuilder>
    {
        public const string InputFulfilledPortName = "Fulfilled";
        public const string InputRejectedPortName = "Rejected";

        private readonly ObservablePort inputFulfilledPort;
        private readonly ObservablePort inputRejectedPort;

        public ContractNode() : base()
        {
            title = "Contract";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.6f, 0.3f));

            // Add an input port for the conditions which fulfil the contract.
            inputFulfilledPort = ObservablePort.Create<Edge>(InputFulfilledPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ConditionBuilder));
            //inputFulfilledPort.Connected += HandleFulfilledConnected;
            //inputFulfilledPort.Disconnected += HandleFulfilledDisconnected;
            inputContainer.Add(inputFulfilledPort);

            // Add an input port for the conditions which reject the contract.
            inputRejectedPort = ObservablePort.Create<Edge>(InputRejectedPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ConditionBuilder));
            //inputRejectedPort.Connected += HandleRejectedConnected;
            //inputRejectedPort.Disconnected += HandleRejectedDisconnected;
            inputContainer.Add(inputRejectedPort);
        }

        private void HandleFulfilledConnected(object sender, PortConnectionEventArgs args)
        {
            var condition = (SimpleGraphViewNode<ConditionBuilder>)args.Edge.output.node;
            SerializedObject.FindProperty("fulfilling").objectReferenceValue = condition.ObjectReference;
            SerializedObject.ApplyModifiedProperties();
            Debug.Log($"Contract fulfilled port connected to{condition.ObjectReference} condition.");
        }

        private void HandleFulfilledDisconnected(object sender,PortConnectionEventArgs args)
        {
            SerializedObject.FindProperty("fulfilling").objectReferenceValue = null;
            SerializedObject.ApplyModifiedProperties();
            Debug.Log($"Contract fulfilled port disconnected.");
        }

        private void HandleRejectedConnected(object sender, PortConnectionEventArgs args)
        {
            var condition = (SimpleGraphViewNode<ConditionBuilder>)args.Edge.output.node;
            SerializedObject.FindProperty("rejecting").objectReferenceValue = condition.ObjectReference;
            SerializedObject.ApplyModifiedProperties();
            Debug.Log($"Contract rejected port connected to{condition.ObjectReference} condition.");
        }

        private void HandleRejectedDisconnected(object sender, PortConnectionEventArgs args)
        {
            SerializedObject.FindProperty("rejecting").objectReferenceValue = null;
            SerializedObject.ApplyModifiedProperties();
            Debug.Log($"Contract rejected port disconnected.");
        }

        protected override void RenderObjectReference()
        {
        }
    }
}
