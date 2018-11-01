using System;
using System.IO;
using System.Threading;
using SoundIOSharp;

/*
 * 実装に当たってはexternal/libsoundio-sharp/example/sio-sine/Program.csを全面的に参考し、
 * 一部そのまま利用しています。
 *
 * 元のコードのライセンスを以下に張っておきます。
 *
 *  The MIT License
 *
 *   Copyright (c) 2017 Atsushi Eno
 *
 *   Permission is hereby granted, free of charge, to any person obtaining a copy
 *   of this software and associated documentation files (the "Software"), to deal
 *   in the Software without restriction, including without limitation the rights
 *   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *   copies of the Software, and to permit persons to whom the Software is
 *   furnished to do so, subject to the following conditions:
 *
 *   The above copyright notice and this permission notice shall be included in
 *   all copies or substantial portions of the Software.
 *
 *   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *   THE SOFTWARE.
 */

namespace MyShogiSoundPlayer.Sound
{
    public class Player
    {
        /// <summary>
        /// サウンドを再生する。
        /// </summary>
        public void Play(WaveFile file, Func<bool> callback)
        {
            var api = new SoundIO();
            #if LINUX
            api.ConnectBackend(SoundIOBackend.PulseAudio);
            #else
            api.Connect();
            #endif
            api.FlushEvents();
            var device = api.GetOutputDevice(api.DefaultOutputDeviceIndex);
            if (device == null || device.ProbeError != 0)
            {
                throw new IOException("device not found");
            }

            var outStream = device.CreateOutStream();
            if (outStream == null)
            {
                throw new IOException("failed to create out stream");
            }
            outStream.SoftwareLatency = 0.0;
            outStream.SampleRate = (int) file.SamplingRate;
            outStream.Layout = SoundIOChannelLayout.GetDefault(1);
            outStream.Format = SoundIODevice.Float32NE;

            var count = 0;
            var finish = false;
            outStream.WriteCallback = (_, max) => WriteCallback(
                outStream, max, ref count, ref finish, file.WaveData, file.NumChannels);
            #if LINUX
            outStream.UnderflowCallback = () => UnderflowCallback(out finish);
            #endif
            outStream.Open();
            outStream.Start();

            for (; ;)
            {
                api.FlushEvents();
                Thread.Sleep(1);
                if (finish)
                {
                    break;
                }
            }
            callback();

            outStream.Dispose();
            device.RemoveReference();
            api.Dispose();
        }

        #if LINUX
        private static void UnderflowCallback(out bool finish)
        {
            finish = true;
        }
        #endif

        private static unsafe void WriteCallback(
            SoundIOOutStream outStream,
            int frameCountMax,
            ref int count, ref bool finish, short[] data, int numChannels)
        {
            if (count >= data.Length || frameCountMax == 0)
            {
                #if MACOS
                finish = true;
                #endif
                return;
            }

            var frameCount = frameCountMax;
            var results = outStream.BeginWrite(ref frameCount);

            SoundIOChannelLayout layout = outStream.Layout;

            for (var frame = 0; frame < frameCount; frame++)
            {
                for (var channel = 0; channel < layout.ChannelCount; channel++)
                {
                    float sample;
                    if (count >= data.Length)
                    {
                        sample = 0.0f;
                    }
                    else
                    {
                        sample = (float)data[count] / short.MaxValue;
                    }

                    var area = results.GetArea(channel);

                    var buf = (float*) area.Pointer;
                    *buf = sample;

                    area.Pointer += area.Step;

                    if (numChannels == 1 && channel + 1 == layout.ChannelCount)
                    {
                        count++;
                    } else if (numChannels > 1 && layout.ChannelCount == 1)
                    {
                        count += numChannels;
                    } else if (numChannels > 1)
                    {
                        count++;
                    }
                }
            }

            outStream.EndWrite();
        }

        public void Debug()
        {
            var api = new SoundIO();
            #if LINUX
            api.ConnectBackend(SoundIOBackend.PulseAudio);
            #else
            api.Connect();
            #endif
            api.FlushEvents();

            System.Console.Error.WriteLine("# of output device: {0}", api.OutputDeviceCount);
            for (var i = 0; i < api.OutputDeviceCount; i++)
            {
                System.Console.Error.WriteLine("=== Handling Device {0} ===", i);
                var device = api.GetOutputDevice(i);
                if (device == null || device.ProbeError != 0)
                {
                    System.Console.Error.WriteLine("Failed to get device {0}", i);
                    continue;
                }

                System.Console.Error.WriteLine("Device Name: {0}", device.Name);

                System.Console.Error.WriteLine("Current Format: {0}", device.CurrentFormat);
                System.Console.Error.WriteLine("Detecting Device Supporting Format:");
                if (device.SupportsFormat (SoundIODevice.Float32NE)) {
                    System.Console.Error.WriteLine("Float32NE");
                } else if (device.SupportsFormat (SoundIODevice.Float64NE)) {
                    System.Console.Error.WriteLine("Float64NE");
                } else if (device.SupportsFormat (SoundIODevice.S32NE)) {
                    System.Console.Error.WriteLine("S32NE");
                } else if (device.SupportsFormat (SoundIODevice.S16NE)) {
                    System.Console.Error.WriteLine("S16NE");
                } else {
                    System.Console.Error.WriteLine("UnknownFormat");
                }

                System.Console.Error.WriteLine("OtherFormat:");
                foreach (var f in device.Formats)
                {
                    System.Console.Error.WriteLine(f);
                }

                System.Console.Error.WriteLine("Current Layout: {0}", device.CurrentLayout.Name);
                System.Console.Error.WriteLine("OtherLayout:");
                foreach (var l in device.Layouts)
                {
                    System.Console.Error.WriteLine(l.Name);
                }

                System.Console.Error.WriteLine("Mono is Null: {0}", SoundIOChannelLayout.GetDefault(1).IsNull);
                System.Console.Error.WriteLine("Stereo is Null: {0}", SoundIOChannelLayout.GetDefault(2).IsNull);
                System.Console.Error.WriteLine("Supports Sample Rate 44100: {0}", device.SupportsSampleRate(44100));

                device.RemoveReference();
            }

            System.Console.Error.WriteLine("Default OutputDevice: {0}", api.DefaultOutputDeviceIndex);

            api.Dispose();
            System.Console.Error.WriteLine("");
        }
    }
}
