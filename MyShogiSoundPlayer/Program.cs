using System;
using System.Threading;
using MyShogiSoundPlayer.Command;
using MyShogiSoundPlayer.Manager;

namespace MyShogiSoundPlayer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1 && args.Length != 2)
            {
                Console.WriteLine("Usage: SoundPlayer.exe [sound dir]");
                Environment.Exit(1);
            }

            FileManager fileManager;
            bool debug = false;
            if (args[0] == "--debug")
            {
                fileManager = new FileManager(args[1]);
                debug = true;
            }
            else
            {
                fileManager = new FileManager(args[0]);
                if (args.Length == 2 && args[1] == "--debug")
                {
                    debug = true;
                }
            }

            if (debug)
            {
                StartDebugMode(fileManager);
            }
            else
            {
                Listen(fileManager);
            }
        }

        private static void StartDebugMode(FileManager fileManager)
        {
            fileManager.Debug();

            var example = fileManager.GetExampleFile();
            var playManager = new PlayManager();
            if (example != "")
            {
                var file = fileManager.Load(example);
                if (file != null)
                {
                    Console.Error.WriteLine("Start Playing {0}", file.Path);
                    playManager.Play(file, "1");
                    Console.Error.WriteLine("Done Playing");
                }
            }

            playManager.Debug();
            Thread.Sleep(1000);
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
            }
        }
    }
}
