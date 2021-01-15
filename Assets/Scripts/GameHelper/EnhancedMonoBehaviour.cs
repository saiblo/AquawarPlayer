using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace GameHelper
{
    public abstract class EnhancedMonoBehaviour : MonoBehaviour
    {
        private readonly ConcurrentQueue<Action> _uiQueue = new ConcurrentQueue<Action>();

        protected internal void SetTimeout(Action action, int timeout)
        {
            new Thread(() =>
            {
                var autoEvent = new AutoResetEvent(false);
                var timer = new Timer(stateInfo =>
                    {
                        _uiQueue.Enqueue(action);
                        ((AutoResetEvent) stateInfo).Set();
                    }
                    , autoEvent, timeout, 0);
                autoEvent.WaitOne();
                timer.Dispose();
            }).Start();
        }

        private TimerCallback Counter(int total, Action<int> repeatedAction)
        {
            var count = 0;
            return stateInfo =>
            {
                var i = count;
                ++count;
                _uiQueue.Enqueue(() => { repeatedAction(i); });
                if (count >= total) ((AutoResetEvent) stateInfo).Set();
            };
        }

        protected internal void Repeat(Action<int> repeatedAction, Action cleanup, int totalTimes, int dueTime,
            int period)
        {
            new Thread(() =>
            {
                var autoEvent = new AutoResetEvent(false);
                var timer = new Timer(Counter(totalTimes, repeatedAction), autoEvent, dueTime, period);
                autoEvent.WaitOne();
                timer.Dispose();
                RunOnUiThread(cleanup);
            }).Start();
        }

        public void RunOnUiThread(Action action)
        {
            _uiQueue.Enqueue(action);
        }

        private void Update()
        {
            while (_uiQueue.TryDequeue(out var action)) action();
            RunPerFrame();
        }

        protected abstract void RunPerFrame();
    }
}