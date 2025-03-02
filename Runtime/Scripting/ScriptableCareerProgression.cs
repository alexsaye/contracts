using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    [CreateAssetMenu(fileName = "New Career Progression", menuName = "Contracts/Asset/Career Progression")]
    public class ScriptableCareerProgression : ScriptableObject
    {
        [SerializeField]
        private ScriptableContract contract;
        public ScriptableContract Contract => contract;

        [SerializeField]
        private List<ScriptableCareerProgression> nextOnFulfilled = new List<ScriptableCareerProgression>();
        public IEnumerable<ScriptableCareerProgression> NextOnFulfilled => nextOnFulfilled;

        [SerializeField]
        private List<ScriptableCareerProgression> nextOnRejected = new List<ScriptableCareerProgression>();
        public IEnumerable<ScriptableCareerProgression> NextOnRejected => nextOnRejected;

        public virtual ICareerProgression Build(UnityEvent updated)
        {
            Debug.Log($"Building career progression {name}");

            // Build the contract so it can start evaluating its conditions.
            var contract = this.contract.Build(updated);

            // Set up the enumerations which will build the next progressions.
            var nextOnRejected = this.nextOnRejected.Select(nextOnRejected => nextOnRejected.Build(updated));
            var nextOnFulfilled = this.nextOnFulfilled.Select(nextOnFulfilled => nextOnFulfilled.Build(updated));

            return new CareerProgression(contract, nextOnFulfilled, nextOnRejected);
        }
    }
}
