using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HexLibrary;
using Shouldly;

namespace HexTest
{
    [TestClass]
    public class CircularQueueTest
    {
        [TestMethod]
        public void TestCreation()
        {
            var queue = new CircularQueue<int>(5);
            queue.Length.ShouldBe(0);
            queue.Capacity.ShouldBe(32);
        }

        [TestMethod]
        public void TestQueing()
        {
            var queue = new CircularQueue<int>(4);

            for (var i = 0; i < queue.Capacity; i++)
            {
                queue.Queue(i);
                queue.Length.ShouldBe(i + 1);
            }

            Should.Throw<IndexOutOfRangeException>(() => queue.Queue(10));

            queue.Zip(Enumerable.Range(0, 16), (v1, v2) => v1 == v2).All(f => true).ShouldBeTrue();

            for (var i = 0; i < 8; i++)
            {
                queue.Dequeue().ShouldBe(i);
                queue.Queue(i + 16);
                queue.Length.ShouldBe(16);
            }
            queue.Zip(Enumerable.Range(8, 16), (v1, v2) => v1 == v2).All(f => true).ShouldBeTrue();
        }
    }
}
