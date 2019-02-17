using System;
using System.Net;
using System.Net.Sockets; // For TcpListener, TcpClient
using PGN.Data;

namespace TestApp
{
    class MyTcpServer
    {

        private const int BUFSIZE = 500; // Size of receive buffer

        public static void Use()
        {            
            TcpListener listener = null;

            try
            {
                // Create a TCPListener to accept client connections
                listener = new TcpListener(IPAddress.Any, 8000);
                listener.Start();
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.ErrorCode + " " + se.Message);
                Environment.Exit(se.ErrorCode);
            }


            byte[] rcvBuffer = new byte[BUFSIZE]; // Receive buffer
            int bytesRcvd;
            // Received byte count

            while(true)
            { 
                // Run forever, accepting and servicing connections
                TcpClient client = null;
                NetworkStream netStream = null;
                try
                {

                    client = listener.AcceptTcpClient(); // Get client connection
                    client.NoDelay = true;
                    netStream = client.GetStream();
                    Console.Write("Handling client - ");

                    // Receive until client closes connection, indicated by 0 return value
                    int totalBytesEchoed = 0;

                    while ((bytesRcvd = netStream.Read(rcvBuffer, 0, rcvBuffer.Length)) > 0)
                    {
                        netStream.Write(rcvBuffer, 0, bytesRcvd);
                        totalBytesEchoed += bytesRcvd;
                    }

                    Console.WriteLine(DateTime.Now + "." + DateTime.Now.Millisecond);

                    NetData data = NetData.RecoverBytes(rcvBuffer);
                    Console.WriteLine("Get: " + data.data as string + "; Time = " + (DateTime.Now - DateTime.Parse(data.time)).Milliseconds.ToString());

                    // Close the stream and socket. We are done with this client!
                    netStream.Close();
                    client.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    netStream.Close();
                }
            }
        }
    }
}
