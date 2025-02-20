using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    [CreateAssetMenu(menuName = "Contracts/Conditions/Composite/Any")]
    public class ScriptableAnyCondition : ScriptableCondition
    {
        [SerializeField]
        private ScriptableCondition[] subconditions;

        public override ICondition Build(UnityEvent update)
        {
            return Condition.Any(subconditions.Select(subcondition => subcondition.Build(update)).ToList());
        }
    }
}
