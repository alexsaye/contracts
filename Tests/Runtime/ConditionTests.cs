using NUnit.Framework;
using System;

namespace Saye.Contracts.Tests
{
    public class ConditionTests
    {
        [Test]
        public void ConditionSatisfiedOnSubscribe()
        {
            var condition = Condition.Always;

            var satisfied = false;
            EventHandler<ConditionEventArgs> conditionHandler = (object sender, ConditionEventArgs e) => satisfied = e.Satisfied;

            condition.OnSatisfied += conditionHandler;
            condition.OnDissatisfied += conditionHandler;
            Assert.IsTrue(satisfied, "Condition should have communicated satisfied on subscribe.");
        }

        [Test]
        public void ConditionDissatisfiedOnSubscribe()
        {
            var condition = Condition.Never;

            var satisfied = true;
            EventHandler<ConditionEventArgs> conditionHandler = (object sender, ConditionEventArgs e) => satisfied = e.Satisfied;

            condition.OnSatisfied += conditionHandler;
            condition.OnDissatisfied += conditionHandler;
            Assert.IsFalse(satisfied, "Condition should have communicated dissatisfied on subscribe.");
        }

        [Test]
        public void ConditionSatisfiedOnEvent()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();

            var satisfied = true;
            EventHandler<ConditionEventArgs> conditionHandler = (object sender, ConditionEventArgs e) => satisfied = e.Satisfied;

            condition.OnSatisfied += conditionHandler;
            condition.OnDissatisfied += conditionHandler;
            Assert.IsFalse(satisfied, "Condition should be dissatisfied on subscribe.");

            subject.Satisfy();
            Assert.IsTrue(satisfied, "Condition should be satisfied on event.");

            subject.Dissatisfy();
            Assert.IsFalse(satisfied, "Condition should be dissatisfied on event.");

            condition.OnSatisfied -= conditionHandler;
            condition.OnDissatisfied -= conditionHandler;
            Assert.IsFalse(satisfied, "Condition should still be dissatisfied after unsubscribing.");

            subject.Satisfy();
            Assert.IsFalse(satisfied, "Condition should not be satisfied on event after unsubscribing.");

            condition.OnSatisfied += conditionHandler;
            condition.OnDissatisfied += conditionHandler;
            Assert.IsTrue(satisfied, "Condition should again be satisfied on subscribe.");

            subject.Dissatisfy();
            Assert.IsFalse(satisfied, "Condition should again be dissatisfied on event.");

            subject.Satisfy();
            Assert.IsTrue(satisfied, "Condition should again be satisfied on event.");

            condition.OnSatisfied -= conditionHandler;
            condition.OnDissatisfied -= conditionHandler;
            Assert.IsTrue(satisfied, "Condition should still be satisfied after unsubscribing.");
        }

        [Test]
        public void AllConditionSatisfied()
        {
            var a = new MockSubject(false);
            var b = new MockSubject(false);
            var condition = Condition.All(a.AsCondition(), b.AsCondition());

            var satisfied = true;
            EventHandler<ConditionEventArgs> conditionHandler = (object sender, ConditionEventArgs e) => satisfied = e.Satisfied;
            condition.OnSatisfied += conditionHandler;
            condition.OnDissatisfied += conditionHandler;
            Assert.IsFalse(satisfied, "Condition should be dissatisfied on subscribe.");

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
            EventHandler<ConditionEventArgs> conditionHandler = (object sender, ConditionEventArgs e) => satisfied = e.Satisfied;
            condition.OnSatisfied += conditionHandler;
            condition.OnDissatisfied += conditionHandler;
            Assert.IsFalse(satisfied, "Condition should be dissatisfied on subscribe.");

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
        public void ConditionSubscriptionInvokesEvents()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();

            var satisfactions = 0;
            var dissatisfactions = 0;
            EventHandler<ConditionEventArgs> satisfiedHandler = (object sender, ConditionEventArgs e) => ++satisfactions;
            EventHandler<ConditionEventArgs> dissatisfiedHandler = (object sender, ConditionEventArgs e) => ++dissatisfactions;

            condition.OnSatisfied += satisfiedHandler;
            condition.OnDissatisfied += dissatisfiedHandler;
            Assert.AreEqual(1, dissatisfactions, "Subscribe communicates initially dissatisfied.");
            Assert.AreEqual(0, satisfactions, "Subscribe does not communicate opposite state.");

            subject.Dissatisfy();
            Assert.AreEqual(1, dissatisfactions, "Subscribe does not communicate still dissatisfied.");
            Assert.AreEqual(0, satisfactions, "Subscribe does not communicate opposite state.");

            subject.Satisfy();
            Assert.AreEqual(1, satisfactions, "Subscribe communicates when becomes satisfied.");
            Assert.AreEqual(1, dissatisfactions, "Subscribe does not communicate opposite state.");

            subject.Dissatisfy();
            Assert.AreEqual(2, dissatisfactions, "Subscribe communicates when becomes dissatisfied.");
            Assert.AreEqual(1, satisfactions, "Subscribe does not communicate opposite state.");

            condition.OnSatisfied -= satisfiedHandler;
            condition.OnDissatisfied -= dissatisfiedHandler;
            Assert.AreEqual(2, dissatisfactions, "Unsubscribe does not communicate dissatisfied.");
            Assert.AreEqual(1, satisfactions, "Unsubscribe does not communicate satisfied.");

            subject.Satisfy();
            Assert.AreEqual(1, satisfactions, "Unsubscribe does not communicate when subject becomes satisfied.");
            Assert.AreEqual(2, dissatisfactions, "Unsubscribe does not communicate opposite state.");

            subject.Dissatisfy();
            Assert.AreEqual(2, dissatisfactions, "Unsubscribe does not communicate when subject becomes dissatisfied.");
            Assert.AreEqual(1, satisfactions, "Unsubscribe does not communicate opposite state.");

            subject.Satisfy();
            Assert.AreEqual(2, dissatisfactions, "Unsubscribe does not communicate opposite state.");
            Assert.AreEqual(1, satisfactions, "Unsubscribe does not communicate when subject becomes satisfied.");

            condition.OnSatisfied += satisfiedHandler;
            condition.OnDissatisfied += dissatisfiedHandler;
            Assert.AreEqual(2, dissatisfactions, "Subscribe does not communicate opposite state.");
            Assert.AreEqual(2, satisfactions, "Subscribe communicates subject satisfied.");
        }
    }
}
