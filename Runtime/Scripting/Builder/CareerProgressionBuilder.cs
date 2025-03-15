using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    [Serializable]
    public class CareerProgressionBuilder : IBuilder<ICareerProgression>
    {
        [SerializeField]
        private ContractGraph contract;

        [SerializeField]
        private List<CareerProgressionBuilder> nextOnFulfilled;
        public IEnumerable<CareerProgressionBuilder> NextOnFulfilled => nextOnFulfilled;

        [SerializeField]
        private List<CareerProgressionBuilder> nextOnRejected;
        public IEnumerable<CareerProgressionBuilder> NextOnRejected => nextOnRejected;

        public CareerProgressionBuilder() : this(null, new List<CareerProgressionBuilder>(), new List<CareerProgressionBuilder>())
        {
        }

        public CareerProgressionBuilder(ContractGraph contract, IEnumerable<CareerProgressionBuilder> nextOnFulfilled, IEnumerable<CareerProgressionBuilder> nextOnRejected)
        {
            this.contract = contract;
            this.nextOnFulfilled = nextOnFulfilled.ToList();
            this.nextOnRejected = nextOnRejected.ToList();
        }

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
