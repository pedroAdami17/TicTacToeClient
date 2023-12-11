using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStateManager : MonoBehaviour
{
    public GameObject submitButton, usernameInput, passwordInput, registerToggle, loginToggle;
    public GameObject nameText, passwordText;
    public GameObject ticTacToeBoard;
    public GameObject waitingForPlayerScreen;

    public GameObject joinGameRoomButton, quitButton;

    public GameObject networkedClient;


    private bool isChangingToggleState = false;

    private TicTacToeLogic ticTacToeLogic;

    void Start()
    {
        submitButton.GetComponent<Button>().onClick.AddListener(SubmitButtonPressed);

        loginToggle.GetComponent<Toggle>().onValueChanged.AddListener((value) => LoginTogglePressed(value));
        registerToggle.GetComponent<Toggle>().onValueChanged.AddListener((value) => RegisterTogglePressed(value));

        joinGameRoomButton.GetComponent<Button>().onClick.AddListener(JoinGameRoomButtonPressed);
        quitButton.GetComponent<Button>().onClick.AddListener(QuitButtonPressed);

        ticTacToeLogic = GameObject.Find("GameManager").GetComponent<TicTacToeLogic>();

        //ChangeState(GameStates.LoginMenu);
        ChangeState(GameStates.Game);
    }

    public void SubmitButtonPressed()
    {
        Debug.Log("Submit Pressed");

        string n = usernameInput.GetComponent<TMP_InputField>().text;
        string p = passwordInput.GetComponent<TMP_InputField>().text;
        
        string msg;

        if(registerToggle.GetComponent<Toggle>().isOn)
        {
            Debug.Log("regiter account");
            msg = ClientToServerSignifiers.Register + "," + n + "," + p;
            
        }
        else
        {
            Debug.Log("login account");
            msg = ClientToServerSignifiers.Login + "," + n + "," + p;
        }

        networkedClient.GetComponent<NetworkClient>().SendMessageToServer(msg);
    }
    public void LoginTogglePressed(bool newValue)
    {
        if (!isChangingToggleState)
        {
            isChangingToggleState = true;
            registerToggle.GetComponent<Toggle>().isOn = !newValue;
            isChangingToggleState = false;
        }
    }

    public void RegisterTogglePressed(bool newValue)
    {
        if (!isChangingToggleState)
        {
            isChangingToggleState = true;
            loginToggle.GetComponent<Toggle>().isOn = !newValue;
            isChangingToggleState = false;
        }

    }

    public void JoinGameRoomButtonPressed()
    {
        networkedClient.GetComponent<NetworkClient>().SendMessageToServer(ClientToServerSignifiers.JoinQueue + "");
        ChangeState(GameStates.WaitingForPlayer);
    }

    public void QuitButtonPressed()
    {
        networkedClient.GetComponent<NetworkClient>().DisconnectFromServer();
        ChangeState(GameStates.MainMenu);
    }

    public void ChangeState(int newState)
    {
        submitButton.SetActive(false);
        usernameInput.SetActive(false); 
        passwordInput.SetActive(false);
        registerToggle.SetActive(false);
        loginToggle.SetActive(false);
        joinGameRoomButton.SetActive(false);
        nameText.SetActive(false);
        passwordText.SetActive(false);
        ticTacToeBoard.SetActive(false);
        waitingForPlayerScreen.SetActive(false);

        if (newState == GameStates.LoginMenu)
        {
            submitButton.SetActive(true);
            usernameInput.SetActive(true);
            passwordInput.SetActive(true);
            registerToggle.SetActive(true);
            loginToggle.SetActive(true);
            nameText.SetActive(true);
            passwordText.SetActive(true);
        }
        else if(newState == GameStates.MainMenu)
        {
            joinGameRoomButton.SetActive(true);
        }
        else if(newState == GameStates.WaitingForPlayer)
        {
            //Activate waiting screen UI
            waitingForPlayerScreen.SetActive(true);
        }
        else if(newState == GameStates.Game)
        {
            //Activate Game UI
            ticTacToeBoard.SetActive(true);
            ticTacToeLogic.StartGame();
        }
    }
}


static public class GameStates
{
    public const int LoginMenu = 1;
    public const int MainMenu = 2;
    public const int WaitingForPlayer = 3;
    public const int Game = 4;
}


