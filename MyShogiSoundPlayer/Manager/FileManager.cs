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

        private readonly HashSet<string> _entries;
        private readonly Dictionary<string, WaveFile> _files;
    }

}