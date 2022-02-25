using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillInfo", menuName = "ScriptableObject/Skill/SkillInfo", order = 0)]
public class SkillInfo : ScriptableObject
{

    new public string name;
    public Sprite icon = null;

    public string skillDescription;

    public SkillCastType castType;
    public SkillFuncType funcType;

    public float DefaultCooldownTime;

}
