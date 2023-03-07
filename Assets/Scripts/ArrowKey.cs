using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowKey : MonoBehaviour
{
    public Sprite[] sprites_unpressed;
    public Sprite[] sprites_pressed;

    public Sprite[] opponentBtn_Sprites;

    [SerializeField] SpriteRenderer sp;

    [SerializeField] SpriteRenderer OpponentBtn;

    public ARROW dir;

    /// <summary>
    /// 0이면 좌, 1이면 상, 2이면 우, 3이면 하
    /// </summary>
    /// <param name="dir"></param>
    public void setSprite(ARROW dir, bool isACCROW = true)
    {
        sp.sprite = sprites_unpressed[(int)dir];
        this.dir = dir;
        if(!isACCROW)
        {
            Destroy(OpponentBtn.gameObject);
        }
    }



    public void pressBtn()
    {
        sp.sprite = sprites_pressed[(int)dir];
    }

    public void failBtn()
    {
        sp.color = Color.red;
    }

    public void refreshBtn()
    {
        sp.sprite = sprites_unpressed[(int)dir];
    }
    public void changeOpponent(bool isRight)
    {
        OpponentBtn.sprite = opponentBtn_Sprites[isRight ? 0 : 1];
    }


}
