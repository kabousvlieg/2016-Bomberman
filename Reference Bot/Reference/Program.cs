using System;
using System.Diagnostics;
using System.IO;
using Reference.Domain.Map;
using Reference.Strategies.AStar;
using Reference.Commands;
using Reference.Strategies.MDP;

namespace Reference
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                char playerKey;
                string outputLocation;
                string inputMap;
                if (args == null || args.Length == 0)
                {
                    playerKey = 'A';
                    outputLocation = Path.Combine(@"C:\Kobus\Werk\Entelect\Game Engine v1.1.0\Reference Bot\", playerKey.ToString());
                    var jsonFileLocation = @"C:\Kobus\Werk\Entelect\Game Engine v1.1.0\Game Engine\Replays\237885122\45\state.json";
                    if ( File.Exists(jsonFileLocation))
                    {
                        inputMap = File.ReadAllText(jsonFileLocation);
                    }
                    else
                    {
                        File.WriteAllText(Path.Combine(outputLocation, "move.txt"), ((int) GameCommand.DoNothing).ToString());
                        return 0;
                    }
                }
                else
                {
                    playerKey = Char.Parse(args[0]);
                    outputLocation = args[1];
                    inputMap = File.ReadAllText(Path.Combine(outputLocation, @"state.json"));
                }

                var map = GameMap.FromJson(inputMap);
                GameCommand command;
                if (playerKey == 'A')
                {
                    var gameStrategy = new MdpStrategy();
                    command = gameStrategy.ExecuteStrategy(map, playerKey);
                }
                else
                {
                    var gameStrategy = new AStarStrategy();
                    command = gameStrategy.ExecuteStrategy(map, playerKey);
                }
               

                Console.WriteLine("Sending Back command " + command);
                File.WriteAllText(Path.Combine(outputLocation, "move.txt"), ((int)command).ToString());
                stopwatch.Stop();
                Debug.WriteLine("[BOT]\tBot finished in {0} ms.", stopwatch.ElapsedMilliseconds);
                if (stopwatch.ElapsedMilliseconds > 2000)
                {
                    //System.Windows.Forms.MessageBox.Show("Overran time limit " + stopwatch.ElapsedMilliseconds.ToString() + "ms");
                }
                //Console.ReadKey();
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
