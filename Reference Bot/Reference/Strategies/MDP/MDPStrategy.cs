using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Reference.Commands;
using Reference.Domain;
using Reference.Domain.Map;
using Reference.Domain.Map.Entities;
using Reference.Domain.Map.Entities.PowerUps;

namespace Reference.Strategies.MDP
{
    public class MdpStrategy : IStrategy
    {
        private GameMap _gameMap;
        private readonly int WallValue = 10;
        private readonly int PowerUpValue = 100;
        private readonly int SuperPowerUpValue = 200;
        private readonly int PenaltyValue = -3;


        enum MdpTypes
        {
            Bomb,
            Me,
            OtherPlayer,
            PowerUp,
            SuperPowerUp,
            Indestructable,
            Wall,
            Path
        }

        struct MdpBlock
        {
            public MdpTypes type;
            public int value;
            public int validValue;
        }

        public GameCommand ExecuteStrategy(GameMap gameMap, char playerKey)
        {
            this._gameMap = gameMap;
            //var nodeMap = new NodeMap(gameMap, playerKey);
            MdpBlock[,] MdpMap = new MdpBlock[gameMap.MapHeight + 1, gameMap.MapWidth + 1];
            
            //Draw map
            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine("");
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    var block = _gameMap.GetBlockAtLocation(x, y);
                    if (block.Entity != null)
                    {
                        if (block.Entity is DestructibleWallEntity)
                        {
                            Debug.Write("+");
                            
                            MdpMap[x,y].type = MdpTypes.Wall;
                            MdpMap[x, y].value = WallValue;
                            MdpMap[x, y].value = WallValue;
                        }
                        else if (block.Entity is IndestructibleWallEntity)
                        {
                            Debug.Write("#");
                            MdpMap[x, y].type = MdpTypes.Indestructable;
                        }
                        else if (block.Entity is PlayerEntity)
                        {
                            if ((block.Entity as PlayerEntity).Key == playerKey)
                                MdpMap[x,y].type = MdpTypes.Me;
                            else
                                MdpMap[x, y].type = MdpTypes.OtherPlayer;
                            Debug.Write((block.Entity as PlayerEntity).Key);
                        }
                        else if (block.Entity is BombEntity)
                        {
                            MdpMap[x, y].type = MdpTypes.Bomb;
                            Debug.Write("?");
                        }
                        else if (block.Entity is BombBagPowerUpEntity)
                        {
                            MdpMap[x, y].type = MdpTypes.PowerUp;
                            Debug.Write("&");
                        }
                        else if (block.Entity is BombRaduisPowerUpEntity)
                        {
                            MdpMap[x, y].type = MdpTypes.PowerUp;
                            Debug.Write("&");
                        }
                        else if (block.Entity is SuperPowerUp)
                        {
                            MdpMap[x, y].type = MdpTypes.SuperPowerUp;
                            Debug.Write("&");
                        }
                        else
                        {
                            MdpMap[x, y].type = MdpTypes.Path;
                            Debug.Write(" ");
                        }
                    }
                    else Debug.Write(" ");

                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("");

            //Calculate MDP
            var stillNotDone = false;
            do
            {
                stillNotDone = false;
                for (var y = 1; y < gameMap.MapHeight; y++)
                {
                    for (var x = 1; x < gameMap.MapWidth; x++)
                    {
                        if (MdpMap[x, y].type == MdpTypes.Path)
                        {
                            //Get largest value neighbour
                            int largestNeighbour;


                            //Calculate our value
                            //If difference is still too big, mark stillNotDone
                        } 
                    }
                }
            } while (stillNotDone);

            //Draw MDP map
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    if (MdpMap[x,y].type == MdpTypes.Me)
                        Debug.Write("A");
                    else if (MdpMap[x, y].type == MdpTypes.OtherPlayer)
                        Debug.Write("B");
                    else if (MdpMap[x, y].type == MdpTypes.Indestructable)
                        Debug.Write("#");
                    else if (MdpMap[x, y].type == MdpTypes.Wall)
                        Debug.Write("+");
                    else
                        Debug.Write(((int)(MdpMap[x,y].value / 2)).ToString());  
                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("");
            return GameCommand.DoNothing;
        }

    }
}


//namespace Reference.Strategies.AStar
//{
//    public class AStarStrategy : IStrategy
//    {
//        private GameMap _gameMap;
//        public GameCommand ExecuteStrategy(GameMap gameMap, char playerKey)
//        {
//            this._gameMap = gameMap;
//            var nodeMap = new NodeMap(gameMap, playerKey);

