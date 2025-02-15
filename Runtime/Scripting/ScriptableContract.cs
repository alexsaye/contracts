using System.Collections.Generic;
using System.Linq;
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
            Debug.Log($"Building contract {name}");

            // Build the contract and its conditions immediately.
            var fulfilling = this.fulfilling != null ? this.fulfilling.Build(updated) : Condition.Never;
            var rejecting = this.rejecting != null ? this.rejecting.Build(updated) : Condition.Never;
            var contract = new Contract(fulfilling, rejecting);

            // Create enumerations which defer building the next progression contracts until enumerated by the career progression.
            var nextOnRejected = this.nextOnRejected.Select(nextOnRejected => nextOnRejected.Build(updated));
            var nextOnFulfilled = this.nextOnFulfilled.Select(nextOnFulfilled => nextOnFulfilled.Build(updated));
            return new CareerProgression(contract, nextOnFulfilled, nextOnRejected);
        }
    }
}
