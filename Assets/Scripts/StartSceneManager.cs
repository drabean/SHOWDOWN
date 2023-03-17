using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;


using Photon.Pun;
using Photon.Realtime;


public class StartSceneManager : MonoBehaviourPunCallbacks
{
    public Animator playerAnim;
    public RuntimeAnimatorController[] anims;
    public int curIndex = 0;

    public static StartSceneManager Inst;

    [SerializeField] GameObject loginBtn;
    [SerializeField] GameObject quickStartBtn;
    [SerializeField] GameObject quickCancleBtn;

   // public Text surverStatusText;

    //public Text NameText;
    public TextMesh nameTextMesh;

    //public Text ZeraText;
    public TextMesh zeraTextMesh;


    public MeshRenderer[] TextMeshes;

    [SerializeField] DAPPXManager DPAAXmanager;

    [SerializeField] Animator Machine_On;

    public Text DappxBtnText;

    [SerializeField] AudioSource audioS;

    public AudioClip[] SFX_UI;
    public AudioClip SFX_ON;

    public AudioSource BGM;


    void Awake()
    {
        //screen 사이즈 강제조정 및 전체화면 모드 비활성화
        Screen.SetResolution(960, 540, false);


        changeBtnStatus(-1);
        Inst = this;

        //DAPPX 연결 해제된상태. 필요시 고칠것
        //DPAAXmanager.LoginDAPPX();

        //포톤 서버 로비 연결
        if (!PhotonNetwork.InLobby) PhotonNetwork.ConnectUsingSettings();
    }


    private void Start()
    {
        curIndex = Random.Range(0, 4);

        Btn_ChangeCharacter();

        setTextMeshLayer();
    }

    void setTextMeshLayer()
    {
        //Inspecter에서 접근이 안되서 코드로 접근.
        foreach(MeshRenderer tm in TextMeshes)
        {
            tm.sortingLayerName = "UI";
        }
    }

    public void ScreenON()
    {
        Machine_On.SetTrigger("MachineOn");
        audioS.clip = SFX_ON;
        audioS.Play();

        StartCoroutine(CO_StartBGM());


    }

    IEnumerator CO_StartBGM()
    {
        yield return new WaitForSeconds(1.0f);
        if(!BGM.isPlaying) BGM.Play();
    }
    #region 포톤 관련
    /// <summary>
    /// 버튼 바꿈. 0:로그인버튼 1: 매칭시작버튼 2: 매칭캔슬버튼 -1: 버튼 활성화 X)
    /// </summary>
    public void changeBtnStatus(int id)
    {
        if(id == 0)
        {
            loginBtn.SetActive(true);
            quickStartBtn.SetActive(false);
            quickCancleBtn.SetActive(false);


            audioS.clip = SFX_UI[1];
            audioS.Play();
        }
        else if(id == 1)
        {
            ///loginBtn.SetActive(false);
            quickStartBtn.SetActive(true);
            quickCancleBtn.SetActive(false);

            // NameText.gameObject.SetActive(true);
            // ZeraText.gameObject.SetActive(true);

            nameTextMesh.gameObject.SetActive(true);
            zeraTextMesh.gameObject.SetActive(true);

        }
        else if(id == 2)
        {
            //loginBtn.SetActive(false);
            quickStartBtn.SetActive(false);
            quickCancleBtn.SetActive(true);

            // NameText.gameObject.SetActive(true);
            // ZeraText.gameObject.SetActive(true);

            nameTextMesh.gameObject.SetActive(true);
            zeraTextMesh.gameObject.SetActive(true);

            audioS.clip = SFX_UI[0];
            audioS.Play();
        }
        else
        {
            //loginBtn.SetActive(false);
            quickStartBtn.SetActive(false);
            quickCancleBtn.SetActive(false);

        }
    }

    /// <summary>
    /// 포톤 서버에 연결 성공했을 때, 화면 켜지는 연출 보여줌. 
    /// </summary>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
     //   surverStatusText.text = "connected to master surver";

        changeBtnStatus(1);
        ScreenON();
    }

    /// <summary>
    /// 방을 만들고 참여하는 방식이 아닌. 지금 생성되어 있는 방중 랜덤한 방에 즉시 참여
    /// </summary>
    public void QuickStart()
    {

        changeBtnStatus(2);

        PhotonNetwork.JoinRandomRoom();
    }

    /// <summary>
    /// 참여에 실패했을때, 즉 생성되어 있는 방이 있을 때, 새로운 방을 만들고 도전자를 기다림.
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
      //  surverStatusText.text = "failed to join room";
        CreateRoom();
    }

    /// <summary>
    /// 누군가 방에 참여했다면,  방에 다른 인원이 참여 할 수 없게 한 뒤, 게임 시작.
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2) PhotonNetwork.LoadLevel("MainScene");

    }

    /// <summary>
    /// 방을 새로 생성하고, surverText 에 다른 플레이어를 기다리고 있다고 알려줌.
    /// </summary>
    public void CreateRoom()
    {
      //  Debug.Log("creating room now ");
       // surverStatusText.text = "waiting for other player";


        int randomRoomNumber = Random.Range(1, 10000);
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)2};

        PhotonNetwork.CreateRoom("Room" + randomRoomNumber, roomOptions);
    }

    /// <summary>
    /// 방 생성 실패시, surverText 띄어주고 새로 방 생성 시도.
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("failed to create room .. trying again");
       // surverStatusText.text = "failed to create room .. trying again";
        CreateRoom();
    }

    /// <summary>
    /// Quick Matching 취소
    /// </summary>
    public void QuickCancel()
    {
        PhotonNetwork.LeaveRoom();
        changeBtnStatus(1);
    }

    /// <summary>
    /// 플레이어 외형을 바꾸는 버튼. 각 플레이어 외형의 Animation은 같은 Animation controller를 override함.
    /// </summary>
    public void Btn_ChangeCharacter()
    {
        curIndex++;
        curIndex %= 4;
        DPAAXmanager.myInfo.characterIndex = curIndex;

        playerAnim.runtimeAnimatorController = anims[curIndex];
    }
    #endregion

}