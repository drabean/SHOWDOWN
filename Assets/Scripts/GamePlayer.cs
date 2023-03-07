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
    //Ȯ�ο�

    public List<ARROW> ArrowList = new List<ARROW>();               //�Է��ؾߵ� ȭ��ǥ ����Ʈ


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

    //ACC ����

    //RAP ����
    public GameObject GaugePrefab;
    public RapidGauge curGauge;

    int maxRapidCount = 10;

    //TIM ����
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
    /// ���� �÷��̾ set����.
    /// </summary>
    public void setEnemy()
    {
        Enemy = isPlayer1 ? GameManager.Inst.player2 : GameManager.Inst.player1;
    }

    /// <summary>
    /// Round �����Ҷ�, ���� ����Type�� ���� Ű���� �������� �󸶳� ��������, ���� UI�� �������� ����.
    /// </summary>
    public void RoundStart()
    {
        if (GameManager.Inst.isBetEnded) return;

        resultDmg = 0;

        if (!photonView.IsMine) return;                             //�� ����Ʈ����, �ڽ��� ��쿡�� �� �Է����� ������

        curIndex = 0;
        isGame = true;

        pointer.SetActive(true);

        resetUI();

        if (GameManager.Inst.gameType == GAMETYPE.ACCARROW)//���� 0. ACCARROW�� ���.
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
        if (GameManager.Inst.gameType == GAMETYPE.RAPID)//���� 2. RAPID�� ���
        {
            getArrow_RAP();
            ShowUI_RAP();
        }
    }

    /// <summary>
    /// ���� ȭ�鿡 �ִ� UI ����������.
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
    /// ���� ���ӿ��� ����� Key GameObject ������.
    /// </summary>
    protected void deleteKeyObject()
    {
        for (int i = 0; i < KeyList.Count; i++)
        {
            Destroy(KeyList[i].gameObject);
        }
    }

    /// <summary>
    /// ACCURATE ���ӿ��� ����� UI ������
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
    /// TIMING ���ӿ��� ����� UI ������
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
    /// RAPID ���ӿ��� ����� UI ������.
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
    /// �÷��̾� ��ġ�� ��ǥ ��ġ�� ���� Lerp�� ���� �ε巴�� �̵�������.
    /// ���� ���� ��忡 ����, �ٸ� update �Լ��� ������.
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


    #region ACCARROW ���� �Լ�
    /// <summary>
    /// �´� Ű�� �����ٸ� resultDmg�� ���������ְ� Ʋ�� Ű�� �����ٸ� ���ҽ�����.
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
    /// Ű �������� ����� ��, ����� �Է� ������ Ȯ���ϱ� ���� ������Ʈ�� �߰�.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="isRight"></param>
    public void ACC_setOpponentBtn(int index, bool isRight)
    {
        photonView.RPC("PUN_SetOpponentBtnIndex", RpcTarget.Others, index);//�ڱ� �ڽ��� Ŭ���̾�Ʈ�� �ִ� Ŭ���� �ƴ�, ���ȭ�鿡 ǥ���ؾ��ϹǷ� RPCTarget.Others�� ����.
        photonView.RPC("PUN_SetOpponentBtn", RpcTarget.Others, isRight);
    }

    /// <summary>
    /// ACCURATE ���ӿ��� ����� Ű�� ��������.
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

    #region RAPID ���� �Լ�
    /// <summary>
    /// �´� Ű�� �����ٸ�, resultDmg�� ������ �ְ� ������ ������ �� Ű�� switch ����.
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
    /// RAPID ���ӿ��� ����� Ű�� ��������.
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
    /// RAPID ���� �Է� ��, �Է� ��� ����ȭ.
    /// </summary>
    [PunRPC]
    void changeResult_RAPID()
    {
        resultDmg++;
    }
    #endregion

    #region TIMING ���� �Լ�
    /// <summary>
    /// Ű �������� �����̴°Ÿ� �ùٸ� Ű�� �������� ������Ű��, ���� ��ġ�� ���� Ű ������ ��ġ�� ���� * 10����
    /// result Damage�� ���������μ�, �� �����̿��� ������ ���� �� result Damage�� ���������� ����.
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
    /// TIMING ���ӿ��� ����� ȭ��ǥ ����.
    /// </summary>
    public void getArrow_TIM()
    {
        ArrowList.Clear();
        int randNum = Random.Range(0, 4);

        ArrowList.Add((ARROW)randNum);
    }

    /// <summary>
    /// TIMING ���� �Է½�, �Է� ��� ����ȭ.
    /// </summary>
    /// <param name="value"></param>
    [PunRPC]
    void changeResult_TIMING(int value)
    {
        resultDmg = value;
    }
    #endregion




    /// <summary>
    /// Ű���� ���� �Է½� ��� UI ����ȭ
    /// </summary>
    /// <param name="index"></param>
    [PunRPC]
    void PUN_SetOpponentBtnIndex(int index)
    {
        OpponentEnemyIndex = index;
    }

    /// <summary>
    /// Ű���� ���� �Է� ����ȭ
    /// </summary>
    /// <param name="isRight"></param>
    [PunRPC]
    void PUN_SetOpponentBtn(bool isRight)
    {
        KeyList[OpponentEnemyIndex].changeOpponent(isRight);
    }

    /// <summary>
    /// ��Ÿ���� �Է½� ��� ������ ä���ֱ�
    /// </summary>
    /// <param name="curAmount"></param>
    public void RAP_setOpponentGauge(int curAmount)
    {
        photonView.RPC("PUN_SetOpponentGauge", RpcTarget.Others, curAmount);
    }

    /// <summary>
    /// ��Ÿ���� ������ ����ȭ
    /// </summary>
    /// <param name="curAmount"></param>
    [PunRPC]
    void PUN_SetOpponentGauge(int curAmount)
    {
        curGauge.fillGauge(false, maxRapidCount, curAmount);
    }

    /// <summary>
    /// Round ������ ���� ������.
    /// </summary>
    public void endRound()
    {
        isGame = false;

        pointer.SetActive(false);
        resetUI();
    }
    /// <summary>
    /// �÷��̾� ������ �Դ� ����
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
    /// �״� �ִϸ��̼� ���� �� ���� ����, �¸����� ������
    /// </summary>
    void die()
    {
        anim.SetTrigger("doDie");
        isDead = true;
        GameManager.Inst.endGame(!isPlayer1);       //�ڽ��� �׾����Ƿ�, �ڽ��� �ƴ� �÷��̾� �¸�.
    }


    /// <summary>
    /// �� �÷��̾� �ʱ�ȭ.
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
    /// ���� ��
    /// </summary>
    public void anim_attack()
    {
        if (isDead) return;

        curMoveSpeed = chargeSpeed;
        anim.SetTrigger("doAttack");
    }

    /// <summary>
    /// ���� �� ���� �ִϸ��̼� �� ��ġ����
    /// </summary>
    public void anim_back()
    {
        if (isDead) return;

        curMoveSpeed = backSpeed;
        anim.SetTrigger("doBack");
    }

    /// <summary>
    /// �ǰ� ��� �� ��ġ����
    /// </summary>
    public void anim_hit()
    {
        if (isDead) return;

        curMoveSpeed = chargeSpeed;
        anim.SetTrigger("doHit");
    }

    //���� ���� �� �� �÷��̾� Ŀ�� ���
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
    //�� ���� ���Ŀ� �÷��̾� Ŀ�� ���
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
    //�ڱ� �ڽ��� �÷��̾� ���� Ŀ�� ���
    public void ToglePlayerCurser()
    {
        if(photonView.IsMine)
        {
            StartCoroutine(playerPointerTogle());
        }
    }
    //ĳ���� ����
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
