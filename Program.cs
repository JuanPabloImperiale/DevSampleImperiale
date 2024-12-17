using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevSample
{
    class Program
    {
        static readonly int _cyclesToRun;
        static readonly int _samplesToLoad;
        static readonly DateTime _sampleStartDate;
        static readonly TimeSpan _sampleIncrement;
        static Program()
        {
            // Note: these settings should not be modified
            // Calculate the number of cycles to run: half the logical cores if more than 1 core is available
            _cyclesToRun = Environment.ProcessorCount > 1 ? Environment.ProcessorCount / 2 : 1;
            // Limit the number of cycles to a maximum of 4
            _cyclesToRun = _cyclesToRun > 4 ? 4 : _cyclesToRun;
            // Define the number of samples to load
            _samplesToLoad = 222222;
            // Set the starting date for the samples
            _sampleStartDate = new DateTime(1990, 1, 1, 1, 1, 1, 1);
        }
        static void Main(string[] args)
        {
            Stopwatch totalMonitor = new Stopwatch();
            totalMonitor.Start();
            LogMessage($"Starting Execution on a {Environment.ProcessorCount} core system. A total of {_cyclesToRun} cycles will be run");
            for (int i = 0; i < _cyclesToRun; i++)
            {
                try
                {
                    TimeSpan cycleElapsedTime = new TimeSpan();
                    // Initialize a stopwatch to measure elapsed time for the cycle
                    Stopwatch cycleTimer = new Stopwatch();
                    // Create a sample generator with the specified start date and increment
                    SampleGenerator sampleGenerator = new SampleGenerator(_sampleStartDate, _sampleIncrement);

                    // Log the start of the sample load process
                    LogMessage($"Cycle {i} Started Sample Load.");
                    // Start the stopwatch to measure sample loading time
                    cycleTimer.Start();
                    // Load the specified number of samples
                    sampleGenerator.LoadSamples(_samplesToLoad);
                    // Stop the stopwatch after loading samples
                    cycleTimer.Stop();
                    // Store the elapsed time for loading samples
                    cycleElapsedTime = cycleTimer.Elapsed;
                    // Log the time taken to load samples
                    LogMessage($"Cycle {i} Finished Sample Load. Load Time: {cycleElapsedTime.TotalMilliseconds.ToString("N")} ms.");

                    // Log the start of the sample validation process
                    LogMessage($"Cycle {i} Started Sample Validation.");
                    // Restart the stopwatch to measure validation time
                    cycleTimer.Restart();
                    // Validate the loaded samples
                    sampleGenerator.ValidateSamples();
                    // Stop the stopwatch after validation
                    cycleTimer.Stop();
                    // Store the elapsed time for sample validation
                    cycleElapsedTime = cycleTimer.Elapsed;
                    // Log the time taken and the total validated samples
                    LogMessage($"Cycle {i} Finished Sample Validation. Total Samples Validated: {sampleGenerator.SamplesValidated}. Validation Time: {cycleElapsedTime.TotalMilliseconds.ToString("N")} ms.");
                    // Initialize a variable to calculate the sum of sample values (changed from float to decimal for higher precision)
                    decimal valueSum = 0;
                    // Iterate through all samples to calculate the total sum
                    foreach (Sample s in sampleGenerator.Samples)
                    {
                        valueSum += (decimal)s.Value; // Explicit cast to ensure compatibility with decimal type
                    }
                    // Log the sum of all sample values with 20 digits of precision (format adjusted to show 20 decimal places)
                    LogMessage($"Cycle {i} Sum of All Samples: {valueSum.ToString("N20")}.");
                    // Log the total time taken for the cycle
                    LogMessage($"Cycle {i} Finished. Total Cycle Time: {cycleElapsedTime.TotalMilliseconds.ToString("N")} ms.");
                }
                catch (Exception ex)
                {
                    LogMessage($"Execution Failed!\n{ex.ToString()}");
                }

            }
            totalMonitor.Stop();
            LogMessage("-----");
            LogMessage($"Execution Finished. Total Elapsed Time: {totalMonitor.Elapsed.TotalMilliseconds.ToString("N")} ms.");
            Console.Read();
        }
        static void LogMessage(string message)
        {
            LogToFile(message);
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fffff")} - {message}");
        }

        static void LogToFile(string message)
        {
            // Define the directory where the log file will be saved (absolute path)
            string logDirectory = @"localDebug";
            // Define the fallback log file path (existing file in the project)
            string fallbackLogFile = Path.Combine(Directory.GetCurrentDirectory(), @"obj\Debug\DevSample.csproj.FileListAbsolute.txt");

            try
            {
                // Attempt to create or clean the C:\Temp directory
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                else
                {
                    // Clean existing log file to avoid conflicts between runs
                    string logFilePath = Path.Combine(logDirectory, "application_log.txt");
                    if (File.Exists(logFilePath))
                    {
                        File.Delete(logFilePath);
                    }
                }

                // Use a fixed log file name to append all logs into the same file
                string logFilePathFinal = Path.Combine(logDirectory, "application_log.txt");

                // Write the log message to the file in C:\Temp
                using (StreamWriter writer = new StreamWriter(logFilePathFinal, true))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                // If there are permission issues, log to the fallback file
                LogMessage($"Unable to write to C:\\Temp, using fallback log at {fallbackLogFile}");

                // Ensure that the fallback file exists
                if (!File.Exists(fallbackLogFile))
                {
                    using (StreamWriter writer = new StreamWriter(fallbackLogFile, true))
                    {
                        writer.WriteLine("Log started at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                }

                // Write the log message to the fallback file
                using (StreamWriter writer = new StreamWriter(fallbackLogFile, true))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                }
            }
            catch (Exception ex)
            {
                // If any other error occurs, log it to the fallback file
                using (StreamWriter writer = new StreamWriter(fallbackLogFile, true))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR: {ex.Message}");
                }
            }
        }
    }
}
