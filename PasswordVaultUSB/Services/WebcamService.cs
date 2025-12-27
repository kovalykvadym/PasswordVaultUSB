using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
namespace PasswordVaultUSB.Services {
    public static class WebcamService {
        private static FilterInfoCollection _videoDevices;
        private static VideoCaptureDevice _videoSource;
        private static Bitmap _lastFrame;
        private static readonly object _lockObj = new object();
        public static void CaptureIntruder(string usbRootPath) {
            Task.Run(() => {
                try {
                    string cameraMoniker = GetBestCameraMoniker();
                    if (string.IsNullOrEmpty(cameraMoniker)) return;

                    _videoSource = new VideoCaptureDevice(cameraMoniker);
                    _videoSource.NewFrame += VideoSource_NewFrame;
                    _videoSource.Start();
                    int attempts = 0;
                    while (_lastFrame == null && attempts < 50) {
                        Thread.Sleep(100);
                        attempts++;
                    }
                    if (_lastFrame != null) {
                        SaveImageToHiddenFolder(usbRootPath);
                    }
                } finally {
                    StopCamera();
                }
            });
        }
        public static List<string> GetIntruderImages(string saveDirectory) {
            var images = new List<string>();
            try {
                string intruderDir = Path.Combine(saveDirectory, "Intruders");
                if (Directory.Exists(intruderDir)) {
                    var files = Directory.GetFiles(intruderDir, "*.jpg");
                    images.AddRange(files);
                }
            } catch { }
            images.Reverse();
            return images;
        }
        private static string GetBestCameraMoniker() {
            try {
                _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (_videoDevices.Count == 0) return null;
                foreach (FilterInfo device in _videoDevices) {
                    string name = device.Name.ToUpper();
                    if (!name.Contains("OBS") && !name.Contains("VIRTUAL")) {
                        return device.MonikerString;
                    }
                }
                return _videoDevices[0].MonikerString;
            } catch {
                return null;
            }
        }
        private static void SaveImageToHiddenFolder(string rootPath) {
            try {
                string fileName = $"INTRUDER_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.jpg";
                string intruderDir = Path.Combine(rootPath, "Intruders");
                DirectoryInfo dirInfo = Directory.CreateDirectory(intruderDir);
                try {
                    dirInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden | FileAttributes.System;
                } catch { }
                string finalPath = Path.Combine(intruderDir, fileName);
                lock (_lockObj) {
                    _lastFrame.Save(finalPath, ImageFormat.Jpeg);
                }
            } catch { }
        }
        private static void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs) {
            lock (_lockObj) {
                _lastFrame?.Dispose();
                _lastFrame = (Bitmap)eventArgs.Frame.Clone();
            }
        }
        private static void StopCamera() {
            if (_videoSource != null && _videoSource.IsRunning) {
                _videoSource.SignalToStop();
                _videoSource.WaitForStop();
                _videoSource = null;
            }
            lock (_lockObj) {
                _lastFrame?.Dispose();
                _lastFrame = null;
            }
        }
    }
}