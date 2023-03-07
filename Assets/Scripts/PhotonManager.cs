using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


using UnityEngine.SceneManagement;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    //포톤상에서 한명이 끊기는 경우에 호출할 함수들.




    public override void OnLeftRoom()
    {
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            Destroy(GameObject.Find("DAPPXManager"));                        //게임이 종료되었으므로 DontDestroy해제를 위해 게임오브젝트 삭제해줌(Start Scene 다시 로딩할때 꼬이지 않도록)
            SceneManager.LoadScene("StartScene");
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (GameManager.Inst.isBetEnded) return;

        //마스터가 나감 -> Player2가 이겼음.
        GameManager.Inst.endGameByDisconnect(false);

    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (GameManager.Inst.isBetEnded) return;
        
        //마스터가 아닌쪽이 나감 -> Player1이 이김.
        GameManager.Inst.endGameByDisconnect(true);
    }

}
