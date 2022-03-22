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
    MovingToTarPosToCast, // TarPos ��ġ�� ��ų�� cast�ϱ� ���� �����̴� ����.
                          // TarPos�� �����ϸ� ��ų�� Castable ���°� �ǰ� Castable ���°� �Ǹ� SkillCasting ���·� �ٲ�
    CrowdControl,
};

public enum PlayerAnimState
{
    idle = 0,
    SkillCasting = 1,
    Moving,
    Impacted
}