using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    private void Awake()
    {
        if(instance != null)
        {
            throw new Exception("More than one instance found!");
        }
        instance = this;

        DontDestroyOnLoad(instance);

    }


    
}
