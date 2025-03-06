using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("Career Progression")]
    [NodeContext(typeof(CareerProgressionGraph))]
    public class CareerProgressionNode : ScriptableGraphNode
    {
        public const string IssuePortName = "Issue";
        public const string FulfilledPortName = "Fulfilled";
        public const string RejectedPortName = "Rejected";

        private ContractGraph contractGraph;
        private ScriptableContract contract;

        private readonly Button assetButton;
        private readonly UnityEditor.Search.ObjectField assetField;
        private readonly ObservablePort issuePort;
        private readonly ObservablePort fulfilledPort;
        private readonly ObservablePort rejectedPort;

        public CareerProgressionNode() : base()
        {
            title = "Career Progression";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.6f, 0.3f, 0.3f));

            // Add an input port for the previous career progression node to issue this progression through.
            issuePort = ObservablePort.Create<Edge>(IssuePortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ScriptableCareerProgression));
            inputContainer.Add(issuePort);

            // Add an output port for the next career progression nodes when fulfilled.
            fulfilledPort = ObservablePort.Create<Edge>(FulfilledPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCareerProgression));
            outputContainer.Add(fulfilledPort);

            // Add an output port for the next career progression nodes when rejected.
            rejectedPort = ObservablePort.Create<Edge>(RejectedPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCareerProgression));
            outputContainer.Add(rejectedPort);

            // Add a field to select a scriptable contract.
            assetField = new()
            {
                searchContext = SearchService.CreateContext("Assets"),
                objectType = typeof(ContractGraph),
            };
            inputContainer.Add(assetField);

            // If a contract graph is assigned, clear the contract field.
            assetField.RegisterValueChangedCallback((e) =>
            {
                if (e.newValue is ContractGraph contractGraph)
                {
                    this.contractGraph = contractGraph;
                    this.contract = null;
                }
                else if (e.newValue is ScriptableContract contract)
                {
                    this.contract = contract;
                    this.contractGraph = null;
                }
            });

            // Add a button to toggle between asset types. TODO: make the asset field just detect and change when dragged in?
            assetButton = new(() =>
            {
                assetField.objectType = (assetField.objectType == typeof(ContractGraph)) ? typeof(ScriptableContract) : typeof(ContractGraph);
                assetField.SetValueWithoutNotify(null);
            });
            assetButton.text = "Toggle Asset Type";
            inputContainer.Add(assetButton);
        }

        public override ScriptableGraphNodeModel Save()
        {
            var model = base.Save();
            if (contractGraph != null)
            {
                model.Asset = contractGraph;
            }
            else if (contract != null)
            {
                model.Asset = contract;
            }
            return model;
        }

        public override void Load(ScriptableGraphNodeModel model)
        {
            base.Load(model);
            if (model.Asset is ContractGraph contractGraph)
            {
                assetField.value = contractGraph;
            }
            else if (model.Asset is ScriptableContract contract)
            {
                assetField.value = contract;
            }
        }
    }
}
