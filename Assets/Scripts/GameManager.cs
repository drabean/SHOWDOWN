using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

using UnityEngine.UI;
using Photon.Pun;

public enum ARROW
{
    LEFT = 0, 
    UP = 1, 
    RIGHT = 2, 
    DOWN = 3
}

public class GameManager : MonoBehaviourPun
{
    public static GameManager Inst;

    public List<ARROW> ArrowList = new List<ARROW>();               //입력해야될 화살표 리스트

    public GamePlayer player1;
    public GamePlayer player2;

    [HideInInspector]public int curKeyCount = 3;

    public GameObject playerPrefab;

    public bool isGameRound;
    public bool isBetEnded;

    public bool isPlayer1In;
    public bool isPlayer2In;


    #region 체력 관련
    public GameObject HeartPrefab;
    [SerializeField] Animator[] player1Hearts = new Animator[3];
    [SerializeField] Animator[] player2Hearts = new Animator[3];
    #endregion

    #region 연출 관련
    CameraControl cam;
    public GameObject SlashFlash;
    public List<Transform> player1Pos = new List<Transform>();
    public List<Transform> player2Pos = new List<Transform>();
    
    public Animator introAnim;
    public Portrait portrait;
    public Image fadeoutPanel;

    public GameObject GameOverFadeout;
    public GameObject GameoverPanel;
    public Text GameOverText;

    public GameOverPanel gameOverPanelScript;
    #endregion

    #region 음향 관련
    public AudioSource audioS;


    public AudioClip SFX_Intro1;
    public AudioClip SFX_Intro2;
    public AudioClip SFX_Impact;
    public AudioClip SFX_Windblow;
    public AudioClip SFX_Attack;
    public AudioClip SFX_Heart;

    #endregion


    #region DAPPX 관련
    public BettingManager bettingManager;

    #endregion

    public bool isGameOver;


    public bool isImPlayer1;

    public bool isPlayer1Win;


    public DAPPXManager DAPPXmanager;

    public RuntimeAnimatorController[] anims;

    //여기부터 업그레이드
    public GAMETYPE gameType;

    bool isThisRoundEnded;

    public int curRound;
    public TextMesh roundText;
    public TextMesh roundCountText;

    public Animator MachineAnim;

    public AudioClip SFX_GameEndSound;

    private void Awake()
    {
        Inst = this;
        cam = Camera.main.GetComponent<CameraControl>();
    }

