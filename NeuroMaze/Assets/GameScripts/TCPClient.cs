using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class TCPClient : MonoBehaviour
{
	/// <summary>
		/// NeuroMaze is designed to run with the Intan recording platform, which offers TCP support
		/// TCP allows Unity to control intans neural recording software remotely.
		/// This TCP Client class handles this connection as well as remote commands 
		/// for Intan recording 
		/// 
		/// CREDIT FOR CODE SKELETON: @danielbierwirth
		/// https://gist.github.com/danielbierwirth/0636650b005834204cb19ef5ae6ccedb
	/// </summary>

	// Buttons to keep track of main session start and TCP connection start
	public Button startButton;
	public Button tcpButton;
	
	// This 'global' bool value is used to communicate TCP connection failure outside of the 
	// update function. The other private bools below are explicitly used to ensure that
	// fail / success code only runs once in the update loop.
	public bool globalFailConnect = false;
	
	public Text Text;	// Text for TCP button which will update based on TCP connection status

	// Flags for various error checking
	public bool intanError = false;
	bool failConnect = false;
	bool successConnect = false;
	

	#region private members 	
	private TcpClient socketConnection;
	private Thread clientReceiveThread;
	#endregion
	// Use this for initialization 	
	void Start()
	{
		// Deactivate Intan recording error upon start
		Text.gameObject.SetActive(false);

		// Start initial attempt to connect to TCP Client
		ConnectToTcpServer();

		// Set buttons
		Button start_button = startButton.GetComponent<Button>();
		Button tcp_button = tcpButton.GetComponent<Button>();
		tcp_button.onClick.AddListener(ConnectToTcpServer);
		start_button.onClick.AddListener(SendMessage);
	}
	// Update is called once per frame
	void Update()
	{
		// If we were unable to connect
		if (failConnect)
        {
			// Reflect failure in TCP button text and color, and need for retry
			tcpButton.GetComponentInChildren<Text>().text = "TCP Connect Fail. Try again?";
			tcpButton.GetComponentInChildren<Image>().color = Color.yellow;
			failConnect = false;
        }

		// If we cpnnect succesfully
		if (successConnect)
        {
			// Reflect success in text/color, and indicate recording sesison can start.
			tcpButton.GetComponentInChildren<Text>().text = "Connected! Session ready.";
            tcpButton.GetComponentInChildren<Image>().color = Color.green;
            successConnect = false;
        }

		// If there is an error on Intan's side from connection attempt
		// show Intan error text
		if (intanError)
		{
			Text.gameObject.SetActive(true);
		}
		else
		{
			Text.gameObject.SetActive(false);
		}
    }

	/// Setup socket connection. 	
	private void ConnectToTcpServer()
	{
		// If a connection attempt failed in the past, change the button to reflect a retry. 
		if (globalFailConnect)
        {
			tcpButton.GetComponentInChildren<Text>().text = "Trying again...";
		} 
		
		else
        {
			tcpButton.GetComponentInChildren<Text>().text = "Connecting to Intan Server...";
		}

		try
		{
			clientReceiveThread = new Thread(new ThreadStart(ListenForData));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();
		}
		catch (Exception e)
		{
			failConnect = true;
			globalFailConnect = true;
			Debug.Log("On client connect exception " + e); 
		}
	}	

	/// Runs in background clientReceiveThread; Listens for incomming data. 	 
	private void ListenForData()
	{
		try
		{
			socketConnection = new TcpClient("localhost", 5000);
			Byte[] bytes = new Byte[1024];
			successConnect = true;

			while (true)
			{
				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream())
				{
					int length;
					// Read incomming stream into byte array. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
					{
						var incommingData = new byte[length];
						Array.Copy(bytes, 0, incommingData, 0, length);
						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incommingData);
						Debug.Log("server message received as: " + serverMessage);

						// If the server says it can't record becuase basefile is not set
						if (serverMessage == "Filename.BaseFilename and Filename.Path must both be specified before recording can occur")
                        {
							// Invoke intan error message
							intanError = true;
							// Reset server message
							serverMessage = "";
						}
					}
				}
			}
		}
		catch (SocketException socketException)
		{
			failConnect = true;
			globalFailConnect = true;
			Debug.Log("Socket exception: " + socketException);
		}
	}
	
	/// Send message to server using socket connection. 	
	private void SendMessage()
	{
		// Initially deactivate Intan base file error message
		intanError = false;
		// If there is nothing to connect to, do nothing
		if (socketConnection == null)
		{
			return;
		}
		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				string stopMessage = "set runmode stop;";
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(stopMessage);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				Debug.Log("Client sent his message - should be received by server");

				string startMessage = "set runmode record;";
				// Convert string message to byte array.                 
				clientMessageAsByteArray = Encoding.ASCII.GetBytes(startMessage);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				Debug.Log("Client sent his message - should be received by server");
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
}