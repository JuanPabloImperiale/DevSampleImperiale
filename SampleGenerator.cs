using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevSample
{
    class SampleGenerator
    {
        private readonly DateTime _sampleStartDate;
        private readonly TimeSpan _sampleIncrement;
        private readonly List<Sample> _sampleList;
        public SampleGenerator(DateTime sampleStartDate, TimeSpan sampleIncrement)
        {
            _sampleList = new List<Sample>();
            _sampleStartDate = sampleStartDate;
            _sampleIncrement = sampleIncrement;
        }
        /// <summary>
        /// Samples should be a time-descending ordered list
        /// </summary>
        public List<Sample> Samples { get { return _sampleList; } }
        public int SamplesValidated { get; private set; }

        public void LoadSamples(int samplesToGenerate)
        {
            //TODO: can we load samples faster?
            _sampleList.Clear(); // Clear the existing sample list
            DateTime date = _sampleStartDate; // Start date for generating samples
            // Instead of inserting at the beginning of the list, add to the end
            // and reverse the list later to maintain the descending order
            for (int i = 0; i < samplesToGenerate; i++)
            {
                Sample s = new Sample(i == 0); // Create a sample, marking the first sample with a flag
                s.LoadSampleAtTime(date); // Load the sample at the given date
                //JPI change
                _sampleList.Add(s); // Add the sample to the end of the list (faster than Insert(0, s)) // _sampleList.Insert(0, s);
                date += _sampleIncrement; // Increment the date for the next sample
            }
            //JPI change
            // Reverse the list to ensure the samples are in time-descending order
            _sampleList.Reverse();
        }

        public async void ValidateSamples()
        {
            //TODO: can we validate samples faster?
            int samplesValidated = 0;
            int sampleCount = _sampleList.Count; // Cache the count for performance

            //JPI OPTION1  Use Parallel.For to split the work across multiple threads
            // Parallel.For(0, sampleCount, i =>
            // {
            //     Sample currentSample = _sampleList[i]; // Cache current sample for better access
            //     Sample? nextSample = (i < sampleCount - 1) ? _sampleList[i + 1] : null; // Cache next sample to avoid calculating in each loop iteration
            //     if (currentSample.ValidateSample(nextSample!, _sampleIncrement))
            //     {
            //         //Safely increment the shared variable using Interlocked
            //         Interlocked.Increment(ref samplesValidated);
            //     }
            // });
            // TIME => 22:06:04.71983 - Execution Finished. Total Elapsed Time: 2,950.171 ms.

            //JPI OPTION 2  Task.Run(() => { ... }): Executes each sample validation in a separate task, allowing parallel execution.
            var tasks = new List<Task>();
            for (int i = 0; i < sampleCount; i++)
            {
                int index = i; // Capture the index for each task to ensure proper access
                tasks.Add(Task.Run(() =>
                {
                    Sample currentSample = _sampleList[index]; // Cache current sample for better access
                    Sample? nextSample = (index < sampleCount - 1) ? _sampleList[index + 1] : null; // Cache next sample

                    // Validamos la muestra
                    if (currentSample.ValidateSample(nextSample!, _sampleIncrement))
                    {
                        // Safely increment the shared variable using Interlocked
                        Interlocked.Increment(ref samplesValidated);
                    }
                }));
            }
            // Wait for all tasks to complete
            await Task.WhenAll(tasks);
            // TIME =>  22:07:42.59249 - Execution Finished. Total Elapsed Time: 2,812.913 ms

            SamplesValidated = samplesValidated;
        }
    }
}
