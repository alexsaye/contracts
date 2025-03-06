using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    [CreateAssetMenu(menuName = "Contracts/Asset/Conditions/Composite")]

    public class CompositeScriptableCondition : ScriptableCondition
    {
        [Serializable]
        public enum CompositeMode
        {
            All,
            Any,
        }

        [SerializeField]
        private CompositeMode mode = CompositeMode.All;

        [SerializeField]
        private List<ScriptableCondition> expectSatisfied;

        [SerializeField]
        private List<ScriptableCondition> expectDissatisfied;

        public void SetMode(CompositeMode mode)
        {
            this.mode = mode;
        }

        public void Add(ScriptableCondition condition)
        {
            if (expectSatisfied == null)
            {
                expectSatisfied = new();
            }
            expectSatisfied.Add(condition);
        }

        public void Remove(ScriptableCondition condition)
        {
            if (expectSatisfied != null)
            {
                expectSatisfied.Remove(condition);
            }
        }

        public override ICondition Build(UnityEvent updated)
        {
            // Build all the subconditions.
            var expectSatisfied = this.expectSatisfied.Select(condition => condition.Build(updated)).ToArray();
            var expectDissatisfied = this.expectDissatisfied.Select(condition => condition.Build(updated)).ToArray();

            // Create the composite condition from the conditions.
            return Condition.Composite(mode switch
            {
                CompositeMode.All => () => expectSatisfied.All(condition => condition.State) && expectDissatisfied.All(condition => !condition.State),
                CompositeMode.Any => () => expectSatisfied.Any(condition => condition.State) || expectDissatisfied.Any(condition => !condition.State),
                _ => throw new NotImplementedException(),
            }, expectSatisfied);
        }

        private void OnValidate()
        {
            // TODO: disallow same condition in satisfied and dissatisfied.
        }
    }
}
