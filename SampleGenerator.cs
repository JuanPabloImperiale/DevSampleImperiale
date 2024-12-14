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
                _sampleList.Add(s); // Add the sample to the end of the list (faster than Insert(0, s))
                date += _sampleIncrement; // Increment the date for the next sample
            }
            // Reverse the list to ensure the samples are in time-descending order
            _sampleList.Reverse();
        }

        public void ValidateSamples()
        {
            //TODO: can we validate samples faster?
            int samplesValidated = 0;
            int sampleCount = _sampleList.Count; // Cache the count for performance
            for (int i = 0; i < sampleCount; i++)
            {
                Sample currentSample = _sampleList[i]; // Cache current sample for better access
                Sample nextSample = (i < sampleCount - 1) ? _sampleList[i + 1] : null; // Cache next sample to avoid calculating in each loop iteration

                if (currentSample.ValidateSample(nextSample, _sampleIncrement)) // Validate the current sample
                    samplesValidated++;
            }
            SamplesValidated = samplesValidated;
        }
    }
}
