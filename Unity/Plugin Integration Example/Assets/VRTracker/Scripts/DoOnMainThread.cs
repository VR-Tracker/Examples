using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DoOnMainThread : MonoBehaviour
{

    public static DoOnMainThread instance;

    public readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one MainThread in the scene");
        }
        else
        {
            instance = this;
        }
    }

    public virtual void Update()
    {
        // dispatch stuff on main thread
        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }
    }
}
