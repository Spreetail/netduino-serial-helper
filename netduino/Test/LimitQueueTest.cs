using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerialAbstraction;

namespace SerialAbstractionTests
{
    [TestClass]
    public class LimitQueueTest
    {
        const int limit = 3;
        [TestMethod]
        public void AddingItemToFullQueueKeepsSameSize()
        {
            // Arrange
            var queue = new LimitQueue(limit: limit);
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);

            // Act
            queue.Enqueue(4);

            // Assert
            Assert.AreEqual(queue.Count, limit);
        }

        [TestMethod]
        public void AddingItemToFullQueueGetsRidOfItemFromFrontOfQueue()
        {
            // Arrange
            var toBeDropped = 1;
            var toBeFront = 2;
            var queue = new LimitQueue(limit: limit);
            queue.Enqueue(toBeDropped);
            queue.Enqueue(toBeFront);
            queue.Enqueue(3);

            // Act
            queue.Enqueue(4);
            var front = (int)queue.Dequeue();

            // Assert
            Assert.AreEqual(front, toBeFront);
            Assert.AreNotEqual(front, toBeDropped);
            Assert.AreEqual(queue.Count, limit - 1);
        }
    }
}
