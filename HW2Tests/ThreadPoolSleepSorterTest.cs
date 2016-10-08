using CS422;
using System;
using NUnit.Framework;

namespace HW2Tests
{
	[TestFixture()]
	public class ThreadPoolSleepSorterTest
	{
		[Test()]
		public void SleepSortTest()
		{
			ThreadPoolSleepSorter sleepsort = new ThreadPoolSleepSorter(Console.Out, 10);

			Console.WriteLine("starting");

			sleepsort.Sort(new byte[] { 2, 5, 1 });

		}
	}
}

