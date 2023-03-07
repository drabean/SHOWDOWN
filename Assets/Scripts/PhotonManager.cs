using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


using UnityEngine.SceneManagement;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    //����󿡼� �Ѹ��� ����� ��쿡 ȣ���� �Լ���.




    public override void OnLeftRoom()
    {
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            Destroy(GameObject.Find("DAPPXManager"));                        //������ ����Ǿ����Ƿ� DontDestroy������ ���� ���ӿ�����Ʈ ��������(Start Scene �ٽ� �ε��Ҷ� ������ �ʵ���)
            SceneManager.LoadScene("StartScene");
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (GameManager.Inst.isBetEnded) return;

        //�����Ͱ� ���� -> Player2�� �̰���.
        GameManager.Inst.endGameByDisconnect(false);

    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (GameManager.Inst.isBetEnded) return;
        
        //�����Ͱ� �ƴ����� ���� -> Player1�� �̱�.
        GameManager.Inst.endGameByDisconnect(true);
    }

}
