using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Reference.Domain.Map;
using Reference.Commands;
using Reference.Domain.Map.Entities;
using Reference.Strategies.MDP;

namespace Reference
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var round = 0;
                while (true)
                {
                    char playerKey;
                    string outputLocation;
                    string inputMap;

                    Console.Clear();
                    Console.WriteLine("Round " + round.ToString() + " Fight!!");
                    if (args == null || args.Length == 0)
                    {
                        playerKey = 'A';
                        outputLocation = Path.Combine(@"C:\Kobus\Werk\Entelect\Game Engine v1.1.0\Game Engine\Bomberman\bin\Debug\Replays\1325634904\" + round.ToString(), playerKey.ToString());
                        var jsonFileLocation = @"C:\Kobus\Werk\Entelect\Game Engine v1.1.0\Game Engine\Bomberman\bin\Debug\Replays\1325634904\" + round.ToString() + @"\state.json";
                        if (File.Exists(jsonFileLocation))
                        {
                            inputMap = File.ReadAllText(jsonFileLocation);
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(outputLocation, "move.txt"), ((int)GameCommand.DoNothing).ToString());
                            return 0;
                        }
                    }
                    else
                    {
                        //round = int.Parse(args[2]);
                        playerKey = Char.Parse(args[0]);
                        //playerKey = Char.Parse(args[0] + round.ToString(), playerKey.ToString();
                        outputLocation = args[1] + @"\" + round.ToString() + @"\" + playerKey.ToString() + @"\";
                        inputMap = File.ReadAllText(args[1] + @"\" + round.ToString() + @"\state.json");
                    }

                    var map = GameMap.FromJson(inputMap);
                    Utils utils;
                    utils = new Utils(map, playerKey);
                    utils.DrawMap();
                    PlayerEntity[] players = new PlayerEntity[4]; //Maximum of 4 players
                    players[0] = null;
                    players[1] = null;
                    players[2] = null;
                    players[3] = null;
                    utils.getPlayers(ref players);
                    foreach (var player in players)
                    {
                        if (player == null)
                            continue;
                        Console.WriteLine("----------------------------");
                        Console.WriteLine("Player Name: " + player.Name);
                        Console.WriteLine("Key: " + player.Key);
                        Console.WriteLine("Points: " + player.Points);
                        if (player.Killed)
                            Console.WriteLine("Status: Dead");
                        else
                            Console.WriteLine("Status: Alive");
                        Console.WriteLine("BombBag: " + player.BombBag);
                        Console.WriteLine("BlastRadius " + player.BombRadius);
                    }
                    
                    var keyInfo = Console.ReadKey();
                    if (keyInfo.Key == ConsoleKey.RightArrow)
                    {
                        round++;
                    }
                    if ((keyInfo.Key == ConsoleKey.LeftArrow) && (round > 0))
                    {
                        round--;
                    }
                }
                
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
