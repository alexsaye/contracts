using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    public class CareerProgressionBuilder : ScriptableObject, IBuilder<ICareerProgression>
    {
        [SerializeField]
        private IBuilder<IContract> contract;

        [SerializeField]
        private List<CareerProgressionBuilder> nextOnFulfilled = new List<CareerProgressionBuilder>();
        public IEnumerable<CareerProgressionBuilder> NextOnFulfilled => nextOnFulfilled;

        [SerializeField]
        private List<CareerProgressionBuilder> nextOnRejected = new List<CareerProgressionBuilder>();
        public IEnumerable<CareerProgressionBuilder> NextOnRejected => nextOnRejected;

        public ICareerProgression Build(UnityEvent updated)
        {
            // Build the contract, if one is provided, otherwise never pass this progression.
            var contract = this.contract != null
                ? this.contract.Build(updated)
                : new Contract(Condition.Never);

            // Set up the enumerations which will build the next progressions.
            var nextOnRejected = this.nextOnRejected.Select(nextOnRejected => nextOnRejected.Build(updated));
            var nextOnFulfilled = this.nextOnFulfilled.Select(nextOnFulfilled => nextOnFulfilled.Build(updated));

            var careerProgression = new CareerProgression(contract, nextOnFulfilled, nextOnRejected);
            return careerProgression;
        }
    }
}
