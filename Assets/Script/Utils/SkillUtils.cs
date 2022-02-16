using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkillUtils
{
    public static Quaternion RotateTo(Quaternion current,Vector2 from, Vector2 to, float rotateSpeed)
    {
        Quaternion q = getAngleTo(from, to);
        return Quaternion.Slerp(current, q, rotateSpeed);
    }

    public static Quaternion getAngleTo(Vector2 from, Vector2 to)
    {
        Vector2 vectorToTarget = to - from;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        return Quaternion.AngleAxis(angle, Vector3.forward);

    }

}
