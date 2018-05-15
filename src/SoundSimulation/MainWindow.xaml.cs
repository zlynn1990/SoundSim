using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SoundSimulation.Generation;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace SoundSimulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string InputSample = "jude.wav";

        private const int ScreenWidth = 1280;
        private const int ScreenHeight = 720;

        private const int MeshRows = 100;
        private const int MeshCols = 100;

        private const uint SampleRate = 44100;

        private Bitmap _imageBitmap;
        private WriteableBitmap _backBuffer;

        private bool _isActive;
        private Thread _updateThread;

        private WavFile _inputFile;

        private SoundMesh _soundMesh;

        private Speaker _speaker;
        private Microphone _microphone;

        public MainWindow()
        {
            _inputFile = new WavFile($"../../../../Samples/{InputSample}");

            InitializeComponent();

            _soundMesh = new SoundMesh(MeshRows, MeshCols, 6);

            _speaker = new Speaker(new FrequencyGenerator(220), _soundMesh, 1);
            //_speaker = new Speaker(new WavGenerator(_inputFile), _soundMesh, 1);

            _soundMesh.AddSpeaker(_speaker);

            _microphone = new Microphone(_soundMesh, MeshRows / 2, MeshCols / 4);
            _soundMesh.AddMicrophone(_microphone);

            _imageBitmap = new Bitmap(ScreenWidth, ScreenHeight, PixelFormat.Format32bppArgb);
            _backBuffer = new WriteableBitmap(ScreenWidth, ScreenHeight, 96, 96, PixelFormats.Bgra32, null);

            BackBuffer.Source = _backBuffer;

            _isActive = true;
            _updateThread = new Thread(GameLoop);
            _updateThread.Start();
        }

        private void GameLoop()
        {
            var frameTimer = new FpsManager(60);

            var font = new Font("Verdana Bold", 14);
            var textBrush = new SolidBrush(Color.White);

            _soundMesh.Simulate(SampleRate);

            while (_isActive && _soundMesh.ElapsedSimulationTime < _inputFile.Length)
            {
                frameTimer.StartFrame();

                using (Graphics graphics = Graphics.FromImage(_imageBitmap))
                {
                    graphics.Clear(Color.Black);

                    _soundMesh.Draw(graphics);
                    _speaker.Draw(graphics);
                    _microphone.Draw(graphics);

                    graphics.DrawString($"FPS: {frameTimer.CurrentFps}", font, textBrush, 0, 0);
                    graphics.DrawString($"Elapsed Time: {Math.Round(_soundMesh.ElapsedSimulationTime, 5)}", font, textBrush, 0, 30);
                }

                WriteFrameToScreen();

                frameTimer.FinishFrame();
            }

            _soundMesh.Stop();

            _microphone.SaveRecording("output.wav", SampleRate);
        }

        private void WriteFrameToScreen()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                var rect = new Rectangle(0, 0, _imageBitmap.Width, _imageBitmap.Height);
                BitmapData bmpData = _imageBitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                var source = new Int32Rect(0, 0, ScreenWidth, ScreenHeight);

                _backBuffer.WritePixels(source, bmpData.Scan0, ScreenWidth * ScreenHeight * 4, ScreenWidth * 4);

                _imageBitmap.UnlockBits(bmpData);


            }), DispatcherPriority.Render, null);
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            _isActive = false;
            _updateThread.Join(1000);
        }
    }
}
