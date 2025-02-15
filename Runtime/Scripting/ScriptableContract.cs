using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Saye.Contracts.Scripting
{
    [CreateAssetMenu(fileName = "New Contract", menuName = "Contracts/Blank Contract")]
    public class ScriptableContract : ScriptableObject
    {
        [SerializeField]
        protected ScriptableCondition fulfilling;

        [SerializeField]
        protected ScriptableCondition rejecting;

        [SerializeField]
        protected List<ScriptableContract> nextOnFulfilled = new List<ScriptableContract>();
        public IEnumerable<ScriptableContract> NextOnFulfilled => nextOnFulfilled;

        [SerializeField]
        protected List<ScriptableContract> nextOnRejected = new List<ScriptableContract>();
        public IEnumerable<ScriptableContract> NextOnRejected => nextOnRejected;

        public virtual IContract Build(UnityEvent update)
        {
            Debug.Log($"Building contract: {this}");
            var fulfilling = this.fulfilling != null ? this.fulfilling.Build(update) : Condition.Never;
            var rejecting = this.rejecting != null ? this.rejecting.Build(update) : Condition.Never;
            var contract = new Contract(fulfilling, rejecting);
            return contract;
        }
    }
}
