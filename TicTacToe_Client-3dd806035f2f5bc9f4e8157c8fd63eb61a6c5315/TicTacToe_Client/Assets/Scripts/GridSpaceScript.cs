using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridSpaceScript : MonoBehaviour
{
    public int buttonIndex; 
    private TicTacToeLogic gameController;
    private TMP_Text buttonText;

    public void SetGameControllerRef(TicTacToeLogic gameManager)
    {
        gameController = gameManager;
        buttonText = GetComponentInChildren<TMP_Text>(); 
    }

    public void SetSpace()
    {
        if (buttonText != null && gameController != null)
        {
            if (buttonText.text == "" && !gameController.IsGameOver)
            {
                buttonText.text = gameController.CurrentPlayer.ToString();
                //gameController.MakeMove(buttonIndex); 
            }
        }
    }
}
