using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GMDG.RainDrop.System
{
    public class LogManager : MonoBehaviour
    {
        private IEnumerator _endOfFrameCoroutine;
        private WaitForEndOfFrame _waitForEndOfFrame;

        #region Unity_Messages

        private void Awake()
        {
#if UNITY_EDITOR && DEBUG_MODE
            _endOfFrameCoroutine = TrackEndOfFrame();
            StartCoroutine(_endOfFrameCoroutine);
#endif
        }

        private IEnumerator TrackEndOfFrame()
        {
            while(true) 
            { 
                yield return _waitForEndOfFrame;
                LogEndOfFrame();
            }
        }

        #endregion

#if UNITY_EDITOR
        [Conditional("DEBUG_MODE")]
#else
        [Conditional("DONT_RUN")]
#endif
        public static void LogEventManager(EEvent eventToLog)
        {
            Debug.Log(string.Format("<color=lightblue>Event: <b>{0}</b></color>", eventToLog));
        }

#if UNITY_EDITOR
        [Conditional("DEBUG_MODE")]
#else
        [Conditional("DONT_RUN")]
#endif
        public static void LogLevelManager(LevelManager context)
        { 
            Debug.Log(string.Format("<color=orange>Level Manager: <b>List of drop count: {0}</b></color>", context.Drops.Count));
            Debug.Log(string.Format("<color=orange>Level Manager: <b>Normal Pool: {0}</b></color>", context.DropsPool.ToString()));
            Debug.Log(string.Format("<color=orange>Level Manager: <b>Golden Pool: {0}</b></color>", context.GoldenDropsPool.ToString()));
        }

#if UNITY_EDITOR
        [Conditional("DEBUG_MODE")]
#else
        [Conditional("DONT_RUN")]
#endif
        public static void LogMessage(string message)
        {
            Debug.Log(string.Format("<color=silver>{0}</color>", message));
        }

#if UNITY_EDITOR
        [Conditional("DEBUG_MODE")]
#else
        [Conditional("DONT_RUN")]
#endif
        public static void LogEndOfFrame()
        {
            Debug.Log("<color=red><b>End of Frame</b></color>");
        }
    }
}
