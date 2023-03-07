using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingTouch : MonoBehaviour
{
    public GameObject keyObject;
    public GameObject frameObject;
    public GameObject invisibleObject;


    public bool isMoving;
    bool isInvisibleMoving;
    public bool isPressed;


    public float moveSpeed;
    public float maxMoveSpeed;

    public float startKeyPosX;
    public float startFramePosX;

    public void startGame()
    {
        isMoving = true;
        isInvisibleMoving = true;

        keyObject.transform.localPosition = Vector2.right * startKeyPosX;
        invisibleObject.transform.localPosition = Vector2.right * startKeyPosX;

        frameObject.transform.localPosition = Vector3.right * startFramePosX;

    }

    private void Update()
    {
        if(isInvisibleMoving)
        {
            invisibleObject.transform.Translate(Vector2.right * Time.deltaTime * moveSpeed);
        }

        if (invisibleObject.transform.position.x >= 4.0f)
        {
            if (GameManager.Inst.isImPlayer1)
            {
                GameManager.Inst.player1.isGame = false;
            }
            else
            {
                GameManager.Inst.player2.isGame = false;
            }


            if (PhotonNetwork.IsMasterClient)
            {
                GameManager.Inst.getEndRoundRequest();
            }
        }


        if (!isMoving) return;

        if (moveSpeed <= maxMoveSpeed)
        {
            moveSpeed += maxMoveSpeed * Time.deltaTime * 0.9f;
        }

        if (isMoving)
        {
            keyObject.transform.Translate(Vector2.right * Time.deltaTime * moveSpeed);
        }


    }

    public void pressKey()
    {
        isPressed = true;
        isMoving = false;
    }

    public float distBetweenArrowFrame()
    {
        //maker의 위치(3.0f)와 키의 사이의 거리 반환.

        return Mathf.Abs(keyObject.transform.position.x - frameObject.transform.position.x);
    }



}
