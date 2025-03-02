
using Contracts.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts
{
    [CreateAssetMenu(menuName = "Contracts/Asset/Conditions/Composite")]

    public class CompositeScriptableCondition : ScriptableCondition
    {
        [Serializable]
        public enum CompositeType
        {
            All,
            Any,
        }

        [SerializeField]
        private CompositeType compositeType = CompositeType.All;

        [SerializeField]
        private List<ScriptableCondition> expectSatisfied;

        [SerializeField]
        private List<ScriptableCondition> expectDissatisfied;

        public override ICondition Build(UnityEvent updated)
        {
            // Build all the subconditions.
            var expectSatisfied = this.expectSatisfied.Select(condition => condition.Build(updated)).ToArray();
            var expectDissatisfied = this.expectDissatisfied.Select(condition => condition.Build(updated)).ToArray();

            // Create the composite condition from the conditions.
            return Condition.Composite(compositeType switch
            {
                CompositeType.All => () => expectSatisfied.All(condition => condition.State) && expectDissatisfied.All(condition => !condition.State),
                CompositeType.Any => () => expectSatisfied.Any(condition => condition.State) || expectDissatisfied.Any(condition => !condition.State),
                _ => throw new NotImplementedException(),
            }, expectSatisfied);
        }

        private void OnValidate()
        {
            // TODO: disallow same condition in satisfied and dissatisfied.
        }
    }
}
