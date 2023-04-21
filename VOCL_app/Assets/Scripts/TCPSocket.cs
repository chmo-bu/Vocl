// This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
// To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/ 
// or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

namespace TCPSocket {
    public class TCPClient {  	
        #region private members 	
        private TcpClient socketConnection; 	
        private Thread clientReceiveThread;
        private readonly object mu = new object();
        private bool received = true;
        #endregion  	

        // constructor
        public TCPClient() {
            ConnectToTcpServer();     
        }  	
        
        /// <summary> 	
        /// Setup socket connection. 	
        /// </summary> 	
        private void ConnectToTcpServer () { 		
            try {  			
                clientReceiveThread = new Thread (new ThreadStart(ListenForData)); 			
                clientReceiveThread.IsBackground = true; 			
                clientReceiveThread.Start();  		
            } 		
            catch (Exception e) { 			
                Debug.Log("On client connect exception " + e); 		
            } 	
        }  	
        /// <summary> 	
        /// Runs in background clientReceiveThread; Listens for incomming data. 	
        /// </summary>     
        public void ListenForData() { 		
            try { 			
                socketConnection = new TcpClient("localhost", 8052);  			
                Byte[] bytes = new Byte[1024];             
                while (true) { 				
                    // Get a stream object for reading 				
                    using (NetworkStream stream = socketConnection.GetStream()) { 					
                        int length; 					
                        // Read incomming stream into byte arrary. 					
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
                            var incommingData = new byte[length]; 						
                            Array.Copy(bytes, 0, incommingData, 0, length); 						
                            // Convert byte array to string message. 						
                            string serverMessage = Encoding.ASCII.GetString(incommingData);
                            // if (serverMessage.Equals("received")) {
                            //     // lock(mu) {
                            //         received = true;
                            //     // }
                            //     Debug.Log("got message!");
                            // }					
                            // Debug.Log("server message received as: " + serverMessage); 					
                        } 				
                    } 			
                }         
            }         
            catch (SocketException socketException) {             
                Debug.Log("Socket exception: " + socketException);         
            }     
        }  	
        /// <summary> 	
        /// Send message to server using socket connection. 	
        /// </summary> 	
        public async void SendMessage(byte[] data) {
            if (socketConnection == null) {             
                return;         
            }
            try { 			
                // Get a stream object for writing. 			
                NetworkStream stream = socketConnection.GetStream(); 			
                if (stream.CanWrite) {                  				
                    // Write byte array to socketConnection stream.                 
                    // stream.Write(data, 0, data.Length);  
                    await stream.WriteAsync(data, 0, data.Length);                 
                    Debug.Log("Client sent his message - should be received by server");
                    // received = false;         
                }         
            } 		
            catch (SocketException socketException) {             
                Debug.Log("Socket exception: " + socketException);         
            }     
        } 
    }

        // State object for receiving data from remote device.
    public class StateObject {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    public class AsynchronousClient {
        // The port number for the remote device.
        private const int port = 11000;

        // ManualResetEvent instances signal completion.
        private  ManualResetEvent connectDone = 
            new ManualResetEvent(false);
        private  ManualResetEvent sendDone = 
            new ManualResetEvent(false);
        private  ManualResetEvent receiveDone = 
            new ManualResetEvent(false);

        // The response from the remote device.
        private  String response = String.Empty;

        public void StartClient(string host, int port, byte[] message) {
            // Connect to a remote device.
            try {
                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
                IPHostEntry ipHostInfo = Dns.Resolve(host);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.
                Socket client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                client.BeginConnect( remoteEP, 
                    new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();

                // Send test data to the remote device.
                // Send(client,"This is a test<EOF>");
                Send(client, message);
                sendDone.WaitOne();

                // Receive the response from the remote device.
                Receive(client);
                receiveDone.WaitOne();

                // Write the response to the console.
                Debug.Log("Response received : " + response);

                // Release the socket.
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                
            } catch (Exception e) {
                Debug.Log(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.
                Socket client = (Socket) ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Debug.Log("Socket connected to " + 
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            } catch (Exception e) {
                Debug.Log(e.ToString());
            }
        }

        private void Receive(Socket client) {
            try {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            } catch (Exception e) {
                Debug.Log(e.ToString());
            }
        }

        private void ReceiveCallback( IAsyncResult ar ) {
            try {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject) ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0) {
                    // There might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(state.buffer,0,bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer,0,StateObject.BufferSize,0,
                        new AsyncCallback(ReceiveCallback), state);
                } else {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1) {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.
                    receiveDone.Set();
                }
            } catch (Exception e) {
                Debug.Log(e.ToString());
            }
        }

        private void Send(Socket client, byte[] data) {
            // // Convert the string data to byte data using ASCII encoding.
            // byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(data, 0, data.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.
                Socket client = (Socket) ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Debug.Log("Sent " + bytesSent +  " bytes to server.");

                // Signal that all bytes have been sent.
                sendDone.Set();
            } catch (Exception e) {
                Debug.Log(e.ToString());
            }
        }
    }
}