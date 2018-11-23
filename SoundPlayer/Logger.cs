using System;
using System.IO;

namespace SoundPlayer
{
    public class Logger : IDisposable
    {
        public Logger(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                _writer = new StreamWriter(path, true);
            }
            else
            {
                _writer = null;
            }
        }

        public void Log(string message)
        {
            _writer?.WriteLine(message);
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }

        private StreamWriter _writer;

    }
}