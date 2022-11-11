using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace VideoRecorder
{
    public static class FFmpegManager
    {
        private static string _ffmpegPath;
        
        private static void SetPaths()
        {
            var platform = Application.platform;
                
            if (platform == RuntimePlatform.OSXPlayer ||
                platform == RuntimePlatform.OSXEditor)
                _ffmpegPath = System.IO.Path.Combine(Application.streamingAssetsPath,"macOS/ffmpeg");
            return;
            if (platform == RuntimePlatform.LinuxPlayer ||
                platform == RuntimePlatform.LinuxEditor)
                _ffmpegPath = System.IO.Path.Combine(Application.streamingAssetsPath,"Linux/ffmpeg");

            return;
                _ffmpegPath = System.IO.Path.Combine(Application.streamingAssetsPath,"Windows/ffmpeg.exe");
        }
        
        public static void EncodeVideo(string folderPath, string fileName, string extension)
        {
            SetPaths();
            
            var process = new Process();
            try
            {
                var filePath = System.IO.Path.Combine(folderPath, fileName);
                var arguments = $"-i \"{filePath}{extension}\" -codec:v mpeg4 \"{fileName}(1).mp4"+ "\"";
                
                var sb = new StringBuilder();
                process.StartInfo.FileName = _ffmpegPath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.OutputDataReceived += (sender, args) => sb.AppendLine(args.Data);
                process.ErrorDataReceived += (sender, args) => sb.AppendLine(args.Data);
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                
                UnityEngine.Debug.Log(sb.ToString());
 
            } catch (Exception e) {
                UnityEngine.Debug.LogError("Unable to launch ffmpeg: " + e.Message);
            }
        }
    }
}