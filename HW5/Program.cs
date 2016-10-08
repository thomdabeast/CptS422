using System.IO;
using System;

namespace CS422
{
    public class Program
    {
        public static void Main(string[] args)
        {
            byte[] buffer = new byte[10]{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            ConcatStream stream = new ConcatStream(
                    new MemoryStream(new byte[1]{ 10 }), new MemoryStream(buffer), 10);

            byte[] b = new byte[11];
            long amountRead = 0;
            amountRead += stream.Read(b, 0, 1);
            stream.Seek(0, SeekOrigin.Begin);
            amountRead += stream.Read(b, 1, 1);
            stream.Seek(0, SeekOrigin.Begin);
            amountRead += stream.Read(b, 2, 1);
            stream.Seek(0, SeekOrigin.Begin);

            foreach(var i in b) Console.Write(i+ " ");
            Console.WriteLine("Amount read: " + amountRead);

            if(amountRead == 11)
            {
                Console.WriteLine("We read in everything");
            }
            else
            {
                Console.WriteLine("We didn't read everything");
            }
        }
    }
}
