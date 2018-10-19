using System;
using System.Collections.Generic;
using System.Linq;

namespace MyShogiSoundPlayer.Command
{
    public class CommandParser
    {
        public static Command Parse(string commandStr)
        {
            var tokens = Tokenize(commandStr);
            if (tokens.Length == 1 && tokens[0] == "exit")
            {
                return new Command
                {
                    Type = CommandType.Exit,
                };
            }
            if (tokens.Length == 2)
            {
                switch (tokens[0])
                {
                    case "is_playing":
                        return new Command
                        {
                            Type = CommandType.IsPlaying,
                            Args = new []{tokens[1]},
                        };
                    case "release":
                        return new Command
                        {
                            Type = CommandType.Release,
                            Args = new []{tokens[1]},
                        };
                }
            }
            if (tokens.Length == 3 && tokens[0] == "play")
            {
                return new Command
                {
                    Type = CommandType.Play,
                    Args = new []{tokens[1], tokens[2]},
                };
            }

            return new Command
            {
                Type = CommandType.Unknown,
            };
        }

        private static string[] Tokenize(string commandStr)
        {
            commandStr = commandStr.Trim();

            var rawTokens = commandStr.Split(" ".ToCharArray());
            return rawTokens.Where(token => token != "").ToArray();
        }

    }
}