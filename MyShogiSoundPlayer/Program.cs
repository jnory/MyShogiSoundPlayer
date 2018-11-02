using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MyShogiSoundPlayer.Command;
using MyShogiSoundPlayer.Manager;

namespace MyShogiSoundPlayer
{
    internal class Program
    {

        private static string parseArgs(string[] args, out bool debug, out bool aggressive)
        {
            HashSet<string> argSet = new HashSet<string>();
            foreach (var arg in args)
            {
                argSet.Add(arg);
            }

            debug = argSet.Contains("--debug");
            debug = debug || argSet.Contains("--debug-aggressive");
            aggressive = argSet.Contains("--debug-aggressive");

            argSet.Remove("--debug");
            argSet.Remove("--debug-aggressive");

            if (argSet.Count != 1)
            {
                return "";
            }

            var dirName = argSet.ToArray()[0];
            return dirName;
        }

        public static void Main(string[] args)
        {
            FileManager fileManager;
            bool debug;
            bool aggressive;
            var dirName = parseArgs(args, out debug, out aggressive);
            if (dirName == "")
            {
                Console.WriteLine("Usage: SoundPlayer.exe [sound dir]");
                Environment.Exit(1);
            }

            fileManager = new FileManager(dirName);
            if (debug)
            {
                StartDebugMode(fileManager, aggressive);
            }
            else
            {
                Listen(fileManager);
            }
        }

        private static void StartDebugMode(FileManager fileManager, bool aggressive)
        {
            fileManager.Debug();

            var playManager = new PlayManager();
            playManager.Debug();

            if (aggressive)
            {
                var filePaths = fileManager.GetFilePaths();
                var i = 0;
                var komas = new List<string>();
                foreach (var path in filePaths)
                {
                    if (path.Contains("koma"))
                    {
                        komas.Add(path);
                    }

                    Console.Error.WriteLine("Playing {0}", path);
                    var file = fileManager.Load(path);
                    if (file != null)
                    {
                        i++;
                        playManager.Play(file, i.ToString());
                    }
                    while (playManager.IsPlaying(i.ToString()))
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            else
            {
                var example = fileManager.GetExampleFile();
                if (example != "")
                {
                    var file = fileManager.Load(example);
                    Console.Error.WriteLine("Start Playing {0}", file.Path);
                    playManager.Play(file, "1");
                    Console.Error.WriteLine("Done Playing");
                }
            }

            Thread.Sleep(1000);
            playManager.Dispose();
        }

        private static void Listen(FileManager manager)
        {
            var playManager = new PlayManager();

            var line = Console.ReadLine();
            while (line != null)
            {
                var command = CommandParser.Parse(line);
                if (command.Type == CommandType.Exit)
                {
                    break;
                }

                switch (command.Type)
                {
                    case CommandType.Play:
                        var file = manager.Load(command.Args[1]);
                        if (file != null)
                        {
                            playManager.Play(file, command.Args[0]);
                        }

                        break;
                    case CommandType.IsPlaying:
                        if (playManager.IsPlaying(command.Args[0]))
                        {
                            Console.WriteLine("yes");
                        }
                        else
                        {
                            Console.WriteLine("no");
                        }

                        break;
                    case CommandType.Release:
                        manager.Release(command.Args[0]);
                        break;
                }

                line = Console.ReadLine();
                Thread.Sleep(10);
            }
            playManager.Dispose();
        }
    }
}
