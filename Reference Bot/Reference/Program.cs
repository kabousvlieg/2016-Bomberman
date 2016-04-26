using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Reference.Domain.Map;
using Reference.Strategies.AStar;

namespace Reference
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                char playerKey;
                string outputLocation;
                string inputMap;
                if (args == null || args.Length == 0)
                {
                    playerKey = 'A';
                    outputLocation = Path.Combine(@"C:\Kobus\Werk\Entelect\Game Engine v1.1.0\Reference Bot\", playerKey.ToString());
                    inputMap =
                        File.ReadAllText(@"C:\Kobus\Werk\Entelect\Game Engine v1.1.0\Game Engine\Replays\32735501\0\state.json");
                }
                else
                {
                    playerKey = Char.Parse(args[0]);
                    outputLocation = args[1];
                    inputMap = File.ReadAllText(Path.Combine(outputLocation, @"state.json"));
                }

                var map = GameMap.FromJson(inputMap);
                var gameStrategy = new AStarStrategy();

                var command = gameStrategy.ExecuteStrategy(map, playerKey);

                Console.WriteLine("Sending Back command " + command);
                File.WriteAllText(Path.Combine(outputLocation, "move.txt"), ((int)command).ToString());

                return 0;
            }
            catch (Exception ex)
            {
                Console.Write(ex);

                return -1;
            }
        }
    }
}
