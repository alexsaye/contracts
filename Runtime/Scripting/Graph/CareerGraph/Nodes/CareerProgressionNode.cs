using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEditor;
using UnityEditor.UIElements;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("Career Progression")]
    [NodeContext(typeof(ScriptableCareerProgressionGraph))]
    public class CareerProgressionNode : ScriptableGraphNode
    {
        public const string InputIssuedPortName = "Issued";
        public const string OutputFulfilledPortName = "Fulfilled";
        public const string OutputRejectedPortName = "Rejected";

        public ScriptableCareerProgression CareerProgression => (ScriptableCareerProgression)Asset;

        private readonly UnityEditor.Search.ObjectField assetField;
        private readonly ObservablePort inputIssuedPort;
        private readonly ObservablePort outputFulfillPort;
        private readonly ObservablePort outputRejectPort;

        public CareerProgressionNode() : base()
        {
            title = "Career Progression";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.6f));

            // Add an input port for the previous career progression node to issue this progression through.
            inputIssuedPort = ObservablePort.Create<Edge>(InputIssuedPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ScriptableCareerProgression));
            inputContainer.Add(inputIssuedPort);

            // Add an output port for the next career progression nodes when fulfilled.
            outputFulfillPort = ObservablePort.Create<Edge>(OutputFulfilledPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCareerProgression));
            outputFulfillPort.Connected += HandleFulfilledConnected;
            outputFulfillPort.Disconnected += HandleFulfilledDisconnected;
            outputContainer.Add(outputFulfillPort);

            // Add an output port for the next career progression nodes when rejected.
            outputRejectPort = ObservablePort.Create<Edge>(OutputRejectedPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCareerProgression));
            outputRejectPort.Connected += HandleRejectedConnected;
            outputRejectPort.Disconnected += HandleRejectedDisconnected;
            outputContainer.Add(outputRejectPort);

            // Add a field to select a scriptable contract.
            assetField = new()
            {
                searchContext = SearchService.CreateContext("Assets"),
                objectType = typeof(ScriptableContractGraph),
                bindingPath = "contractGraph",
            };
            inputContainer.Add(assetField);
        }

        private void HandleFulfilledConnected(object sender, PortConnectionEventArgs args)
        {
            if (args.Edge.input.node is CareerProgressionNode progression)
            {
                var nextOnFulfilledProperty = SerializedAsset.FindProperty("nextOnFulfilled");
                var index = nextOnFulfilledProperty.arraySize;
                nextOnFulfilledProperty.InsertArrayElementAtIndex(index);
                nextOnFulfilledProperty.GetArrayElementAtIndex(index).objectReferenceValue = progression.CareerProgression;
                SerializedAsset.ApplyModifiedProperties();
                Debug.Log($"Career progression fulfilled port connected to{progression} progression.");
            }
        }

        private void HandleFulfilledDisconnected(object sender, PortConnectionEventArgs args)
        {
            if (args.Edge.input.node is CareerProgressionNode progression)
            {
                var nextOnFulfilledProperty = SerializedAsset.FindProperty("nextOnFulfilled");
                var index = nextOnFulfilledProperty.arraySize;
                nextOnFulfilledProperty.DeleteArrayElementAtIndex(index);
                SerializedAsset.ApplyModifiedProperties();
                Debug.Log($"Career progression fulfilled port disconnected.");
            }
        }

        private void HandleRejectedConnected(object sender, PortConnectionEventArgs args)
        {
            if (args.Edge.input.node is CareerProgressionNode progression)
            {
                var nextOnRejectedProperty = SerializedAsset.FindProperty("nextOnRejected");
                var index = nextOnRejectedProperty.arraySize;
                nextOnRejectedProperty.InsertArrayElementAtIndex(index);
                nextOnRejectedProperty.GetArrayElementAtIndex(index).objectReferenceValue = progression.CareerProgression;
                SerializedAsset.ApplyModifiedProperties();
                Debug.Log($"Career progression rejected port connected to{progression} progression.");
            }
        }

        private void HandleRejectedDisconnected(object sender, PortConnectionEventArgs args)
        {
            if (args.Edge.input.node is CareerProgressionNode progression)
            {
                var nextOnRejectedProperty = SerializedAsset.FindProperty("nextOnRejected");
                var index = nextOnRejectedProperty.arraySize;
                nextOnRejectedProperty.DeleteArrayElementAtIndex(index);
                SerializedAsset.ApplyModifiedProperties();
                Debug.Log($"Career progression rejected port disconnected.");
            }
        }

        protected override ScriptableObject CreateDefaultAsset()
        {
            return ScriptableObject.CreateInstance<ScriptableCareerProgression>();
        }

        protected override void SetupAssetElements()
        {
            inputContainer.Bind(SerializedAsset);
        }
    }
}
