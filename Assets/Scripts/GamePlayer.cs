using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

using Photon.Pun;
using Photon.Realtime;

public enum GAMETYPE
{
    ACCARROW = 0,
    TIMING = 1,
    RAPID = 2,
}


public class GamePlayer : MonoBehaviourPun
{
    public GameObject ArrowPrefab;

    public List<ArrowKey> KeyList = new List<ArrowKey>();

    public Sprite portrait;

    public AudioSource audioS;

    public AudioClip SFX_InputKey;
    public AudioClip SFX_Hit;

    CameraControl cam;
    //확인용

    public List<ARROW> ArrowList = new List<ARROW>();               //입력해야될 화살표 리스트


    public bool isGame;
    public int curIndex;

    public Transform destination;

    [SerializeField] Animator anim;

    public int HP;

    public bool isPlayer1;

    public int resultDmg;

    public GameObject pointer;
    public GameObject playerPointer;

    float chargeSpeed = 200f;
    float backSpeed = 7f;

    float curMoveSpeed;

    bool isDead;

    public int OpponentEnemyIndex;


    public GamePlayer Enemy;

    //ACC 관련

    //RAP 관련
    public GameObject GaugePrefab;
    public RapidGauge curGauge;

    int maxRapidCount = 10;

    //TIM 관련
    public GameObject TimingPrefab;
    public TimingTouch curTiming;
    bool pressedTimingKey;

    private void Awake()
    {
        cam = Camera.main.GetComponent<CameraControl>();
        curMoveSpeed = chargeSpeed;
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("setPlayer", RpcTarget.All, true);
            }
            else
            {
                photonView.RPC("setPlayer", RpcTarget.All, false);
            }

