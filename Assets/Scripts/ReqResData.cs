//유저 정보
public class Res_GetUserProfile
{
	// 응답 결과
	//
	public string Status;
	public int StatusCode;
	public string Message;

	// 유저 정보
	[System.Serializable]
	public class UserProfile
	{
		public string referral_by;
		public string referral_code;
		public string username;
		public string email_id;
		public string public_address;
		public string _id;
		public string upline;
	}
	public UserProfile userProfile;


	public override string ToString()
	{
		return $"Status :{Status} StatusCode:{StatusCode} Message:{Message}";
	}
}


//세션 정보
[System.Serializable]
public class Res_GetSessionID
{
	// 응답 결과
	//
	public string Status;
	public int StatusCode;
	public string Message;

	// 유저의 Session ID
	public string sessionId;

	public override string ToString()
	{
		return $"Status :{Status} StatusCode:{StatusCode} Message:{Message}";
	}
}


//배팅정보
[System.Serializable]
public class Res_Settings
{
	// 응답 결과
	public string message;

	[System.Serializable]
	public class Settings
	{
		public string _id;
		public string game_id;
		public bool betting;
		public bool maintenance;
		public string createdAt;
		public string updatedAt;
		public int __v;

		public override string ToString()
		{
			return $"_id: {_id} game_id: {game_id} betting: {betting} maintenance: {maintenance}  createdAt: {createdAt}  updatedAt: {updatedAt}  __v: {__v} ";
		}
	}

	[System.Serializable]
	public class BetInfo
	{
		public string _id;
		public string game_id;
		public int amount;
		public int platform_fee;
		public int developer_fee;
		public int win_reward;
		public int win_amount;
		public string createdAt;
		public string updatedAt;
		public int __v;
		public override string ToString()
		{
			return $"_id: {_id} game_id: {game_id} amount: {amount} platform_fee: {platform_fee}  developer_fee: {developer_fee}  win_reward: {win_reward}  win_amount: {win_amount} createdAt: {createdAt} updatedAt: {updatedAt} __v: {__v} ";
		}
	}

	[System.Serializable]
	public class Data
	{
		public Settings settings;
		public BetInfo[] bets;

		public override string ToString()
		{
			string retStr = settings.ToString() + "\n";
			for (int i = 0; i < bets.Length; ++i)
				retStr += bets[i].ToString() + "\n";

			return retStr;
		}
	}
	public Data data;

	public override string ToString()
	{
		return $"message:{message}\n data {data.ToString()}";
	}
}
