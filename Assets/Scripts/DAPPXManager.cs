using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


using Photon.Pun;

[System.Serializable]
public class UserInfo
{

    public int characterIndex;
    public string PlayerID;
    public string SessionID;
    public string BettingID;

    public string PlayerNickname;
}

public class DAPPXManager : MonoBehaviourPun
{
    string API_KEY = "3pW2nA9lAhKmJCxhrUaH5f";

    string BettingID;

    public Text serverStatusText;

    public float reward;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        //test
        StartSceneManager.Inst.nameTextMesh.text = myInfo.PlayerNickname;
    }

    /// <summary>
    /// Master측에서 호출. 배팅 만듬.
    /// </summary>
    /// 

    [PunRPC]
    public void MakeBat()
    {
        //Debug.Log("TRY BAT");
        if (!PhotonNetwork.IsMasterClient) return;

        if (GameManager.Inst.isPlayer1In && GameManager.Inst.isPlayer2In)
        {
            //Debug.Log("MASTER BATTING");
            //Betting_Zera();
        }
    }

    #region userInfo 동기화
    public UserInfo myInfo;

    public UserInfo player1Info;
    public UserInfo player2Info;

    public void setPlayer1UserInfo(UserInfo info)
    {
        photonView.RPC("PUN_Player1PlayerID", RpcTarget.All, info.PlayerID);
        photonView.RPC("PUN_Player1SessionID", RpcTarget.All, info.SessionID);
        photonView.RPC("PUN_Player1BettingID", RpcTarget.All, info.BettingID);
        photonView.RPC("PUN_Player1Nickname", RpcTarget.All, info.PlayerNickname);

        photonView.RPC("PUN_Player1SetAnim", RpcTarget.All, info.characterIndex);

        photonView.RPC("MakeBat", RpcTarget.All);

    }

    [PunRPC]
    void PUN_Player1PlayerID(string playerID)
    {
        player1Info.PlayerID = playerID;
    }
    [PunRPC]
    void PUN_Player1SessionID(string sessionID)
    {
        player1Info.SessionID = sessionID;
    }
    [PunRPC]
    void PUN_Player1BettingID(string bettingID)
    {
        player1Info.BettingID = bettingID;
    }
    [PunRPC]
    void PUN_Player1Nickname(string playerNickname)
    {
        player1Info.PlayerNickname = playerNickname;


        GameManager.Inst.portrait.setTextPlayer1(playerNickname);
        GameManager.Inst.isPlayer1In = true;
    }

    [PunRPC]
    void PUN_Player1SetAnim(int index)
    {
        player1Info.characterIndex = index;

        Debug.Log("PORTRAITSETPLAYER1" +  + index);
        GameManager.Inst.portrait.setPortrait(true, index);
    }
    
    public void setPlayer2UserInfo(UserInfo info)
    {
        photonView.RPC("PUN_Player2PlayerID", RpcTarget.All, info.PlayerID);
        photonView.RPC("PUN_Player2SessionID", RpcTarget.All, info.SessionID);
        photonView.RPC("PUN_Player2BettingID", RpcTarget.All, info.BettingID);
        photonView.RPC("PUN_Player2Nickname", RpcTarget.All, info.PlayerNickname);

        photonView.RPC("PUN_Player2SetAnim", RpcTarget.All, info.characterIndex);

        photonView.RPC("MakeBat", RpcTarget.All);
    }

    [PunRPC]
    void PUN_Player2PlayerID(string playerID)
    {
        player2Info.PlayerID = playerID;
    }
    [PunRPC]
    void PUN_Player2SessionID(string sessionID)
    {
        player2Info.SessionID = sessionID;
    }
    [PunRPC]
    void PUN_Player2BettingID(string bettingID)
    {
        player2Info.BettingID = bettingID;
    }
    [PunRPC]
    void PUN_Player2Nickname(string playerNickname)
    {
        player2Info.PlayerNickname = playerNickname;

        GameManager.Inst.portrait.setTextPlayer2(playerNickname);
        GameManager.Inst.isPlayer2In = true;
    }

    [PunRPC]
    void PUN_Player2SetAnim(int index)
    {
        player2Info.characterIndex = index;

        Debug.Log("PORTRAITSETPLAYER2"+ index);
        GameManager.Inst.portrait.setPortrait(false, index);
    }


    #endregion

    #region DAPPX API 초기화 관련

    Res_GetUserProfile resGetUserProfile = null;
    Res_GetSessionID resGetSessionID = null;
    Res_Settings resSettings = null;

    [SerializeField] string FullAppsStagingURL = "https://odin-api-sat.browseosiris.com";
    [SerializeField] string FullAppsProductionURL = "https://odin-api.browseosiris.com";



    string getBaseURL()
    {
        // 프로덕션 단계라면
        //return FullAppsProductionURL;

        // 스테이징 단계(개발)라면
        return FullAppsStagingURL;
    }


    //중요! 여기서 DAPP ARCADE 관련 코드 현재 빼두었으므로 필요시 살릴것.
    public void LoginDAPPX()
    {
        StartCoroutine(processRequestGetUserInfo());
    }


    IEnumerator processRequestGetUserInfo()
    {
        // 유저 정보
        yield return requestGetUserInfo((response) =>
        {
            if (response != null)
            {
                //Debug.Log("## " + response.ToString());
                serverStatusText.text = "recieving User Information...";
                resGetUserProfile = response;
            }
        });

        if(resGetUserProfile == null)
        {
            serverStatusText.text = "Failed to login DAPPX\n Please login and Click again.";
            StartSceneManager.Inst.changeBtnStatus(0);

            yield break;
        }
        //Debug.Log("TEST: " + resGetUserProfile.userProfile._id);
        myInfo.PlayerID = resGetUserProfile.userProfile._id;
        myInfo.PlayerNickname = resGetUserProfile.userProfile.username;

        StartSceneManager.Inst.nameTextMesh.text = resGetUserProfile.userProfile.username;

        GetSession();
    }
    /// <summary>
    /// To get user’s information.This is also used to authenticate if session-id is valid or not.
    /// This can determine if the Odin is currently running or not. 
    ///	If Odin is not running, the API  is not accesible as well.
    ///	Inform the User to run the Osiris and Connect to Odin via Meta wallet.
    ///	
    /// 유저의 정보를 얻어 온다. 이것은 또한 Session ID 가 유효하지에 따라 인증에 사용된다.
    /// 이것은 Odin이 현재 실행 중인지에 따라서 결정된다.(Odin이 실행 중이어야 옳바른 데이터를 얻을 수 있다는 의미)
    /// Odin이 실행 중이지 않으면, API는 접근할 수 없다.
    /// Osiris 를 실행하기 위해서 유저를 알려주고, Meta wallet 을 통해서 odin 에 연결한다.
    /// </summary>
    //
    delegate void resCallback_GetUserInfo(Res_GetUserProfile response);
    IEnumerator requestGetUserInfo(resCallback_GetUserInfo callback)
    {
        // get user profile
        UnityWebRequest www = UnityWebRequest.Get("http://localhost:8546/api/getuserprofile");
        yield return www.SendWebRequest();

        Debug.Log(www.downloadHandler.text);
        Res_GetUserProfile res_getUserProfile = JsonUtility.FromJson<Res_GetUserProfile>(www.downloadHandler.text);
        callback(res_getUserProfile);

    }

    public void GetSession()
    {
        StartCoroutine(processRequestGetSessionID());
    }

    IEnumerator processRequestGetSessionID()
    {
        // 유저 정보
        yield return requestGetSessionID((response) =>
        {
            if (response != null)
            {
                //Debug.Log("## " + response.ToString());
                serverStatusText.text = "recieving Session ID...";
                resGetSessionID = response;
            }
        });


        myInfo.SessionID = resGetSessionID.sessionId;
        GetBetting();

    }

    delegate void resCallback_GetSessionID(Res_GetSessionID response);
    IEnumerator requestGetSessionID(resCallback_GetSessionID callback)
    {
        // get session id
        UnityWebRequest www = UnityWebRequest.Get("http://localhost:8546/api/getsessionid");
        yield return www.SendWebRequest();
        Res_GetSessionID res_getSessionID = JsonUtility.FromJson<Res_GetSessionID>(www.downloadHandler.text);
        callback(res_getSessionID);
    }


    public void GetBetting()
    {
        StartCoroutine(processRequestSettings());
    }
    IEnumerator processRequestSettings()
    {
        yield return requestSettings((response) =>
        {
            if (response != null)
            {
                //Debug.Log("## Settings : " + response.ToString());
                serverStatusText.text = "recieving Batting ID...";
                resSettings = response;
            }
        });

        //Debug.Log(resSettings.ToString());
        myInfo.BettingID = resSettings.data.bets[0]._id;
        reward = resSettings.data.bets[0].amount;

        CheckZeraBalance();

        if (!PhotonNetwork.InLobby) PhotonNetwork.ConnectUsingSettings();

    }

    delegate void resCallback_Settings(Res_Settings response);
    IEnumerator requestSettings(resCallback_Settings callback)
    {
        string url = getBaseURL() + "/v1/betting/settings";


        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("api-key", API_KEY);
        yield return www.SendWebRequest();

        Res_Settings res = JsonUtility.FromJson<Res_Settings>(www.downloadHandler.text);
        callback(res);
        //UnityWebRequest www = new UnityWebRequest(URL);
    }





    #endregion

    #region 배팅

    public class ReqBettingPlaceBet
    {
        public string[] players_session_id;
        public string bet_id;
    }

    // Response Place Bet
    public class ResBettingPlaceBet
    {
        public string message;
        [System.Serializable]
        public class Data
        {
            public string betting_id;
        }
        public Data data;
    }

    //제라 배팅
    public void Betting_Zera()
    {
        StartCoroutine(processRequestBetting_Zera());

        //Debug.Log("BATTING");
    }

    IEnumerator processRequestBetting_Zera()
    {
        ResBettingPlaceBet resBettingPlaceBet = null;

        ReqBettingPlaceBet reqBettingPlaceBet = new ReqBettingPlaceBet();
        reqBettingPlaceBet.players_session_id = new string[] { player1Info.SessionID, player2Info.SessionID };

        reqBettingPlaceBet.bet_id = resSettings.data.bets[0]._id;

        yield return requestCoinPlaceBet(reqBettingPlaceBet, (response) =>
        {
            if (response != null)
            {
                Debug.Log("## CoinPlaceBet : " + response.message);
                resBettingPlaceBet = response;

                photonView.RPC("PUN_BettingID", RpcTarget.All, resBettingPlaceBet.data.betting_id);

            }
        });
    }

    [PunRPC] void PUN_BettingID(string id)
    {
        BettingID = id;
    }


    //
    // Request Method : POST 
    // Body Type : json
    delegate void resCallback_BettingPlaceBet(ResBettingPlaceBet response);
    IEnumerator requestCoinPlaceBet(ReqBettingPlaceBet req, resCallback_BettingPlaceBet callback)
    {
        string url = getBaseURL() + "/v1/betting/" + "zera" + "/place-bet";

        string reqJsonData = JsonUtility.ToJson(req);
        Debug.Log(reqJsonData);


        UnityWebRequest www = UnityWebRequest.Post(url, reqJsonData);
        byte[] buff = System.Text.Encoding.UTF8.GetBytes(reqJsonData);
        www.uploadHandler = new UploadHandlerRaw(buff);
        www.SetRequestHeader("api-key", API_KEY);
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();

        Debug.Log(www.downloadHandler.text);
        ResBettingPlaceBet res = JsonUtility.FromJson<ResBettingPlaceBet>(www.downloadHandler.text);
        callback(res);
    }


    #endregion

    #region 승자정하기

    public class ReqBettingDeclareWinner
    {
        public string betting_id;
        public string winner_player_id;
        public object match_details;
    }

    // Response Declare Winner
    public class ResBettingDeclareWinner
    {
        public string message;
        public class Data
        {
            public int amount_won;
        }
        public Data data;
    }

    public void Zera_DeclareWinner(bool isPlayer1)
    {
        if (GameManager.Inst.isBetEnded)
        {
            return;
        }
        photonView.RPC("PUN_EndBet", RpcTarget.All);


        StartCoroutine(processRequestBetting_Zera_DeclareWinner(isPlayer1));
    }

    [PunRPC]
    void PUN_EndBet()
    {
        GameManager.Inst.isBetEnded = true;
    }
    IEnumerator processRequestBetting_Zera_DeclareWinner(bool isPlayer1)
    {
        ResBettingDeclareWinner resBettingDeclareWinner = null;
        ReqBettingDeclareWinner reqBettingDeclareWinner = new ReqBettingDeclareWinner();

        //reqBettingDeclareWinner.betting_id = resSettings.data.bets[0]._id;

        reqBettingDeclareWinner.betting_id = BettingID;
        Debug.Log(BettingID);

        reqBettingDeclareWinner.winner_player_id = isPlayer1 ? player1Info.PlayerID : player2Info.PlayerID;

        yield return requestCoinDeclareWinner(reqBettingDeclareWinner, (response) =>
        {
            if (response != null)
            {
                //Debug.Log("## CoinDeclareWinner : " + response.message);

                resBettingDeclareWinner = response;
                //Debug.Log(resBettingDeclareWinner.data.ToString());
            }
        });

        

    } 
    //
    // Request Method : POST 
    // Body Type : json
    delegate void resCallback_BettingDeclareWinner(ResBettingDeclareWinner response);
    IEnumerator requestCoinDeclareWinner(ReqBettingDeclareWinner req, resCallback_BettingDeclareWinner callback)
    {
        string url = getBaseURL() + "/v1/betting/" + "zera" + "/declare-winner";

        string reqJsonData = JsonUtility.ToJson(req);
        Debug.Log(reqJsonData); 


        UnityWebRequest www = UnityWebRequest.Post(url, reqJsonData);
        byte[] buff = System.Text.Encoding.UTF8.GetBytes(reqJsonData);
        www.uploadHandler = new UploadHandlerRaw(buff);
        www.SetRequestHeader("api-key", API_KEY);
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        Debug.Log(www.downloadHandler.text);
        ResBettingDeclareWinner res = JsonUtility.FromJson<ResBettingDeclareWinner>(www.downloadHandler.text);

        callback(res);
    }


    #endregion


    #region 배팅 취소

    //
    // Request Method : POST 
    // Body Type : json
    delegate void resCallback_BettingDisconnect(ResBettingDisconnect response);
    IEnumerator requestCoinDisconnect(ReqBettingDisconnect req, resCallback_BettingDisconnect callback)
    {
        string url = getBaseURL() + "/v1/betting/" + "zera" + "/disconnect";

        string reqJsonData = JsonUtility.ToJson(req);


        UnityWebRequest www = UnityWebRequest.Post(url, reqJsonData);
        byte[] buff = System.Text.Encoding.UTF8.GetBytes(reqJsonData);
        www.uploadHandler = new UploadHandlerRaw(buff);
        www.SetRequestHeader("api-key", API_KEY);
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();

        Debug.Log(www.downloadHandler.text);
        ResBettingDisconnect res = JsonUtility.FromJson<ResBettingDisconnect>(www.downloadHandler.text);
        callback(res);
    }

    public class ReqBettingDisconnect
    {
        public string betting_id;
    }

    // Response Disconnect
    public class ResBettingDisconnect
    {
        public string message;
        public class Data
        {
        }
        public Data data;
    }

    #endregion

    #region 제라 잔액

    // Zera 잔고 확인
    public void CheckZeraBalance()
    {
        StartCoroutine(processRequestZeraBalance());

        serverStatusText.text = "Recieving Zera Balance";
    }

    IEnumerator processRequestZeraBalance()
    {
        yield return requestZeraBalance(resGetSessionID.sessionId, (response) =>
        {
            if (response != null)
            {
                //Debug.Log("## Response Zera Balance : " + response.ToString());

                serverStatusText.text = "Done";
                StartSceneManager.Inst.zeraTextMesh.text =  response.data.balance.ToString() + " Zera";
            }
        });
    }


    delegate void resCallback_BalanceInfo(Res_BalanceInfo response);
    IEnumerator requestZeraBalance(string sessionID, resCallback_BalanceInfo callback)
    {
        string url = getBaseURL() + ("/v1/betting/" + "zera" + "/balance/" + sessionID);

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("api-key", API_KEY);
        yield return www.SendWebRequest();
        //Debug.Log(www.downloadHandler.text);
        Res_BalanceInfo res = JsonUtility.FromJson<Res_BalanceInfo>(www.downloadHandler.text);
        callback(res);
        //UnityWebRequest www = new UnityWebRequest(URL);
    }


    [System.Serializable]
    public class Res_BalanceInfo
    {
        // 응답 결과
        public string message;  // 
        [System.Serializable]
        public class Balance
        {
            public int balance;
        }
        public Balance data;

        public override string ToString()
        {
            return $"message:{message} Balance : {data.balance}";
        }
    }


    #endregion
}
