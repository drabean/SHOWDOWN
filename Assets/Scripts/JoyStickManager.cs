using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class JoyStickManager : MonoBehaviourPun
{
    [SerializeField] ArcadeMachine arcadeMachine;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            photonView.RPC("PUN_ArrowLeft", RpcTarget.All, GameManager.Inst.isImPlayer1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            photonView.RPC("PUN_ArrowUp", RpcTarget.All, GameManager.Inst.isImPlayer1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            photonView.RPC("PUN_ArrowRight", RpcTarget.All, GameManager.Inst.isImPlayer1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            photonView.RPC("PUN_ArrowDown", RpcTarget.All, GameManager.Inst.isImPlayer1);
        }
    }

    [PunRPC]
    void PUN_ArrowLeft(bool isPlayer1)
    {
        arcadeMachine.JoystickLeft(isPlayer1);
    }
    [PunRPC]
    void PUN_ArrowUp(bool isPlayer1)
    {
        arcadeMachine.JoystickUp(isPlayer1);
    }
    [PunRPC]
    void PUN_ArrowRight(bool isPlayer1)
    {
        arcadeMachine.JoystickRight(isPlayer1);
    }
    [PunRPC]
    void PUN_ArrowDown(bool isPlayer1)
    {
        arcadeMachine.JoystickDown(isPlayer1);
    }
}
