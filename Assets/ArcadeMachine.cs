using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcadeMachine : MonoBehaviour
{
    public Animator player1Joystick;
    public Animator player2Joystick;

    public void JoystickLeft(bool isPlayer1)
    {
        if (isPlayer1) player1Joystick.SetTrigger("Left");
        else player2Joystick.SetTrigger("Left");
    }
    public void JoystickUp(bool isPlayer1)
    {
        if (isPlayer1) player1Joystick.SetTrigger("Up");
        else player2Joystick.SetTrigger("Up");
    }
    public void JoystickRight(bool isPlayer1)
    {
        if (isPlayer1) player1Joystick.SetTrigger("Right");
        else player2Joystick.SetTrigger("Right");
    }
    public void JoystickDown(bool isPlayer1)
    {
        if (isPlayer1) player1Joystick.SetTrigger("Down");
        else player2Joystick.SetTrigger("Down");
    }

}
