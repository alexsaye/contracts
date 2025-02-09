using NUnit.Framework;

namespace Saye.Contracts.Tests
{
    public class ConditionTests
    {
        [Test]
        public void ConditionSatisfiedOnConstruction()
        {
            var condition = Condition.Always;
            Assert.IsTrue(condition.Satisfied, "Condition should be satisfied on construction.");
        }

        [Test]
        public void ConditionDissatisfiedOnConstruction()
        {
            var condition = Condition.Never;
            Assert.IsFalse(condition.Satisfied, "Condition should be dissatisfied on construction.");
        }

        [Test]
        public void ConditionSatisfiedOnAssert()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();
            Assert.IsFalse(condition.Satisfied, "Condition should be dissatisfied on construction.");

            var emissions = 0;
            var satisfied = false;
            condition.OnSatisfied += (object sender, ConditionStatusEventArgs e) =>
            {
                satisfied = true;
                ++emissions;
            };
            condition.OnDissatisfied += (object sender, ConditionStatusEventArgs e) =>
            {
                satisfied = false;
                ++emissions;
            };

            subject.Satisfy();
            Assert.IsFalse(condition.Satisfied, "Condition should not have changed before assert.");
            Assert.AreEqual(0, emissions, "Condition should not have communicated even the unchanged dissatisfied state before assert.");

            condition.Assert();
            Assert.IsTrue(condition.Satisfied, "Condition should be satisfied on assert.");
            Assert.IsTrue(satisfied, "Condition should have communicated satisfied on assert.");
        }

        [Test]
        public void ConditionDissatisfiedOnAssert()
        {
            var subject = new MockSubject(true);
            var condition = subject.AsCondition();
            Assert.IsTrue(condition.Satisfied, "Condition should be satisfied on construction.");

            var emissions = 0;
            var satisfied = true;
            condition.OnSatisfied += (object sender, ConditionStatusEventArgs e) =>
            {
                satisfied = true;
                ++emissions;
            };
            condition.OnDissatisfied += (object sender, ConditionStatusEventArgs e) =>
            {
                satisfied = false;
                ++emissions;
            };

            subject.Dissatisfy();
            Assert.IsTrue(condition.Satisfied, "Condition should not have changed before assert.");
            Assert.AreEqual(0, emissions, "Condition should not have communicated even the unchanged satisfied state before assert.");

            condition.Assert();
            Assert.IsFalse(condition.Satisfied, "Condition should still be dissatisfied on assert.");
            Assert.IsFalse(satisfied, "Condition should have communicated dissatisfied on assert.");

        }

        [Test]
        public void ConditionSatisfiedOnBind()
        {
            var condition = Condition.Always;

            var satisfied = false;
            condition.OnSatisfied += (object sender, ConditionStatusEventArgs e) => satisfied = true;
            condition.OnDissatisfied += (object sender, ConditionStatusEventArgs e) => satisfied = false;

            condition.Bind();
            Assert.IsTrue(satisfied, "Condition should be satisfied on bind.");

            condition.Unbind();
            Assert.IsTrue(satisfied, "Condition should still be satisfied on unbind.");
        }

        [Test]
        public void ConditionDissatisfiedOnBind()
        {
            var condition = Condition.Never;

            var satisfied = true;
            condition.OnSatisfied += (object sender, ConditionStatusEventArgs e) => satisfied = true;
            condition.OnDissatisfied += (object sender, ConditionStatusEventArgs e) => satisfied = false;

            condition.Bind();
            Assert.IsFalse(satisfied, "Condition should be dissatisfied on bind.");

            condition.Unbind();
            Assert.IsFalse(satisfied, "Condition should still be dissatisfied on unbind.");
        }

        [Test]
        public void ConditionSatisfiedOnEvent()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();

            var satisfied = true;
            condition.OnSatisfied += (object sender, ConditionStatusEventArgs e) => satisfied = true;
            condition.OnDissatisfied += (object sender, ConditionStatusEventArgs e) => satisfied = false;

            condition.Bind();
            Assert.IsFalse(satisfied, "Condition should be dissatisfied on bind.");

            subject.Satisfy();
            Assert.IsTrue(satisfied, "Condition should be satisfied on event.");

