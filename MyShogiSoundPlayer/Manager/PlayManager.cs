using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyShogiSoundPlayer.Sound;

namespace MyShogiSoundPlayer.Manager
{
    public class PlayManager: IDisposable
    {
        public PlayManager()
        {
            _playing = new Dictionary<string, DateTime>();
            _lockObjectKoma = new object();
            _lockObjectYomi = new object();
        }

        public void Dispose()
        {
            _player?.Dispose();
            _komaPlayer?.Dispose();
        }

        public void Play(WaveFile file, string playId)
        {
            var now = DateTime.Now;
            var timeout = now.AddMilliseconds(file.SoundMiliSec + 100);
            var isKoma = file.Path.Contains("koma");
            Task task;
            lock (_playing)
            {
                _playing.Add(playId, timeout);
            }

            if (isKoma)
            {
                task = new Task(() => PlayAsyncKoma(file, playId));
                lock (_lockObjectKoma)
                {
                    if (_komaPlayer == null)
                    {
                        _komaPlayer = new Player();
                    }
                    task.Start();
                }
            }
            else
            {
                task = new Task(() => PlayAsyncYomi(file, playId));
                lock (_lockObjectYomi)
                {
                    if (_player == null)
                    {
                        _player = new Player();
                    }
                    task.Start();
                }
            }
        }

        private void PlayAsyncKoma(WaveFile file, string playId)
        {
            lock (_lockObjectKoma)
            {
                _komaPlayer.PlayKoma(file);
            }

            lock (_playing)
            {
                _playing.Remove(playId);
            }
        }

        private void PlayAsyncYomi(WaveFile file, string playId)
        {
            lock (_lockObjectYomi)
            {
                _player.Play(file);
            }

            lock (_playing)
            {
                _playing.Remove(playId);
            }
        }

        public bool IsPlaying(string playId)
        {
            lock (_playing)
            {
                var playing = _playing.ContainsKey(playId);
                return playing;
            }
        }


        public void Debug()
        {
            _player.Debug();
        }

        private Player _player;
        private Player _komaPlayer;
        private Dictionary<string, DateTime> _playing;
        private object _lockObjectKoma;
        private object _lockObjectYomi;
    }
}