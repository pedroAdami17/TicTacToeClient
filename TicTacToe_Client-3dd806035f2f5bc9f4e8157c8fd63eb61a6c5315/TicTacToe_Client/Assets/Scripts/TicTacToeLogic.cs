using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeLogic : MonoBehaviour
{
    public Button[] buttonList;
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;
    public GameObject restartButton;
    public GameObject networkedClient;
    public GameObject gameStateManager;

    private const int BoardSize = 9;
    private char[] gameBoard;
    private char currentPlayer;
    private int moveCount;
    private bool gameIsOver;

    public char CurrentPlayer => currentPlayer;
    public bool IsGameOver => gameIsOver;

    private bool isObserver = false;

    public void StartGame()
    {
        InitializeGameBoard();
        SetupButtonListeners();
        UpdateLocalGame();
    }

    private void InitializeGameBoard()
    {
        gameBoard = new char[BoardSize];
        for (int i = 0; i < BoardSize; i++)
        {
            gameBoard[i] = ' ';
        }

        currentPlayer = 'X';
        moveCount = 0;
        gameIsOver = false;

        gameOverPanel.SetActive(false);
    }

    private void SetupButtonListeners()
    {
        buttonList = GameObject.FindGameObjectsWithTag("TicTacToeButton")
            .Select(buttonGO => buttonGO.GetComponent<Button>()).ToArray();

        foreach (var button in buttonList)
        {
            button.onClick.AddListener(() => OnButtonClick(button));
        }
    }

    private void OnButtonClick(Button button)
    {
        int position = System.Array.IndexOf(buttonList, button);
        if (!gameIsOver && gameBoard[position] == ' ' && !IsObserver())
        {
            char playerSymbol = currentPlayer;
            networkedClient.GetComponent<NetworkClient>().MakeMove(position, playerSymbol); //send position index and current player to the server
        }
        Debug.Log(position + " " + currentPlayer);
    }

    public void MakeMove(int position, char playerSymbol)
    {
        if (!gameIsOver && gameBoard[position] == ' ')
        {
            gameBoard[position] = playerSymbol;
            UpdateButtonVisuals(position, playerSymbol);

            UpdateLocalGame();
        }
    }


    private void UpdateLocalGame()
    {
        CheckLocalWinCondition();
    }

    private void ChangePlayer()
    {
        currentPlayer = (currentPlayer == 'X') ? 'O' : 'X';
    }

    private void CheckLocalWinCondition()
    {
        if (CheckWinCondition())
        {
            GameOver(currentPlayer + " wins locally!");
        }
        else if (moveCount >= BoardSize)
        {
            GameOver("It's a draw!");
        }
        else
        {
            moveCount++;
            ChangePlayer();
        }
    }

    private bool CheckWinCondition()
    {
        // Check rows
        for (int i = 0; i < 3; i++)
        {
            if (gameBoard[i * 3] == currentPlayer && gameBoard[i * 3 + 1] == currentPlayer && gameBoard[i * 3 + 2] == currentPlayer)
                return true;
        }

        // Check columns
        for (int i = 0; i < 3; i++)
        {
            if (gameBoard[i] == currentPlayer && gameBoard[i + 3] == currentPlayer && gameBoard[i + 6] == currentPlayer)
                return true;
        }

        // Check diagonals
        if ((gameBoard[0] == currentPlayer && gameBoard[4] == currentPlayer && gameBoard[8] == currentPlayer) ||
            (gameBoard[2] == currentPlayer && gameBoard[4] == currentPlayer && gameBoard[6] == currentPlayer))
        {
            return true;
        }

        return false;
    }

    private void GameOver(string result)
    {
        gameIsOver = true;

        gameOverText.text = result;
        gameOverPanel.SetActive(true);
        restartButton.SetActive(true);
    }

    public void RestartGame()
    {
        StartGame();
        foreach (var button in buttonList)
        {
            button.interactable = true;
            button.GetComponentInChildren<TMP_Text>().text = "";
        }
        moveCount = 0;
        gameStateManager.GetComponent<GameStateManager>().ChangeState(GameStates.Game);
    }

    public void RestartButton()
    {
        networkedClient.GetComponent<NetworkClient>().RestartGame();
    }

    private void UpdateButtonVisuals(int position, char playerSymbol)
    {
        buttonList[position].GetComponentInChildren<TMP_Text>().text = playerSymbol.ToString();
    }


    public void HandleServerUpdate(int position, char playerSymbol)
    {
        gameBoard[position] = playerSymbol;
        UpdateButtonVisuals(position, playerSymbol);
        CheckLocalWinCondition();
    }

    public void SetObserverStatus(bool observer)
    {
        isObserver = observer;
    }

    public bool IsObserver()
    {
        return isObserver;
    }

}
