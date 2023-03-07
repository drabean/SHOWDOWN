using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RapidGauge : MonoBehaviour
{
    public SpriteRenderer playerGauge;
    float playerGaugeSizeX = 4.8125f;
    float playerGaugeSizeY = 0.1875f;
    public SpriteRenderer enemyGauge;
    float enemyGaugeSizeX = 3.4375f;
    float enemyGaugeSizeY = 0.0625f;

    public void fillGauge(bool isPlayer, int maxValue, int curValue)
    {
        if (isPlayer)
        {
            if(playerGauge != null)playerGauge.size = new Vector2(playerGaugeSizeX * ((float)curValue / maxValue), playerGaugeSizeY);
        }
        else
        {
            if(enemyGauge != null)enemyGauge.size = new Vector2(enemyGaugeSizeX * ((float)curValue / maxValue), enemyGaugeSizeY);
        }
    }
}
