using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerInputManager : MonoBehaviour
{
    #region serialize fields

    [SerializeField] KeyCode Key_Stop;
    [SerializeField] KeyCode[] Key_for_Skills;
    [SerializeField] LayerMask CharacterMask;

    #endregion


    CursorState cursorState;
    [SerializeField] PlayerController player;

    PlayerSkillContainer skillContainer;

    Skill[] playerSkills;

    Skill readySkill;

    Vector2 cursorPosOnWorld;
    GameObject objUnderCursor;



    // Start is called before the first frame update

    private void Awake()
    {
        getDependentComponents();

    }
    void Start()
    {



        playerSkills = skillContainer.Skills;
    }

    #region init functions

    void getDependentComponents()
    {
        skillContainer = player.gameObject.GetComponent<PlayerSkillContainer>();
    }

    void initInputManager()
    {
        cursorState = CursorState.Normal;

    }

    #endregion

    // Update is called once per frame
    void Update()
    {

        UpdateMouseInput();
        UpdateKeyInput();
        

    }


    void UpdateMouseInput()
    {
        UpdateObjectUnderCursor();


            if (Input.GetMouseButtonDown(0)) //마우스 좌클릭 시에
            {
            if (cursorState == CursorState.Targeting) // Targeting 커서면
            {
                if(readySkill.CastType == SkillCastType.Targeting && objUnderCursor != null)
                {
                    if (readySkill.GetState() != SkillState.CoolDown)
                    {

                        player.OrderToCastToTarget(readySkill, objUnderCursor);

                    }

                }
                else if(readySkill.CastType == SkillCastType.NonTargeting)
                {
                    player.OrderToCastToTarPos(readySkill, cursorPosOnWorld);

                }
            }
            clearCastState();
            //나중에 클릭하면 indicator도 setActive(false)해줘야됨.
        }
        else if (Input.GetMouseButtonDown(1)) // 우클릭
        {
            player.OrderToMove(cursorPosOnWorld);


            clearCastState();
            //나중에 클릭하면 indicator도 setActive(false)해줘야됨.
        }


    }

    void UpdateKeyInput()
    {

        if (Input.GetKeyDown(Key_Stop))
        {
            player.OrderToStop();
        }
        else
        {
            for (int i = 0; i < Key_for_Skills.Length; i++)
            {
                if (Input.GetKeyDown(Key_for_Skills[i]))
                {
                    
                    readySkill=playerSkills[i];

                    if(readySkill == null)
                    {
                        break;
                    }

                    if(readySkill.GetState() == SkillState.CoolDown)
                    {
                        GameManager.instance.BattleAlert("Cool Down", 3, 0.01f);
                        break;
                    }

                    if (readySkill.CastType == SkillCastType.Targeting ||
                        readySkill.CastType == SkillCastType.NonTargeting)
                    {
                        cursorState = CursorState.Targeting;
                        break;
                    }
                    else if (readySkill.CastType == SkillCastType.Toggle)
                    {

                        player.OrderToCastImmediately(readySkill);
                        break;
                    }
                        
                }
            }
        }


    }

    void clearCastState()
    {
        cursorState = CursorState.Normal;
        readySkill = null;
    }



    void UpdateObjectUnderCursor()
    {
        cursorPosOnWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D collUnderCursor = Physics2D.OverlapPoint(cursorPosOnWorld, CharacterMask);
        if (collUnderCursor != null) objUnderCursor = collUnderCursor.gameObject;
        else objUnderCursor = null;
    }

}
