using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BREACHv3Server
{
    class SocketUtilities
    {
        internal static Chilkat.Socket InitializeSocket(int port)
        {
            Chilkat.Socket listenSocket = new Chilkat.Socket();

            bool success;
            success = listenSocket.UnlockComponent("Socket!TEAM!BEAN_60AD13D4C7nG");
            if (success != true)
            {
                Console.WriteLine(listenSocket.LastErrorText + "\r\n");
                throw new Exception("Error!");
            }

            success = listenSocket.BindAndListen(port, 25);
            if (success != true)
            {
                Console.WriteLine(listenSocket.LastErrorText + "\r\n");
                throw new Exception("Error!");
            }

            return listenSocket;
        }
    }
}
