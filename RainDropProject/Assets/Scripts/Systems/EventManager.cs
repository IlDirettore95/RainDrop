using System;
using System.Collections.Generic;

namespace GMDG.RainDrop.System
{
    /*
     * This class implements the Pub/Sub design pattern
     * An event (enum) can be published or a listener (with void <name-method>(object[]) signature) can be subscribed or unsubscribed
     */
    public class EventManager
    {
        private static EventManager _instance;
        public static EventManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EventManager();
                }
                return _instance;
            }
        }

        private Dictionary<Event, Action<object[]>> _listenersDictionary = new Dictionary<Event, Action<object[]>>();

        private EventManager() { }

        public void Subscribe(Event eventName, Action<object[]> listener)
        {
            if (!_listenersDictionary.ContainsKey(eventName))
            {
                _listenersDictionary[eventName] = listener;
            }
            else
            {
                _listenersDictionary[eventName] += listener;
            }
        }

        public void Unsubscribe(Event eventName, Action<object[]> listener)
        {      
            if (_listenersDictionary.ContainsKey(eventName))
            {
                _listenersDictionary[eventName] -= listener;

                if (_listenersDictionary[eventName] == null)
                {
                    _listenersDictionary.Remove(eventName);
                }
            }
        }

        public void Publish(Event eventName, params object[] args)
        {
            LogManager.LogEventManager(eventName);

            if (_listenersDictionary.ContainsKey(eventName))
            {
                _listenersDictionary[eventName]?.Invoke(args);
            }
        }

        public override string ToString()
        {
            string text = string.Empty;

            foreach (Event e in _listenersDictionary.Keys)
            {
                text += "Event registred: " + e + "\tListeners: " + _listenersDictionary[e]?.GetInvocationList().Length + Environment.NewLine;
            }

            return text;
        }
    }

    public enum Event
    {
        // Boot------------------------------------------------------------------------
        // Called when the boot has finished initializing the systems
        OnSystemsLoaded,
        // Called when the boot has finished its work and started to unload
        OnBootFinished,

        // GameManager-----------------------------------------------------------------
        // Called when the game manager finished its awake
        OnGameManagerLoaded,
        // Called when the game manager finished its onDestroy
        OnGameManagerDestroyed,
        // Called when the game manager state changed
        OnGameManagerChangedState,

        // LevelManager----------------------------------------------------------------
        // Called when the level manager finished its awake
        OnLevelManagerLoaded,
        // Called when the level manager finished its onDestroy
        OnLevelManagerDestroyed,
    }
}