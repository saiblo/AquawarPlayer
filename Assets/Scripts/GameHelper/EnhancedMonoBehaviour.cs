using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;

namespace GameHelper
{
    public abstract class EnhancedMonoBehaviour : MonoBehaviour
    {
        private readonly ConcurrentQueue<Action> _uiQueue = new ConcurrentQueue<Action>();

        protected internal void SetTimeout(Action action, int timeout)
        {
            IEnumerator DelayCoroutine()
            {
                yield return new WaitForSeconds((float)timeout / 1000);
                _uiQueue.Enqueue(action);
            }

            StartCoroutine(DelayCoroutine());
        }

        protected internal void Repeat(Action<int> repeatedAction, Action cleanup, int totalTimes, int dueTime,
            int period)
        {
            IEnumerator RepeatCoroutine()
            {
                yield return new WaitForSeconds((float)dueTime / 1000);
                for (var i = 0; i < totalTimes; i++)
                {
                    var j = i;
                    _uiQueue.Enqueue(() => { repeatedAction(j); });
                    yield return new WaitForSeconds((float)period / 1000);
                }
                _uiQueue.Enqueue(cleanup);
            }

            StartCoroutine(RepeatCoroutine());
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