using NUnit.Framework;
using System.Linq;

namespace Saye.Contracts.Tests
{
    public class CareerProgressionTests
    {
        [Test]
        public void CareerProgressionPending()
        {
            var contractA = new Contract(Condition.Never, Condition.Never);
            var contractPromotion = new Contract(Condition.Never, Condition.Never);
            var contractDemotion = new Contract(Condition.Never, Condition.Never);
            var progression = new CareerProgression(contractA, contractPromotion, contractDemotion);
            Assert.AreEqual(0, progression.CurrentState.Count(), "Has not progressed.");
        }

        [Test]
        public void CareerProgressionFulfilled()
        {
            var contractA = new Contract(Condition.Always, Condition.Never);
            var contractPromotion = new Contract(Condition.Never, Condition.Never);
            var contractDemotion = new Contract(Condition.Never, Condition.Never);
            var progression = new CareerProgression(contractA, contractPromotion, contractDemotion);
            Assert.AreEqual(1, progression.CurrentState.Count(), "Has a single progression.");
            Assert.AreSame(contractPromotion, progression.CurrentState.First().Contract, "Has progressed to contract Promotion.");
        }

        [Test]
        public void CareerProgressionRejected()
        {
            var contractA = new Contract(Condition.Never, Condition.Always);
            var contractPromotion = new Contract(Condition.Never, Condition.Never);
            var contractDemotion = new Contract(Condition.Never, Condition.Never);
            var progression = new CareerProgression(contractA, contractPromotion, contractDemotion);
            Assert.AreEqual(1, progression.CurrentState.Count(), "Has a single progression.");
            Assert.AreSame(contractDemotion, progression.CurrentState.First().Contract, "Has progressed to contract Demotion.");
        }
    }
}
