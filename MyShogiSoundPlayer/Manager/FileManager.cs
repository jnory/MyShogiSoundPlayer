using System.Collections.Generic;
using System.IO;
using MyShogiSoundPlayer.Sound;

namespace MyShogiSoundPlayer.Manager
{
    class FileManager
    {
        public FileManager(string path)
        {
            _entries = new HashSet<string>(
                Directory.GetFiles(path, "*.wav", SearchOption.AllDirectories));
            _files = new Dictionary<string, WaveFile>();
            _path = path;
        }

        public WaveFile Load(string path)
        {
            if (!_entries.Contains(path))
            {
                return null;
            }

            try
            {
                return _files[path];
            }
            catch (KeyNotFoundException)
            {
                var file = new WaveFile(path);
                _files[path] = file;
                return file;
            }
        }

        public void Release(string path)
        {
            if (_files.ContainsKey(path))
            {
                _files.Remove(path);
            }
        }

        public void Debug()
        {
            System.Console.Error.WriteLine("Audio Directory Path: {0}", _path);
            System.Console.Error.WriteLine("Recognized Files:");
            foreach (var name in _entries)
            {
                System.Console.Error.WriteLine(name);
            }
            System.Console.Error.WriteLine();
        }

        public string GetExampleFile()
        {
            foreach (var name in _entries)
            {
                return name;
            }

            return "";
        }

        public string[] GetFilePaths()
        {
            string[] ret = new string[_entries.Count];
            _entries.CopyTo(ret);
            return ret;
        }


        private readonly string _path;
        private readonly HashSet<string> _entries;
        private readonly Dictionary<string, WaveFile> _files;
    }

}