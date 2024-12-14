using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevSample
{
    class Sample
    {
        public Sample(bool isFirstSample)
        {
            IsFirstSample = isFirstSample;
        }
        public bool IsFirstSample
        {
            get;
            private set;
        }
        public DateTime Timestamp
        {
            get;
            private set;
        }
        public long Value
        {
            get;
            private set;
        }
        public bool HasBeenValidated
        {
            get;
            private set;
        }
        public void LoadSampleAtTime(DateTime timestamp)
        {
            // This function simulates a CPU-intensive operation by performing an unnecessary loop.
            // The purpose is to simulate the loading of a sample without actually optimizing the CPU time.
            Timestamp = timestamp;
            Value = timestamp.Ticks / 10000;
            
            // Simulate CPU workload (no real effect here)
            for (int i = 0; i < 1000; i++) ;
        }

        public bool ValidateSample(Sample previousSample, TimeSpan sampleInterval)
        {
            // Simulates CPU work during sample validation.
            // This loop does nothing but represents a CPU time cost.
            for (int i = 0; i < 5000; i++) ;
            // Checks if the previous sample is null and if this is not the first sample.
            // If it's not the first sample and there's no previous sample, an exception is thrown.
            if (previousSample == null && !IsFirstSample)
                throw new Exception("Validation Failed: Previous sample is null, but this is not the first sample.");
            // If there is a previous sample, validates that its timestamp matches
            // the expected interval between samples.
            // If not, an exception is thrown.
            else if (previousSample != null && previousSample.Timestamp != this.Timestamp - sampleInterval)
                throw new Exception($"Validation Failed: Timestamps do not match. Expected: {previousSample.Timestamp + sampleInterval}, but got: {this.Timestamp}");
            // Marks the sample as validated after passing the validation.
            HasBeenValidated = true;
            // Returns 'true' if the validation was successful.
            return true;
        }
    }
}
