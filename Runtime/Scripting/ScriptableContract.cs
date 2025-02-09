using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Saye.Contracts.Scripting
{
    [CreateAssetMenu(fileName = "New Contract", menuName = "Contracts/Blank Contract")]
    public class ScriptableContract : ScriptableObject
    {
        [SerializeField]
        protected ScriptableCondition obligation;

        [SerializeField]
        protected ScriptableCondition violation;

        [SerializeField]
        protected List<ScriptableContract> nextOnFulfilled = new List<ScriptableContract>();
        public IEnumerable<ScriptableContract> NextOnFulfilled => nextOnFulfilled;

        [SerializeField]
        protected List<ScriptableContract> nextOnBreached = new List<ScriptableContract>();
        public IEnumerable<ScriptableContract> NextOnBreached => nextOnBreached;

        public virtual IContract Build(UnityEvent update)
        {
            Debug.Log($"Building contract: {this}");
            var obligation = this.obligation != null ? this.obligation.Build(update) : Condition.Never;
            var violation = this.violation != null ? this.violation.Build(update) : Condition.Never;
            var contract = Contract.Observe(obligation, violation);
            return contract;
        }
    }
}
