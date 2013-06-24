using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Net;
using Socket = Chilkat.Socket;

namespace SSLProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            int receivedSinceLastPush = 0;
            Socket listenSocket = InitializeSocketLibrary();
            Socket outboundSocket = InitializeSocketLibrary();
            int sequence = 0;
            File.Delete(@"c:\readBytesRealTime.txt");

            bool success = listenSocket.BindAndListen(443, 25);
            if (success != true)
            {
                Console.WriteLine(listenSocket.LastErrorText + "\r\n");
                return;
            }

            int i = 0;

            while (true)
            {
                if (++i % 100 == 0)
                {
                    Console.Write("## RESET...");
                    Thread.Sleep(1000);
                    listenSocket.Close(90000);
                    outboundSocket.Close(90000);
                    listenSocket = InitializeSocketLibrary();
                    outboundSocket = InitializeSocketLibrary();
                    if (listenSocket.BindAndListen(443, 25) != true)
                    {
                        Console.WriteLine(listenSocket.LastErrorText + "\r\n");
                        return;
                    }

                    GC.WaitForFullGCComplete();
                    Console.WriteLine(" Done!");
                }


                Chilkat.Socket connectedSocket = null;
                do
                {
                    try
                    {
                        connectedSocket = listenSocket.AcceptNextConnection(6000000);
                    }
                    catch (System.AccessViolationException e)
                    {
                        connectedSocket = null;
                        Console.WriteLine("Error: " + e);
                        Thread.Sleep(500);
                    }
                } while (connectedSocket == null);

                if (connectedSocket == null)
                {
                    Console.WriteLine(listenSocket.LastErrorText);
                    return;
                }


                //outboundSocket.Connect("157.56.163.183", 443, false, 10000);
                //outboundSocket.Connect("96.43.148.92", 443, false, 10000);
                
                //BOX/VOZAA
                //outboundSocket.Connect("184.173.20.253", 443, false, 10000);
                // OWA
                outboundSocket.Connect("157.56.163.101", 443, false, 10000);
                
                 //outboundSocket.Connect("96.43.145.46", 443, false, 10000);

                //outboundSocket.Connect("www.hispla.com", 443, false, 10000);
                if (success != true)
                {
                    Console.WriteLine(listenSocket.LastErrorText + "\r\n");
                    return;
                }


                //  Set maximum timeouts for reading an writing (in millisec)
                connectedSocket.MaxReadIdleMs = 90000;
                connectedSocket.MaxSendIdleMs = 90000;
                outboundSocket.MaxReadIdleMs = 90000;
                outboundSocket.MaxSendIdleMs = 90000;

                int received = 0;
                bool receivingClient = false;
                bool receivingServer = false;

                while (true)
                {
                    if (!connectedSocket.IsConnected)
                        break;


                    byte[] bytes = null;

                    if (!receivingClient)
                    {
                        receivingClient = true;

                        try
                        {
                            success = connectedSocket.AsyncReceiveBytes();
                        }
                        catch (System.AccessViolationException e)
                        {
                            Console.WriteLine("Error2: " + e);
                            Thread.Sleep(100);
                            break;
                        }


                        if (success != true)
                        {
                            Console.WriteLine(connectedSocket.LastErrorText + "\r\n");
                            return;
                        }
                    }

                    // 5712 vs 5728
                    // 3594

                    if (receivingClient
                        && connectedSocket.AsyncReceiveFinished)
                    {
                        receivingClient = false;
                        bytes = connectedSocket.AsyncReceivedBytes;
                        if (bytes != null && bytes.Length > 0)
                        {
                            Console.WriteLine(" >>> rcv: " + receivedSinceLastPush);
                            //                            File.Delete(@"c:\readBytesRealTime.txt");

                            bool writeCompleted = false;
                            do
                            {
                                // NEW
                                if (receivedSinceLastPush == 0) break;

                                if (!File.Exists(@"c:\readBytesRealTime.txt"))
                                    break;
                                try
                                {
                                    using (FileStream fs = new FileStream(@"c:\readBytesRealTime.txt",
                                                    FileMode.Append, FileAccess.Write, FileShare.Read))
                                    {
                                        using (StreamWriter w = new StreamWriter(fs))
                                        {
                                            w.WriteLine("--- " + receivedSinceLastPush);
                                            Console.WriteLine("\n----------------\n");
                                            w.Close();
                                            writeCompleted = true;
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    Thread.Sleep(1);
                                }
                            } while (!writeCompleted);




                            writeCompleted = false;
                            do
                            {

                                // NEW
                                if (receivedSinceLastPush == 0) break;

                                try
                                {
                                    using (FileStream fs = new FileStream(@"c:\readBytes.txt",
                                                    FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                                    {
                                        using (StreamWriter w = new StreamWriter(fs))
                                        {
                                            w.WriteLine(sequence + " " + receivedSinceLastPush);
                                            w.Close();
                                            writeCompleted = true;
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Thread.Sleep(1);
                                }
                            } while (!writeCompleted);
                            receivedSinceLastPush = 0;

                            //if (bytes != null && bytes.Length > 250)
                            //Console.WriteLine(" < " + System.Text.Encoding.UTF8.GetString(bytes).Substring(0, 200));

                            //else if (bytes != null && bytes.Length > 0)
                            //Console.WriteLine(" < " + System.Text.Encoding.UTF8.GetString(bytes));

                            success = outboundSocket.SendBytes(bytes);
                            if (success != true)
                            {
                                Console.WriteLine(connectedSocket.LastErrorText + "\r\n");
                                return;
                            }
                        }
                    }



                    byte[] bytes2 = null;

                    if (!receivingServer)
                    {
                        receivingServer = true;

                        try
                        {
                            success = outboundSocket.AsyncReceiveBytes();
                        }
                        catch (System.AccessViolationException e)
                        {
                            Console.WriteLine("Error2: " + e);
                            Thread.Sleep(100);
                            break;
                        }

                        if (success != true)
                        {
                            Console.WriteLine("@2 " + outboundSocket.LastErrorText + "\r\n");
                            return;
                        }
                    }

                    if (receivingServer
                        && outboundSocket.AsyncReceiveFinished)
                    {
                        receivingServer = false;

                        bytes2 = outboundSocket.AsyncReceivedBytes;

                        if (bytes2 != null && bytes2.Length > 0)
                        {
                            received += bytes2.Length;
                            Console.WriteLine("<<" + bytes2.Length);

                            bool writeCompleted = false;
                            sequence++;
                            //Console.Write(++sequence + " // ");
                            do
                            {
                                try
                                {

                                    using (StreamWriter w = new StreamWriter(new FileStream(@"c:\readBytesRealTime.txt",
                                                    FileMode.Append, FileAccess.Write, FileShare.Read)))

                                    //using (StreamWriter w = new StreamWriter(@"c:\readBytesRealTime.txt", true))
                                    {
                                        w.WriteLine(sequence + " " + bytes2.Length);
                                        w.Close();
                                        writeCompleted = true;
                                    }

                                }
                                catch (Exception e)
                                {
                                    Thread.Sleep(1);
                                }
                            } while (!writeCompleted);




                            Console.Title = "received: " + received;
                            receivedSinceLastPush += bytes2.Length;

                            //if (bytes2 != null && bytes2.Length > 250)
                            //Console.WriteLine(" < " + System.Text.Encoding.UTF8.GetString(bytes2).Substring(0, 200));

                            //else if (bytes2 != null && bytes2.Length > 0)
                            //Console.WriteLine(" < " + System.Text.Encoding.UTF8.GetString(bytes2));

                            connectedSocket.SendBytes(bytes2);
                        }

                        else if (connectedSocket.IsConnected && !outboundSocket.IsConnected)
                        {
                            // Kill it
                            //Console.WriteLine(" ...kill it with fire");
                            connectedSocket.Close(10000);
                            break;
                        }

                    }


                }
                /* For connection Closeo
                bool writeCompleted = false;
                do
                {
                    try
                    {
                        using (FileStream fs = new FileStream(@"c:\readBytes.txt",
                                        FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                        {
                            using (StreamWriter w = new StreamWriter(fs))
                            {
                                w.WriteLine(received);
                                w.Close();
                                writeCompleted = true;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Thread.Sleep(1);
                    }
                } while (!writeCompleted);*/


                //  Close the connection with the client.
                //  Wait a max of 20 seconds (20000 millsec)
                outboundSocket.Close(10000);
                connectedSocket.Close(10000);
                Console.WriteLine("Socket Closed < " + received);
            }
        }


        public static Socket InitializeSocketLibrary()
        {
            Chilkat.Socket listenSocket = new Chilkat.Socket();

            if (listenSocket.UnlockComponent("Socket!TEAM!BEAN_60AD13D4C7nG") != true)
            {
                Console.WriteLine("Failed to unlock component");
                throw new Exception("Failed to unlock component");
            }

            return listenSocket;
        }

    }
}
