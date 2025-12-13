using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordVaultUSB.Services
{
    public static class WebcamService
    {
        private static FilterInfoCollection _videoDevices;
        private static VideoCaptureDevice _videoSource;
        private static Bitmap _lastFrame;
        private static readonly object _lockObj = new object();

        public static void CaptureIntruder(string saveDirectory)
        {
            Task.Run(() =>
            {
                try
                {
                    _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                    if (_videoDevices.Count == 0) return;

                    // Логіка вибору камери (ігноруємо OBS/Virtual)
                    FilterInfo preferredDevice = null;

                    foreach (FilterInfo device in _videoDevices)
                    {
                        if (!device.Name.ToUpper().Contains("OBS") &&
                            !device.Name.ToUpper().Contains("VIRTUAL"))
                        {
                            preferredDevice = device;
                            break;
                        }
                    }

                    if (preferredDevice == null)
                    {
                        preferredDevice = _videoDevices[0];
                    }

                    _videoSource = new VideoCaptureDevice(preferredDevice.MonikerString);
                    _videoSource.NewFrame += VideoSource_NewFrame;
                    _videoSource.Start();

                    // Чекаємо кадр (5 секунд)
                    int attempts = 0;
                    while (_lastFrame == null && attempts < 50)
                    {
                        Thread.Sleep(100);
                        attempts++;
                    }

                    if (_lastFrame != null)
                    {
                        string fileName = $"INTRUDER_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.jpg";

                        // Створюємо приховану папку Intruders
                        string intruderDir = Path.Combine(saveDirectory, "Intruders");
                        if (!Directory.Exists(intruderDir))
                        {
                            Directory.CreateDirectory(intruderDir);
                            var dirInfo = new DirectoryInfo(intruderDir);
                            dirInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                        }

                        string finalPath = Path.Combine(intruderDir, fileName);

                        lock (_lockObj)
                        {
                            _lastFrame.Save(finalPath, ImageFormat.Jpeg);
                        }
                    }

                    StopCamera();
                }
                catch
                {
                    // Тихо гасимо будь-які помилки, щоб не видати себе
                    StopCamera();
                }
            });
        }

        private static void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            lock (_lockObj)
            {
                _lastFrame?.Dispose();
                _lastFrame = (Bitmap)eventArgs.Frame.Clone();
            }
        }

        private static void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.WaitForStop();
                _videoSource = null;
            }
            _lastFrame?.Dispose();
            _lastFrame = null;
        }
    }
}