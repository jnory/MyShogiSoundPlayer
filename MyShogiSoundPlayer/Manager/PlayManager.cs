using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Threading.Tasks;
using MyShogiSoundPlayer.Sound;

namespace MyShogiSoundPlayer.Manager
{
    public class PlayManager
    {
        public PlayManager()
        {
            _player = new Player();
            _playing = new Dictionary<string, DateTime>();
        }

        public void Play(WaveFile file, string playId)
        {
            var now = DateTime.Now;
            var timeout = now.AddMilliseconds(file.SoundMiliSec + 100);
            lock (_playing)
            {
                _playing.Add(playId, timeout);
            }

            Task task = new Task(() => PlayAsync(file, playId));
            task.Start();
        }

        public bool IsPlaying(string playId)
        {
            var now = DateTime.Now;
            lock (_playing)
            {
                var playing = _playing.ContainsKey(playId);
                if (playing)
                {
                    var timeout = _playing[playId];
                    if (timeout < now)
                    {
                        Console.Error.WriteLine("timeout={0}", playId);
                        _playing.Remove(playId);
                    }
                }
                return playing;
            }
        }

        private void PlayAsync(WaveFile file, string playId)
        {
            _player.Play(file, () =>
            {
                lock (_playing)
                {
                    _playing.Remove(playId);
                }

                return true;
            });
        }

        public void Debug()
        {
            _player.Debug();
        }

        private Player _player;
        private Dictionary<string, DateTime> _playing;
    }
}