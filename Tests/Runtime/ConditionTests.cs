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
            condition.State += (sender, e) => satisfied = e.CurrentState;

            Assert.IsFalse(satisfied, "Should be dissatisfied initially.");
        }

        [Test]
        public void ConditionUpdatesOnSatisfaction()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();

            var satisfied = false;
            condition.State += (sender, e) => satisfied = e.CurrentState;

            subject.Satisfy();
            Assert.IsTrue(satisfied, "Should be satisfied after event.");
        }

        [Test]
        public void ConditionUpdatesOnDissatisfaction()
        {
            var subject = new MockSubject(true);
            var condition = subject.AsCondition();

            var satisfied = true;
            condition.State += (sender, e) => satisfied = e.CurrentState;

            subject.Dissatisfy();
            Assert.IsFalse(satisfied, "Should be dissatisfied after event.");
        }

        [Test]
        public void ConditionDoesNotReactAfterUnsubscribe()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();

            var satisfied = false;
            void handleState(object sender, StateEventArgs<bool> e) => satisfied = e.CurrentState;

            condition.State += handleState;
            condition.State -= handleState;

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
            condition.State += (sender, e) => satisfied = e.CurrentState;

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
            condition.State += (sender, e) => satisfied = e.CurrentState;

            subjectA.Satisfy();
            Assert.IsTrue(satisfied, "Should be satisfied when A is satisfied.");

            subjectA.Dissatisfy();
            Assert.IsFalse(satisfied, "Should be dissatisfied when both A and B are dissatisfied.");

            subjectB.Satisfy();
            Assert.IsTrue(satisfied, "Should be satisfied when B is satisfied.");
        }
    }

}
