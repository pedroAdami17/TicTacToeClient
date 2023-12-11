using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientMessageProcessor : MonoBehaviour
{
    public GameObject gameStateManager;

    public void ProcessReceivedMsg(string msg)
    {
        Debug.Log("Msg received = " + msg);

        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);

        if (signifier == ServerToClientSignifiers.RegisterComplete)
        {
            gameStateManager.GetComponent<GameStateManager>().ChangeState(GameStates.MainMenu);
        }
        else if (signifier == ServerToClientSignifiers.LoginComplete)
        {
            gameStateManager.GetComponent<GameStateManager>().ChangeState(GameStates.MainMenu);
        }
        else if (signifier == ServerToClientSignifiers.GameStart)
        {
            gameStateManager.GetComponent<GameStateManager>().ChangeState(GameStates.Game);
        }
        else if (signifier == ServerToClientSignifiers.UpdateGameBoard)
        {
            int position;
            char playerSymbol;

            if (int.TryParse(csv[1], out position) && csv[2].Length == 1)
            {
                playerSymbol = csv[2][0];

                // Update the local game board representation
                gameStateManager.GetComponent<TicTacToeLogic>().HandleServerUpdate(position, playerSymbol);
            }
            else
            {
                Debug.LogError("Invalid format for UpdateGameBoard message. Received: " + msg);
            }
        }
        else if (signifier == ServerToClientSignifiers.GameReset)
        {
            gameStateManager.GetComponent<TicTacToeLogic>().RestartGame();
        }
        else if (signifier == ServerToClientSignifiers.Observer)
        {
            gameStateManager.GetComponent<TicTacToeLogic>().SetObserverStatus(true);
            Debug.Log("You are now an observer.");
        }
    }
}

public static class ClientToServerSignifiers
{
    public const int Register = 1;
    public const int Login = 2;
    public const int JoinQueue = 3;
    public const int MakeMove = 4;
    public const int GameRestart = 5;
    public const int QuitGame = 6;
}

public static class ServerToClientSignifiers
{
    public const int LoginComplete = 1;
    public const int LoginFailed = 2;
    public const int RegisterComplete = 3;
    public const int RegisterFailed = 4;
    public const int GameStart = 5;
    public const int UpdateGameBoard = 6;
    public const int GameReset = 7;
    public const int PlayerDisconnected = 8;
    public const int Observer = 9;
}
