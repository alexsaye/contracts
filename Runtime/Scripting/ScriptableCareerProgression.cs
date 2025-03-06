using Contracts.Scripting.Graph;
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
        private ScriptableContractGraph contractGraph;

        [SerializeField]
        private ScriptableContract contract;

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
            IContract contract;
            if (this.contract != null && contractGraph != null)
            {
                Debug.LogError("Cannot have both a contract and a contract graph.");
                return null;
            }
            else if (this.contract != null)
            {
                contract = this.contract.Build(updated);
            }
            else if (contractGraph != null)
            {
                contract = contractGraph.Build(updated);
            }
            else
            {
                Debug.LogError("Must have either a contract or a contract graph.");
                return null;
            }

            // Set up the enumerations which will build the next progressions.
            var nextOnRejected = this.nextOnRejected.Select(nextOnRejected => nextOnRejected.Build(updated));
            var nextOnFulfilled = this.nextOnFulfilled.Select(nextOnFulfilled => nextOnFulfilled.Build(updated));

            return new CareerProgression(contract, nextOnFulfilled, nextOnRejected);
        }
    }
}
