using NUnit.Framework;
using System;

namespace Saye.Contracts.Tests
{
    public class ConditionTests
    {
        [Test]
        public void ConditionSatisfiedOnObserve()
        {
            var condition = Condition.Always;

            var satisfied = false;
            void handleState(object sender, ObservableStateEventArgs<bool> e) => satisfied = e.State;

            condition.State += handleState;
            Assert.IsTrue(satisfied, "Condition should have communicated satisfied on observe.");
        }

        [Test]
        public void ConditionDissatisfiedOnObserve()
        {
            var condition = Condition.Never;

            var satisfied = true;
            void handleState(object sender, ObservableStateEventArgs<bool> e) => satisfied = e.State;

            condition.State += handleState;
            Assert.IsFalse(satisfied, "Condition should have communicated dissatisfied on observe.");
        }

        [Test]
        public void ConditionSatisfiedOnEvent()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();

            var satisfied = true;
            void handleState(object sender, ObservableStateEventArgs<bool> e) => satisfied = e.State;

            condition.State += handleState;
            Assert.IsFalse(satisfied, "Condition should be dissatisfied on observe.");

            subject.Satisfy();
            Assert.IsTrue(satisfied, "Condition should be satisfied on event.");

            subject.Dissatisfy();
            Assert.IsFalse(satisfied, "Condition should be dissatisfied on event.");

            condition.State -= handleState;
            Assert.IsFalse(satisfied, "Condition should still be dissatisfied after unobserving.");

            subject.Satisfy();
            Assert.IsFalse(satisfied, "Condition should not be satisfied on event after unobserving.");

            condition.State += handleState;
            Assert.IsTrue(satisfied, "Condition should again be satisfied on observe.");

            subject.Dissatisfy();
            Assert.IsFalse(satisfied, "Condition should again be dissatisfied on event.");

            subject.Satisfy();
            Assert.IsTrue(satisfied, "Condition should again be satisfied on event.");

            condition.State -= handleState;
            Assert.IsTrue(satisfied, "Condition should still be satisfied after unobserving.");
        }

        [Test]
        public void AllConditionSatisfied()
        {
            var a = new MockSubject(false);
            var b = new MockSubject(false);
            var condition = Condition.All(a.AsCondition(), b.AsCondition());

            var satisfied = true;
            void handleState(object sender, ObservableStateEventArgs<bool> e) => satisfied = e.State;
            condition.State += handleState;
            Assert.IsFalse(satisfied, "Condition should be dissatisfied on observe.");

            a.Satisfy();
            Assert.IsFalse(satisfied, "Condition should still be dissatisfied when only a is satisfied.");

            b.Satisfy();
            Assert.IsTrue(satisfied, "Condition should be satisfied when both a and b are satisfied.");

            a.Dissatisfy();
            Assert.IsFalse(satisfied, "Condition should be dissatisfied when only b is satisfied.");

            a.Satisfy();
            Assert.IsTrue(satisfied, "Condition should be satisfied again when a is satisfied.");

            b.Dissatisfy();
            Assert.IsFalse(satisfied, "Condition should be dissatisfied when b is dissatisfied.");
        }

        [Test]
        public void AnyConditionSatisfied()
        {
            var a = new MockSubject(false);
            var b = new MockSubject(false);
            var condition = Condition.Any(a.AsCondition(), b.AsCondition());

            var satisfied = true;
            void handleState(object sender, ObservableStateEventArgs<bool> e) => satisfied = e.State;
            condition.State += handleState;
            Assert.IsFalse(satisfied, "Condition should be dissatisfied on observe.");

            a.Satisfy();
            Assert.IsTrue(satisfied, "Condition should be satisfied when only a is satisfied.");

            b.Satisfy();
            Assert.IsTrue(satisfied, "Condition should still be satisfied when both a and b are satisfied.");

            a.Dissatisfy();
            Assert.IsTrue(satisfied, "Condition should still be satisfied when only b is satisfied.");

            b.Dissatisfy();
            Assert.IsFalse(satisfied, "Condition should be dissatisfied when both a and b are dissatisfied.");
        }

        [Test]
        public void ConditionInvokesEvents()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();

            var satisfactions = 0;
            var dissatisfactions = 0;
            void handleState(object sender, ObservableStateEventArgs<bool> e)
            {
                if (e.State)
                {
                    ++satisfactions;
                }
                else
                {
                    ++dissatisfactions;
                }
            }

            condition.State += handleState;
            Assert.AreEqual(1, dissatisfactions, "Observe communicates initially dissatisfied.");
            Assert.AreEqual(0, satisfactions, "Observe does not communicate opposite state.");

            subject.Dissatisfy();
            Assert.AreEqual(1, dissatisfactions, "Observe does not communicate still dissatisfied.");
            Assert.AreEqual(0, satisfactions, "Observe does not communicate opposite state.");

            subject.Satisfy();
            Assert.AreEqual(1, satisfactions, "Observe communicates when becomes satisfied.");
            Assert.AreEqual(1, dissatisfactions, "Observe does not communicate opposite state.");

            subject.Dissatisfy();
            Assert.AreEqual(2, dissatisfactions, "Observe communicates when becomes dissatisfied.");
            Assert.AreEqual(1, satisfactions, "Observe does not communicate opposite state.");

            condition.State -= handleState;
            Assert.AreEqual(2, dissatisfactions, "Unobserve does not communicate dissatisfied.");
            Assert.AreEqual(1, satisfactions, "Unobserve does not communicate satisfied.");

            subject.Satisfy();
            Assert.AreEqual(1, satisfactions, "Unobserve does not communicate when subject becomes satisfied.");
            Assert.AreEqual(2, dissatisfactions, "Unobserve does not communicate opposite state.");

            subject.Dissatisfy();
            Assert.AreEqual(2, dissatisfactions, "Unobserve does not communicate when subject becomes dissatisfied.");
            Assert.AreEqual(1, satisfactions, "Unobserve does not communicate opposite state.");

            subject.Satisfy();
            Assert.AreEqual(2, dissatisfactions, "Unobserve does not communicate opposite state.");
            Assert.AreEqual(1, satisfactions, "Unobserve does not communicate when subject becomes satisfied.");

            condition.State += handleState;
            Assert.AreEqual(2, dissatisfactions, "Observe does not communicate opposite state.");
            Assert.AreEqual(2, satisfactions, "Observe communicates subject satisfied.");
        }
    }
}
