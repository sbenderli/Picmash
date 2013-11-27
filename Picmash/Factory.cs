using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Picmash
{
    public class Factory
    {
        public static long CurrentTotalBytes { get; set; }
        public static long MaxTotalBytes { get; private set; }
        public static object Locker;
        public static Dictionary<string, FileInfo> FileDictionary { get; set; }
        public int PoolSize { get; private set; }
        public static string FactoryRootDir { get; private set; }
        public static CountdownEvent Signaller;
        private ManualResetEvent[] doneEvents;
        public Factory(int poolSize, string startDir, long maxBytes)
        {
            //Create pool
            this.PoolSize = poolSize;
            FactoryRootDir = startDir;
            doneEvents = new ManualResetEvent[poolSize];
            FileDictionary = new Dictionary<string, FileInfo>();
            MaxTotalBytes = maxBytes;
            Locker = new object();
            Signaller = new CountdownEvent(poolSize);
        }

        public void StartWork()
        {
            Console.WriteLine("launching {0} tasks...", PoolSize);

            for (int i = 0; i < PoolSize; i++)
            {
                FileWorker fw = new FileWorker(FactoryRootDir);
                ThreadPool.QueueUserWorkItem(fw.ThreadPoolCallback, i);
            }

            Signaller.Wait();
            Console.WriteLine("All pickings are complete. Total Size:{0} bytes", MaxTotalBytes);

            foreach (var file in FileDictionary)
            {
                Console.WriteLine(String.Format("{0}:{1}", file.Value.FullName, file.Value.Length));
            }

        }
    }

    public class FileWorker
    {
        private Random _r;
        private bool _shouldStop = false;
        private int _threadIndex = -1;
        public string WorkerRootDir { get; private set; }
        // Constructor. 
        public FileWorker(string rootDir)
        {
            WorkerRootDir = rootDir;
            _r = new Random();
        }

        // Wrapper method for use with thread pool. 
        public void ThreadPoolCallback(Object threadContext)
        {
            _threadIndex = (int)threadContext;

            Console.WriteLine("thread {0} started in rootDir: {1}", _threadIndex, this.WorkerRootDir);

            while (!_shouldStop)
            {
                FileInfo f = PickFiles(WorkerRootDir);
                if(f != null)
                    Console.WriteLine("thread {0} picked:{1}", _threadIndex, f.FullName);
            }
            

            Console.WriteLine("thread {0} is done", _threadIndex);
            Factory.Signaller.Signal();
        }

        // Recursive method that calculates the Nth Fibonacci number. 
        public FileInfo PickFiles(string rootDir)
        {
            //File or directory?
            var dirInfo = new DirectoryInfo(rootDir);
            bool isFile = _r.Next() % 2 == 0;

            if (isFile)
            {
                System.IO.FileInfo[] files = null;
                try
                {
                    files = dirInfo.GetFiles("*.jpg")
                        .Union(dirInfo.GetFiles("*.jpeg")).ToArray()
                        .Union(dirInfo.GetFiles("*.tif")).ToArray()
                        .Union(dirInfo.GetFiles("*.tiff")).ToArray()
                        .Union(dirInfo.GetFiles("*.gif")).ToArray()
                        .Union(dirInfo.GetFiles("*.png")).ToArray()    
                        ;
                }
                catch (UnauthorizedAccessException)
                {
                    return null;
                }

                if (files.Length > 0)
                {
                    FileInfo file = files[_r.Next(0, files.Length)];

                    lock (Factory.Locker)
                    {
                        string hash = "";
                        try
                        {
                            hash = HashFunctions.HashFile(file.FullName);
                        }
                        catch (Exception)
                        {
                            //if file in use, try another file
                            return null;
                        }
                        if (!Factory.FileDictionary.ContainsKey(hash) && Factory.CurrentTotalBytes + file.Length <= Factory.MaxTotalBytes)
                        {
                            Factory.FileDictionary.Add(hash, file);
                            Factory.CurrentTotalBytes += file.Length;
                           
                            return file;
                        }
                    }
                    if (Factory.CurrentTotalBytes + file.Length > Factory.MaxTotalBytes)
                    {
                        _shouldStop = true;
                        return null;
                    }
                    else return null;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                System.IO.DirectoryInfo[] dirInfos = null;
                try
                {
                    dirInfos = dirInfo.GetDirectories("*.*");
                }
                catch (Exception)
                {
                    //Go back to root, try again
                    return null;
                }

                if (dirInfos.Length > 0)
                {
                    string nextDir = dirInfos[_r.Next(0, dirInfos.Length)].FullName;
                    return PickFiles(nextDir);
                }
                else
                {
                    //Go back to root, try again
                    return null;
                }

            }
        }
    }

}
