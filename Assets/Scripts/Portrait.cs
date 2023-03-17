using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portrait : MonoBehaviour
{
   public TextMesh player1Text;
    public TextMesh player2Text;

    public Sprite[] Portraits;

    public SpriteRenderer player1Portrait;
    public SpriteRenderer player2Portrait;

    public TextMesh player1HPName;
    public TextMesh player2HPName;


    public void setTextPlayer1(string name)
    {
        player1Text.text = name;
        player1Text.GetComponent<MeshRenderer>().sortingLayerName = "Arcade Machine";
        player1Text.GetComponent<MeshRenderer>().sortingOrder = 4;

        player1HPName.text = name;
        player1HPName.GetComponent<MeshRenderer>().sortingLayerName = "UI";
        player1HPName.GetComponent<MeshRenderer>().sortingOrder =2;

        Debug.Log("SETPLAYERNAME1" + name);
    }

    public void setTextPlayer2(string name)
    {
        player2Text.text = name;
        player2Text.GetComponent<MeshRenderer>().sortingLayerName = "Arcade Machine";
        player2Text.GetComponent<MeshRenderer>().sortingOrder = 7;

        player2HPName.text = name;
        player2HPName.GetComponent<MeshRenderer>().sortingLayerName = "UI";
        player2HPName.GetComponent<MeshRenderer>().sortingOrder = 4;

        Debug.Log("SETPLAYERNAME2" + name);
    }

    public void setPortrait(bool isPlayer1, int index)
    {
        if (isPlayer1) player1Portrait.sprite = Portraits[index];
        else player2Portrait.sprite = Portraits[index];
    }

}
