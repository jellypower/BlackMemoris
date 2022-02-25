using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillTree : MonoBehaviour
{

    #region Singleton
    public static PlayerSkillTree instance;

    private void Awake()
    {
        if(instance != null)
        {
            Debug.LogWarning("More than one skilltree found!");
            return;
        }

        instance = this;
    }
    #endregion

    public delegate void OnSkillChanged(int a);
    public OnSkillChanged onSkillChangedCallback;

    public List<Skill> UsableSkills = new List<Skill>();
    

    public void Add(Skill skill)
    {
        UsableSkills.Add(skill);
    }

    public void Remove(Skill skill)
    {
        UsableSkills.Remove(skill);
    }
}