//            return FindBestCommand(nodeMap);
//        }

//        GameCommand FindBestCommand(NodeMap nodeMap)
//        {
//            var path = FindBestPath(nodeMap);

//            for (int y = 1; y <= _gameMap.MapHeight; y++)
//            {
//                for (int x = 1; x <= _gameMap.MapWidth; x++)
//                {
//                    var block = _gameMap.GetBlockAtLocation(x, y);
//                    var node = path.FirstOrDefault(n => n.Location.X == x && n.Location.Y == y);
//                    if (node != null)
//                    {
//                        Console.Write(".");
//                    }
//                    else if (block.Entity != null)
//                    {
//                        if (block.Entity is DestructibleWallEntity) Console.Write("#");
//                        else if (block.Entity is PlayerEntity) Console.Write((block.Entity as PlayerEntity).Key);
//                        else Console.Write("*");

//                    }
//                    else
//                    {
//                        Console.Write(" ");
//                    }
//                }
//                Console.WriteLine();
//            }

//            var inExplosionRange = nodeMap.IsInExplosionRange(nodeMap.PlayerNode);

//            if (!inExplosionRange && nodeMap.PlayerBombs.Any(x => x.BombTimer > 2))
//            {
//                return GameCommand.TriggerBomb;
//            }

//            if (!(path[0].NodeEntity is DestructibleWallEntity) || inExplosionRange)
//            {
//                Console.WriteLine("Executing move command");
//                return GetMovementCommand(nodeMap.PlayerNode, path[0]);
//            }

//            if (path[0].NodeEntity is DestructibleWallEntity && nodeMap.PlayerBombs.Count < 1)
//            {
//                Console.WriteLine("Executing place bomb command");
//                return GameCommand.PlaceBomb;
//            }

//            return GameCommand.DoNothing;
//        }

//        IList<Node> FindBestPath(NodeMap nodeMap)
//        {
//            var startNode = nodeMap.PlayerNode;

//            var endNode = nodeMap.Nodes.Where(x => x.NodeEntity is IPowerUp).OrderBy(n => DistanceFromPlayer(nodeMap.PlayerNode, n)).FirstOrDefault();

//            if (endNode == null)
//            {
//                var tempPath = Path.Combine(Path.GetTempPath(), _gameMap.MapSeed + "_" + nodeMap.Player.Key + ".state");
//                if (!File.Exists(tempPath))
//                    File.Create(tempPath).Close();

//                var botState = File.ReadAllText(tempPath);
//                if (!String.IsNullOrWhiteSpace(botState))
//                {
//                    var state = JsonConvert.DeserializeObject<BotState>(botState);
//                    if (state != null)
//                    {
//                        Console.WriteLine("Targeting Previous Node " + state.PreviousNodeLocation);
//                        endNode = nodeMap.Nodes.FirstOrDefault(n => n.Location.IsSameLocation(state.PreviousNodeLocation) && n.NodeEntity != null);
//                    }
//                }

//                if (endNode == null)
//                {
//                    var furthestWall = nodeMap.Nodes.Where(x => x.NodeEntity is DestructibleWallEntity)
//                        .OrderBy(n => DistanceFromPlayer(nodeMap.PlayerNode, n))
//                        .LastOrDefault();

//                    if (furthestWall != null)
//                    {
//                        endNode = furthestWall;
//                        var state = new BotState()
//                        {
//                            PreviousNodeLocation = furthestWall.Location
//                        };

//                        File.WriteAllText(tempPath, JsonConvert.SerializeObject(state));
//                    }
//                }
//            }

//            if (endNode == null)
//            {
//                endNode = nodeMap.Nodes[new Random().Next(nodeMap.Nodes.Count)];
//            }

//            return new PathFinder(nodeMap).FindBestPath(startNode, endNode);
//        }

//        GameCommand GetMovementCommand(Node nodeA, Node nodeB)
//        {
//            var locA = nodeA.Location;
//            var locB = nodeB.Location;

//            if (locA.X == locB.X)
//            {
//                return locA.Y > locB.Y ? GameCommand.MoveUp : GameCommand.MoveDown;
//            }

//            if (locA.Y == locB.Y)
//            {
//                return locA.X > locB.X ? GameCommand.MoveLeft : GameCommand.MoveRight;
//            }

//            return GameCommand.DoNothing;
//        }

//        int DistanceFromPlayer(Node p, Node n)
//        {
//            var x1 = p.Location.X;
//            var x2 = n.Location.X;
//            var y1 = p.Location.Y;
//            var y2 = p.Location.Y;

//            return (int)Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2));
//        }
//    }
//}
