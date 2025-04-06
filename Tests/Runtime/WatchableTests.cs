using NUnit.Framework;
using System.Collections.Generic;

namespace Contracts.Tests
{
    public class WatchableTests
    {
        [Test]
        public void WatchableWatched()
        {
            var watchable = new ReadOnlyWatchable<int>();
            Assert.IsFalse(watchable.Watched, "Is not watched on construction.");

            var historyA = new List<bool>();
            void handleWatchedA(object sender, WatchedUpdatedEventArgs e) => historyA.Add(e.Watched);

            watchable.WatchedUpdated += handleWatchedA;
            Assert.IsFalse(watchable.Watched, "Is not watched when Watched gains handlers.");
            Assert.AreEqual(new List<bool> { false }, historyA, "Has pushed unwatched to handler A.");

            void handleStateA(object sender, StateUpdatedEventArgs<int> e) { }
            watchable.StateUpdated += handleStateA;
            Assert.IsTrue(watchable.Watched, "Is watched when State gains its first handler.");
            Assert.AreEqual(new List<bool> { false, true }, historyA, "Has pushed watched to handler A.");

            var historyB = new List<bool>();
            void handleWatchedB(object sender, WatchedUpdatedEventArgs e) => historyB.Add(e.Watched);
            watchable.WatchedUpdated += handleWatchedB;
            Assert.AreEqual(new List<bool> { false, true }, historyA, "Has not pushed watched to handler A again.");
            Assert.AreEqual(new List<bool> { true }, historyB, "Has pushed watched to handler B.");

            void handleStateB(object sender, StateUpdatedEventArgs<int> e) { }
            watchable.StateUpdated += handleStateB;
            Assert.IsTrue(watchable.Watched, "Is still watched when State gains its second handler.");
            Assert.AreEqual(new List<bool> { false, true }, historyA, "Has not pushed to handler A again.");
            Assert.AreEqual(new List<bool> { true }, historyB, "Has not pushed to handler B again.");

            watchable.StateUpdated -= handleStateA;
            Assert.IsTrue(watchable.Watched, "Is still watched when State loses one of its handlers.");
            Assert.AreEqual(new List<bool> { false, true }, historyA, "Has not pushed to handler A again.");
            Assert.AreEqual(new List<bool> { true }, historyB, "Has not pushed to handler B again.");

            watchable.StateUpdated -= handleStateB;
            Assert.IsFalse(watchable.Watched, "Is not watched when State loses its last handler.");
            Assert.AreEqual(new List<bool> { false, true, false }, historyA, "Has pushed unwatched to handler A again.");
            Assert.AreEqual(new List<bool> { true, false }, historyB, "Has pushed unwatched to handler B again.");

            watchable.WatchedUpdated -= handleWatchedA;
            watchable.StateUpdated += handleStateA;
            Assert.AreEqual(new List<bool> { false, true, false }, historyA, "Has not pushed to unsubscribed handler A.");
            Assert.AreEqual(new List<bool> { true, false, true }, historyB, "Has pushed watched to handler B.");

            watchable.WatchedUpdated -= handleWatchedB;
            watchable.StateUpdated -= handleStateA;
            Assert.AreEqual(new List<bool> { true, false, true }, historyB, "Has not pushed to unsubscribed handler B.");
        }

        [Test]
        public void WatchableState()
        {
            var watchable = new Watchable<int>();
            Assert.AreEqual(0, watchable.State, "Has default state of 0 on construction.");

            var historyA = new List<int>();
            void handleStateA(object sender, StateUpdatedEventArgs<int> e) => historyA.Add(e.State);
            watchable.StateUpdated += handleStateA;
            Assert.AreEqual(new List<int> { 0 }, historyA, "Has pushed default state to handler A once.");

            watchable.State = 1;
            Assert.AreEqual(new List<int> { 0, 1 }, historyA, "Has pushed changed state to handler A once.");

            var historyB = new List<int>();
            void handleStateB(object sender, StateUpdatedEventArgs<int> e) => historyB.Add(e.State);
            watchable.StateUpdated += handleStateB;
            Assert.AreEqual(new List<int> { 0, 1 }, historyA, "Has not pushed state to handler A again.");
            Assert.AreEqual(new List<int> { 1 }, historyB, "Has pushed state to handler B once.");

            watchable.State = 1;
            Assert.AreEqual(new List<int> { 0, 1 }, historyA, "Has not pushed unchanged state to handler A.");
            Assert.AreEqual(new List<int> { 1 }, historyB, "Has not pushed unchanged state to handler B.");

            watchable.State = 2;
            Assert.AreEqual(new List<int> { 0, 1, 2 }, historyA, "Has pushed changed state to handler A.");
            Assert.AreEqual(new List<int> { 1, 2 }, historyB, "Has pushed changed state to handler B.");

            watchable.StateUpdated -= handleStateA;
            watchable.State = 3;
            Assert.AreEqual(new List<int> { 0, 1, 2 }, historyA, "Has not pushed changed state to unsubscribed handler A.");
            Assert.AreEqual(new List<int> { 1, 2, 3 }, historyB, "Has pushed changed state to handler B.");

            watchable.StateUpdated -= handleStateB;
            watchable.State = 4;
            Assert.AreEqual(new List<int> { 1, 2, 3 }, historyB, "Has not pushed changed state to unsubscribed handler B.");
        }
    }
}
