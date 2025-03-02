using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting.Graph
{
    [CreateAssetMenu(fileName = "New Contract Graph", menuName = "Contracts/Contract Graph")]
    public class ContractGraph : ScriptableGraph
    {
        private void Awake()
        {
            // Add a default contract node.
            var contractNode = new NodeSaveData(typeof(ContractNode), new Rect(300, 0, 0, 0));
            Nodes.Add(contractNode);

            // Add a default condition node.
            var conditionNode = new NodeSaveData(typeof(ConditionNode), new Rect(-300, 0, 0, 0));
            Nodes.Add(conditionNode);
        }
    }
}
