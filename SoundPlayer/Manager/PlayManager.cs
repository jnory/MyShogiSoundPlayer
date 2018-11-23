using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SoundPlayer.Sound;

namespace SoundPlayer.Manager
{
    public class PlayManager
    {
        [DllImport("wplay")]
        static extern unsafe int playSound(
            short * waveData, uint nData,
            uint samplingRate, ushort numChannels, uint soundMiliSec);

        [DllImport("wplay")]
        static extern void printDebugInfo();

        [DllImport("wplay")]
        static extern bool checkCompatibility();

        public PlayManager()
        {
            Type t = typeof(PlayManager);
            Marshal.PrelinkAll(t);
            _playing = new Dictionary<string, bool>();
        }

        public bool CheckCompatibility()
        {
            return checkCompatibility();
        }

        public void Play(WaveFile file, string playId)
        {
            lock (_playing)
            {
                try
                {
                    _playing.Add(playId, true);
                }
                catch (ArgumentException e)
                {
                    return;
                }
            }

            Task task = new Task(()=>PlayAsync(file, playId));
            task.Start();
        }

        private unsafe void PlayAsync(WaveFile file, string playId)
        {
            fixed (short *waveData = file.WaveData)
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
            printDebugInfo();
        }

        private Dictionary<string, bool> _playing;
    }
}