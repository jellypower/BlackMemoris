using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{

    [SerializeField]BattleSystemUIManager BattleUI;

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

    public void BattleAlert(string message, float messageLastTime, float fadeTime)
    {
        BattleUI.Alert(message, messageLastTime, fadeTime);
    }


    
}
