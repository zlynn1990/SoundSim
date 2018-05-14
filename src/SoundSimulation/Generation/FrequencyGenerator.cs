using System;

namespace SoundSimulation.Generation
{
    class FrequencyGenerator : ISoundGenerator
    {
        private readonly int _frequency;

        public FrequencyGenerator(int frequency)
        {
            _frequency = frequency;
        }

        public double GetAmplitude(double time)
        {
            return Math.Sin(2 * Math.PI * _frequency * time);
        }
    }
}
