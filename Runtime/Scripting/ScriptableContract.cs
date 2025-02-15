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

        public virtual ICareerProgression Build(UnityEvent updated)
        {
            Debug.Log($"Building contract: {this}");
            var fulfilling = this.fulfilling != null ? this.fulfilling.Build(updated) : Condition.Never;
            var rejecting = this.rejecting != null ? this.rejecting.Build(updated) : Condition.Never;
            var contract = new Contract(fulfilling, rejecting);

            var nextOnRejected = this.nextOnRejected.ConvertAll(nextOnRejected => nextOnRejected.Build(updated));
            var nextOnFulfilled = this.nextOnFulfilled.ConvertAll(nextOnFulfilled => nextOnFulfilled.Build(updated));
            return new CareerProgression(contract, nextOnFulfilled, nextOnRejected);
        }
    }
}
