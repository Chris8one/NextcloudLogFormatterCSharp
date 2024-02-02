using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace NextcloudLogFormatter
{
    internal class Program
    {
        // Define a class to represent the structure of a log entry
        private class LogEntry
        {
            public int? level { get; set; }
            public string? reqId { get; set; }
            public string? time { get; set; }
            public string? remoteAddr { get; set; }
            public string? user { get; set; }
            public string? app { get; set; }
            public string? method { get; set; }
            public string? url { get; set; }
            public string? message { get; set; }
            public string? userAgent { get; set; }
            public Newtonsoft.Json.Linq.JObject? exception { get; set; }
            public dynamic? data { get; set; }
            public string? version { get; set; }
        }

        // Main entry point of the program
        static void Main(string[] args)
        {
            // Check if the correct number of command-line arguments is provided
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: dotnet run log_file.log");
                Environment.Exit(1);
            }

            // Get the input filename from the command-line arguments
            string inputFilename = args[0];

            // Generate filenames for the formatted output files
            string filteredOutputFilename = $"{Path.GetFileNameWithoutExtension(inputFilename)}_formatted_filtered";
            string allOutputFilename = $"{Path.GetFileNameWithoutExtension(inputFilename)}_all_formatted";

            // Format and analyze the logs, and get the result filenames
            var result = FormatAndAnalyzeLogs(inputFilename, filteredOutputFilename, allOutputFilename);

            // Print messages indicating the creation of the new log files
            Console.WriteLine($"Filtered formatted log file created: {result.Item1}");
            Console.WriteLine($"All formatted log file created: {result.Item2}");
        }

        // Function to format a log entry and handle potential errors
        static string? FormatLogEntry(string logEntry)
        {
            // Define a mapping of log levels to their corresponding text representations
            Dictionary<int, string> levelMapping = new Dictionary<int, string>
            {
                { 0, "DEBUG" },
                { 1, "INFO" },
                { 2, "WARNING" },
                { 3, "ERROR" },
                { 4, "FATAL" }
            };

            try
            {
                // Deserialize the JSON log entry into a LogEntry object
                LogEntry? logJson = JsonConvert.DeserializeObject<LogEntry>(logEntry);

                // Check if deserialization was successful
                if (logJson != null)
                {
                    // Access the log level and set a default value if it's null
                    int logLevel = logJson.level ?? -1;

                    // Map the log level to its text representation
                    string logLevelText = levelMapping.ContainsKey(logLevel) ? levelMapping[logLevel] : "UNKNOWN";

                    // Format the log entry
                    string formattedLog = $"{logLevelText}\n";
                    formattedLog += $"Request ID: {logJson.reqId ?? "Unknown"}\n";
                    formattedLog += $"Time: {logJson.time ?? "Unknown"}\n";
                    formattedLog += $"Remote Address: {logJson.remoteAddr ?? "Unknown"}\n";
                    formattedLog += $"User: {logJson.user ?? "Unknown"}\n";
                    formattedLog += $"App: {logJson.app ?? "Unknown"}\n";
                    formattedLog += $"Method: {logJson.method ?? "Unknown"}\n";
                    formattedLog += $"URL: {logJson.url ?? "Unknown"}\n";
                    formattedLog += $"Message: {logJson.message ?? "Unknown"}\n";
                    formattedLog += $"User Agent: {logJson.userAgent ?? "Unknown"}\n";
                    formattedLog += $"Exception: {GetExceptionMessage(logJson.exception)}\n";
                    formattedLog += $"Data: {logJson.data ?? "Unknown"}\n";
                    formattedLog += $"Version: {logJson.version ?? "Unknown"}\n";

                    return formattedLog;
                }
                else
                {
                    // Handle the case when deserialization fails, logJson is null
                    Console.WriteLine("Deserialization failed for log entry: " + logEntry);
                    return null;
                }
            }
            catch (JsonException e)
            {
                // Handle JSON decoding error
                Console.WriteLine($"Error decoding JSON: {e.Message}");
                return null;
            }
        }

        // Helper function to extract exception message from JObject
        static string GetExceptionMessage(Newtonsoft.Json.Linq.JObject? exception)
        {
            return exception?["message"]?.ToString() ?? "Unknown";
        }

        // Function to format and analyze logs from an input file
        static (string, string) FormatAndAnalyzeLogs(string inputFilename, string filteredOutputFilename, string allOutputFilename)
        {
            // Read all lines from the input log file
            string[] logContent = File.ReadAllLines(inputFilename);

            // Lists to store filtered and all formatted log entries
            List<string> filteredLogs = new List<string>();
            List<string> allLogs = new List<string>();

            // Process each log entry
            foreach (string logEntry in logContent)
            {
                // Deserialize the JSON log entry into a LogEntry object
                LogEntry? logJson = JsonConvert.DeserializeObject<LogEntry>(logEntry);

                // Check if deserialization was successful
                if (logJson != null)
                {
                    // Access the log level and set a default value if it's null
                    int logLevel = logJson.level ?? -1;

                    // Format the log entry
                    string? formattedLog = FormatLogEntry(logEntry);

                    // Check if formatting was successful
                    if (formattedLog != null)
                    {
                        // Add the formatted log entry to the appropriate lists based on log level
                        if (logLevel == 3 || logLevel == 4)
                        {
                            filteredLogs.Add(formattedLog);
                        }

                        allLogs.Add(formattedLog);
                    }
                    else
                    {
                        // Handle the case when FormatLogEntry returns null
                        Console.WriteLine("Formatting failed for log entry: " + logEntry);
                    }
                }
                else
                {
                    // Handle the case when deserialization fails
                    Console.WriteLine("Deserialization failed for log entry: " + logEntry);
                }
            }

            // Generate a timestamp for unique filenames
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Append the timestamp to the output filenames
            filteredOutputFilename = $"{Path.GetFileNameWithoutExtension(filteredOutputFilename)}[{timestamp}].txt";
            allOutputFilename = $"{Path.GetFileNameWithoutExtension(allOutputFilename)}[{timestamp}].txt";

            // Write the filtered and all formatted logs to new files
            File.WriteAllLines(filteredOutputFilename, filteredLogs);
            File.WriteAllLines(allOutputFilename, allLogs);

            // Return the filenames of the created output files
            return (filteredOutputFilename, allOutputFilename);
        }
    }
}
