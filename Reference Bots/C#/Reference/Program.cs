<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
                    playerKey = 'C';
                    outputLocation = Path.Combine(@"C:\Kobus\Werk\Entelect\Game Engine v1.1.0\Game Engine\Bomberman\bin\Debug\Replays\812046530\178\", playerKey.ToString());
                    var jsonFileLocation = @"C:\Kobus\Werk\Entelect\Game Engine v1.1.0\Game Engine\Bomberman\bin\Debug\Replays\812046530\179\state.json";
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
                //if (playerKey == 'A')
                //{
                    var gameStrategy = new MdpStrategy();
                    command = gameStrategy.ExecuteStrategy(map, playerKey);
                //}
                //else
                //{
                //    var gameStrategy = new AStarStrategy();
                //    command = gameStrategy.ExecuteStrategy(map, playerKey);
                //}


                Console.WriteLine("Sending Back command " + command);
                File.WriteAllText(Path.Combine(outputLocation, "move.txt"), ((int)command).ToString());
                stopwatch.Stop();
                Debug.WriteLine("[BOT]\tBot finished in {0} ms.", stopwatch.ElapsedMilliseconds);
                Debug.WriteLine("[BOT]\tBot moved " + command.ToString());
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
=======
﻿using System;
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
                    outputLocation = Path.Combine(@"C:\Users\hennie.brink\Desktop\Bomberman\Game\", playerKey.ToString());
                    inputMap =
                        File.ReadAllText(@"C:\Users\hennie.brink\Desktop\Bomberman\Replays\259502815\10\A\state.json");
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
>>>>>>> 109822ff23a14212bfc6b41a11aec0ed2b9fb453
