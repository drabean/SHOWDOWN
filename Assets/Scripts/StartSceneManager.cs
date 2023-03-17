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
        //screen ������ �������� �� ��üȭ�� ��� ��Ȱ��ȭ
        Screen.SetResolution(960, 540, false);


        changeBtnStatus(-1);
        Inst = this;

        //DAPPX ���� �����Ȼ���. �ʿ�� ��ĥ��
        //DPAAXmanager.LoginDAPPX();

        //���� ���� �κ� ����
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
        //Inspecter���� ������ �ȵǼ� �ڵ�� ����.
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
    #region ���� ����
    /// <summary>
    /// ��ư �ٲ�. 0:�α��ι�ư 1: ��Ī���۹�ư 2: ��Īĵ����ư -1: ��ư Ȱ��ȭ X)
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
    /// ���� ������ ���� �������� ��, ȭ�� ������ ���� ������. 
    /// </summary>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
     //   surverStatusText.text = "connected to master surver";

        changeBtnStatus(1);
        ScreenON();
    }

    /// <summary>
    /// ���� ����� �����ϴ� ����� �ƴ�. ���� �����Ǿ� �ִ� ���� ������ �濡 ��� ����
    /// </summary>
    public void QuickStart()
    {

        changeBtnStatus(2);

        PhotonNetwork.JoinRandomRoom();
    }

    /// <summary>
    /// ������ ����������, �� �����Ǿ� �ִ� ���� ���� ��, ���ο� ���� ����� �����ڸ� ��ٸ�.
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
      //  surverStatusText.text = "failed to join room";
        CreateRoom();
    }

    /// <summary>
    /// ������ �濡 �����ߴٸ�,  �濡 �ٸ� �ο��� ���� �� �� ���� �� ��, ���� ����.
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2) PhotonNetwork.LoadLevel("MainScene");

    }

    /// <summary>
    /// ���� ���� �����ϰ�, surverText �� �ٸ� �÷��̾ ��ٸ��� �ִٰ� �˷���.
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
    /// �� ���� ���н�, surverText ����ְ� ���� �� ���� �õ�.
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
    /// Quick Matching ���
    /// </summary>
    public void QuickCancel()
    {
        PhotonNetwork.LeaveRoom();
        changeBtnStatus(1);
    }

    /// <summary>
    /// �÷��̾� ������ �ٲٴ� ��ư. �� �÷��̾� ������ Animation�� ���� Animation controller�� override��.
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