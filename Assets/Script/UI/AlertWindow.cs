using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AlertWindow : MonoBehaviour
{

    public float DefaultOpacity;

    public float opacity;

    float messageLastTime;
    float fadeSpeed;

    CanvasGroup canvasGroup;

    TextMeshProUGUI textMesh;




    private void Awake()
    {
        DefaultOpacity = Mathf.Clamp01(DefaultOpacity);

        canvasGroup = GetComponent<CanvasGroup>();
        textMesh = transform.Find("AlertMessage").GetComponent<TextMeshProUGUI>();

    }


    private void Update()
    {

        if (canvasGroup.alpha <= 0) return;


        if (messageLastTime >= 0)
        {
            messageLastTime -= Time.deltaTime;
            return;
        }


        canvasGroup.alpha -= fadeSpeed;
    }

    public void Alert(string message,float messageLastTime, float fadeSpeed)
        // lastTime�� �޼����� ���ӵǴ� �ð�
        // fadeSpeed�� �޼����� ������� �ӵ�. ��:(0~1)
    {

        opacity = DefaultOpacity;
        this.fadeSpeed = fadeSpeed;

        this.messageLastTime = messageLastTime;

        textMesh.text = message;
        canvasGroup.alpha = DefaultOpacity;



    }
}
