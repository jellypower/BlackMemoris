public enum OrderState
{
    None = 0,
    MoveToTargetPos = 1,
    CastToTarget,
    CastToGround,
    StartCastSkill,
    Stop

}
public enum CursorState
{
    Normal = 0,
    Targeting
}

public enum PlayerActionState
{
    idle = 0,
    SkillCasting = 1,
    Moving,
    MovingToTargetToCast,
    MovingToTarPosToCast, // TarPos 위치에 스킬을 cast하기 위해 움직이는 상태.
                          // TarPos에 도달하면 스킬이 Castable 상태가 되고 Castable 상태가 되면 SkillCasting 상태로 바뀜
    CrowdControl,
};

public enum PlayerAnimState
{
    idle = 0,
    SkillCasting = 1,
    Moving,
    Impacted
}