            subject.Dissatisfy();
            Assert.IsFalse(satisfied, "Condition should be dissatisfied on event.");

            condition.Unbind();
            Assert.IsFalse(satisfied, "Condition should still be dissatisfied after unbindping.");

            subject.Satisfy();
            Assert.IsFalse(satisfied, "Condition should not be satisfied on event after unbindping.");

            condition.Bind();
            Assert.IsTrue(satisfied, "Condition should again be satisfied on bind.");

            subject.Dissatisfy();
            Assert.IsFalse(satisfied, "Condition should again be dissatisfied on event.");

            subject.Satisfy();
            Assert.IsTrue(satisfied, "Condition should again be satisfied on event.");

            condition.Unbind();
            Assert.IsTrue(satisfied, "Condition should still be satisfied after unbindping.");
        }

        [Test]
        public void AllConditionSatisfied()
        {
            var a = new MockSubject(false);
            var b = new MockSubject(false);
            var condition = Condition.All(a.AsCondition(), b.AsCondition());

            var satisfied = true;
            condition.OnSatisfied += (object sender, ConditionStatusEventArgs e) => satisfied = true;
            condition.OnDissatisfied += (object sender, ConditionStatusEventArgs e) => satisfied = false;

            condition.Bind();
            Assert.IsFalse(satisfied, "Condition should be dissatisfied on bind.");

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
            condition.OnSatisfied += (object sender, ConditionStatusEventArgs e) => satisfied = true;
            condition.OnDissatisfied += (object sender, ConditionStatusEventArgs e) => satisfied = false;

            condition.Bind();
            Assert.IsFalse(satisfied, "Condition should be dissatisfied on bind.");

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
            condition.OnSatisfied += (object sender, ConditionStatusEventArgs e) => ++satisfactions;
            condition.OnDissatisfied += (object sender, ConditionStatusEventArgs e) => ++dissatisfactions;

            condition.Bind();
            Assert.AreEqual(1, dissatisfactions, "Bind communicates initially dissatisfied.");
            Assert.AreEqual(0, satisfactions, "Bind does not communicate opposite state.");

            subject.Dissatisfy();
            Assert.AreEqual(1, dissatisfactions, "Bind does not communicate still dissatisfied.");
            Assert.AreEqual(0, satisfactions, "Bind does not communicate opposite state.");

            subject.Satisfy();
            Assert.AreEqual(1, dissatisfactions, "Bind does not communicate opposite state.");
            Assert.AreEqual(1, satisfactions, "Bind communicates when becomes satisfied.");

            subject.Dissatisfy();
            Assert.AreEqual(2, dissatisfactions, "Bind communicates when becomes dissatisfied.");
            Assert.AreEqual(1, satisfactions, "Bind does not communicate opposite state.");

            condition.Unbind();
            Assert.AreEqual(2, dissatisfactions, "Unbind does not communicate dissatisfied.");
            Assert.AreEqual(1, satisfactions, "Unbind does not communicate satisfied.");

            subject.Satisfy();
            Assert.AreEqual(2, dissatisfactions, "Unbind does not communicate opposite state.");
            Assert.AreEqual(1, satisfactions, "Unbind does not communicate when subject becomes satisfied.");

            condition.Assert();
            Assert.AreEqual(2, dissatisfactions, "Assert does not communicate opposite state.");
            Assert.AreEqual(2, satisfactions, "Assert communicates subject satisfied.");

            subject.Dissatisfy();
            Assert.AreEqual(2, dissatisfactions, "Unbind does not communicate when subject becomes dissatisfied.");
            Assert.AreEqual(2, satisfactions, "Unbind does not communicate opposite state.");

            condition.Assert();
            Assert.AreEqual(3, dissatisfactions, "Assert communicates subject dissatisfied.");
            Assert.AreEqual(2, satisfactions, "Assert does not communicate opposite state.");

            subject.Satisfy();
            Assert.AreEqual(3, dissatisfactions, "Unbind does not communicate opposite state.");
            Assert.AreEqual(2, satisfactions, "Unbind does not communicate when subject becomes satisfied.");

            condition.Bind();
            Assert.AreEqual(3, dissatisfactions, "Bind does not communicate opposite state.");
            Assert.AreEqual(3, satisfactions, "Bind communicates subject satisfied.");
        }
    }
}
