using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Text;

public class NetworkClient : MonoBehaviour
{
    NetworkDriver networkDriver;
    NetworkConnection networkConnection;
    NetworkPipeline reliableAndInOrderPipeline;
    NetworkPipeline nonReliableNotInOrderedPipeline;
    const ushort NetworkPort = 9001;
    const string IPAddress = "192.168.0.169";

    public GameObject gameStateManager;
    public TicTacToeLogic tictactoe;

    public ClientMessageProcessor messageProcessor;

    void Start()
    {
        networkDriver = NetworkDriver.Create();
        reliableAndInOrderPipeline = networkDriver.CreatePipeline(typeof(FragmentationPipelineStage), typeof(ReliableSequencedPipelineStage));
        nonReliableNotInOrderedPipeline = networkDriver.CreatePipeline(typeof(FragmentationPipelineStage));
        networkConnection = default(NetworkConnection);
        NetworkEndPoint endpoint = NetworkEndPoint.Parse(IPAddress, NetworkPort, NetworkFamily.Ipv4);
        networkConnection = networkDriver.Connect(endpoint);
    }

    public void OnDestroy()
    {
        networkConnection.Disconnect(networkDriver);
        networkConnection = default(NetworkConnection);
        networkDriver.Dispose();
    }

    void Update()
    {
        #region Check Input and Send Msg

        //if (Input.GetKeyDown(KeyCode.A))
        //    SendMessageToServer("Hello server's world, sincerely your network client");

        #endregion

        networkDriver.ScheduleUpdate().Complete();

        #region Check for client to server connection

        if (!networkConnection.IsCreated)
        {
            Debug.Log("Client is unable to connect to server");
            return;
        }

        #endregion

        #region Manage Network Events

        NetworkEvent.Type networkEventType;
        DataStreamReader streamReader;
        NetworkPipeline pipelineUsedToSendEvent;

        while (PopNetworkEventAndCheckForData(out networkEventType, out streamReader, out pipelineUsedToSendEvent))
        {
            if (pipelineUsedToSendEvent == reliableAndInOrderPipeline)
                Debug.Log("Network event from: reliableAndInOrderPipeline");
            else if (pipelineUsedToSendEvent == nonReliableNotInOrderedPipeline)
                Debug.Log("Network event from: nonReliableNotInOrderedPipeline");

            switch (networkEventType)
            {
                case NetworkEvent.Type.Connect:
                    Debug.Log("We are now connected to the server");
                    break;
                case NetworkEvent.Type.Data:
                    int sizeOfDataBuffer = streamReader.ReadInt();
                    NativeArray<byte> buffer = new NativeArray<byte>(sizeOfDataBuffer, Allocator.Persistent);
                    streamReader.ReadBytes(buffer);
                    byte[] byteBuffer = buffer.ToArray();
                    string msg = Encoding.Unicode.GetString(byteBuffer);
                    messageProcessor.ProcessReceivedMsg(msg);
                    buffer.Dispose();
                    break;
                case NetworkEvent.Type.Disconnect:
                    Debug.Log("Client has disconnected from server");
                    networkConnection = default(NetworkConnection);
                    break;
            }
        }

        #endregion
    }

    private bool PopNetworkEventAndCheckForData(out NetworkEvent.Type networkEventType, out DataStreamReader streamReader, out NetworkPipeline pipelineUsedToSendEvent)
    {
        networkEventType = networkConnection.PopEvent(networkDriver, out streamReader, out pipelineUsedToSendEvent);

        if (networkEventType == NetworkEvent.Type.Empty)
            return false;
        return true;
    }

    //private void ProcessReceivedMsg(string msg)
    //{
    //    Debug.Log("Msg received = " + msg);

    //    string[] csv = msg.Split(',');
    //    int signifier = int.Parse(csv[0]);

    //    if(signifier == ServerToClientSignifiers.RegisterComplete)
    //    {
    //        gameStateManager.GetComponent<GameStateManager>().ChangeState(GameStates.MainMenu);
    //    }
    //    else if(signifier == ServerToClientSignifiers.LoginComplete)
    //    {
    //        gameStateManager.GetComponent<GameStateManager>().ChangeState(GameStates.MainMenu);
    //    }
    //    else if(signifier == ServerToClientSignifiers.GameStart)
    //    {
    //        gameStateManager.GetComponent<GameStateManager>().ChangeState(GameStates.Game);
    //    }
    //    else if(signifier == ServerToClientSignifiers.UpdateGameBoard)
    //    {
    //        int position;
    //        char playerSymbol;

    //        if (int.TryParse(csv[1], out position) && csv[2].Length == 1)
    //        {
    //            playerSymbol = csv[2][0];

    //            // Update the local game board representation
    //            gameStateManager.GetComponent<TicTacToeLogic>().HandleServerUpdate(position, playerSymbol);
    //        }
    //        else
    //        {
    //            Debug.LogError("Invalid format for UpdateGameBoard message. Received: " + msg);
    //        }
    //    }
    //    else if(signifier == ServerToClientSignifiers.GameReset)
    //    {
    //        gameStateManager.GetComponent<TicTacToeLogic>().RestartGame();
    //    }
    //}

    public void SendMessageToServer(string msg)
    {
        byte[] msgAsByteArray = Encoding.Unicode.GetBytes(msg);
        NativeArray<byte> buffer = new NativeArray<byte>(msgAsByteArray, Allocator.Persistent);

        DataStreamWriter streamWriter;
        networkDriver.BeginSend(reliableAndInOrderPipeline, networkConnection, out streamWriter);
        streamWriter.WriteInt(buffer.Length);
        streamWriter.WriteBytes(buffer);
        networkDriver.EndSend(streamWriter);

        buffer.Dispose();
    }

    public void DisconnectFromServer()
    {
        if (networkConnection.IsCreated)
        {
            networkConnection.Disconnect(networkDriver);
            networkConnection = default(NetworkConnection);
        }
    }


    public void MakeMove(int position, char playerSymbol)
    {
        // Check if the move is valid and within bounds
        if (position >= 0 && position < 9)
        {
            // Send the move to the server
            string msg = ClientToServerSignifiers.MakeMove + "," + position + "," + playerSymbol;

            SendMessageToServer(msg);
            Debug.Log(msg);
        }
        else
        {
            Debug.LogError("Invalid move position");
        }
    }

    public void RestartGame()
    {
        string msg = ClientToServerSignifiers.GameRestart + ",";
        SendMessageToServer(msg);
    }
}


