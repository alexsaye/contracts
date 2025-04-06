using NUnit.Framework;

namespace Contracts.Tests
{
    public class ConditionTests
    {
        [Test]
        public void ConditionInitiallyDissatisfied()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();

            var satisfied = true;
            condition.StateUpdated += (sender, e) => satisfied = e.State;

            Assert.IsFalse(satisfied, "Should be dissatisfied initially.");
        }

        [Test]
        public void ConditionUpdatesOnSatisfaction()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();

            var satisfied = false;
            condition.StateUpdated += (sender, e) => satisfied = e.State;

            subject.Satisfy();
            Assert.IsTrue(satisfied, "Should be satisfied after event.");
        }

        [Test]
        public void ConditionUpdatesOnDissatisfaction()
        {
            var subject = new MockSubject(true);
            var condition = subject.AsCondition();

            var satisfied = true;
            condition.StateUpdated += (sender, e) => satisfied = e.State;

            subject.Dissatisfy();
            Assert.IsFalse(satisfied, "Should be dissatisfied after event.");
        }

        [Test]
        public void ConditionDoesNotReactAfterUnsubscribe()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();

            var satisfied = false;
            void handleState(object sender, StateUpdatedEventArgs<bool> e) => satisfied = e.State;

            condition.StateUpdated += handleState;
            condition.StateUpdated -= handleState;

            subject.Satisfy();
            Assert.IsFalse(satisfied, "Should not change state after unsubscribe.");
        }

        [Test]
        public void AllConditionRequiresAllToSatisfy()
        {
            var subjectA = new MockSubject(false);
            var subjectB = new MockSubject(false);
            var condition = Condition.All(subjectA.AsCondition(), subjectB.AsCondition());

            var satisfied = true;
            condition.StateUpdated += (sender, e) => satisfied = e.State;

            subjectA.Satisfy();
            Assert.IsFalse(satisfied, "Should not be satisfied when only A is satisfied.");

            subjectB.Satisfy();
            Assert.IsTrue(satisfied, "Should be satisfied when both A and B are satisfied.");
        }

        [Test]
        public void AnyConditionRequiresAtLeastOneToSatisfy()
        {
            var subjectA = new MockSubject(false);
            var subjectB = new MockSubject(false);
            var condition = Condition.Any(subjectA.AsCondition(), subjectB.AsCondition());

            var satisfied = false;
            condition.StateUpdated += (sender, e) => satisfied = e.State;

            subjectA.Satisfy();
            Assert.IsTrue(satisfied, "Should be satisfied when A is satisfied.");

            subjectA.Dissatisfy();
            Assert.IsFalse(satisfied, "Should be dissatisfied when both A and B are dissatisfied.");

            subjectB.Satisfy();
            Assert.IsTrue(satisfied, "Should be satisfied when B is satisfied.");
        }
    }

}
