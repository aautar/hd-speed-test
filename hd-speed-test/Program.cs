using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace HDSpeedTest
{
    struct Sample
    {
        public string type;
        public int chunkSize;
        public double readTimeMilliseconds;
        public double writeTimeMilliseconds;
        public double readBytesPerMilliseconds;
        public double writeBytesPerMilliseconds;

        public string toJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    class HumanReadableSample
    {
        private Sample baseSample;

        public HumanReadableSample(Sample s)
        {
            baseSample = s;
        }

        public string toJson()
        {
            Dictionary<string, object> humanReadableFields = new Dictionary<string, object>();
            humanReadableFields.Add("type", baseSample.type);
            humanReadableFields.Add("chunkSize", (baseSample.chunkSize / 1024.0).ToString() + "K");
            humanReadableFields.Add("readTime", Math.Round((baseSample.readTimeMilliseconds / 1000.0), 3).ToString() + "s");
            humanReadableFields.Add("writeTime", Math.Round((baseSample.writeTimeMilliseconds / 1000.0), 3).ToString() + "s");
            humanReadableFields.Add("readPerf", Math.Round(((baseSample.chunkSize / 1024.0 / 1024.0) / (baseSample.readTimeMilliseconds / 1000.0)), 3).ToString() + "MBps");
            humanReadableFields.Add("writePerf", Math.Round(((baseSample.chunkSize / 1024.0 / 1024.0) / (baseSample.writeTimeMilliseconds / 1000.0)), 3).ToString().ToString() + "MBps");

            return JsonConvert.SerializeObject(humanReadableFields);
        }
    }

    class Program
    {
        static private List<Sample> samples = new List<Sample>();

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

        static Sample RecordSample(int chunkSize, TimeSpan writeTime, TimeSpan readTime)
        {
            Sample s = new Sample();
            s.type = "sample";
            s.chunkSize = chunkSize;
            s.writeTimeMilliseconds = writeTime.TotalMilliseconds;
            s.readTimeMilliseconds = readTime.TotalMilliseconds;
            s.writeBytesPerMilliseconds = chunkSize / writeTime.TotalMilliseconds;
            s.readBytesPerMilliseconds = chunkSize / readTime.TotalMilliseconds;

            samples.Add(s);

            return s;
        }

        static void OutputKeyValueMeta(string key, string value)
        {
            Dictionary<string, string> kv = new Dictionary<string, string>();
            kv.Add(key, value);

            Dictionary<string, object> lineParts = new Dictionary<string, object>();
            lineParts.Add("type", "meta");
            lineParts.Add("message", kv);

            Console.WriteLine(JsonConvert.SerializeObject(lineParts));
        }

        static void Main(string[] args)
        {
            int SAMPLES_PER_CHUNK = 10;

            if(args.Length == 0)
            {
                Console.WriteLine("USAGE: hd-speed-test.exe <TESTPATH> [FORMAT]");
                return;
            }

            string outputFor = "machine";
            if(args.Length == 2 && args[1] == "--human")
            {
                outputFor = "human";
            }

            string testFilepath = Path.Combine(args[0], "hd-speed-test.data");

            try
            {
                OutputKeyValueMeta("testPath", testFilepath);
                OutputKeyValueMeta("samplesPerChunk", SAMPLES_PER_CHUNK.ToString());

                for (int i = 1; i <= 8092; i *= 2)
                {
                    for (int j = 0; j < SAMPLES_PER_CHUNK; j++)
                    {
                        int numBytes = i * 1024;

                        TimeSpan writeTime = Program.WriteTestBuffer(testFilepath, numBytes);
                        TimeSpan readTime = Program.ReadTestBuffer(testFilepath, numBytes);

                        Sample s = RecordSample(numBytes, writeTime, readTime);

                        if(outputFor == "machine")
                        {
                            Console.WriteLine(s.toJson());
                        }

                        if(outputFor == "human")
                        {
                            Console.WriteLine(new HumanReadableSample(s).toJson());
                        }

                    }
                }
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                File.Delete(testFilepath);
            }           

        }
    }
}
