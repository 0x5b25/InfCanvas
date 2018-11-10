using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CanvasApp.Utilities
{
    class JobManDefs
    {
        public static readonly int workerNum = 4; 
    }

    class ParallelJobManager:IDisposable
    {
        static ParallelJobManager _self = new ParallelJobManager();
        public static ParallelJobManager Get() { return _self; }

        private bool _workerRun = true;
        private CountdownEvent _allStop, _workDone;
        private Action<int, int> _job;

        //------------------Manager thread-------------------
        Utilities.Queue<Tuple<Action<int, int>,ManualResetEventSlim>> _jobs = new Queue<Tuple<Action<int, int>, ManualResetEventSlim>>();
        List<Tuple<Thread, ManualResetEventSlim>> _workers = new List<Tuple<Thread, ManualResetEventSlim>>();
        ManualResetEventSlim _notifyNewJob = new ManualResetEventSlim(false);
        Thread _man;
        bool _manRun = true;
        void ManagerThread()
        {
            while (_manRun)
            {
                if (_jobs.GetDepth() <= 0)
                {
                    //No new jobs, wait for new
                    _notifyNewJob.Reset();
                }
                _notifyNewJob.Wait();
                //New job got
                var j = _jobs.Pop();
                ManualResetEventSlim finishFlag = j.Item2;
                _job = j.Item1;
                //Execute job
                if(_workDone == null)
                {
                    //No worker thread, start 4 workers
                    StartWorkers(JobManDefs.workerNum);
                }
                DoOnce();
                //All worker threads returned
                if(finishFlag != null)
                {
                    //is a synchornized job, notify sender the completion
                    finishFlag.Set();
                }
            }
        }
        //---------------------------------------------------

        ParallelJobManager() {
            _man = new Thread(ManagerThread);
            _man.Name = "ParallelJobMan";
            _man.Start();
        }

        ManualResetEventSlim _wait = new ManualResetEventSlim();
        int waitingWorkers = 0;

        public void WaitForOtherWorkers()
        {
            lock (_wait)
            {
                if(waitingWorkers >= _workDone.InitialCount - 1)
                {
                    //The last worker to be synced
                    waitingWorkers = 0;
                    _wait.Set();
                    return;
                }
                _wait.Reset();
                waitingWorkers++;
            }
            _wait.Wait();
        } 

        public void DoJob(Action<int, int> job)
        {
            if (job != null)
            {
                using (var flag = new ManualResetEventSlim(false))
                {
                    //Add to queue
                    var j = new Tuple<Action<int, int>, ManualResetEventSlim>(job, flag);
                    _jobs.Push(j);
                    //Notify mamager thread
                    _notifyNewJob.Set();
                    //Wait for completion
                    flag.Wait();
                }
            }
        }

        public void DoJobAsync(Action<int, int> job)
        {
            if (job != null)
            {
                //Add to queue
                //var flag = new ManualResetEventSlim(false);
                var j = new Tuple<Action<int, int>, ManualResetEventSlim>(job, null);
                _jobs.Push(j);
                //Notify mamager thread
                _notifyNewJob.Set();
            }
        }

        void DoOnce()
        {
            if (!_workerRun || _job == null)
                return;

            //Dispatch workers
            foreach(var w in _workers)
            {
                //Release the lock on the corresponding worker
                w.Item2.Set();
            }
            //Wait for all workers to return
            _workDone.Wait();
            _workDone.Reset();
        }

        public void StopAll()
        {
            //There are no previously started workers
            if (_allStop == null) return;
            foreach (var w in _workers)
            {
                //Release the lock on the corresponding worker
                w.Item2.Set();
            }
            //Wait for all workers to exit
            _allStop.Wait();
            //Clear registery
            _workers.Clear();
            //Release resources
            _allStop.Dispose();
            _workDone.Dispose();
            _allStop = _workDone = null;
            _job = null;
        }

        public void StartWorkers(int num)
        {
            StopAll();
            _allStop = new CountdownEvent(num);
            _workDone = new CountdownEvent(num);

            //Thread initialization
            for (int i = 0; i < num; i++)
            {
                //Buffer worker index
                int index = i;
                //Create a locker
                ManualResetEventSlim e = new ManualResetEventSlim(false);
                //Create the worker
                Thread t = new Thread(() => { Worker(index, e); })
                {
                    Name = "Worker " + index
                };
                //Regisrer worker
                _workers.Add(new Tuple<Thread, ManualResetEventSlim>(t, e));
                //Start worker
                t.Start();
            }
        }

        private void Worker(int index, ManualResetEventSlim locker)
        {
            //Thread initialize
            while (true)
            {
                //Wait until initialization is done or job is assigned
                locker.Wait();
                //Pause and wait after 1 loop
                locker.Reset();
                if (_workerRun)
                {
                    _job.Invoke(index, _allStop.InitialCount);//Thread.Sleep(3000);
                    _workDone.Signal();
                }
                else break;
            }
            //Thread exit
            _allStop.Signal();
        }

        public void Dispose()
        {
            _manRun = false;
            StopAll();
            _allStop.Dispose();
            _workDone.Dispose();
            _self = null;
        }
    }
}
