using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("Career Progression")]
    [NodeContext(typeof(CareerGraph))]
    public class CareerProgressionNode : ScriptableGraphNode
    {
        [NodeInput(Port.Capacity.Multi)]
        public bool From;

        [NodeOutput(Port.Capacity.Single)]
        public bool Fulfill;

        [NodeOutput(Port.Capacity.Single)]
        public bool Reject;

        public ContractGraph Contract;

        public CareerProgressionNode() : base("Career Progression", new Color(0.6f, 0.6f, 0.3f))
        {
            var contractField = new ObjectField("Contract")
            {
                objectType = typeof(ContractGraph),
                searchContext = SearchService.CreateContext("Assets")
            };
            contractField.RegisterValueChangedCallback((evt) => Contract = (ContractGraph)evt.newValue);
            extensionContainer.Add(contractField);
        }

        public override NodeSaveData Save()
        {
            var nodeSave = base.Save();
            nodeSave.Item = Contract;
            return nodeSave;
        }

        public override void Load(NodeSaveData saveData)
        {
            base.Load(saveData);
            Contract = (ContractGraph)saveData.Item;
        }
    }
}
