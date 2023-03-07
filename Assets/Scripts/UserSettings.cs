using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserSettings : MonoBehaviour
{

    public UserInfo info;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

}
