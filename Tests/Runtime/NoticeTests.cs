using NUnit.Framework;
using System.Collections.Generic;

namespace Saye.Contracts.Tests
{
    public class NoticeTests
    {
        [Test]
        public void NoticeNoticed()
        {
            var notice = new ReadOnlyNotice<int>();
            Assert.IsFalse(notice.IsNoticed, "Is not noticed on construction.");

            var historyA = new List<bool>();
            void handleNoticedA(object sender, NoticedEventArgs e) => historyA.Add(e.IsNoticed);

            notice.Noticed += handleNoticedA;
            Assert.IsFalse(notice.IsNoticed, "Is not noticed when Noticed gains handlers.");
            Assert.AreEqual(new List<bool> { false }, historyA, "Has pushed unnoticed to handler A.");

            void handleStateA(object sender, StateEventArgs<int> e) { }
            notice.State += handleStateA;
            Assert.IsTrue(notice.IsNoticed, "Is noticed when State gains its first handler.");
            Assert.AreEqual(new List<bool> { false, true }, historyA, "Has pushed noticed to handler A.");

            var historyB = new List<bool>();
            void handleNoticedB(object sender, NoticedEventArgs e) => historyB.Add(e.IsNoticed);
            notice.Noticed += handleNoticedB;
            Assert.AreEqual(new List<bool> { false, true }, historyA, "Has not pushed noticed to handler A again.");
            Assert.AreEqual(new List<bool> { true }, historyB, "Has pushed noticed to handler B.");

            void handleStateB(object sender, StateEventArgs<int> e) { }
            notice.State += handleStateB;
            Assert.IsTrue(notice.IsNoticed, "Is still noticed when State gains its second handler.");
            Assert.AreEqual(new List<bool> { false, true }, historyA, "Has not pushed to handler A again.");
            Assert.AreEqual(new List<bool> { true }, historyB, "Has not pushed to handler B again.");

            notice.State -= handleStateA;
            Assert.IsTrue(notice.IsNoticed, "Is still noticed when State loses one of its handlers.");
            Assert.AreEqual(new List<bool> { false, true }, historyA, "Has not pushed to handler A again.");
            Assert.AreEqual(new List<bool> { true }, historyB, "Has not pushed to handler B again.");

            notice.State -= handleStateB;
            Assert.IsFalse(notice.IsNoticed, "Is not noticed when State loses its last handler.");
            Assert.AreEqual(new List<bool> { false, true, false }, historyA, "Has pushed unnoticed to handler A again.");
            Assert.AreEqual(new List<bool> { true, false }, historyB, "Has pushed unnoticed to handler B again.");

            notice.Noticed -= handleNoticedA;
            notice.State += handleStateA;
            Assert.AreEqual(new List<bool> { false, true, false }, historyA, "Has not pushed to unsubscribed handler A.");
            Assert.AreEqual(new List<bool> { true, false, true }, historyB, "Has pushed noticed to handler B.");

            notice.Noticed -= handleNoticedB;
            notice.State -= handleStateA;
            Assert.AreEqual(new List<bool> { true, false, true }, historyB, "Has not pushed to unsubscribed handler B.");
        }

        [Test]
        public void NoticeState()
        {
            var notice = new Notice<int>();
            Assert.AreEqual(0, notice.CurrentState, "Has default state of 0 on construction.");

            var historyA = new List<int>();
            void handleStateA(object sender, StateEventArgs<int> e) => historyA.Add(e.CurrentState);
            notice.State += handleStateA;
            Assert.AreEqual(new List<int> { 0 }, historyA, "Has pushed default state to handler A once.");

            notice.CurrentState = 1;
            Assert.AreEqual(new List<int> { 0, 1 }, historyA, "Has pushed changed state to handler A once.");

            var historyB = new List<int>();
            void handleStateB(object sender, StateEventArgs<int> e) => historyB.Add(e.CurrentState);
            notice.State += handleStateB;
            Assert.AreEqual(new List<int> { 0, 1 }, historyA, "Has not pushed state to handler A again.");
            Assert.AreEqual(new List<int> { 1 }, historyB, "Has pushed state to handler B once.");

            notice.CurrentState = 1;
            Assert.AreEqual(new List<int> { 0, 1 }, historyA, "Has not pushed unchanged state to handler A.");
            Assert.AreEqual(new List<int> { 1 }, historyB, "Has not pushed unchanged state to handler B.");

            notice.CurrentState = 2;
            Assert.AreEqual(new List<int> { 0, 1, 2 }, historyA, "Has pushed changed state to handler A.");
            Assert.AreEqual(new List<int> { 1, 2 }, historyB, "Has pushed changed state to handler B.");

            notice.State -= handleStateA;
            notice.CurrentState = 3;
            Assert.AreEqual(new List<int> { 0, 1, 2 }, historyA, "Has not pushed changed state to unsubscribed handler A.");
            Assert.AreEqual(new List<int> { 1, 2, 3 }, historyB, "Has pushed changed state to handler B.");

            notice.State -= handleStateB;
            notice.CurrentState = 4;
            Assert.AreEqual(new List<int> { 1, 2, 3 }, historyB, "Has not pushed changed state to unsubscribed handler B.");
        }
    }
}
