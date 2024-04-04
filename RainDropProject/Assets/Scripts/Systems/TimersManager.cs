using System;
using System.Collections.Generic;
using UnityEngine;

namespace GMDG.RainDrop.System
{
    /*
     * This manager handles all timers in the game
     */
    public class TimersManager : MonoBehaviour
    {
        public static TimersManager Instance;

        private List<Timer> _timers;

        #region Unity_Messages

        private void Awake()
        {
            // Singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }

            _timers = new List<Timer>();

            EventManager.Instance.Publish(EEvent.OnTimerManagerLoaded);
        }

        private void OnDestroy()
        {
            EventManager.Instance.Publish(EEvent.OnTimerManagerDestroyed);
        }

        private void Update()
        {
            for (int i = 0; i < _timers.Count; i++) 
            { 
                Timer timer = _timers[i];

                if (timer.State != ETimerState.Running)
                {
                    continue;
                }

                timer.Tick(Time.deltaTime);
            }
        }

        #endregion

        public void Subscribe(Timer timer)
        {
            _timers.Add(timer);
        }

        public void Unsubscribe(Timer timer) 
        { 
            _timers.Remove(timer);
        }
    }

    /*
     * Class of a timer
     * Please when the holder of the instance is destroyed the Timer should be unsubscribed (if it is) from the manager (calling Stop) 
     * otherwise it will leak in the manager
     */
    public class Timer
    {
        public float MaxTimeToWait { get; private set; }
        public float TimeLeft { get; private set; }

        public ETimerState State { get; private set; }

        private Action _callBack;

        public Timer()
        {
            MaxTimeToWait = -1f;
            TimeLeft = -1f;
            State = ETimerState.Stopped;
        }

        // Start the timer and will invoke the callback when finished
        public void Start(float amount, Action callback)
        {
            if (State == ETimerState.Stopped || State == ETimerState.Finished)
            {
                MaxTimeToWait = amount;
                TimeLeft = amount;
                State = ETimerState.Running;
                _callBack = callback;

                TimersManager.Instance.Subscribe(this);
            }
        }

        // Stops the timer
        public void Stop()
        {
            if (State == ETimerState.Running || State == ETimerState.Paused) 
            {
                MaxTimeToWait = -1f;
                TimeLeft = -1f;
                State = ETimerState.Stopped;
                _callBack = null;

                TimersManager.Instance.Unsubscribe(this);
            }
        }

        // Restart the timer
        public void Restart()
        {
            if (State == ETimerState.Running || State == ETimerState.Paused)
            {
                TimeLeft = MaxTimeToWait;

                if (State == ETimerState.Paused)
                {
                    State = ETimerState.Running;
                    TimersManager.Instance.Subscribe(this);
                }
            }
        }

        // Pause the timer
        public void Pause()
        {
            if (State == ETimerState.Running)
            {
                State = ETimerState.Paused;
            }
        }

        // Unpause the timer
        public void Unpause()
        {
            if (State == ETimerState.Paused)
            {
                State = ETimerState.Running;
            }
        }

        // Update the timer. It is called by the TimersManager
        public void Tick(float delta)
        {
            TimeLeft -= delta;

            if (TimeLeft <= 0)
            {
                MaxTimeToWait = -1f;
                TimeLeft = -1f;
                State = ETimerState.Finished;
                TimersManager.Instance.Unsubscribe(this);
                _callBack?.Invoke();
            }
        }

        public override string ToString()
        {
            string text = string.Empty;

            text += string.Format("MaxTime: {0}, TimeLeft: {1}, State: {2}", MaxTimeToWait, TimeLeft, State); 

            return text;
        }
    }

    // Stopped  -> (Start)          -> Running
    // Running  -> (Stop)           -> Stopped
    //          -> (Restart)        -> Running
    //          -> (Pause)          -> Paused
    //          -> (time elapsed)   -> Finished
    // Paused   -> (Unpause)        -> Running
    //          -> (Restart)        -> Running
    //          -> (Stop)           -> Stopped
    // Finished -> (Start)          -> Running
    public enum ETimerState
    {
        Running,
        Paused,
        Stopped,
        Finished
    }
}
