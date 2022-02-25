using System;
using UnityEngine;


public class GameConst : MonoBehaviour
{
    public static GameConst instance; //singleton
    #region singleton
    private void Awake()
    {
        if (instance != null)
        {
            throw new Exception("More than one instance found!");
        }
        instance = this;

        DontDestroyOnLoad(instance);

    }
    #endregion


    [SerializeField]private int usableSkillMax;
    public int UsableSkillMax { get { return usableSkillMax; } }

    [SerializeField]LayerMask characterLayerMask;
    public LayerMask CharacterLayerMask { get { return characterLayerMask; } }

    public static int ForceAdjuster = 200;


}
