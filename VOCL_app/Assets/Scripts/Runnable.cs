using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Runnable : MonoBehaviour
{
    public class Singleton<T> where T : class
    {
        #region Private Data
        static private T sm_Instance = null;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the Singleton instance of T.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (sm_Instance == null)
                    CreateInstance();
                return sm_Instance;
            }
        }
        #endregion

        #region Singleton Creation
        /// <summary>
        /// Create the singleton instance.
        /// </summary>
        private static void CreateInstance()
        {
            if (typeof(MonoBehaviour).IsAssignableFrom(typeof(T)))
            {
                string singletonName = "_" + typeof(T).Name;

                GameObject singletonObject = GameObject.Find(singletonName);
                if (singletonObject == null)
                    singletonObject = new GameObject(singletonName);
#if SINGLETONS_VISIBLE
        singletonObject.hideFlags = HideFlags.DontSave;
#else
                singletonObject.hideFlags = HideFlags.HideAndDontSave;
#endif
                sm_Instance = singletonObject.GetComponent<T>();
                if (sm_Instance == null)
                    sm_Instance = singletonObject.AddComponent(typeof(T)) as T;
            }
            else
            {
                sm_Instance = Activator.CreateInstance(typeof(T)) as T;
            }

            if (sm_Instance == null)
                throw new Exception("Failed to create instance " + typeof(T).Name);
        }
        #endregion
    }


    #region Public Properties
    /// <summary>
    /// Returns the Runnable instance.
    /// </summary>
    public static Runnable Instance { get { return Singleton<Runnable>.Instance; } }
    #endregion

    #region Public Interface
    /// <summary>
    /// Start a co-routine function.
    /// </summary>
    /// <param name="routine">The IEnumerator returns by the co-routine function the user is invoking.</param>
    /// <returns>Returns a ID that can be passed into Stop() to halt the co-routine.</returns>
    public static int Run(IEnumerator routine)
    {
        Routine r = new Routine(routine);
        return r.ID;
    }

    /// <summary>
    /// Stops a active co-routine.
    /// </summary>
    /// <param name="ID">THe ID of the co-routine to stop.</param>
    public static void Stop(int ID)
    {
        Routine r = null;
        if (Instance.m_Routines.TryGetValue(ID, out r))
            r.Stop = true;
    }

    /// <summary>
    /// Check if a routine is still running.
    /// </summary>
    /// <param name="id">The ID returned by Run().</param>
    /// <returns>Returns true if the routine is still active.</returns>
    static public bool IsRunning(int id)
    {
        return Instance.m_Routines.ContainsKey(id);
    }

#if UNITY_EDITOR
    private static bool sm_EditorRunnable = false;

    /// <summary>
    /// This function enables the Runnable in edit mode.
    /// </summary>
    public static void EnableRunnableInEditor()
    {
        if (!sm_EditorRunnable)
        {
            sm_EditorRunnable = true;
            UnityEditor.EditorApplication.update += UpdateRunnable;
        }
    }
    static void UpdateRunnable()
    {
        if (!Application.isPlaying)
            Instance.UpdateRoutines();
    }

#endif
    #endregion

    #region Private Types
    /// <summary>
    /// This class handles a running co-routine.
    /// </summary>
    private class Routine : IEnumerator
    {
        #region Public Properties
        public int ID { get; private set; }
        public bool Stop { get; set; }
        #endregion

        #region Private Data
        private bool m_bMoveNext = false;
        private IEnumerator m_Enumerator = null;
        #endregion

        public Routine(IEnumerator a_enumerator)
        {
            m_Enumerator = a_enumerator;
            Runnable.Instance.StartCoroutine(this);
            Stop = false;
            ID = Runnable.Instance.m_NextRoutineId++;

            Runnable.Instance.m_Routines[ID] = this;
#if ENABLE_RUNNABLE_DEBUGGING
                Debug.Log( string.Format("Coroutine {0} started.", ID ) ); 
#endif
        }

        #region IEnumerator Interface
        public object Current { get { return m_Enumerator.Current; } }
        public bool MoveNext()
        {
            m_bMoveNext = m_Enumerator.MoveNext();
            if (m_bMoveNext && Stop)
                m_bMoveNext = false;

            if (!m_bMoveNext)
            {
                Runnable.Instance.m_Routines.Remove(ID);      // remove from the mapping
#if ENABLE_RUNNABLE_DEBUGGING
                    Debug.Log( string.Format("Coroutine {0} stopped.", ID ) );
#endif
            }

            return m_bMoveNext;
        }
        public void Reset() { m_Enumerator.Reset(); }
        #endregion
    }
    #endregion

    #region Private Data
    private Dictionary<int, Routine> m_Routines = new Dictionary<int, Routine>();
    private int m_NextRoutineId = 1;
    #endregion

    /// <summary>
    /// THis can be called by the user to force all co-routines to get a time slice, this is usually
    /// invoked from an EditorApplication.Update callback so we can use runnable in Editor mode.
    /// </summary>
    public void UpdateRoutines()
    {
        if (m_Routines.Count > 0)
        {
            // we are not in play mode, so we must manually update our co-routines ourselves
            List<Routine> routines = new List<Routine>();
            foreach (var kp in m_Routines)
                routines.Add(kp.Value);

            foreach (var r in routines)
                r.MoveNext();
        }
    }
}
