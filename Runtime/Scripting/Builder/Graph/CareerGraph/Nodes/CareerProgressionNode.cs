using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEditor;
using UnityEditor.UIElements;
using SimpleGraph;

namespace Contracts.Scripting
{
    [NodeMenu("Career Progression")]
    [NodeContext(typeof(CareerProgressionGraph))]
    public class CareerProgressionNode : SimpleGraphViewNode<CareerProgressionBuilder>
    {
        public const string InputIssuedPortName = "Issued";
        public const string OutputFulfilledPortName = "Fulfilled";
        public const string OutputRejectedPortName = "Rejected";

        private readonly UnityEditor.Search.ObjectField assetField;
        private readonly ObservablePort inputIssuedPort;
        private readonly ObservablePort outputFulfillPort;
        private readonly ObservablePort outputRejectPort;

        public CareerProgressionNode() : base()
        {
            title = "Career Progression";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.6f));

            // Add an input port for the previous career progression node to issue this progression through.
            inputIssuedPort = ObservablePort.Create<Edge>(InputIssuedPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(CareerProgressionBuilder));
            inputContainer.Add(inputIssuedPort);

            // Add an output port for the next career progression nodes when fulfilled.
            outputFulfillPort = ObservablePort.Create<Edge>(OutputFulfilledPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(CareerProgressionBuilder));
            outputFulfillPort.Connected += HandleFulfilledConnected;
            outputFulfillPort.Disconnected += HandleFulfilledDisconnected;
            outputContainer.Add(outputFulfillPort);

            // Add an output port for the next career progression nodes when rejected.
            outputRejectPort = ObservablePort.Create<Edge>(OutputRejectedPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(CareerProgressionBuilder));
            outputRejectPort.Connected += HandleRejectedConnected;
            outputRejectPort.Disconnected += HandleRejectedDisconnected;
            outputContainer.Add(outputRejectPort);

            // Add a field to select a scriptable contract.
            assetField = new()
            {
                searchContext = SearchService.CreateContext("Assets"),
                objectType = typeof(ContractGraph),
                bindingPath = "contract",
            };
            inputContainer.Add(assetField);
        }

        private void HandleFulfilledConnected(object sender, PortConnectionEventArgs args)
        {
            if (args.Edge.input.node is CareerProgressionNode progression)
            {
                var nextOnFulfilledProperty = SerializedObject.FindProperty("nextOnFulfilled");
                var index = nextOnFulfilledProperty.arraySize;
                nextOnFulfilledProperty.InsertArrayElementAtIndex(index);
                nextOnFulfilledProperty.GetArrayElementAtIndex(index).objectReferenceValue = progression.ObjectReference;
                SerializedObject.ApplyModifiedProperties();
                Debug.Log($"Career progression fulfilled port connected to{progression} progression.");
            }
        }

        private void HandleFulfilledDisconnected(object sender, PortConnectionEventArgs args)
        {
            if (args.Edge.input.node is CareerProgressionNode progression)
            {
                var nextOnFulfilledProperty = SerializedObject.FindProperty("nextOnFulfilled");
                var index = nextOnFulfilledProperty.arraySize;
                nextOnFulfilledProperty.DeleteArrayElementAtIndex(index);
                SerializedObject.ApplyModifiedProperties();
                Debug.Log($"Career progression fulfilled port disconnected.");
            }
        }

        private void HandleRejectedConnected(object sender, PortConnectionEventArgs args)
        {
            if (args.Edge.input.node is CareerProgressionNode progression)
            {
                var nextOnRejectedProperty = SerializedObject.FindProperty("nextOnRejected");
                var index = nextOnRejectedProperty.arraySize;
                nextOnRejectedProperty.InsertArrayElementAtIndex(index);
                nextOnRejectedProperty.GetArrayElementAtIndex(index).objectReferenceValue = progression.ObjectReference;
                SerializedObject.ApplyModifiedProperties();
                Debug.Log($"Career progression rejected port connected to{progression} progression.");
            }
        }

        private void HandleRejectedDisconnected(object sender, PortConnectionEventArgs args)
        {
            if (args.Edge.input.node is CareerProgressionNode progression)
            {
                var nextOnRejectedProperty = SerializedObject.FindProperty("nextOnRejected");
                var index = nextOnRejectedProperty.arraySize;
                nextOnRejectedProperty.DeleteArrayElementAtIndex(index);
                SerializedObject.ApplyModifiedProperties();
                Debug.Log($"Career progression rejected port disconnected.");
            }
        }

        protected override CareerProgressionBuilder CreateObjectReference()
        {
            return ScriptableObject.CreateInstance<CareerProgressionBuilder>();
        }

        protected override void RenderObjectReference()
        {
            inputContainer.Bind(SerializedObject);
        }
    }
}
