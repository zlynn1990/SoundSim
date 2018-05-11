using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using SoundSimulation.Properties;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace SoundSimulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int ScreenWidth = 1280;
        private const int ScreenHeight = 720;

        private Bitmap _imageBitmap;
        private WriteableBitmap _backBuffer;

        private bool _isActive;
        private Thread _updateThread;

        private SoundMesh _soundMesh;

        public MainWindow()
        {
            InitializeComponent();

            _soundMesh = new SoundMesh(60, 100, 11);

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
            var meshBrush = new SolidBrush(Color.Red);

            _soundMesh.Simulate(441000);

            while (_isActive)
            {
                frameTimer.StartFrame();

                using (Graphics graphics = Graphics.FromImage(_imageBitmap))
                {
                    graphics.Clear(Color.Black);
                    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

                    RectangleF[] meshPositions = _soundMesh.GetSnapshot();

                    graphics.FillRectangles(meshBrush, meshPositions);

                    graphics.DrawString($"FPS: {frameTimer.CurrentFps}", font, textBrush, 0, 0);
                    graphics.DrawString($"Elapsed Time: {Math.Round(_soundMesh.ElapsedSimulationTime, 5)}", font, textBrush, 0, 30);
                }

                WriteFrameToScreen();

                frameTimer.FinishFrame();
            }

            _soundMesh.Stop();
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
