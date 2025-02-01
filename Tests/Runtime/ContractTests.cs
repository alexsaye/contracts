using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Saye.Contracts.Tests
{
    public class ContractTests
    {
        private class MockSubject
        {
            public bool Satisfied { get; private set; }

            public event EventHandler Event;

            public MockSubject(bool satisfied)
            {
                Satisfied = satisfied;
            }

            public void Satisfy()
            {
                Satisfied = true;
                Event?.Invoke(this, EventArgs.Empty);
            }

            public void Dissatisfy()
            {
                Satisfied = false;
                Event?.Invoke(this, EventArgs.Empty);
            }
        }

        private ICondition MockCondition(MockSubject subject)
        {
            return Condition.When(
                assert: () => subject.Satisfied,
                bind: handler => subject.Event += handler,
                unbind: handler => subject.Event -= handler);
        }

        [Test]
        public void ContractPendingOnConstruction()
        {
            var fulfillingContract = new Contract(Condition.Always, Condition.Never);
            Assert.AreEqual(ContractStatus.Pending, fulfillingContract.Status);

            var violatingContract = new Contract(Condition.Never, Condition.Always);
            Assert.AreEqual(ContractStatus.Pending, violatingContract.Status);
        }

        [Test]
        public void ContractPendingOnBind()
        {
            var neverendingContract = new Contract(Condition.Never, Condition.Never);
            neverendingContract.Bind();
            Assert.AreEqual(ContractStatus.Pending, neverendingContract.Status, "Neverending contract should remain pending when bound.");
            neverendingContract.Unbind();
            Assert.AreEqual(ContractStatus.Pending, neverendingContract.Status, "Neverending contract should remain pending when unbound.");
            neverendingContract.Bind();
            Assert.AreEqual(ContractStatus.Pending, neverendingContract.Status, "Neverending contract should remain pending when rebound.");
        }

        [Test]
        public void ContractFulfilledOnBind()
        {
            var fulfillingContract = new Contract(Condition.Always, Condition.Never);
            fulfillingContract.Bind();
            Assert.AreEqual(ContractStatus.Fulfilled, fulfillingContract.Status, "Fulfilling contract should be fulfilled when bound.");
            fulfillingContract.Unbind();
            Assert.AreEqual(ContractStatus.Fulfilled, fulfillingContract.Status, "Fulfilling contract should remain fulfilled when unbound.");
            fulfillingContract.Bind();
            Assert.AreEqual(ContractStatus.Fulfilled, fulfillingContract.Status, "Fulfilling contract should remain fulfilled when rebound.");
        }

        [Test]
        public void ContractBreachedOnBind() {
            var breachingContract = new Contract(Condition.Never, Condition.Always);
            breachingContract.Bind();
            Assert.AreEqual(ContractStatus.Breached, breachingContract.Status, "Breaching contract should be breached when bound.");
            breachingContract.Unbind();
            Assert.AreEqual(ContractStatus.Breached, breachingContract.Status, "Breaching contract should remain breached when unbound.");
            breachingContract.Bind();
            Assert.AreEqual(ContractStatus.Breached, breachingContract.Status, "Breaching contract should remain breached when rebound.");
        }

        [Test]
        public void ContractFulfilledOrBreachedOnBoundCondition()
        {
            var subject = new MockSubject(false);
            var condition = MockCondition(subject);

            var unboundFulfillingContract = new Contract(condition, Condition.Never);
            unboundFulfillingContract.Bind();
            unboundFulfillingContract.Unbind();
            var unboundBreachingContract = new Contract(Condition.Never, condition);
            unboundBreachingContract.Bind();
            unboundBreachingContract.Unbind();

            var boundFulfillingContract = new Contract(condition, Condition.Never);
            boundFulfillingContract.Bind();

            var boundBreachingContract = new Contract(Condition.Never, condition);
            boundBreachingContract.Bind();

            Assert.AreEqual(ContractStatus.Pending, unboundFulfillingContract.Status, "Unbound fulfilling contract should be pending when condition is unsatisfied.");
            Assert.AreEqual(ContractStatus.Pending, unboundBreachingContract.Status, "Unbound breaching contract should be pending when condition is unsatisfied.");
            Assert.AreEqual(ContractStatus.Pending, boundFulfillingContract.Status, "Bound fulfilling contract should remain pending when condition is unsatisfied.");
            Assert.AreEqual(ContractStatus.Pending, boundBreachingContract.Status, "Bound breaching contract should remain pending when condition is unsatisfied.");

            subject.Satisfy();
            Assert.AreEqual(ContractStatus.Pending, unboundFulfillingContract.Status, "Unbound fulfilling contract should remain pending when condition is satisfied.");
            Assert.AreEqual(ContractStatus.Pending, unboundBreachingContract.Status, "Unbound breaching contract should remain pending when condition is satisfied.");
            Assert.AreEqual(ContractStatus.Fulfilled, boundFulfillingContract.Status, "Bound fulfilling contract should be fulfilled when condition is satisfied.");
            Assert.AreEqual(ContractStatus.Breached, boundBreachingContract.Status, "Bound breaching contract should be breached when condition is satisfied.");

            subject.Dissatisfy();
            Assert.AreEqual(ContractStatus.Pending, unboundFulfillingContract.Status, "Unbound fulfilling contract should remain pending when condition is dissatisfied.");
            Assert.AreEqual(ContractStatus.Pending, unboundBreachingContract.Status, "Unbound breaching contract should remain pending when condition is dissatisfied.");
            Assert.AreEqual(ContractStatus.Fulfilled, boundFulfillingContract.Status, "Bound fulfilling contract should remain fulfilled when condition is dissatisfied.");
            Assert.AreEqual(ContractStatus.Breached, boundBreachingContract.Status, "Bound breaching contract should remain breached when condition is dissatisfied.");
        }
    }
}
