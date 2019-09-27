using System;
using System.Collections.Generic;
using System.Text;

namespace FTTClient.Core
{
    public static class Manager
    {
        public static Client Client;
        public static ClientPage CPage;
        public static bool Subscribed = false;

        public static StringBuilder @string = new StringBuilder();

        public static string IP;
        public static int Port;
    }
}
