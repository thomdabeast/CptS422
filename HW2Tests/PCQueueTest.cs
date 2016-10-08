using CS422;
using NUnit.Framework;
using System;

namespace HW2Tests
{
	[TestFixture()]
	public class PCQueueTest
	{
		[Test()]
		public void EnqueueDequeueTest()
		{
			PCQueue queue = new PCQueue();

			for (int i = 0; i < 10; i++)
			{
				queue.Enqueue(i);
			}

			for (int i = 0; i < 10; i++)
			{
				int dqed = -1;

				if (queue.Dequeue(ref dqed))
				{
					Console.WriteLine(dqed + ":" + i);
					if (dqed != i)
					{
						Assert.Fail();
					}
				}
			}

			Assert.Pass();
		}

		[Test()]
		public void DequeueNullTest()
		{
			PCQueue queue = new PCQueue();

			int t = 0;

			Assert.IsFalse(queue.Dequeue(ref t));
		}

		[Test()]
		public void ConcurrentTest()
		{
			const int NUMBERS = 100000;
			//PCQueue queue = new PCQueue();
			//Thread enqueuer = new Thread(
			//Thread dequeuer = new Thread();
		}
	}
}

