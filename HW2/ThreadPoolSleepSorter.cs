using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CS422
{
	public class ThreadPoolSleepSorter : IDisposable
	{
		delegate void Tasker();
		TextWriter output;
		BlockingCollection<Tasker> collection;
		ushort numThreads;

		public ThreadPoolSleepSorter(TextWriter output, ushort threadCount)
		{
			collection = new BlockingCollection<Tasker>();
			this.output = output;

			// Default value
			if (threadCount == 0)
			{
				threadCount = 64;
			}

			numThreads = threadCount;

			// Spawn threads and start 'em up
			for (int i = 0; i < threadCount; i++)
			{
				new Thread(new ThreadStart(ThreadWorkerFunc)).Start();
			}
		}

		void ThreadWorkerFunc() 
		{
			while (true)
			{
				// Wait and try to get a task.
				Tasker task = collection.Take();

				// Master thread doesn't want us any more... :(
				if (null == task)
				{
					break;
				}

				// Make the call!
				task.Invoke();
			}
		}

		public void Sort(byte[] values)
		{
			for (int i = 0; i < values.Length; i++)
			{
				int v = (int)values[i];

				// Add task for workers
				collection.Add(() =>
				{
					// The core of SleepSort
					Thread.Sleep(v * 1000);
					output.WriteLine(v);
				});
			}

		}

		public void Dispose()
		{
			for (int i = 0; i < numThreads; i++)
			{
				// In the words of KC Wang: Kill our children
				collection.Add(null);
			}
		}
	}
}