            playerPointer.SetActive(true);
            StartCoroutine(playerPointerTogle_Start());
        }
        else
        {
            playerPointer.SetActive(false);
        }

        GameManager.Inst.setEnemy();

    }

    /// <summary>
    /// 상대방 플레이어를 set해줌.
    /// </summary>
    public void setEnemy()
    {
        Enemy = isPlayer1 ? GameManager.Inst.player2 : GameManager.Inst.player1;
    }

    /// <summary>
    /// Round 시작할때, 현재 게임Type에 따라 키보드 프리팹을 얼마나 가져오고, 무슨 UI를 보여줄지 정함.
    /// </summary>
    public void RoundStart()
    {
        if (GameManager.Inst.isBetEnded) return;

        resultDmg = 0;

        if (!photonView.IsMine) return;                             //이 포인트에서, 자신인 경우에만 뭐 입력할지 떠야함

        curIndex = 0;
        isGame = true;

        pointer.SetActive(true);

        resetUI();

        if (GameManager.Inst.gameType == GAMETYPE.ACCARROW)//게임 0. ACCARROW의 경우.
        {
            getArrow_ACC();

            ShowUI_ACC();
        }

        if (GameManager.Inst.gameType == GAMETYPE.TIMING)
        {
            pressedTimingKey = false;
            getArrow_TIM();

            ShowUI_TIM();
        }
        if (GameManager.Inst.gameType == GAMETYPE.RAPID)//게임 2. RAPID의 경우
        {
            getArrow_RAP();
            ShowUI_RAP();
        }
    }

    /// <summary>
    /// 지금 화면에 있는 UI 삭제시켜줌.
    /// </summary>
    void resetUI()
    {
        deleteKeyObject();

        KeyList.Clear();

        if (curGauge != null)
        {
            Destroy(curGauge.gameObject);
        }
        if (curTiming != null)
        {
            Destroy(curTiming.gameObject);
        }
    }

    /// <summary>
    /// 직전 게임에서 사용한 Key GameObject 없애줌.
    /// </summary>
    protected void deleteKeyObject()
    {
        for (int i = 0; i < KeyList.Count; i++)
        {
            Destroy(KeyList[i].gameObject);
        }
    }

    /// <summary>
    /// ACCURATE 게임에서 사용할 UI 보여줌
    /// </summary>
    protected void ShowUI_ACC()
    {
        for (int i = 0; i < ArrowList.Count; i++)
        {
            var tempArrow = Instantiate(ArrowPrefab).GetComponent<ArrowKey>();
            tempArrow.setSprite(ArrowList[i]);
            tempArrow.transform.position = new Vector3(-0.4375f * (ArrowList.Count - 1) + 0.875f * i, 0.5f, 0);

            KeyList.Add(tempArrow);

        }
    }

    /// <summary>
    /// TIMING 게임에서 사용할 UI 보여줌
    /// </summary>
    void ShowUI_TIM()
    {
        curTiming = Instantiate(TimingPrefab).GetComponent<TimingTouch>();
        curTiming.transform.position = Vector2.up * 0.3f;


        var tempArrow = Instantiate(ArrowPrefab).GetComponent<ArrowKey>();
        tempArrow.setSprite(ArrowList[0], false);
        tempArrow.transform.parent = curTiming.transform;
        tempArrow.transform.localPosition = Vector2.right * -3.5f;


        curTiming.keyObject = tempArrow.gameObject;

        KeyList.Add(tempArrow);

        curTiming.startGame();

    }

    /// <summary>
    /// RAPID 게임에서 사용할 UI 보여줌.
    /// </summary>
    protected void ShowUI_RAP()
    {
        for (int i = 0; i < ArrowList.Count; i++)
        {
            var tempArrow = Instantiate(ArrowPrefab).GetComponent<ArrowKey>();
            tempArrow.setSprite(ArrowList[i], false);
            tempArrow.transform.position = new Vector3(-0.4375f * (ArrowList.Count - 1) + 0.875f * i, 0f, 0);

            KeyList.Add(tempArrow);

        }

        curGauge = Instantiate(GaugePrefab).GetComponent<RapidGauge>();
        curGauge.transform.position = new Vector2(-2.32f, -0.8f);

    }

    /// <summary>
    /// 플레이어 위치를 목표 위치를 향해 Lerp를 통해 부드럽게 이동시켜줌.
    /// 현재 게임 모드에 따라, 다른 update 함수를 실행함.
    /// </summary>
    protected void Update()
    {
        if (isDead)
        {
            return;
        }
        if (destination != null) transform.position = Vector3.Lerp(transform.position, destination.position, curMoveSpeed * Time.deltaTime);
        //if (destination != null) transform.position = Vector3.MoveTowards(transform.position, destination.position, curMoveSpeed * Time.deltaTime);

        if (!isGame) return;

        if (GameManager.Inst.gameType == GAMETYPE.ACCARROW)
        {
            update_ACC();
        }

        if (GameManager.Inst.gameType == GAMETYPE.TIMING)
        {
            update_TIM();
        }

        if (GameManager.Inst.gameType == GAMETYPE.RAPID)
        {
            update_RAP();
        }

    }


    #region ACCARROW 관련 함수
    /// <summary>
    /// 맞는 키를 눌렀다면 resultDmg를 증가시켜주고 틀린 키를 눌렀다면 감소시켜줌.
    /// </summary>
    void update_ACC()
    {

        pointer.transform.position = KeyList[curIndex].transform.position + Vector3.up * 0.75f;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            cam.shakeCam(0.1f, 0.03f);

            audioS.clip = SFX_InputKey;
            audioS.Play();

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if ((int)ArrowList[curIndex] == 0)
                {
                    KeyList[curIndex].pressBtn();
                    photonView.RPC("changeResult_ACCARROW", RpcTarget.All, true);
                    Enemy.ACC_setOpponentBtn(curIndex, true);

                }
                else
                {
                    KeyList[curIndex].failBtn();
                    photonView.RPC("changeResult_ACCARROW", RpcTarget.All, false);
                    Enemy.ACC_setOpponentBtn(curIndex, false);

                }
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if ((int)ArrowList[curIndex] == 1)
                {
                    KeyList[curIndex].pressBtn();
                    photonView.RPC("changeResult_ACCARROW", RpcTarget.All, true);
                    Enemy.ACC_setOpponentBtn(curIndex, true);
                }
                else
                {
                    KeyList[curIndex].failBtn();
                    photonView.RPC("changeResult_ACCARROW", RpcTarget.All, false);
                    Enemy.ACC_setOpponentBtn(curIndex, false);
                }

            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if ((int)ArrowList[curIndex] == 2)
                {
                    KeyList[curIndex].pressBtn();
                    photonView.RPC("changeResult_ACCARROW", RpcTarget.All, true);
                    Enemy.ACC_setOpponentBtn(curIndex, true);
                }
                else
                {
                    KeyList[curIndex].failBtn();
                    photonView.RPC("changeResult_ACCARROW", RpcTarget.All, false);
                    Enemy.ACC_setOpponentBtn(curIndex, false);
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if ((int)ArrowList[curIndex] == 3)
                {
                    KeyList[curIndex].pressBtn();
                    photonView.RPC("changeResult_ACCARROW", RpcTarget.All, true);
                    Enemy.ACC_setOpponentBtn(curIndex, true);
                }
                else
                {
                    KeyList[curIndex].failBtn();
                    photonView.RPC("changeResult_ACCARROW", RpcTarget.All, false);
                    Enemy.ACC_setOpponentBtn(curIndex, false);
                }
            }
            curIndex++;
        }


        if (curIndex == ArrowList.Count)
        {
            isGame = false;
            GameManager.Inst.getEndRoundRequest();

        }
    }

    /// <summary>
    /// 키 프리팹을 상대할 때, 상대의 입력 정도를 확인하기 위한 오브젝트를 추가.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="isRight"></param>
    public void ACC_setOpponentBtn(int index, bool isRight)
    {
        photonView.RPC("PUN_SetOpponentBtnIndex", RpcTarget.Others, index);//자기 자신의 클라이언트에 있는 클론이 아닌, 상대화면에 표시해야하므로 RPCTarget.Others로 접근.
        photonView.RPC("PUN_SetOpponentBtn", RpcTarget.Others, isRight);
    }

    /// <summary>
    /// ACCURATE 게임에서 사용할 키를 생성해줌.
    /// </summary>
    public void getArrow_ACC()
    {

        ArrowList.Clear();

        for (int i = 0; i < GameManager.Inst.curKeyCount; i++)
        {
            int randNum = Random.Range(0, 4);

            ArrowList.Add((ARROW)randNum);
        }


    }

    [PunRPC]
    void changeResult_ACCARROW(bool isSuccess)
    {
        if (isSuccess) resultDmg++;
        else resultDmg--;
    }

    #endregion

    #region RAPID 관련 함수
    /// <summary>
    /// 맞는 키를 눌렀다면, resultDmg를 변경해 주고 다음에 눌러야 할 키를 switch 해줌.
    /// </summary>
    void update_RAP()
    {
            pointer.transform.position = KeyList[curIndex].transform.position + Vector3.up * 0.75f;

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    if ((int)ArrowList[curIndex] == 0)
                    {
                        cam.shakeCam(0.1f, 0.03f);

                        audioS.clip = SFX_InputKey;
                        audioS.Play();

                        KeyList[curIndex].pressBtn();
                        photonView.RPC("changeResult_RAPID", RpcTarget.All);

                        Enemy.RAP_setOpponentGauge(resultDmg);

                        curIndex++;
                        curIndex %= 2;

                        curGauge.fillGauge(true, maxRapidCount, resultDmg);
                        if (Enemy.curGauge != null) Enemy.curGauge.fillGauge(false, maxRapidCount, resultDmg);

                        KeyList[curIndex].refreshBtn();

                    }
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if ((int)ArrowList[curIndex] == 1)
                    {
                        cam.shakeCam(0.1f, 0.03f);

                        audioS.clip = SFX_InputKey;
                        audioS.Play();

                        KeyList[curIndex].pressBtn();
                        photonView.RPC("changeResult_RAPID", RpcTarget.All);

                        Enemy.RAP_setOpponentGauge(resultDmg);

                        curIndex++;
                        curIndex %= 2;

                        curGauge.fillGauge(true, maxRapidCount, resultDmg);
                        if (Enemy.curGauge != null) Enemy.curGauge.fillGauge(false, maxRapidCount, resultDmg);

                        KeyList[curIndex].refreshBtn();
                    }

                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    if ((int)ArrowList[curIndex] == 2)
                    {
                        cam.shakeCam(0.1f, 0.03f);

                        audioS.clip = SFX_InputKey;
                        audioS.Play();

                        KeyList[curIndex].pressBtn();
                        photonView.RPC("changeResult_RAPID", RpcTarget.All);

                        Enemy.RAP_setOpponentGauge(resultDmg);

                        curIndex++;
                        curIndex %= 2;

                        curGauge.fillGauge(true, maxRapidCount, resultDmg);
                        if (Enemy.curGauge != null) Enemy.curGauge.fillGauge(false, maxRapidCount, resultDmg);

                        KeyList[curIndex].refreshBtn();
                    }
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if ((int)ArrowList[curIndex] == 3)
                    {
                        cam.shakeCam(0.1f, 0.03f);

                        audioS.clip = SFX_InputKey;
                        audioS.Play();

                        KeyList[curIndex].pressBtn();
                        photonView.RPC("changeResult_RAPID", RpcTarget.All);

                        Enemy.RAP_setOpponentGauge(resultDmg);

                        curIndex++;
                        curIndex %= 2;

                        curGauge.fillGauge(true, maxRapidCount, resultDmg);
                        if (Enemy.curGauge != null) Enemy.curGauge.fillGauge(false, maxRapidCount, resultDmg);

                        KeyList[curIndex].refreshBtn();
                    }
                }
            }

            if (resultDmg >= maxRapidCount)
            {
                isGame = false;
                GameManager.Inst.getEndRoundRequest();
            }

        }

    /// <summary>
    /// RAPID 게임에서 사용할 키를 생성해줌.
    /// </summary>
    public void getArrow_RAP()
    {
        ArrowList.Clear();

        int randNum = Random.Range(0, 4);

        ArrowList.Add((ARROW)randNum);

        randNum += Random.Range(1, 3);
        randNum %= 4;

        ArrowList.Add((ARROW)randNum);
    }

    /// <summary>
    /// RAPID 게임 입력 시, 입력 결과 동기화.
    /// </summary>
    [PunRPC]
    void changeResult_RAPID()
    {
        resultDmg++;
    }
    #endregion

    #region TIMING 관련 함수
    /// <summary>
    /// 키 프리팹이 움직이는거를 올바른 키를 눌렀을때 중지시키고, 기준 위치와 현재 키 프리팹 위치의 역수 * 10으로
    /// result Damage를 설정함으로서, 더 가까이에서 눌렀을 수록 더 result Damage가 높아지도록 설정.
    /// </summary>
    void update_TIM()
    {
        pointer.transform.position = KeyList[0].transform.position + Vector3.up * 0.75f;
        if (!pressedTimingKey)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {

                if (curTiming.distBetweenArrowFrame() < 1.5f)
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        if ((int)ArrowList[0] == 0)
                        {
                            cam.shakeCam(0.1f, 0.03f);

                            audioS.clip = SFX_InputKey;
                            audioS.Play();

                            KeyList[0].pressBtn();
                            curTiming.pressKey();
                            resultDmg = (int)(1 / curTiming.distBetweenArrowFrame()) * 10;
                            photonView.RPC("changeResult_TIMING", RpcTarget.All, resultDmg);

                            pressedTimingKey = true;

                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        if ((int)ArrowList[0] == 1)
                        {
                            cam.shakeCam(0.1f, 0.03f);

                            audioS.clip = SFX_InputKey;
                            audioS.Play();

                            KeyList[0].pressBtn();
                            curTiming.pressKey();
                            resultDmg = (int)(1 / curTiming.distBetweenArrowFrame()) * 10;
                            photonView.RPC("changeResult_TIMING", RpcTarget.All, resultDmg);

                            pressedTimingKey = true;
                        }

                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        if ((int)ArrowList[0] == 2)
                        {
                            cam.shakeCam(0.1f, 0.03f);

                            audioS.clip = SFX_InputKey;
                            audioS.Play();

                            KeyList[0].pressBtn();
                            curTiming.pressKey();
                            resultDmg = (int)(1 / curTiming.distBetweenArrowFrame()) * 10;
                            photonView.RPC("changeResult_TIMING", RpcTarget.All, resultDmg);

                            pressedTimingKey = true;
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        if ((int)ArrowList[0] == 3)
                        {
                            cam.shakeCam(0.1f, 0.03f);

                            audioS.clip = SFX_InputKey;
                            audioS.Play();

                            KeyList[0].pressBtn();
                            curTiming.pressKey();
                            resultDmg = (int)(1 / curTiming.distBetweenArrowFrame()) * 10;
                            photonView.RPC("changeResult_TIMING", RpcTarget.All, resultDmg);

                            pressedTimingKey = true;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// TIMING 게임에서 사용할 화살표 생성.
    /// </summary>
    public void getArrow_TIM()
    {
        ArrowList.Clear();
        int randNum = Random.Range(0, 4);

        ArrowList.Add((ARROW)randNum);
    }

    /// <summary>
    /// TIMING 게임 입력시, 입력 결과 동기화.
    /// </summary>
    /// <param name="value"></param>
    [PunRPC]
    void changeResult_TIMING(int value)
    {
        resultDmg = value;
    }
    #endregion




    /// <summary>
    /// 키보드 게임 입력시 상대 UI 동기화
    /// </summary>
    /// <param name="index"></param>
    [PunRPC]
    void PUN_SetOpponentBtnIndex(int index)
    {
        OpponentEnemyIndex = index;
    }

    /// <summary>
    /// 키보드 게임 입력 동기화
    /// </summary>
    /// <param name="isRight"></param>
    [PunRPC]
    void PUN_SetOpponentBtn(bool isRight)
    {
        KeyList[OpponentEnemyIndex].changeOpponent(isRight);
    }

    /// <summary>
    /// 연타게임 입력시 상대 게이지 채워주기
    /// </summary>
    /// <param name="curAmount"></param>
    public void RAP_setOpponentGauge(int curAmount)
    {
        photonView.RPC("PUN_SetOpponentGauge", RpcTarget.Others, curAmount);
    }

    /// <summary>
    /// 연타게임 게이지 동기화
    /// </summary>
    /// <param name="curAmount"></param>
    [PunRPC]
    void PUN_SetOpponentGauge(int curAmount)
    {
        curGauge.fillGauge(false, maxRapidCount, curAmount);
    }

    /// <summary>
    /// Round 끝내는 판정 내리기.
    /// </summary>
    public void endRound()
    {
        isGame = false;

        pointer.SetActive(false);
        resetUI();
    }
    /// <summary>
    /// 플레이어 데미지 입는 판정
    /// </summary>
    public void damage()
    {
        HP--;
        GameManager.Inst.breakHeart(isPlayer1, HP);
        if (HP <= 0) die();

        audioS.clip = SFX_Hit;
        audioS.Play();
    }

    /// <summary>
    /// 죽는 애니메이션 실행 및 게임 종료, 승리판정 내리기
    /// </summary>
    void die()
    {
        anim.SetTrigger("doDie");
        isDead = true;
        GameManager.Inst.endGame(!isPlayer1);       //자신이 죽었으므로, 자신이 아닌 플레이어 승리.
    }


    /// <summary>
    /// 각 플레이어 초기화.
    /// </summary>
    /// <param name="isPlayer1"></param>
    [PunRPC]
    public void setPlayer(bool isPlayer1)
    {
        if (isPlayer1)
        {
            transform.localScale = Vector3.one;
            playerPointer.transform.localScale = Vector3.one;
            transform.position = GameManager.Inst.player1Pos[0].position;
            GameManager.Inst.player1 = this;


        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
            playerPointer.transform.localScale = new Vector3(-1, 1, 1);
            transform.position = GameManager.Inst.player2Pos[0].position;
            GameManager.Inst.player2 = this;

            this.isPlayer1 = false;
        }
    }


    /// <summary>
    /// 공격 애
    /// </summary>
    public void anim_attack()
    {
        if (isDead) return;

        curMoveSpeed = chargeSpeed;
        anim.SetTrigger("doAttack");
    }

    /// <summary>
    /// 공격 후 후퇴 애니메이션 및 위치변경
    /// </summary>
    public void anim_back()
    {
        if (isDead) return;

        curMoveSpeed = backSpeed;
        anim.SetTrigger("doBack");
    }

    /// <summary>
    /// 피격 모션 및 위치변경
    /// </summary>
    public void anim_hit()
    {
        if (isDead) return;

        curMoveSpeed = chargeSpeed;
        anim.SetTrigger("doHit");
    }

    //게임 시작 할 때 플레이어 커서 토글
    IEnumerator playerPointerTogle_Start()
    {
        playerPointer.SetActive(true);
        yield return new WaitForSeconds(5.0f);

        playerPointer.SetActive(false);
        yield return new WaitForSeconds(0.15f);
        playerPointer.SetActive(true);
        yield return new WaitForSeconds(0.15f);

        playerPointer.SetActive(false);
        yield return new WaitForSeconds(0.15f);
        playerPointer.SetActive(true);
        yield return new WaitForSeconds(0.15f);

        playerPointer.SetActive(false);
    }
    //한 라운드 이후에 플레이어 커서 토글
    IEnumerator playerPointerTogle()
    {
        playerPointer.SetActive(true);
        yield return new WaitForSeconds(1.0f);

        playerPointer.SetActive(false);
        yield return new WaitForSeconds(0.15f);
        playerPointer.SetActive(true);
        yield return new WaitForSeconds(0.15f);

        playerPointer.SetActive(false);
        yield return new WaitForSeconds(0.15f);
        playerPointer.SetActive(true);
        yield return new WaitForSeconds(0.15f);

        playerPointer.SetActive(false);
    }
    //자기 자신의 플레이어 위의 커서 토글
    public void ToglePlayerCurser()
    {
        if(photonView.IsMine)
        {
            StartCoroutine(playerPointerTogle());
        }
    }
    //캐릭터 변경
    public void setAnim(int index)
    {
        photonView.RPC("PUN_SetAnim", RpcTarget.All, index);
    }

    [PunRPC]
    void PUN_SetAnim(int index)
    {
        anim.runtimeAnimatorController = GameManager.Inst.anims[index];
    }

}
