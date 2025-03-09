using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    public class ContractBuilder : ScriptableObject, IBuilder<IContract>
    {
        [SerializeField]
        private ConditionBuilder fulfilling;

        [SerializeField]
        private ConditionBuilder rejecting;

        public IContract Build(UnityEvent updated)
        {
            // Build the fulfilling condition, if one is provided, otherwise never fulfill the contract.
            var fulfilling = this.fulfilling != null
                ? this.fulfilling.Build(updated)
                : Condition.Never;

            // Build the rejecting condition, if one is provided, otherwise never reject the contract.
            var rejecting = this.rejecting != null
                ? this.rejecting.Build(updated)
                : Condition.Never;

            var contract = new Contract(fulfilling, rejecting);
            return contract;
        }
    }
}
