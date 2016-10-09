using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CS422
{
	public class ThreadPool : IDisposable
	{
		const ushort DEFAULT_NUMBER_OF_THREADS = 64;
		public delegate void Tasker();
		BlockingCollection<Tasker> collection;
		ushort numThreads;

        public ThreadPool() { }

		public ThreadPool(ushort threadCount)
		{
            Start(threadCount);
        }

        public void Start(ushort threadCount)
        {
            collection = new BlockingCollection<Tasker>();

            numThreads = (threadCount <= 0) ? DEFAULT_NUMBER_OF_THREADS : threadCount;

            // Spawn threads and start 'em up
            for (int i = 0; i < numThreads; i++)
            {
                new Thread(new ThreadStart(ThreadWorkerFunc)).Start();
            }
        }

        internal void Start(int number_threads)
        {
            throw new NotImplementedException();
        }

        void ThreadWorkerFunc() 
		{
			while (true)
			{
				// Wait and try to get a task.
				Tasker task = collection.Take();

				// Master doesn't want us any more... :( 
				if (null == task)
				{
					break;
				}

				// Make the call!
				task.Invoke();
			}
		}

        public void AddTask(Tasker task)
        {
            collection.Add(task);
        }

        /// <summary>
        /// Blocking call that waits until all created threads are destroyed.
        /// </summary>
        public void Dispose()
		{
			for (int i = 0; i < numThreads; i++)
			{
				// In the words of KC Wang: Kill our children
				collection.Add(null);
			}

            // Wait until collection is empty
            while (collection.Count > 0) { }
		}
	}
}

