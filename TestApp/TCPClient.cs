using System;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using PGN.Data;

namespace TestApp
{
    class MyTcpClient
    {

        public static void Use()
        {        
            // Convert input String to bytes

            byte[] byteBuffer = NetData.GetBytesData(new NetData("Test", new PGN.General.User()));
            TcpClient client = null;
            NetworkStream netStream = null;

            try
            {

                // Create socket that is connected to server on specified port

                client = new TcpClient(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString(), 8000);
                client.NoDelay = true;
                Console.WriteLine("Connected to server... sending echo string");

                netStream = client.GetStream();

                // Send the encoded string to the server
                netStream.Write(byteBuffer, 0, byteBuffer.Length);
                netStream.Flush();
                Console.WriteLine(DateTime.Now + "." + DateTime.Now.Millisecond);
                Console.WriteLine("Sent {0} bytes to server...", byteBuffer.Length);

                int totalBytesRcvd = 0;   // Total bytes received so far
                int bytesRcvd = 0;
                // Bytes received in last read

                // Receive the same string back from the server
                while (totalBytesRcvd < byteBuffer.Length)
                {
                    if ((bytesRcvd = netStream.Read(byteBuffer, totalBytesRcvd, byteBuffer.Length - totalBytesRcvd)) == 0)
                    {
                        Console.WriteLine("Connection closed prematurely.");
                        break;
                    }
                    totalBytesRcvd += bytesRcvd;
                }

                Console.WriteLine("Received {0} bytes from server: {1}", totalBytesRcvd, NetData.RecoverBytes(byteBuffer).data as string);


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                netStream.Close();
                client.Close();
            }
        }
    }
}