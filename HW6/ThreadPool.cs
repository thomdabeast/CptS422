using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CS422
{
	public class ThreadPool : IDisposable
	{
		const ushort DEFAULT_NUMBER_OF_THREADS = 64;
		delegate void Tasker();
		TextWriter output;
		BlockingCollection<Tasker> collection;
		ushort numThreads;

		public ThreadPool() { }

		public ThreadPool(TextWriter output, ushort threadCount)
		{
			collection = new BlockingCollection<Tasker>();
			this.output = output;

			// Default value
			if (threadCount == 0)
			{
				threadCount = DEFAULT_NUMBER_OF_THREADS;
			}

			numThreads = threadCount;

			// Spawn threads and start 'em up
			for (int i = 0; i < threadCount; i++)
			{
				new Thread(new ThreadStart(ThreadWorkerFunc)).Start();
			}
		}

		public ThreadPool(ushort threadCount)
		{
			collection = new BlockingCollection<Tasker>();

			numThreads = (threadCount <= 0) ? DEFAULT_NUMBER_OF_THREADS : threadCount;
		}

		public void SetCapacity(ushort numThreads)
		{
			numThreads = (numThreads <= 0) ? DEFAULT_NUMBER_OF_THREADS : numThreads;
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

