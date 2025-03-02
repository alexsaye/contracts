using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("Career Progression")]
    [NodeContext(typeof(CareerProgressionGraph))]
    public class CareerProgressionNode : ScriptableGraphNode
    {
        private ContractGraph contractGraph;
        private ScriptableContract contract;

        private readonly Button assetButton;
        private readonly UnityEditor.Search.ObjectField assetField;
        private readonly Port previousPort;
        private readonly Port fulfilledPort;
        private readonly Port rejectedPort;

        public CareerProgressionNode() : base("Career Progression", new Color(0.6f, 0.6f, 0.3f))
        {
            // Add an input port for the previous career progression node.
            previousPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(CareerProgressionNode));
            previousPort.name = previousPort.portName = "Previous";
            inputContainer.Add(previousPort);

            // Add an output port for the next career progression nodes when fulfilled.
            fulfilledPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(CareerProgressionNode));
            fulfilledPort.name = fulfilledPort.portName = "Fulfilled";
            outputContainer.Add(fulfilledPort);

            // Add an output port for the next career progression nodes when rejected.
            rejectedPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(CareerProgressionNode));
            rejectedPort.name = rejectedPort.portName = "Rejected";
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

        public override NodeSaveData Save()
        {
            var nodeSave = base.Save();
            if (contractGraph != null)
            {
                nodeSave.Value = contractGraph;
            }
            else if (contract != null)
            {
                nodeSave.Value = contract;
            }
            return nodeSave;
        }

        public override void Load(NodeSaveData saveData)
        {
            base.Load(saveData);
            if (saveData.Value is ContractGraph contractGraph)
            {
                assetField.value = contractGraph;
            }
            else if (saveData.Value is ScriptableContract contract)
            {
                assetField.value = contract;
            }
        }
    }
}
