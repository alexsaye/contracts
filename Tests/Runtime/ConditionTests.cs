using NUnit.Framework;
using System;

namespace Saye.Contracts.Tests
{
    public class ConditionTests
    {
        [Test]
        public void ConditionUpdatedOnEvent()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();

            var satisfied = true;
            void handleState(object sender, StateEventArgs<bool> e) => satisfied = e.CurrentState;

            condition.State += handleState;
            Assert.IsFalse(satisfied, "Is dissatisfied on subscribe.");

            subject.Satisfy();
            Assert.IsTrue(satisfied, "Is satisfied on event.");

            subject.Dissatisfy();
            Assert.IsFalse(satisfied, "Is dissatisfied on event.");

            condition.State -= handleState;
            Assert.IsFalse(satisfied, "Is still dissatisfied after unsubscribing.");

            subject.Satisfy();
            Assert.IsFalse(satisfied, "Is not satisfied on event after unsubscribing.");

            condition.State += handleState;
            Assert.IsTrue(satisfied, "Is satisfied on subscribe.");

            subject.Dissatisfy();
            Assert.IsFalse(satisfied, "Is dissatisfied on event.");

            subject.Satisfy();
            Assert.IsTrue(satisfied, "Is satisfied on event.");

            condition.State -= handleState;
            Assert.IsTrue(satisfied, "Is still satisfied after unsubscribing.");
        }

        [Test]
        public void AllConditionUpdatedOnEvent()
        {
            var subjectA = new MockSubject(false);
            var subjectB = new MockSubject(false);
            var condition = Condition.All(subjectA.AsCondition(), subjectB.AsCondition());

            var satisfied = true;
            void handleState(object sender, StateEventArgs<bool> e) => satisfied = e.CurrentState;
            condition.State += handleState;
            Assert.IsFalse(satisfied, "Is dissatisfied on subscribe.");

            subjectA.Satisfy();
            Assert.IsFalse(satisfied, "Is still dissatisfied when only subject A is satisfied.");

            subjectB.Satisfy();
            Assert.IsTrue(satisfied, "Is satisfied when both subject A and subject B are satisfied.");

            subjectA.Dissatisfy();
            Assert.IsFalse(satisfied, "Is dissatisfied when only subject B is satisfied.");

            subjectA.Satisfy();
            Assert.IsTrue(satisfied, "Is satisfied again when subject A is satisfied.");

            subjectB.Dissatisfy();
            Assert.IsFalse(satisfied, "Is dissatisfied when b is dissatisfied.");
        }

        [Test]
        public void AnyConditionUpdatedOnEvent()
        {
            var a = new MockSubject(false);
            var b = new MockSubject(false);
            var condition = Condition.Any(a.AsCondition(), b.AsCondition());

            var satisfied = true;
            void handleState(object sender, StateEventArgs<bool> e) => satisfied = e.CurrentState;
            condition.State += handleState;
            Assert.IsFalse(satisfied, "Is dissatisfied on subscribe.");

            a.Satisfy();
            Assert.IsTrue(satisfied, "Is satisfied when only subject A is satisfied.");

            b.Satisfy();
            Assert.IsTrue(satisfied, "Is still satisfied when both subject A and subject B are satisfied.");

            a.Dissatisfy();
            Assert.IsTrue(satisfied, "Is still satisfied when only subject B is satisfied.");

            b.Dissatisfy();
            Assert.IsFalse(satisfied, "Is dissatisfied when both subject A and subject B are dissatisfied.");
        }
    }
}
