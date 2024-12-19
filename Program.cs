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
            _sampleIncrement = TimeSpan.FromMinutes(1);
        }
        static void Main(string[] args)
        {
            Stopwatch totalMonitor = new Stopwatch();
            totalMonitor.Start();
            LogMessage($"Starting Execution on a {Environment.ProcessorCount} core system. A total of {_cyclesToRun} cycles will be run");

            // Use Parallel.ForEach with a specified MaxDegreeOfParallelism to control CPU usage
            var cycleIndices = Enumerable.Range(0, _cyclesToRun); // Generate cycle indices
            ParallelOptions parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 4 // Limit to 4 concurrent tasks
            };
            Parallel.ForEach(cycleIndices, parallelOptions, i =>
            {
                try
                {
                    TimeSpan cycleElapsedTime = new TimeSpan();
                    Stopwatch cycleTimer = new Stopwatch();
                    // Create a sample generator with the specified start date and increment
                    SampleGenerator sampleGenerator = new SampleGenerator(_sampleStartDate, _sampleIncrement);
                    // Log the start of the sample load process
                    LogMessage($"Cycle {i} Started Sample Load.");
                    cycleTimer.Start();
                    // Load the specified number of samples
                    sampleGenerator.LoadSamples(_samplesToLoad);
                    cycleTimer.Stop();
                    // Store and log the elapsed time for loading samples
                    cycleElapsedTime = cycleTimer.Elapsed;
                    LogMessage($"Cycle {i} Finished Sample Load. Load Time: {cycleElapsedTime.TotalMilliseconds:N} ms.");
                    // Log the start of the sample validation process
                    LogMessage($"Cycle {i} Started Sample Validation.");
                    cycleTimer.Restart();
                    // Validate the loaded samples
                    sampleGenerator.ValidateSamples();
                    cycleTimer.Stop();
                    // Store and log the elapsed time for sample validation
                    cycleElapsedTime = cycleTimer.Elapsed;
                    LogMessage($"Cycle {i} Finished Sample Validation. Total Samples Validated: {sampleGenerator.SamplesValidated}. Validation Time: {cycleElapsedTime.TotalMilliseconds:N} ms.");
                    // Calculate the sum of sample values
                    decimal valueSum = 0;
                    foreach (Sample s in sampleGenerator.Samples)
                    {
                        valueSum += (decimal)s.Value; // Explicit cast for precision
                    }
                    // Log the sum of all sample values with precision
                    LogMessage($"Cycle {i} Sum of All Samples: {valueSum:N20}.");
                    LogMessage($"Cycle {i} Finished. Total Cycle Time: {cycleElapsedTime.TotalMilliseconds:N} ms.");
                }
                catch (Exception ex)
                {
                    LogMessage($"Execution Failed in Cycle {i}!\n{ex}");
                }
            });
            totalMonitor.Stop();
            LogMessage("-----");
            LogMessage($"Execution Finished. Total Elapsed Time: {totalMonitor.Elapsed.TotalMilliseconds:N} ms.");
            Console.Read();
        }
        static void LogMessage(string message)
        {
            LogToFile(message);
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fffff")} - {message}");
        }

        static void LogToFile(string message)
        {
            //JPI changes
            // Define the directory where the log file will be saved (absolute path)
            string logDirectory = @"localDebug";
            // Define the fallback log file path (existing file in the project)
            string fallbackLogFile = Path.Combine(Directory.GetCurrentDirectory(), @"obj\Debug\DevSample.csproj.FileListAbsolute.txt");
            try
            {
                // Attempt to write to C:\Temp first
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                // Use a fixed log file name to append all logs into the same file
                string logFilePath = Path.Combine(logDirectory, "application_log.txt");
                // Write the log message to the file in C:\Temp
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - {message}");
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
                    writer.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - {message}");
                }
            }
            catch (Exception ex)
            {
                // If any other error occurs, log it to the fallback file
                using (StreamWriter writer = new StreamWriter(fallbackLogFile, true))
                {
                    writer.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - ERROR: {ex.Message}");
                }
            }
        }
    }
}
