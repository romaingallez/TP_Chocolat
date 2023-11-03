using System;
using System.IO;

namespace ProjetChocolat.Logging
{
    public static class Logger
    {
        private static readonly string logFilePath = "log.txt"; // Chemin du fichier de log

        public static void LogAction(string user, string action, string item, string time = null)
        {
            time = time ?? DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            string logEntry = $"{time} {action} {item} par {user}.";

            // Écriture dans le fichier de log
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(logEntry);
            }
        }
    }
}