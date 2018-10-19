using System.Collections.Generic;
using System.Threading.Tasks;
using MyShogiSoundPlayer.Sound;

namespace MyShogiSoundPlayer.Manager
{
    public class PlayManager
    {
        public PlayManager()
        {
            _player = new Player();
            _playing = new HashSet<string>();
        }

        public void Play(WaveFile file, string playId)
        {
            Task task = new Task(() => PlayAsync(file, playId));
            task.Start();
        }

        public bool IsPlaying(string playId)
        {
            lock (_playing)
            {
                return _playing.Contains(playId);
            }
        }

        private void PlayAsync(WaveFile file, string playId)
        {
            lock (_playing)
            {
                _playing.Add(playId);
            }

            _player.Play(file);

            lock (_playing)
            {
                _playing.Remove(playId);
            }
        }

        private Player _player;
        private HashSet<string> _playing;
    }
}