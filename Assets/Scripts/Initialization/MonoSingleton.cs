using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    //private instance of class to be initialized as a singleton
    private static T _instance;

    //public property instance 
    public static T Instance
    {
        get
        {
            //null check manager class trying to initialize as singleton
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                //null check manager class trying to initialize as singleton
                if (_instance == null)
                {
                    Debug.LogWarning("The " + typeof(T).ToString() + " is NULL.");
                }
            }
               
            //instance of manager class being initialized
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this as T;

        Initialization();
    }

    public virtual void Initialization()
    {
        //optional override
    }
}
