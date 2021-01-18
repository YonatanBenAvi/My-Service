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
            try
            {
                // Establish the remote endpoint for the socket.  
                // This example uses port 11000 on the local computer.  
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);

                    MyNewService.WriteToLog(String.Format("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString()));


                    while (true)
                    {
                        /*
                        // Encode the data string into a byte array.  
                        byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                        // Send the data through the socket.  
                        int bytesSent = sender.Send(msg);
                        */

                        // Receive the response from the remote device.  
                        int bytesRec = sender.Receive(bytes);

                        String msg = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                        //MyNewService.WriteToLog(String.Format("Echoed test = {0}",msg));
                        HandleCommand(msg, sender);


                        if (msg.Equals("done")){
                            break;
                        }

                    }
                    // Release the socket.  
                    MyNewService.WriteToLog("closing connection");
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    MyNewService.WriteToLog("ArgumentNullException : {0}" + ane.ToString());
                }
                catch (SocketException se)
                {
                    MyNewService.WriteToLog("SocketException : {0}" + se.ToString());
                }
                catch (Exception e)
                {
                    MyNewService.WriteToLog("Unexpected exception : {0}" + e.ToString());
                }

            }
            catch (Exception e)
            {
                MyNewService.WriteToLog(e.ToString());
            }
        }


        public static void Wait(TimeSpan t)
        {
            Thread.Sleep(t);
            MyNewService.WriteToLog("i finished waiting: " + t);

        }

        public static void HandleCommand(String command, Socket sender)
        {
            MyNewService.WriteToLog(command.Substring(0, command.IndexOf(" ")));
            switch (command.Substring(0, command.IndexOf(" ")))
            {
                case "time":
                    DateTime timeNow = DateTime.Now;
                    DateTime inputTime = new DateTime(2020, 12, 27, 9, 12, 0);
                    TimeSpan value1 = inputTime.Subtract(timeNow);
                    MyNewService.WriteToLog("waiting: " + value1);
                    Thread thread = new Thread(() => Wait(value1));
                    thread.Start();
                    break;
                case "done":
                    MyNewService.WriteToLog("i am done");
                    break;
                case "chrome":
                    MyNewService.WriteToLog("opening chrome");
                    System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe");
                    break;
                case "ipconfig":
                    MyNewService.WriteToLog("runing ipconfig");
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = @"/C ipconfig > C:\Users\david\Desktop\test\ipconfig.txt";
                    process.StartInfo = startInfo;
                    process.Start();
                    break;
                case "file":
                    // There is a text file test.txt located in the root directory.
                    string fileName = "C:\\Users\\david\\Desktop\\test\\test1.txt";
                    // Send file fileName to remote device
                    MyNewService.WriteToLog("Sending {0} to the host." + fileName);
                    // Send the data through the socket.  
                    string readText = File.ReadAllText(fileName) + "<EOF>";
                    byte[] msg = Encoding.ASCII.GetBytes(readText);
                    sender.Send(msg);
                    break;
                default:
                    MyNewService.WriteToLog("command not recognized");
                    break;


            }
        }

       

        public static int Main(String[] args)
        {
            StartClient();
            return 0;
        }
    }
}