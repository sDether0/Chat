using System;
using System.IO;

namespace Logger
{
    namespace Writer
    {
        public static class DebugLog
        {
            private const string VkApiPath = "Debug.log";

            public static void WriteLine(this string log)
            {
                string final = DateTime.Now.ToString("U") + " " + log + "\n";
                File.AppendAllText(VkApiPath, final);
            }
        }
    }
}