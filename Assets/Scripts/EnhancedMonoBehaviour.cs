using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class EnhancedMonoBehaviour : MonoBehaviour
{
    private readonly Queue<Action> _uiQueue = new Queue<Action>();

    protected void SetTimeout(Action action, int timeout)
    {
        Timer timer = null;
        timer = new Timer(state =>
            {
                _uiQueue.Enqueue(action);
                // ReSharper disable once AccessToModifiedClosure
                timer?.Dispose();
            }
            , null, timeout, 0);
    }

    protected void RunOnUiThread(Action action)
    {
        _uiQueue.Enqueue(action);
    }

    private void Update()
    {
        while (_uiQueue.Count > 0) _uiQueue.Dequeue()();
        RunPerFrame();
    }

    protected abstract void RunPerFrame();
}