using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum SkillFuncType
{
    Attack,
    Transport,
    Buff
}

public enum SkillCastType
{
    Targeting,
    NonTargeting,
    Toggle,
    Passive
}

public enum SkillState
{
    Castable,
    FarToCast,
    TargetNotFound,
    CoolDown,
    Casting
}