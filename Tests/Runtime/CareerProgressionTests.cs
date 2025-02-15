using NUnit.Framework;
using System.Linq;

namespace Saye.Contracts.Tests
{
    public class CareerProgressionTests
    {
        [Test]
        public void CareerProgressionPending()
        {
            var contract = new Contract(Condition.Never);
            var promotionContract = new Contract(Condition.Never);
            var demotionContract = new Contract(Condition.Never);
            var progression = new CareerProgression(contract, promotionContract, demotionContract);
            Assert.AreEqual(0, progression.CurrentState.Count(), "Has not progressed.");
        }

        [Test]
        public void CareerProgressionFulfilled()
        {
            var contract = new Contract(Condition.Always);
            var promotionContract = new Contract(Condition.Never);
            var demotionContract = new Contract(Condition.Never);
            var progression = new CareerProgression(contract, promotionContract, demotionContract);
            Assert.AreEqual(1, progression.CurrentState.Count(), "Has a single progression.");
            Assert.AreSame(promotionContract, progression.CurrentState.First().Contract, "Has progressed to the promotion.");
        }

        [Test]
        public void CareerProgressionRejected()
        {
            var contract = new Contract(Condition.Never, Condition.Always);
            var promotionContract = new Contract(Condition.Never);
            var demotionContract = new Contract(Condition.Never);
            var progression = new CareerProgression(contract, promotionContract, demotionContract);
            Assert.AreEqual(1, progression.CurrentState.Count(), "Has a single progression.");
            Assert.AreSame(demotionContract, progression.CurrentState.First().Contract, "Has progressed to the demotion.");
        }
    }
}
