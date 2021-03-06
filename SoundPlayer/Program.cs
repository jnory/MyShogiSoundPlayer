﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SoundPlayer.Command;
using SoundPlayer.Manager;

namespace SoundPlayer
{
    internal class Program
    {

        private static string ParseArgs(string[] args, out bool debug, out bool aggressive)
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

        private static Logger GetLogger()
        {
            var path = Environment.GetEnvironmentVariable("MYSHOGI_SOUNDPLAYER_LOGPATH");
            return new Logger(path);
        }

        public static void Main(string[] args)
        {
            var dirName = ParseArgs(args, out var debug, out var aggressive);
            if (dirName == "")
            {
                Console.Error.WriteLine("Usage: SoundPlayer.exe [sound dir]");
                Environment.Exit(1);
            }

            var fileManager = new FileManager(dirName);
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
                foreach (var path in filePaths)
                {
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
        }

        private static void Listen(FileManager manager)
        {
            var logger = GetLogger();

            var playManager = new PlayManager();
            if (!playManager.CheckCompatibility())
            {
                Console.Error.WriteLine("Unsupported Device");
                return;
            }

            var line = Console.ReadLine();
            while (line != null)
            {
                logger.Log(line);

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
                            logger.Log("yes");
                            Console.WriteLine("yes");
                        }
                        else
                        {
                            logger.Log("no");
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

            logger.Dispose();
        }
    }
}
