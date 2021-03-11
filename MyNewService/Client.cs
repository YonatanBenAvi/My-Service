using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;

namespace MyNewService
{

    public class SynchronousSocketClient
    {

        public static void StartClient()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            // try
            //{

            // Establish the remote endpoint for the socket.  
            // This example uses port 11000 on the local computer.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP  socket.  
            Socket sender = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint. Catch any errors.  
            //try
            //{
            sender.Connect(remoteEP);

            Console.WriteLine(String.Format("Socket connected to {0}",
                sender.RemoteEndPoint.ToString()));


            while (true)
            {

                String msg = ReciveMessageFromServer(sender);

                String returnMsg = HandleCommand(msg, sender);

                SendMessageToServer(sender, returnMsg);


                if (msg.Equals("done") || msg.Equals("quit"))
                {
                    break;
                }

            }

            // Release the socket.  
            MyNewService.WriteToLog("closing connection");
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();


            /*
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}" + ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}" + se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}" + e.ToString());
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
            */
        }




        public static void Wait(TimeSpan t, String command, Socket sender)
        {
            Thread.Sleep(t);
            MyNewService.WriteToLog("i finished waiting: " + t);
            HandleCommand(command, sender);
        }






        public static String HandleCommand(String command, Socket sender)
        {
            String[] commandArray = command.Split(' ');
            String command1 = commandArray[0];
            String msgToBeReturned = "";
            MyNewService.WriteToLog("command 1: " + command1);
            switch (command1)
            {
                case "time":
                    //should accept like this:
                    // time 2021,02,15,08,0,0 command
                    DateTime timeNow = DateTime.Now;
                    int[] timeArr = Array.ConvertAll(commandArray[1].Split(','), int.Parse);
                    DateTime inputTime = new DateTime(timeArr[0], timeArr[1], timeArr[2], timeArr[3], timeArr[4], timeArr[5]);
                    TimeSpan value1 = inputTime.Subtract(timeNow);
                    Console.WriteLine("waiting: " + value1);
                    Thread thread = new Thread(() => Wait(value1, commandArray[2], sender));
                    thread.Start();
                    msgToBeReturned = "thread started successfully";
                    break;
                case "done":
                case "quit":
                    MyNewService.WriteToLog("the server has requested to disconnect");
                    msgToBeReturned = "by by server";
                    break;
                case "chrome":
                    MyNewService.WriteToLog("opening chrome");
                    var test = System.Diagnostics.Process.Start(@"C:\Program Files\Google\Chrome\Application\chrome.exe");
                    if (test.GetType() == null)
                    {
                        MyNewService.WriteToLog("null");
                    }
                    msgToBeReturned = "opening chrome";
                    break;
                case "ipconfig":
                    MyNewService.WriteToLog("runing ipconfig");
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = @"/C ipconfig > C:\Users\shay\Desktop\client\ipconfig.txt";
                    process.StartInfo = startInfo;
                    process.Start();
                    msgToBeReturned = "done ipconfig";
                    break;
                case "send_file":
                    // There is a text file test.txt located in the root directory.
                    string fileName = @commandArray[1];
                    // Send file fileName to remote device
                    Console.WriteLine("Sending {0} to the host." + fileName);
                    byte[] file = File.ReadAllBytes(fileName);
                    String fileLen = file.Length.ToString().PadLeft(10, '0'); ;
                    byte[] fileLenBytes = Encoding.ASCII.GetBytes(fileLen);
                    sender.Send(fileLenBytes);
                    sender.Send(file);
                    msgToBeReturned = "file has been sent succesfully";

                    break;
                case "info":
                    msgToBeReturned = "I am yonatans client";
                    break;
                default:
                    MyNewService.WriteToLog("command not recognized");
                    msgToBeReturned = "command not recognized";
                    break;
            }
            return msgToBeReturned;
        }

        public static String ReciveMessageFromServer(Socket sender)
        {
            byte[] size = new byte[10];

            // Receive the response from the remote device.  
            int sizeLen = sender.Receive(size);

            int rcvSize = Int32.Parse(Encoding.ASCII.GetString(size, 0, sizeLen));


            byte[] bytes = new byte[rcvSize];
            int bytesRec = sender.Receive(bytes);

            String msg = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            return msg;
        }

        public static void SendMessageToServer(Socket sender, String response)
        {
            String msgLen = response.Length.ToString().PadLeft(10, '0');

            // Echo the data back to the client.  
            byte[] msg = Encoding.ASCII.GetBytes(msgLen + response);

            sender.Send(msg);
        }



        public static int Main(String[] args)
        {
            StartClient();
            return 0;
        }
    }
}