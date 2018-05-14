using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SoundSimulation
{
    class WavFile
    {
        public double Length { get { return (double)Samples.Length / SampleRate; } }
        public uint SampleRate { get; set; }
        public ushort BitsPerSample { get; set; }

        public ushort ChannelCount { get; set; }

        public float[] Samples { get; set; }

        public WavFile() { }

        public WavFile(string path)
        {
            var wavFileBytes = File.ReadAllBytes(path);

            using (var ms = new MemoryStream(wavFileBytes))
            using (var wr = new BinaryReader(ms))
            {
                wr.ReadBytes(4); // RIFF header
                wr.ReadInt32(); // File size
                wr.ReadBytes(8); // Format header "WAVEfmt "
                wr.ReadInt32(); // Length of format chunk (always 32 bit value '16')
                wr.ReadUInt16(); // Audio format

                // Get the # of audio channels
                ChannelCount = wr.ReadUInt16();

                // Sample rate in hz
                SampleRate = wr.ReadUInt32();

                wr.ReadInt32(); // Bytes per second
                wr.ReadUInt16(); // Block alignment

                // Bytes per sample
                BitsPerSample = wr.ReadUInt16();

                wr.ReadBytes(4); // Data header
                wr.ReadBytes(4); // Subchuck size in bytes = numsamples * numchannels * bits/sample / 8

                int sampleCount = (wavFileBytes.Length - 44) / (BitsPerSample / 8);

                Samples = new float[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                {
                    Samples[i] = wr.ReadSingle();
                }
            }
        }

        public void Save(string path)
        {
            using (var wavStream = new FileStream(path, FileMode.Create))
            using (var wr = new BinaryWriter(wavStream))
            {
                // RIFF header
                wr.Write(Encoding.ASCII.GetBytes("RIFF"));

                // File size and format WAVE header
                wr.Write(36 + Samples.Length * BitsPerSample / 8);
                wr.Write(Encoding.ASCII.GetBytes("WAVEfmt "));

                // PCM format chunk (always 16 and 1)
                wr.Write(16);
                wr.Write((ushort)3);

                // Number of channels and sample rate
                wr.Write(ChannelCount);
                wr.Write(SampleRate);
                wr.Write(SampleRate * BitsPerSample);
                wr.Write((ushort)(ChannelCount * BitsPerSample / 8));

                // Bits per sample
                wr.Write(BitsPerSample);

                // Data section
                wr.Write(Encoding.ASCII.GetBytes("data"));
                wr.Write(ChannelCount * Samples.Length * BitsPerSample / 8);

                foreach (float sample in Samples)
                {
                    wr.Write(sample);
                }
            }
        }
    }
}
