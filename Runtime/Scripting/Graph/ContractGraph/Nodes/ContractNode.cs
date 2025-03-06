using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(ScriptableContractGraph))]
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    public class ContractNode : ScriptableGraphNode
    {
        public const string InputFulfillPortName = "Fulfilled";
        public const string InputRejectPortName = "Rejected";

        public ScriptableContract Contract => (ScriptableContract)serializedObject.targetObject;
        private SerializedObject serializedObject;

        private readonly ObservablePort inputFulfillPort;
        private readonly ObservablePort inputRejectPort;

        public ContractNode() : base()
        {
            title = "Contract";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.6f, 0.3f));

            // Add an input port for the conditions which fulfil the contract.
            inputFulfillPort = ObservablePort.Create<Edge>(InputFulfillPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(IConditionNode));
            inputFulfillPort.Connected += HandleFulfillConnected;
            inputFulfillPort.Disconnected += HandleFulfillDisconnected;
            inputContainer.Add(inputFulfillPort);

            // Add an input port for the conditions which reject the contract.
            inputRejectPort = ObservablePort.Create<Edge>(InputRejectPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(IConditionNode));
            inputRejectPort.Connected += HandleRejectConnected;
            inputRejectPort.Disconnected += HandleRejectDisconnected;
            inputContainer.Add(inputRejectPort);
        }

        private void HandleFulfillConnected(object sender, PortConnectionEventArgs args)
        {
            var condition = (IConditionNode)args.Edge.output.node;
            serializedObject.FindProperty("fulfilling").objectReferenceValue = condition.Condition;
            Debug.Log($"Contract fulfill port connected to{condition.Condition} condition.");
        }

        private void HandleFulfillDisconnected(object sender,PortConnectionEventArgs args)
        {
            serializedObject.FindProperty("fulfilling").objectReferenceValue = null;
            Debug.Log($"Contract fulfill port disconnected.");
        }

        private void HandleRejectConnected(object sender, PortConnectionEventArgs args)
        {
            var condition = (IConditionNode)args.Edge.output.node;
            serializedObject.FindProperty("rejecting").objectReferenceValue = condition.Condition;
            Debug.Log($"Contract reject port connected to{condition.Condition} condition.");
        }

        private void HandleRejectDisconnected(object sender, PortConnectionEventArgs args)
        {
            serializedObject.FindProperty("rejecting").objectReferenceValue = null;
            Debug.Log($"Contract reject port disconnected.");
        }

        public override ScriptableGraphNodeModel Save()
        {
            var model = base.Save();
            model.Asset = Contract;
            return model;
        }

        public override void Load(ScriptableGraphNodeModel model)
        {
            base.Load(model);
            ScriptableContract contract;
            if (model != null && model.Asset is ScriptableContract loadedContract)
            {
                contract = loadedContract;
            }
            else
            {
                contract = ScriptableObject.CreateInstance<ScriptableContract>();
            }
            serializedObject = new SerializedObject(contract);
            mainContainer.Bind(serializedObject);
        }
    }
}
