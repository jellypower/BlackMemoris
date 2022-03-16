using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerSkillContainer : MonoBehaviour
{


    public Skill[] Skills;
    
    public const int SkillMax = 7;


    private void Awake()
    {
        initSkillContainer();
    }

    private void Start()
    {
        tmpInit();
    }


    void initSkillContainer()
    {

        Skills = new Skill[SkillMax];
        
    }

    void tmpInit()
    {
        if (name == "Player")
        {

            GameObject obj = transform.Find("BasicAttack").gameObject;
            if (obj == null) throw new Exception("BasicAttack GameObject not found");
            Skills[4] = obj.GetComponent<Skill>();
            if (Skills[4] == null) throw new Exception("BasicAttack's Skill component not found");

            obj = transform.Find("ManaBomb").gameObject;
            if (obj == null) throw new Exception("ManaBomb GameObject not found");
            Skills[3] = obj.GetComponent<Skill>();
            if (Skills[3] == null) throw new Exception("ManaBomb's Skill component not found");
        }

    }


}
