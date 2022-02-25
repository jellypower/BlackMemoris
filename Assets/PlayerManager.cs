using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    public static GameObject PlayerGameObj;
    public static PlayerController Player;


    private void Awake()
    {
        if (instance != null)
        {
            throw new Exception("More than one instance found!");
        }
        instance = this;

        PlayerGameObj = GameObject.FindWithTag("Player");
        if (PlayerGameObj == null) throw new Exception("No Player Found!");
        Player = PlayerGameObj.GetComponent<PlayerController>();
        if (Player == null) throw new Exception("No PlayerController found from GameObject!");

    }

        void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
