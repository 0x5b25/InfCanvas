using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CanvasApp.Utilities
{
    class ParallelJobManager:IDisposable
    {
        static ParallelJobManager _self = new ParallelJobManager();
        public static ParallelJobManager Get() { return _self; }

        private readonly object _cv = new object();
        private bool _workerRun = true;
        private CountdownEvent _allStop, _workDone;
        private Action<int, int> _job;

        //------------------Manager thread-------------------
        Utilities.Queue<Tuple<Action<int, int>,ManualResetEventSlim>> _jobs = new Queue<Tuple<Action<int, int>, ManualResetEventSlim>>();
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
                Action<int, int> job = j.Item1;
                //Execute job
                _job = job;
                if(_workDone == null)
                {
                    //No worker thread, start 4 workers
                    StartWorkers(4);
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
            _man.Start();
        }

        ManualResetEventSlim _wait = new ManualResetEventSlim();
        int waitingWorkers = 0;

        public void WaitForOtherWorkers()
        {
            lock (_wait)
            {
                if(waitingWorkers == _workDone.InitialCount - 1)
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

            //lock cv in case some threads may run before all workers aer notified
            lock (_cv)
                Monitor.PulseAll(_cv);

            _workDone.Wait();
            _workDone.Reset();

        }

        public void StopAll()
        {
            //There are no previously started workers
            if (_allStop == null) return;

            _workerRun = false;
            //lock (_cv)
                Monitor.PulseAll(_cv);
            //Wait for all workers to exit
            _allStop.Wait();
            _allStop.Dispose();
            _workDone.Dispose();
            _allStop = _workDone = null;
            _job = null;
        }

        public int StartWorkers(int num)
        {
            StopAll();
            int cycles = 0;
            _allStop = new CountdownEvent(num);
            _workDone = new CountdownEvent(num);

            //Thread initialization
            for (int i = 0; i < num; i++)
            {
                int index = i;
                Thread t = new Thread(() => { Worker(index); });
                t.Start();

                while (true)
                {
                    if (t.ThreadState == ThreadState.WaitSleepJoin)
                        break;
                    cycles++;
                }
            }
            return cycles;
        }

        private void Worker(int index)
        {
            //Thread initialize
            while (true)
            {
                //Wait until initialization is done
                
                lock (_cv)
                {
                    Monitor.Wait(_cv);
                }
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
            ((IDisposable)_allStop).Dispose();
        }
    }
}
