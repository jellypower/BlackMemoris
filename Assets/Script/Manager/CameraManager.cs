using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Start is called before the first frame update
    public float cameraSpeed;

    GameObject player;


    void Start()
    {
        player = GameObject.FindWithTag("Player");
        
    }

    // Update is called once per frame
    void Update()
    {


        Vector2 mousePosOnScreen = Input.mousePosition;


        if (Input.GetKey(KeyCode.LeftArrow) || mousePosOnScreen.x <= 0)
        {
            transform.position += (Vector3.left * cameraSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.RightArrow) || mousePosOnScreen.x >= Screen.width-1)
        {
            transform.position += (Vector3.right * cameraSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.UpArrow) || mousePosOnScreen.y >= Screen.height-1)
        {
            transform.position += (Vector3.up * cameraSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.DownArrow) || mousePosOnScreen.y <= 0)
        {
            transform.position += (Vector3.down * cameraSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            transform.position = player.transform.position + Vector3.back * 10;
        }
    }
}
