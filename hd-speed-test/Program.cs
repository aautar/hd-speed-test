using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace HDSpeedTest
{
    class Program
    {
        static byte[] CreateTestBuffer(int size)
        {
            Random r = new Random();
            byte[] testBuf = new byte[size];
            r.NextBytes(testBuf);

            return testBuf;
        }

        static TimeSpan WriteTestBuffer(string filepath, int testBufferSize)
        {
            byte[] testBuf = CreateTestBuffer(testBufferSize);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            File.WriteAllBytes(filepath, testBuf);

            stopWatch.Stop();
            return stopWatch.Elapsed;
        }

        static TimeSpan ReadTestBuffer(string filepath, int testBufferSize)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            File.ReadAllBytes(filepath);

            stopWatch.Stop();
            return stopWatch.Elapsed;
        }

        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("USAGE: hd-speed-test.exe <TESTPATH>");
                return;
            }

            string testFilepath = Path.Combine(args[0], "hd-speed-test.data");
            Console.WriteLine(String.Format("testFilepath = {0}", testFilepath));

            double readPerfMbps = 0.0;
            double writePerfMbps = 0.0;

            for (int i = 1; i <= 8092; i*=2)
            {
                for (int j = 0; j < 10; j++)
                {
                    int numBytes = i * 1024;

                    TimeSpan writeTime = Program.WriteTestBuffer(testFilepath, numBytes);
                    TimeSpan readTime = Program.ReadTestBuffer(testFilepath, numBytes);

                    Console.WriteLine(String.Format("{0}K chunk, sample {1}, write time = {2}", i, j, writeTime));
                    Console.WriteLine(String.Format("{0}K chunk, sample {1}, read time = {2}", i, j, readTime));

                    writePerfMbps = (numBytes / writeTime.TotalSeconds) / 1024.0 / 1024.0;
                    readPerfMbps = (numBytes / readTime.TotalSeconds) / 1024.0 / 1024.0;

                    Console.WriteLine(String.Format("write performance = {0} Mb/s", writePerfMbps));
                    Console.WriteLine(String.Format("read performance = {0} Mb/s", readPerfMbps));
                }
            }

            File.Delete(testFilepath);

        }
    }
}
