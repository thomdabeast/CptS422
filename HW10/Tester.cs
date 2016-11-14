using System;
using System.Numerics;

namespace CS422
{
    public class Tester
    {
        public static void Main(string[] args)
        {
			BigNum biggy = new BigNum(2.5.ToString());
			BigNum a = new BigNum(2.5.ToString());

			biggy *= a;

			Console.WriteLine(biggy);
        }
    }
}
