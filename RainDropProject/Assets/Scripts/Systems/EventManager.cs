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

        private Dictionary<EEvent, Action<object[]>> _listenersDictionary = new Dictionary<EEvent, Action<object[]>>();

        private EventManager() { }

        public void Subscribe(EEvent eventName, Action<object[]> listener)
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

        public void Unsubscribe(EEvent eventName, Action<object[]> listener)
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

        public void Publish(EEvent eventName, params object[] args)
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

            foreach (EEvent e in _listenersDictionary.Keys)
            {
                text += "Event registred: " + e + "\tListeners: " + _listenersDictionary[e]?.GetInvocationList().Length + Environment.NewLine;
            }

            return text;
        }
    }

    public enum EEvent
    {
        // Boot------------------------------------------------------------------------
        // Called when the boot has finished initializing the systems
        OnBootSystemsLoaded,
        // Called when the boot has finished its work and started to unload
        OnBootFinished,

        // GameManager-----------------------------------------------------------------
        // Called when the game manager finished its awake
        OnGameManagerLoaded,
        // Called when the game manager finished its onDestroy
        OnGameManagerDestroyed,
        // Called when the game manager state changed
        OnGameManagerChangedState,
        // Called when the game points changed
        OnGameManagerChangedPoints,

        // LevelManager----------------------------------------------------------------
        // Called when the level manager finished its awake
        OnLevelManagerLoaded,
        // Called when the level manager finished its onDestroy
        OnLevelManagerDestroyed,
        // Called when a drop is spawned
        OnLevelManagerDropSpawned,
        // Called when a drop is despawned
        OnLevelManagerDropDespawned,
        // Called when a submitted result explodes a drop
        OnLevelManagerDropExplosion,
        // Called when a submitted result explodes a golden drop
        OnLevelManagerGoldenDropExplosion,

        // UIManager-------------------------------------------------------------------
        // Called when the ui manager finished its awake
        OnUIManagerLoaded,
        // Called when the ui manager finished its onDestroy
        OnUIManagerDestroyed,
        // Called when the start game button is clicked in the main menu
        OnUIManagerStartGameClicked,
        // Called when a valid potential result is submitted via input field
        OnUIManagerResultWasSubmitted,
    }
}