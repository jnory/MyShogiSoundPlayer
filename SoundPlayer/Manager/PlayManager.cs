using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SoundPlayer.Sound;

namespace SoundPlayer.Manager
{
    public class PlayManager: IDisposable
    {
        [DllImport("wplay")]
        static extern unsafe int playSound(
            short * waveData, uint nData,
            uint samplingRate, ushort numChannels, uint soundMiliSec);

        public PlayManager()
        {
            Type t = typeof(PlayManager);
            Marshal.PrelinkAll(t);
            _playing = new Dictionary<string, DateTime>();
        }

        public void Dispose()
        {
        }

        public void Play(WaveFile file, string playId)
        {
            var now = DateTime.Now;
            var timeout = now.AddMilliseconds(file.SoundMiliSec + 100);
            lock (_playing)
            {
                _playing.Add(playId, timeout);
            }

            Task task = new Task(()=>PlayAsync(file, playId));
            task.Start();
        }

        private unsafe void PlayAsync(WaveFile file, string playId)
        {
            fixed (short * waveData = file.WaveData)
            {
                playSound(
                    waveData, (uint) file.WaveData.Length, file.SamplingRate, file.NumChannels, file.SoundMiliSec);
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
        }

        private Dictionary<string, DateTime> _playing;
    }
}