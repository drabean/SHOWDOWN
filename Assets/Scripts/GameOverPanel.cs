using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameOverPanel : MonoBehaviour
{
    public Sprite[] sprites;

    public SpriteRenderer isWinImage;
    public Text statusText;
    public Text coinText;

    public Animator playerAnim;

    public Animator coinAnim;


    public void setGameOverPanel(bool isImPlayer1, bool isPlayer1Win)
    {
        if(isImPlayer1 == isPlayer1Win)
        {
            isWinImage.sprite = sprites[0];
            coinText.text = "You earned \n" + ( GameManager.Inst.DAPPXmanager.reward )+ " zera!";
            coinAnim.SetTrigger("CoinSpin");
        }        
       else
        {
            isWinImage.sprite = sprites[1];
            coinText.text = "You lose \n" + GameManager.Inst.DAPPXmanager.reward + " zera!";
            coinAnim.SetTrigger("CoinBreak");
        }

        if(isImPlayer1)
        {
            playerAnim.runtimeAnimatorController = GameManager.Inst.anims[GameManager.Inst.DAPPXmanager.player1Info.characterIndex];
        }
        else
        {
            playerAnim.runtimeAnimatorController = GameManager.Inst.anims[GameManager.Inst.DAPPXmanager.player2Info.characterIndex];
        }

        playAnim(isImPlayer1, isPlayer1Win);
    }

    public void playAnim(bool isImPlayer1, bool isPlayer1Win)
    {
        if (isImPlayer1 == isPlayer1Win)
        {
            coinAnim.SetTrigger("CoinSpin");
        }
        else
        {
            coinAnim.SetTrigger("CoinBreak");
        }
    }

}
