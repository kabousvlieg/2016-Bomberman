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
        private PlayerEntity _player;

        private readonly int WallValue = 10;
        private readonly int PowerUpValue = 100;
        private readonly int SuperPowerUpValue = 200;
        private readonly int PenaltyValue = 3;
        //private readonly int BombValue = -100;
        private readonly int PathWhenBombValue = 100;


        private enum MdpTypes
        {
            Bomb,
            Me,
            OtherPlayer,
            PowerUp,
            SuperPowerUp,
            Indestructable,
            Wall,
            Path,
            PathAsGoal
        }

        private struct MdpBlock
        {
            public MdpTypes type;
            public int value;
            public bool validValue;
            public bool inRangeOfBomb;
        }

        public GameCommand ExecuteStrategy(GameMap gameMap, char playerKey)
        {
            this._gameMap = gameMap;
            //var nodeMap = new NodeMap(gameMap, playerKey);
            MdpBlock[,] mdpMap = new MdpBlock[gameMap.MapWidth + 1, gameMap.MapHeight + 1];
            
            DrawMap(playerKey);
            assignBombValues(mdpMap, playerKey); 
            if (mdpMap[_player.Location.X, _player.Location.Y].inRangeOfBomb)
            {
                AssignValuesForEscape(mdpMap);                
            }
            else
            {
                AssignValuesNormally(mdpMap);
            }
            DrawMdpMap(mdpMap);
            CalculateMdp(mdpMap);
            DrawMdpMap(mdpMap);
            var bestMove = CalculateBestMoveFromMdp(mdpMap);
            bestMove = OverrideMdpMoveWithRuleEngine(bestMove, mdpMap);
            return bestMove;
        }

        private GameCommand OverrideMdpMoveWithRuleEngine(GameCommand bestMdpMove, MdpBlock[,] mdpMap)
        {
            //Are we currently in bomb range?
            if (mdpMap[_player.Location.X, _player.Location.Y].inRangeOfBomb)
                return bestMdpMove;
            //Check for survival
                //Will our bestMdpMove move into explosion
                //Will our bestMdpMove move into range of bomb

            //Check if we can blow up the enemy
            //Check if we can plant a bomb
            if (CanWePlantABomb())
                return GameCommand.PlaceBomb;
            return bestMdpMove;
        }

        private bool CanWePlantABomb()
        {
            if (_player.BombBag == 0)
                return false;
            GameBlock block;
            if (_player.Location.X > 1)
            {
                block = _gameMap.GetBlockAtLocation(_player.Location.X - 1, _player.Location.Y);
                if (block.Entity != null)
                {
                    if (block.Entity is DestructibleWallEntity)
                        return true;
                }
            }
            if (_player.Location.X < _gameMap.MapWidth)
            {
                block = _gameMap.GetBlockAtLocation(_player.Location.X + 1, _player.Location.Y);
                if (block.Entity != null)
                {
                    if (block.Entity is DestructibleWallEntity)
                        return true;
                }
            }
            if (_player.Location.Y > 1)
            {
                block = _gameMap.GetBlockAtLocation(_player.Location.X, _player.Location.Y - 1);
                if (block.Entity != null)
                {
                    if (block.Entity is DestructibleWallEntity)
                        return true;
                }
            }
            if (_player.Location.Y < _gameMap.MapHeight)
            {
                block = _gameMap.GetBlockAtLocation(_player.Location.X, _player.Location.Y + 1);
                if (block.Entity != null)
                {
                    if (block.Entity is DestructibleWallEntity)
                        return true;
                }
            }
            return false;
        }

        private GameCommand CalculateBestMoveFromMdp(MdpBlock[,] mdpMap)
        {
            //TODO 
            //1 - We can still get stuck here...
            var largestMdpValue = int.MinValue;
            var bestMove = GameCommand.DoNothing;
            if (_player.Location.X > 1)
            {
                if (mdpMap[_player.Location.X - 1, _player.Location.Y].validValue &&
                    ( mdpMap[_player.Location.X - 1, _player.Location.Y].type == MdpTypes.Path ||
                      mdpMap[_player.Location.X - 1, _player.Location.Y].type == MdpTypes.PathAsGoal ) &&
                    mdpMap[_player.Location.X - 1, _player.Location.Y].value > largestMdpValue)
                {
                    if (mdpMap[_player.Location.X - 1, _player.Location.Y].inRangeOfBomb &&
                        !mdpMap[_player.Location.X, _player.Location.Y].inRangeOfBomb)
                    {
                        //do nothing
                    }
                    else
                    {
                        largestMdpValue = mdpMap[_player.Location.X - 1, _player.Location.Y].value;
                        bestMove = GameCommand.MoveLeft;
                    }
                }
            }
            if (_player.Location.X < _gameMap.MapWidth)
            {
                if (mdpMap[_player.Location.X + 1, _player.Location.Y].validValue &&
                    ( mdpMap[_player.Location.X + 1, _player.Location.Y].type == MdpTypes.Path ||
                      mdpMap[_player.Location.X + 1, _player.Location.Y].type == MdpTypes.PathAsGoal ) &&
                    mdpMap[_player.Location.X + 1, _player.Location.Y].value > largestMdpValue)
                {
                    if (mdpMap[_player.Location.X + 1, _player.Location.Y].inRangeOfBomb &&
                        !mdpMap[_player.Location.X, _player.Location.Y].inRangeOfBomb)
                    {
                        //do nothing
                    }
                    else
                    {
                        largestMdpValue = mdpMap[_player.Location.X + 1, _player.Location.Y].value;
                        bestMove = GameCommand.MoveRight;
                    }
                }
            }
            if (_player.Location.Y > 1)
            {
                if (mdpMap[_player.Location.X, _player.Location.Y - 1].validValue &&
                    ( mdpMap[_player.Location.X, _player.Location.Y - 1].type == MdpTypes.Path ||
                      mdpMap[_player.Location.X, _player.Location.Y - 1].type == MdpTypes.PathAsGoal) &&
                    mdpMap[_player.Location.X, _player.Location.Y - 1].value > largestMdpValue)
                {
                    if (mdpMap[_player.Location.X, _player.Location.Y - 1].inRangeOfBomb &&
                        !mdpMap[_player.Location.X, _player.Location.Y].inRangeOfBomb)
                    {
                        //do nothing
                    }
                    else
                    {
                        largestMdpValue = mdpMap[_player.Location.X, _player.Location.Y - 1].value;
                        bestMove = GameCommand.MoveUp;
                    }
                }
            }
            if (_player.Location.Y < _gameMap.MapHeight)
            {
                if (mdpMap[_player.Location.X, _player.Location.Y + 1].validValue &&
                    ( mdpMap[_player.Location.X, _player.Location.Y + 1].type == MdpTypes.Path ||
                      mdpMap[_player.Location.X, _player.Location.Y + 1].type == MdpTypes.PathAsGoal ) &&
                    mdpMap[_player.Location.X, _player.Location.Y + 1].value > largestMdpValue)
                {
                    if (mdpMap[_player.Location.X, _player.Location.Y + 1].inRangeOfBomb &&
                        !mdpMap[_player.Location.X, _player.Location.Y].inRangeOfBomb)
                    {
                        //do nothing
                    }
                    else
                    {
                        largestMdpValue = mdpMap[_player.Location.X, _player.Location.Y + 1].value;
                        bestMove = GameCommand.MoveDown;
                    }
                }
            }
            return bestMove;
        }

        private void CalculateMdp(MdpBlock[,] mdpMap)
        {
            //TODO notes:
            //1 - What about squares with multiple destructible walls, should be worth more?
        
            var stillNotDone = false;
            var stopwatch = Stopwatch.StartNew();
            do
            {
                //if (stopwatch.ElapsedMilliseconds > 1000)
                //    break;
                stillNotDone = false;
                for (var y = 1; y <= _gameMap.MapHeight; y++)
                {
                    for (var x = 1; x <= _gameMap.MapWidth; x++)
                    {
                        //if (mdpMap[x, y].inRangeOfBomb) continue;
                        if ( (mdpMap[x, y].type != MdpTypes.Path) &&
                             (mdpMap[x, y].type != MdpTypes.Bomb) )
                        {
                            continue;
                        }
                        //Get largest value neighbour
                        var largestNeighbour = int.MinValue;
                        var largestNeigbourValid = false;
                        if (x > 1)
                        {
                            if (mdpMap[x - 1, y].validValue)
                            {
                                if (mdpMap[x - 1, y].value > largestNeighbour)
                                {
                                    largestNeighbour = mdpMap[x - 1, y].value;
                                    largestNeigbourValid = true;
                                }
                            }
                        }
                        if (x < _gameMap.MapWidth)
                        {
                            if (mdpMap[x + 1, y].validValue)
                            {
                                if (mdpMap[x + 1, y].value > largestNeighbour)
                                {
                                    largestNeighbour = mdpMap[x + 1, y].value;
                                    largestNeigbourValid = true;
                                }
                            }
                        }
                        if (y > 1)
                        {
                            if (mdpMap[x, y - 1].validValue)
                            {
                                if (mdpMap[x, y - 1].value > largestNeighbour)
                                {
                                    largestNeighbour = mdpMap[x, y - 1].value;
                                    largestNeigbourValid = true;
                                }
                            }
                        }
                        if (y < _gameMap.MapHeight)
                        {
                            if (mdpMap[x, y + 1].validValue)
                            {
                                if (mdpMap[x, y + 1].value > largestNeighbour)
                                {
                                    largestNeighbour = mdpMap[x, y + 1].value;
                                    largestNeigbourValid = true;
                                }
                            }
                        }

                        int calculatedValue;
                        //Calculate our value
                        if (largestNeigbourValid)
                        {
                            calculatedValue = largestNeighbour - PenaltyValue;
                            mdpMap[x, y].value = calculatedValue;
                            mdpMap[x, y].validValue = true;
                        }
                        else
                            stillNotDone = true;
                        //If difference is still too big, mark stillNotDone
                        //TODO limit the amount of iterations we will go through
                    }
                }
                DrawMdpMap(mdpMap);
            } while (stillNotDone);
        }

        private void DrawMdpMap(MdpBlock[,] mdpMap, bool printSign = false)
        {
            //Scale output to biggest value
            var largestValue = 1;
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    if ((mdpMap[x, y].validValue) &&
                        (mdpMap[x,y].type == MdpTypes.Path) &&
                        (Math.Abs(mdpMap[x, y].value) > largestValue))
                    {
                        largestValue = Math.Abs(mdpMap[x, y].value);
                    }
                }
            }

            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine("");
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    if (mdpMap[x, y].type == MdpTypes.Me)
                        Debug.Write(printSign? "AA" : "A");
                    else if (mdpMap[x, y].type == MdpTypes.OtherPlayer)
                        Debug.Write(printSign ? "BB" : "B");
                    else if (mdpMap[x, y].type == MdpTypes.Indestructable)
                        Debug.Write(printSign ? "##" : "#");
                    else if (mdpMap[x, y].type == MdpTypes.Wall)
                        Debug.Write(printSign ? "++" : "+");
                    else if (mdpMap[x, y].type == MdpTypes.PathAsGoal)
                        Debug.Write(printSign ? ".." : ".");
                    else if (mdpMap[x, y].validValue == false)
                        Debug.Write(printSign ? "??" : "?");
                    else
                    {
                        if (Math.Abs(mdpMap[x, y].value) <= largestValue)
                        {
                            //Console.ForegroundColor = mdpMap[x, y].value > 0 ? ConsoleColor.Red : ConsoleColor.Yellow;
                            if (printSign)
                                Debug.Write(mdpMap[x, y].value > 0 ? "-" : "=");
                            Debug.Write(((int) (Math.Abs(mdpMap[x, y].value)*9/largestValue)).ToString());
                            //Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                            Debug.Write("??");
                    }
                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("");
        }

        private void AssignValuesNormally(MdpBlock[,] mdpMap)
        {
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    var block = _gameMap.GetBlockAtLocation(x, y);
                    if (block.Bomb != null)
                        continue;

                    //Defaults
                    mdpMap[x, y].validValue = false;
                    mdpMap[x, y].value = Int32.MinValue;
                    if (block.Entity != null)
                    {
                        assignBlockEntityValues(mdpMap, block, x, y);
                    }
                    else
                    {
                        mdpMap[x, y].type = MdpTypes.Path;
                    }
                }
            }
        }

        private void AssignValuesForEscape(MdpBlock[,] mdpMap)
        {
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    var block = _gameMap.GetBlockAtLocation(x, y);
                    if (block.Bomb != null)
                        continue;

                    //Defaults
                    mdpMap[x, y].validValue = false;
                    mdpMap[x, y].value = Int32.MinValue;
                    if (block.Entity != null)
                    {
                        assignBlockEntityValuesForEscape(mdpMap, block, x, y);
                    }
                    else
                    {
                        if (!mdpMap[x, y].inRangeOfBomb)
                        {
                            mdpMap[x, y].type = MdpTypes.PathAsGoal;
                            mdpMap[x, y].validValue = true;
                            mdpMap[x, y].value = PathWhenBombValue;
                        }
                        else
                        {
                            mdpMap[x, y].type = MdpTypes.Path;
                        }
                    }
                }
            }
        }

        private void assignBombValues(MdpBlock[,] MdpMap, char playerKey)
        {
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    //TODO 
                    //1 - Take into account types of stuff that block bomb blasts
                    //2 - Differentiate between my bombs and enemy bombs
                    var block = _gameMap.GetBlockAtLocation(x, y);
                    if (block.Entity != null)
                    {
                        var entity = (block.Entity as PlayerEntity);
                        if (block.Entity is PlayerEntity)
                        {
                            if (entity.Key == playerKey)
                            {
                                MdpMap[x, y].type = MdpTypes.Me;
                                _player = entity;
                            }
                            else
                                MdpMap[x, y].type = MdpTypes.OtherPlayer;
                        }
                    }
                    if (block.Bomb == null) continue;
                    MdpMap[x, y].type = MdpTypes.Bomb;
                    //MdpMap[x, y].value = BombValue;
                    //MdpMap[x, y].validValue = true;
                    MdpMap[x, y].inRangeOfBomb = true;
                    for (int range = 1; range <= block.Bomb.BombRadius; range++)
                    {
                        if (x - range > 1)
                        {
                            MdpMap[x - range, y].inRangeOfBomb = true;
                        }
                        if (x + range < _gameMap.MapWidth)
                        {
                            MdpMap[x + range, y].inRangeOfBomb = true;
                        }
                        if (y - range > 1)
                        {
                            MdpMap[x, y - range].inRangeOfBomb = true;
                        }
                        if (y + range < _gameMap.MapHeight)
                        {
                            MdpMap[x, y + range].inRangeOfBomb = true;
                        }
                    }
                }
            }
        }

        private void assignBlockEntityValues(MdpBlock[,] MdpMap, GameBlock block, int x, int y)
        {
            if (block.Entity is DestructibleWallEntity)
            {
                MdpMap[x, y].type = MdpTypes.Wall;
                MdpMap[x, y].value = WallValue;
                MdpMap[x, y].validValue = true;
            }
            else if (block.Entity is IndestructibleWallEntity)
            {
                MdpMap[x, y].type = MdpTypes.Indestructable;
            }
            else if (block.Entity is BombEntity)
            {
                MdpMap[x, y].type = MdpTypes.Bomb;
                //MdpMap[x, y].value = BombValue;
                //MdpMap[x, y].validValue = true;
            }
            else if (block.Entity is BombBagPowerUpEntity)
            {
                MdpMap[x, y].type = MdpTypes.PowerUp;
                MdpMap[x, y].value = PowerUpValue;
                MdpMap[x, y].validValue = true;
            }
            else if (block.Entity is BombRaduisPowerUpEntity)
            {
                MdpMap[x, y].type = MdpTypes.PowerUp;
                MdpMap[x, y].value = PowerUpValue;
                MdpMap[x, y].validValue = true;
            }
            else if (block.Entity is SuperPowerUp)
            {
                MdpMap[x, y].type = MdpTypes.SuperPowerUp;
                MdpMap[x, y].value = SuperPowerUpValue;
                MdpMap[x, y].validValue = true;
            }
            else
            {
                MdpMap[x, y].type = MdpTypes.Path;
            }
        }

        private void assignBlockEntityValuesForEscape(MdpBlock[,] mdpMap, GameBlock block, int x, int y)
        {
            if (block.Entity is DestructibleWallEntity)
            {
                mdpMap[x, y].type = MdpTypes.Wall;
            }
            else if (block.Entity is IndestructibleWallEntity)
            {
                mdpMap[x, y].type = MdpTypes.Indestructable;
            }
            else if (block.Entity is BombEntity)
            {
                mdpMap[x, y].type = MdpTypes.Bomb;
                //mdpMap[x, y].value = BombValue;
                //mdpMap[x, y].validValue = true;
            }
            else if (block.Entity is BombBagPowerUpEntity)
            {
                mdpMap[x, y].type = MdpTypes.PowerUp;
            }
            else if (block.Entity is BombRaduisPowerUpEntity)
            {
                mdpMap[x, y].type = MdpTypes.PowerUp;
            }
            else if (block.Entity is SuperPowerUp)
            {
                mdpMap[x, y].type = MdpTypes.SuperPowerUp;
            }
            else
            {
                if (!mdpMap[x, y].inRangeOfBomb)
                {
                    mdpMap[x, y].type = MdpTypes.PathAsGoal;
                    mdpMap[x, y].validValue = true;
                    mdpMap[x, y].value = PathWhenBombValue;
                }
                else
                {
                    mdpMap[x, y].type = MdpTypes.Path;
                };
            }
        }

        private void DrawMap(char playerKey)
        {
            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine("");
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    var block = _gameMap.GetBlockAtLocation(x, y);
                    if (block.Bomb != null)
                    {
                       if (block.Entity is PlayerEntity)
                        {
                            Debug.Write(Char.ToLower((block.Entity as PlayerEntity).Key));
                        }
                        else
                            Debug.Write(block.Bomb.BombTimer);
                    }
                    else if (block.Entity != null)
                    {
                        if (block.Entity is DestructibleWallEntity)
                            Debug.Write("+");
                        else if (block.Entity is IndestructibleWallEntity)
                            Debug.Write("#");
                        else if (block.Entity is PlayerEntity)
                            Debug.Write((block.Entity as PlayerEntity).Key);
                        else if (block.Entity is BombBagPowerUpEntity)
                            Debug.Write("&");
                        else if (block.Entity is BombRaduisPowerUpEntity)
                            Debug.Write("&");
                        else if (block.Entity is SuperPowerUp)
                            Debug.Write("&");
                        else
                            Debug.Write(" ");
                    }
                    else if (block.Exploding)
                        Debug.Write("*");
                    else Debug.Write(" ");
                }
                Debug.WriteLine("");
            }
        }
    }
}
