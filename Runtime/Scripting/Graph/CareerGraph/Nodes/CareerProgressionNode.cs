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
        public const string InputPortName = "Issued";
        public const string OutputFulfillPortName = "Fulfilled";
        public const string OutputRejectPortName = "Rejected";

        public ScriptableCareerProgression CareerProgression => (ScriptableCareerProgression)serializedObject.targetObject;
        private SerializedObject serializedObject;

        private readonly Button assetButton;
        private readonly UnityEditor.Search.ObjectField assetField;
        private readonly ObservablePort inputPort;
        private readonly ObservablePort outputFulfillPort;
        private readonly ObservablePort outputRejectPort;

        public CareerProgressionNode() : base()
        {
            title = "Career Progression";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.6f, 0.3f, 0.3f));

            // Add an input port for the previous career progression node to issue this progression through.
            inputPort = ObservablePort.Create<Edge>(InputPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ScriptableCareerProgression));
            inputContainer.Add(inputPort);

            // Add an output port for the next career progression nodes when fulfilled.
            outputFulfillPort = ObservablePort.Create<Edge>(OutputFulfillPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCareerProgression));
            outputContainer.Add(outputFulfillPort);

            // Add an output port for the next career progression nodes when rejected.
            outputRejectPort = ObservablePort.Create<Edge>(OutputRejectPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCareerProgression));
            outputContainer.Add(outputRejectPort);

            // Add a field to select a scriptable contract.
            assetField = new()
            {
                searchContext = SearchService.CreateContext("Assets"),
            };
            inputContainer.Add(assetField);

            // Add a button to toggle between asset types.
            assetButton = new(ToggleAssetType);
            assetButton.text = "Toggle Asset Type";
            inputContainer.Add(assetButton);
        }

        public override ScriptableGraphNodeModel Save()
        {
            var model = base.Save();
            model.Asset = CareerProgression;
            return model;
        }

        public override void Load(ScriptableGraphNodeModel model)
        {
            base.Load(model);
            ScriptableCareerProgression careerProgression;
            if (model != null && model.Asset is ScriptableCareerProgression loadedCareerProgression)
            {
                careerProgression = loadedCareerProgression;
            }
            else
            {
                careerProgression = ScriptableObject.CreateInstance<ScriptableCareerProgression>();
            }

            serializedObject = new SerializedObject(careerProgression);
            mainContainer.Bind(serializedObject);

            // TODO: for some reason this isn't detecting the contract
            if (serializedObject.FindProperty("contract").objectReferenceValue != null)
            {
                UseContractAssetType();
            }
            else
            {
                UseContractGraphAssetType();
            }
        }

        private void UseContractAssetType()
        {
            Debug.Log("Using contract asset type.");
            assetField.objectType = typeof(ScriptableContract);
            assetField.bindingPath = "contract";
        }

        private void UseContractGraphAssetType()
        {
            Debug.Log("Using contract graph asset type.");
            assetField.objectType = typeof(ScriptableContractGraph);
            assetField.bindingPath = "contractGraph";
        }

        private void ToggleAssetType()
        {
            assetField.value = null;
            if (assetField.objectType == typeof(ScriptableContractGraph))
            {
                UseContractAssetType();
            }
            else
            {
                UseContractGraphAssetType();
            }
        }
    }
}
