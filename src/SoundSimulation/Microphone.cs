using System;
using System.Collections.Generic;
using System.Drawing;

namespace SoundSimulation
{
    class Microphone
    {
        private readonly MeshNode _listeningNode;
        private readonly Vector2 _initialNodePosition;

        private readonly List<double> _amplitudes;

        private Brush _brush;

        public Microphone(SoundMesh soundMesh, int row, int col)
        {
            _listeningNode = soundMesh.GetNode(row, col);

            _initialNodePosition = new Vector2(_listeningNode.Position.X, _listeningNode.Position.Y);

            _amplitudes = new List<double>();

            _brush = new SolidBrush(Color.Blue);
        }

        public void Update(double elapsedTime)
        {
            double difference = _listeningNode.Position.X - _initialNodePosition.X;

            _amplitudes.Add(difference);
        }

        public void Draw(Graphics graphics)
        {
            graphics.FillRectangle(_brush, _listeningNode.GetBounds());
        }

        public void SaveRecording(string outputPath, uint sampleRate)
        {
            double maxAmplitude = 0;

            // Find the max amplitude
            foreach (double amplitude in _amplitudes)
            {
                if (Math.Abs(amplitude) > maxAmplitude)
                {
                    maxAmplitude = Math.Abs(amplitude);
                }
            }

            // Normalize all amplitudes and generate samples
            var normalizedAmplitudes = new List<float>();

            foreach (double amplitude in _amplitudes)
            {
                float normalizedAmplitude = (float)(amplitude / maxAmplitude);

                normalizedAmplitudes.Add(normalizedAmplitude);
            }

            var waveFile = new WavFile
            {
                SampleRate = sampleRate,
                ChannelCount = 1,
                BitsPerSample = 32,
                Samples = normalizedAmplitudes.ToArray()
            };

            waveFile.Save(outputPath);
        }
    }
}
