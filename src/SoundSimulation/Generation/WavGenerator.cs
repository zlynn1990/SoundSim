using System;

namespace SoundSimulation.Generation
{
    class WavGenerator : ISoundGenerator
    {
        private WavFile _wavFile;

        private float _peakAmplitude;

        public WavGenerator(WavFile wavFile)
        {
            _wavFile = wavFile;

            for (var i = 0; i < _wavFile.Samples.Length; i++)
            {
                float sample = _wavFile.Samples[i];

                float absoluteAmplitude = Math.Abs(sample);

                if (absoluteAmplitude < 10 && absoluteAmplitude > _peakAmplitude)
                {
                    _peakAmplitude = Math.Abs(sample);
                }
            }
        }

        public double GetAmplitude(double time)
        {
            int sampleIndex = (int)(_wavFile.SampleRate * time);

            if (sampleIndex >= _wavFile.Samples.Length)
            {
                return 0;
            }

            float sample = _wavFile.Samples[sampleIndex] / _peakAmplitude;

            if (sample < -1 || sample > 1)
            {
                return 0;
            }

            return sample;
        }
    }
}
