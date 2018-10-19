using System;
using MyShogiSoundPlayer.Command;
using MyShogiSoundPlayer.Manager;
using MyShogiSoundPlayer.Sound;

namespace MyShogiSoundPlayer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: SoundPlayer.exe [sound dir]");
                Environment.Exit(1);
            }

            var fileManager = new FileManager(args[0]);
            Listen(fileManager);
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
