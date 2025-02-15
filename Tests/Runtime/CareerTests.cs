using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Saye.Contracts.Tests
{
    public class CareerTests
    {
        [Test]
        public void CareerIssues()
        {
            var career = new Career();
            var history = new List<IContract>();
            void handleIssued(object sender, IssuedEventArgs e) => history.Add(e.Contract);
            career.Issued += handleIssued;

            var fulfilledContract = new Contract(Condition.Always);
            career.Issue(fulfilledContract);
            Assert.AreEqual(1, history.Count, "Has issued one contract.");
            Assert.AreSame(fulfilledContract, history.Last(), "Has issued the fulfilled contract.");
            Assert.AreEqual(0, career.Pending.Count(), "Has no pending contracts.");

            var pendingContract = new Contract(Condition.Never);
            career.Issue(pendingContract);
            Assert.AreEqual(2, history.Count, "Has issued a second contract.");
            Assert.AreSame(pendingContract, history.Last(), "Has issued the pending contract.");
            Assert.AreEqual(1, career.Pending.Count(), "Has one pending contract.");
            Assert.AreSame(pendingContract, career.Pending.Last(), "Has contract Pending pending.");
        }

        [Test]
        public void CareerIssuesNextOnFulfilled()
        {
            var career = new Career();
            var history = new List<IContract>();
            void handleIssued(object sender, IssuedEventArgs e) => history.Add(e.Contract);
            career.Issued += handleIssued;

            var contract = new Contract(Condition.Always);
            var promotionContract = new Contract(Condition.Never);
            var demotionContract = new Contract(Condition.Never);
            career.Issue(new CareerProgression(contract, new ICareerProgression[] { new CareerProgression(promotionContract) }, new ICareerProgression[] { new CareerProgression(demotionContract) }));
            Assert.AreEqual(2, history.Count, "Has issued two contracts.");
            Assert.AreSame(contract, history.First(), "Has issued the given contract first.");
            Assert.AreSame(promotionContract, history.Last(), "Has issued the promotion next.");
            Assert.AreEqual(1, career.Pending.Count(), "Has one pending contract.");
            Assert.AreSame(promotionContract, career.Pending.Last(), "Has the promotion pending.");
        }

        [Test]
        public void CareerIssuesNextOnRejected()
        {
            var career = new Career();
            var history = new List<IContract>();
            void handleIssued(object sender, IssuedEventArgs e) => history.Add(e.Contract);
            career.Issued += handleIssued;

            var contract = new Contract(Condition.Never, Condition.Always);
            var promotionContract = new Contract(Condition.Never);
            var demotionContract = new Contract(Condition.Never);
            career.Issue(new CareerProgression(contract, new ICareerProgression[] { new CareerProgression(promotionContract) }, new ICareerProgression[] { new CareerProgression(demotionContract) }));
            Assert.AreEqual(2, history.Count, "Has issued two contracts.");
            Assert.AreSame(contract, history.First(), "Has issued the given contract first.");
            Assert.AreSame(demotionContract, history.Last(), "Has issued the demotion next.");
            Assert.AreEqual(1, career.Pending.Count(), "Has one pending contract.");
            Assert.AreSame(demotionContract, career.Pending.Last(), "Has the demotion pending.");
        }
    }
}
