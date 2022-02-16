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
    MovingToTarPosToCast,
    Impacted,
};

public enum PlayerAnimState
{
    idle = 0,
    SkillCasting = 1,
    Moving,
    Impacted
}