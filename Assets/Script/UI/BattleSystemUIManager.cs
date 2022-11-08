using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleSystemUIManager : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] PlayerSkillContainer skillContainer;
    [SerializeField] GameObject[] slots;
    Image[] coolDownMasks;

    [SerializeField]AlertWindow AlertWindow;


    private void Awake()
    {
        coolDownMasks = new Image[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            coolDownMasks[i] = slots[i].transform.Find("CooldownMask").GetComponent<Image>();
        }
    }

    private void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {
        UpdateCooldownMask();

    }

    void UpdateCooldownMask()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if(skillContainer.Skills[i] != null)
                coolDownMasks[i].fillAmount = skillContainer.Skills[i].coolDownTimer/skillContainer.Skills[i].CooldownTime;
        }
    }

    public void Alert(string message, float messageLastTime, float fadeSpeed)
    {
        AlertWindow.Alert(message, messageLastTime, fadeSpeed);
    }
}