    private void Start()
    {
        DAPPXmanager = GameObject.Find("DAPPXManager").GetComponent<DAPPXManager>();

        setUserInfo();

        PhotonNetwork.Instantiate("Player", player2Pos[0].position, Quaternion.identity).GetComponent<GamePlayer>().setAnim(DAPPXmanager.myInfo.characterIndex);//시작할때 플레이어를 만들고, 각 플레이어 위치로 초기화함. 초기화는 Player 안에서 실행.

        //photonView.RPC("setPlayerName", RpcTarget.All);

        isImPlayer1 = PhotonNetwork.IsMasterClient;

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("PUN_StartIntro", RpcTarget.All);
            photonView.RPC("PUN_showRound", RpcTarget.All);
        }

    }

    public void setEnemy()
    {
        player1?.setEnemy();
        player2?.setEnemy();
    }


    /// <summary>
    /// Photon을 통해 PlayerInfo를 저장하고, 동기화함.
    /// </summary>
    void setUserInfo()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            DAPPXmanager.setPlayer2UserInfo(DAPPXmanager.myInfo);
        }
        else
        {
            DAPPXmanager.setPlayer1UserInfo(DAPPXmanager.myInfo);
        }
    }

    /// <summary>
    /// 양 쪽 플레이어에게 StartIntro(화면 암전) 실행.
    /// </summary>
    [PunRPC]
    void PUN_StartIntro()
    {
        StartCoroutine(CO_Intro());
    }

    IEnumerator CO_Intro()
    {
        fadeoutPanel.color = new Color(0, 0, 0, 0.6f);

        yield return new WaitForSeconds(0.8f);

        audioS.clip = SFX_Intro1;
        audioS.Play();

        introAnim.SetTrigger("Play");

        yield return new WaitForSeconds(1.5f);
        audioS.clip = SFX_Intro2;
        audioS.Play();


        yield return new WaitForSeconds(1.5f);

        Destroy(introAnim.gameObject);


        fadeoutPanel.color = Color.clear;
        StartCoroutine(CO_CreateHeart());


        if (PhotonNetwork.IsMasterClient)
        {

            float randNum = Random.Range(3f, 6f);

            photonView.RPC("PUN_ChangeKeyCount", RpcTarget.All, Random.Range(3, 8));
            int randGameNum = Random.Range(0, 3);

            photonView.RPC("PUN_ChangeGame", RpcTarget.All, randGameNum);//여기서 체크
            isThisRoundEnded = false;

            StartCoroutine(CO_WaitForRandomSeconds(randNum));
        }

    }

    [PunRPC]
    void PUN_ChangeGame(int index)
    {
        gameType = (GAMETYPE)index;
    }

    [PunRPC] 
    void PUN_ChangeKeyCount(int count)
    {
        curKeyCount = count;
    }
    IEnumerator CO_WaitForRandomSeconds(float randTime)
    {

        if (isGameOver) yield break;
        audioS.clip = SFX_Windblow;
        audioS.Play();

        photonView.RPC("PUN_StartFadeout", RpcTarget.All, randTime);
        //StartCoroutine(CO_fadeout(randNum));

        yield return new WaitForSeconds(randTime);

        photonView.RPC("PUN_StartRound", RpcTarget.All);
    }

    [PunRPC]
    void PUN_StartFadeout(float randNum)
    {
        StartCoroutine(CO_fadeout(randNum));
    }


    IEnumerator CO_fadeout(float fadeoutTimeLeft)
    {
        float fadeoutValue = 0;
        float fadeoutAmount = 0.2f;

        while (fadeoutTimeLeft > 0)
        {
            fadeoutTimeLeft -= Time.deltaTime;


            fadeoutValue += fadeoutAmount * Time.deltaTime;
            fadeoutAmount -= Time.deltaTime * 0.02f;

            if (fadeoutValue < 0.9f) fadeoutPanel.color = new Color(0, 0, 0, fadeoutValue);

            yield return null;
        }
        fadeoutPanel.color = Color.clear;
    }



    [PunRPC] 
    void PUN_StartRound()
    {
        startRound();
    }


    void startRound()
    {
        fadeoutPanel.color = Color.clear;


        audioS.clip = SFX_Impact;
        audioS.Play();

        isGameRound = true;

        player1.RoundStart();
        player2.RoundStart();
    }

    public void getEndRoundRequest()
    {
        photonView.RPC("endRound", RpcTarget.All);
    }

    [PunRPC]
    void endRound()
    {
        player1.endRound();
        player2.endRound();

        isGameRound = false;

        if (isThisRoundEnded)
        {
            return;
        }
        isThisRoundEnded = true;
        startDamage();
    }

    void startDamage()
    {
        StartCoroutine(CO_DamagePhase());
    }

    IEnumerator CO_DamagePhase()
    {
        audioS.clip = SFX_Attack;
        audioS.Play();

        bool isPlayer1Win = checkWhoWin();

        if (isPlayer1Win)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("PUN_Attack", RpcTarget.All, true);

            }
        }
        else
        {

            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("PUN_Attack", RpcTarget.All, false);

            }
        }

        photonView.RPC("showSlashEffect", RpcTarget.All, isPlayer1Win);

        yield return new WaitForSeconds(0.7f);

        Idle();

        yield return new WaitForSeconds(0.5f);
    }

    [PunRPC]
    void showSlashEffect(bool isPlayer1)
    {
        StartCoroutine(CO_SlashEffect(isPlayer1));
    }

    IEnumerator CO_SlashEffect(bool isPlayer1)
    {
        if(isPlayer1)
        {
            SlashFlash.transform.localScale = new Vector3(1.5f, 2f, 1);
        }
        else
        {
            SlashFlash.transform.localScale = new Vector3(-1.5f, 2f, 1);
        }

        cam.shakeCam(0.2f, 0.07f);
        yield return new WaitForSeconds(0.06f);

        SlashFlash.SetActive(true);
        if (isPlayer1)
        {
            MachineAnim.SetTrigger("shakeRight");
        }
        else
        {
            MachineAnim.SetTrigger("shakeLeft");
        }

        yield return new WaitForSeconds(0.03f);
        SlashFlash.SetActive(false);

        player1.resultDmg = 0;
        player2.resultDmg = 0;
    }

    

    IEnumerator CO_CreateHeart()
    {
        yield return new WaitForSeconds(0.2f);
        for(int i = 0; i < 3; i++)
        {
            player1Hearts[i] =  Instantiate(HeartPrefab, new Vector3((-1) * (1.35f + 1.0f * i), 1.93f, 0), Quaternion.identity).GetComponent<Animator>();


            player2Hearts[i] =  Instantiate(HeartPrefab, new Vector3((1.35f + 1.0f * i), 1.93f, 0), Quaternion.identity).GetComponent<Animator>();

            audioS.clip = SFX_Heart;
            audioS.Play();
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator CO_SlowTime()
    {
        Time.timeScale = 0.6f;

        yield return new WaitForSeconds(0.5f);

        Time.timeScale = 1.0f;
    }


    IEnumerator CO_GameEnd()
    {
        cam.shakeCam(0.2f, 0.07f);
        yield return new WaitForSeconds(1.0f);


        audioS.clip = SFX_GameEndSound;
        audioS.Play();

        GameOverFadeout.SetActive(true);
        GameoverPanel.SetActive(true);


        gameOverPanelScript.setGameOverPanel(isImPlayer1, isPlayer1Win);

 


    }

    [PunRPC]
    void PUN_Attack(bool isPlayer1)
    {
        cam.shakeCam(0.2f, 0.07f);
        StartCoroutine(CO_SlowTime());

        if (isPlayer1)
        {
            player1.destination = player1Pos[2];
            player1.anim_attack();

            player2.destination = player2Pos[1];
            player2.anim_hit();
            player2.damage();

        }
        else
        {
            player2.destination = player2Pos[2];
            player2.anim_attack();

            player1.destination = player1Pos[1];
            player1.anim_hit();
            player1.damage();
        }
    }

    void Idle()
    {
       
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("PUN_Idle", RpcTarget.All);

            float randNum = Random.Range(3f, 7f);//다음라운드시작까지기다릴시간

            photonView.RPC("PUN_ChangeKeyCount", RpcTarget.All, Random.Range(3, 8));

            int randGameNum = Random.Range(0, 3);

            photonView.RPC("PUN_ChangeGame", RpcTarget.All, randGameNum);//여기서 체크

            isThisRoundEnded = false;

            photonView.RPC("PUN_showRound", RpcTarget.All);
            StartCoroutine(CO_WaitForRandomSeconds(randNum));
        }
    }

    [PunRPC]
    void PUN_Idle()
    {
        player1.destination = player1Pos[0];
        player1.anim_back();
        player2.destination = player2Pos[0];
        player2.anim_back();
    }

    /// <summary>
    /// True시 Player1의 승리, False시 Player2의 승리.
    /// </summary>
    /// <returns></returns>
    bool checkWhoWin()
    {
        int player1Dmg = player1.resultDmg;
        int player2Dmg = player2.resultDmg;

        //Debug.Log("Player1 DMG: " + player1Dmg + " Player2 DMG: " + player2Dmg);


        if (player1Dmg > player2Dmg) return true;
        else return false;

    }

    /// <summary>
    /// true일씨 Player1 승리, False일시 Player2 승리.
    /// </summary>
    /// <param name="isPlayer1"></param>
    public void endGame(bool isPlayer1)
    {
        isGameOver = true;


        GameOverText.text = (isPlayer1 ? DAPPXmanager.player1Info.PlayerNickname : DAPPXmanager.player2Info.PlayerNickname) + " \nWin!";

        photonView.RPC("setWhoWin", RpcTarget.All, isPlayer1);

        //DAPPXManager 안쓸려고 막아둔거. 필요시 해제
        //DAPPXmanager.Zera_DeclareWinner(isPlayer1);

        StartCoroutine(CO_GameEnd());


    }

    [PunRPC]
    void setWhoWin(bool isPlayer1Win)
    {
        this.isPlayer1Win = isPlayer1Win;
    }

    public void endGameByDisconnect(bool isPlayer1)
    { 

        GameOverText.text = "Your Enemy \nRan Away...";

        photonView.RPC("setWhoWin", RpcTarget.All, isPlayer1);

        //DAPPXManager 안쓸려고 막아둔거. 필요시 해제
        //DAPPXmanager.Zera_DeclareWinner(isPlayer1);

        StartCoroutine(CO_GameEnd());
    }

    /// <summary>
    /// 어떤 플레이어가 맞았는지 판단해서, 체력바 깨지는 모션 넣어줌.
    /// </summary>
    /// <param name="isPlayer1"></param>
    /// <param name="index"></param>
    public void breakHeart(bool isPlayer1, int index)
    {
        if(isPlayer1)
        {
            player1Hearts[index].SetTrigger("break");
            Destroy(player1Hearts[index].gameObject, 2.0f);
        }
        else
        {
            player2Hearts[index].SetTrigger("break");
            Destroy(player2Hearts[index].gameObject, 2.0f);
        }
    }



    public void returnToLobby()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    [PunRPC]
    public void PUN_showRound()
    {
        roundText.GetComponent<MeshRenderer>().sortingLayerName = "UI";
        roundText.GetComponent<MeshRenderer>().sortingOrder = 3;
        roundCountText.GetComponent<MeshRenderer>().sortingLayerName = "UI";
        roundCountText.GetComponent<MeshRenderer>().sortingOrder = 3;

        curRound++;
        roundCountText.text = curRound.ToString();

        photonView.RPC("PUN_ToglePlayerCursor", RpcTarget.All);

    }

    /// <summary>
    /// 
    /// </summary>
    [PunRPC]
    void PUN_ToglePlayerCursor()
    {
        if (player1 != null && player2 != null)
        {
            player1.ToglePlayerCurser();
            player2.ToglePlayerCurser();
        }
    }



}
