using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Saye.Contracts.Scripting
{
    [CreateAssetMenu(menuName = "Contracts/Conditions/Composite/All")]
    public class ScriptableAllCondition : ScriptableCondition
    {
        [SerializeField]
        private ScriptableCondition[] subconditions;

        public override ICondition Build(UnityEvent update)
        {
            return Condition.All(subconditions.Select(subcondition => subcondition.Build(update)).ToList());
        }
    }
}
