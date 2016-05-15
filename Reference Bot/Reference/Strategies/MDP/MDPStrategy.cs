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
        private readonly int PathWhenBombValue = 500;


        private enum MdpTypes
        {
            //Bomb,
            //Me,
            OtherPlayer,
            //PowerUp,
            //SuperPowerUp,
            Indestructable,
            //Wall,
            Path,
            //PathAsGoal
        }

        private struct MdpBlock
        {
            public MdpTypes Type;
            public int Value;
            public bool ValidValue;
            public int ItemOnBlockValue;
            public bool ValidItemOnBlockValue;
            public bool InRangeOfBomb;
        }

        public GameCommand ExecuteStrategy(GameMap gameMap, char playerKey)
        {
            this._gameMap = gameMap;
            //var nodeMap = new NodeMap(gameMap, playerKey);
            MdpBlock[,] mdpMap = new MdpBlock[gameMap.MapWidth + 1, gameMap.MapHeight + 1];
            
            DrawMap(playerKey);
            assignBombValues(mdpMap, playerKey); 
            if (mdpMap[_player.Location.X, _player.Location.Y].InRangeOfBomb)
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
            if (mdpMap[_player.Location.X, _player.Location.Y].InRangeOfBomb)
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
                if (mdpMap[_player.Location.X - 1, _player.Location.Y].ValidValue &&
                    mdpMap[_player.Location.X - 1, _player.Location.Y].Type == MdpTypes.Path  &&
                    mdpMap[_player.Location.X - 1, _player.Location.Y].Value > largestMdpValue)
                {
                    if (mdpMap[_player.Location.X - 1, _player.Location.Y].InRangeOfBomb &&
                        !mdpMap[_player.Location.X, _player.Location.Y].InRangeOfBomb)
                    {
                        //do nothing
                    }
                    else
                    {
                        largestMdpValue = mdpMap[_player.Location.X - 1, _player.Location.Y].Value;
                        bestMove = GameCommand.MoveLeft;
                    }
                }
            }
            if (_player.Location.X < _gameMap.MapWidth)
            {
                if (mdpMap[_player.Location.X + 1, _player.Location.Y].ValidValue &&
                    mdpMap[_player.Location.X + 1, _player.Location.Y].Type == MdpTypes.Path &&
                    mdpMap[_player.Location.X + 1, _player.Location.Y].Value > largestMdpValue)
                {
                    if (mdpMap[_player.Location.X + 1, _player.Location.Y].InRangeOfBomb &&
                        !mdpMap[_player.Location.X, _player.Location.Y].InRangeOfBomb)
                    {
                        //do nothing
                    }
                    else
                    {
                        largestMdpValue = mdpMap[_player.Location.X + 1, _player.Location.Y].Value;
                        bestMove = GameCommand.MoveRight;
                    }
                }
            }
            if (_player.Location.Y > 1)
            {
                if (mdpMap[_player.Location.X, _player.Location.Y - 1].ValidValue &&
                    mdpMap[_player.Location.X, _player.Location.Y - 1].Type == MdpTypes.Path &&
                    mdpMap[_player.Location.X, _player.Location.Y - 1].Value > largestMdpValue)
                {
                    if (mdpMap[_player.Location.X, _player.Location.Y - 1].InRangeOfBomb &&
                        !mdpMap[_player.Location.X, _player.Location.Y].InRangeOfBomb)
                    {
                        //do nothing
                    }
                    else
                    {
                        largestMdpValue = mdpMap[_player.Location.X, _player.Location.Y - 1].Value;
                        bestMove = GameCommand.MoveUp;
                    }
                }
            }
            if (_player.Location.Y < _gameMap.MapHeight)
            {
                if (mdpMap[_player.Location.X, _player.Location.Y + 1].ValidValue &&
                    mdpMap[_player.Location.X, _player.Location.Y + 1].Type == MdpTypes.Path &&
                    mdpMap[_player.Location.X, _player.Location.Y + 1].Value > largestMdpValue)
                {
                    if (mdpMap[_player.Location.X, _player.Location.Y + 1].InRangeOfBomb &&
                        !mdpMap[_player.Location.X, _player.Location.Y].InRangeOfBomb)
                    {
                        //do nothing
                    }
                    else
                    {
                        largestMdpValue = mdpMap[_player.Location.X, _player.Location.Y + 1].Value;
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
                if (stopwatch.ElapsedMilliseconds > 500)
                    return;
                stillNotDone = false;
                for (var y = 1; y <= _gameMap.MapHeight; y++)
                {
                    for (var x = 1; x <= _gameMap.MapWidth; x++)
                    {
                        //if (mdpMap[x, y].inRangeOfBomb) continue;
                        if (mdpMap[x, y].Type != MdpTypes.Path)
                        {
                            continue;
                        }
                        //Get largest value neighbour
                        var largestNeighbour = int.MinValue;
                        var largestNeigbourValid = false;
                        if (x > 1)
                        {
                            if (mdpMap[x - 1, y].ValidValue)
                            {
                                if (mdpMap[x - 1, y].Value > largestNeighbour)
                                {
                                    largestNeighbour = mdpMap[x - 1, y].Value;
                                    largestNeigbourValid = true;
                                }
                            }
                        }
                        if (x < _gameMap.MapWidth)
                        {
                            if (mdpMap[x + 1, y].ValidValue)
                            {
                                if (mdpMap[x + 1, y].Value > largestNeighbour)
                                {
                                    largestNeighbour = mdpMap[x + 1, y].Value;
                                    largestNeigbourValid = true;
                                }
                            }
                        }
                        if (y > 1)
                        {
                            if (mdpMap[x, y - 1].ValidValue)
                            {
                                if (mdpMap[x, y - 1].Value > largestNeighbour)
                                {
                                    largestNeighbour = mdpMap[x, y - 1].Value;
                                    largestNeigbourValid = true;
                                }
                            }
                        }
                        if (y < _gameMap.MapHeight)
                        {
                            if (mdpMap[x, y + 1].ValidValue)
                            {
                                if (mdpMap[x, y + 1].Value > largestNeighbour)
                                {
                                    largestNeighbour = mdpMap[x, y + 1].Value;
                                    largestNeigbourValid = true;
                                }
                            }
                        }

                        int calculatedValue;
                        //Calculate our value
                        if (largestNeigbourValid)
                        {
                            if (mdpMap[x, y].ValidItemOnBlockValue)
                            {
                                if (mdpMap[x, y].ItemOnBlockValue > largestNeighbour)
                                {
                                    mdpMap[x, y].Value = mdpMap[x, y].ItemOnBlockValue;
                                    mdpMap[x, y].ValidValue = true;
                                }
                                else
                                {
                                    calculatedValue = largestNeighbour - PenaltyValue;
                                    if (Math.Abs(calculatedValue - mdpMap[x, y].Value) > 10)
                                        stillNotDone = true;
                                    mdpMap[x, y].Value = calculatedValue;
                                    mdpMap[x, y].ValidValue = true;
                                }
                            }
                            else
                            {
                                calculatedValue = largestNeighbour - PenaltyValue;
                                if (Math.Abs(calculatedValue - mdpMap[x, y].Value) > 10)
                                    stillNotDone = true;
                                mdpMap[x, y].Value = calculatedValue;
                                mdpMap[x, y].ValidValue = true;
                            }
                        }
                        else if (mdpMap[x, y].ValidItemOnBlockValue)
                        {
                            mdpMap[x, y].Value = mdpMap[x, y].ItemOnBlockValue;
                            mdpMap[x, y].ValidValue = true;
                        }
                        else
                        {
                            stillNotDone = true;
                        }
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
                    if ((mdpMap[x, y].ValidValue) &&
                        (mdpMap[x,y].Type == MdpTypes.Path) &&
                        (Math.Abs(mdpMap[x, y].Value) > largestValue))
                    {
                        largestValue = Math.Abs(mdpMap[x, y].Value);
                    }
                }
            }

            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine(" 1234567890123456789012");
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                Debug.Write(y%9);
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    if (mdpMap[x, y].Type == MdpTypes.OtherPlayer)
                        Debug.Write(printSign ? "BB" : "B");
                    else if (mdpMap[x, y].Type == MdpTypes.Indestructable)
                        Debug.Write(printSign ? "##" : "#");
                    else if (mdpMap[x, y].ValidItemOnBlockValue)
                    {
                        if (mdpMap[x, y].ItemOnBlockValue == PowerUpValue)
                            Debug.Write(printSign ? "!!" : "!");
                        else if (mdpMap[x, y].ItemOnBlockValue == SuperPowerUpValue)
                            Debug.Write(printSign ? "&&" : "&");
                        else if (mdpMap[x, y].ItemOnBlockValue == PathWhenBombValue)
                            Debug.Write(printSign ? ".." : ".");
                        else if (mdpMap[x, y].ItemOnBlockValue == WallValue)
                            Debug.Write(printSign ? "++" : "+");
                    }
                    else if (mdpMap[x, y].ValidValue == false)
                        Debug.Write(printSign ? "??" : "?");
                    else
                    {
                        if (Math.Abs(mdpMap[x, y].Value) <= largestValue)
                        {
                            //Console.ForegroundColor = mdpMap[x, y].value > 0 ? ConsoleColor.Red : ConsoleColor.Yellow;
                            if (printSign)
                                Debug.Write(mdpMap[x, y].Value > 0 ? "-" : "=");
                            Debug.Write(((int) (Math.Abs(mdpMap[x, y].Value)*9/largestValue)).ToString());
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
                    //if (block.Bomb != null)
                    //    continue;

                    //Defaults
                    mdpMap[x, y].ValidValue = false;
                    mdpMap[x, y].Value = Int32.MinValue;
                    mdpMap[x, y].ValidItemOnBlockValue = false;
                    mdpMap[x, y].ItemOnBlockValue = Int32.MinValue;
                    if (block.Entity != null)
                    {
                        assignBlockEntityValues(mdpMap, block, x, y);
                    }
                    else
                    {
                        mdpMap[x, y].Type = MdpTypes.Path;
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
                    //if (block.Bomb != null)
                    //    continue;

                    //Defaults
                    mdpMap[x, y].ValidValue = false;
                    mdpMap[x, y].Value = Int32.MinValue;
                    mdpMap[x, y].ValidItemOnBlockValue = false;
                    mdpMap[x, y].ItemOnBlockValue = Int32.MinValue;
                    if (block.Entity != null)
                    {
                        assignBlockEntityValuesForEscape(mdpMap, block, x, y);
                    }
                    else //Path
                    {
                        if (!mdpMap[x, y].InRangeOfBomb)
                        {
                            mdpMap[x, y].Type = MdpTypes.Path;
                            mdpMap[x, y].ValidItemOnBlockValue = true;
                            mdpMap[x, y].ItemOnBlockValue = PathWhenBombValue;
                        }
                        else
                        {
                            mdpMap[x, y].Type = MdpTypes.Path;
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
                                MdpMap[x, y].Type = MdpTypes.Path;
                                _player = entity;
                            }
                            else
                                MdpMap[x, y].Type = MdpTypes.OtherPlayer;
                        }
                    }
                    if (block.Bomb == null) continue;
                    MdpMap[x, y].InRangeOfBomb = true;
                    bool bombBlockedXMinusDirection = false;
                    bool bombBlockedXPlusDirection = false;
                    bool bombBlockedYMinusDirection = false;
                    bool bombBlockedYPlusDirection = false;
                    for (int range = 1; range <= block.Bomb.BombRadius; range++)
                    {
                        GameBlock blockInRange;
                        if ( (x - range > 1) && (!bombBlockedXMinusDirection) )
                        {
                            blockInRange = _gameMap.GetBlockAtLocation(x - range, y);
                            if (blockInRange.Entity != null)
                            {
                                if ((blockInRange.Entity is DestructibleWallEntity) ||
                                    (blockInRange.Entity is IndestructibleWallEntity) ||
                                    (blockInRange.Entity is DestructibleWallEntity)/* ||
                                    (blockInRange.Entity is PlayerEntity)*/)
                                {
                                    bombBlockedXMinusDirection = true;
                                }
                                else
                                {
                                    MdpMap[x - range, y].InRangeOfBomb = true;
                                }
                            }
                            else
                                MdpMap[x - range, y].InRangeOfBomb = true;
                        }
                        if ( (x + range < _gameMap.MapWidth) && (!bombBlockedXPlusDirection) )
                        {
                            blockInRange = _gameMap.GetBlockAtLocation(x + range, y);
                            if (blockInRange.Entity != null)
                            {
                                if ((blockInRange.Entity is DestructibleWallEntity) ||
                                    (blockInRange.Entity is IndestructibleWallEntity) ||
                                    (blockInRange.Entity is DestructibleWallEntity)/* ||
                                    (blockInRange.Entity is PlayerEntity)*/)
                                {
                                    bombBlockedXPlusDirection = true;
                                }
                                else
                                {
                                    MdpMap[x + range, y].InRangeOfBomb = true;
                                }
                            }
                            else
                                MdpMap[x + range, y].InRangeOfBomb = true;
                        }
                        if ( (y - range > 1) && (!bombBlockedYMinusDirection) )
                        {
                            blockInRange = _gameMap.GetBlockAtLocation(x, y - range);
                            if (blockInRange.Entity != null)
                            {
                                if ((blockInRange.Entity is DestructibleWallEntity) ||
                                    (blockInRange.Entity is IndestructibleWallEntity) ||
                                    (blockInRange.Entity is DestructibleWallEntity)/* ||
                                    (blockInRange.Entity is PlayerEntity)*/)
                                {
                                    bombBlockedYMinusDirection = true;
                                }
                                else
                                {
                                    MdpMap[x, y - range].InRangeOfBomb = true;
                                }
                            }
                            else
                                MdpMap[x, y - range].InRangeOfBomb = true;
                        }
                        if ( (y + range < _gameMap.MapHeight) && (!bombBlockedYPlusDirection) )
                        {
                            blockInRange = _gameMap.GetBlockAtLocation(x, y + range);
                            if (blockInRange.Entity != null)
                            {
                                if ((blockInRange.Entity is DestructibleWallEntity) ||
                                    (blockInRange.Entity is IndestructibleWallEntity) ||
                                    (blockInRange.Entity is DestructibleWallEntity)/* ||
                                    (blockInRange.Entity is PlayerEntity)*/)
                                {
                                    bombBlockedYPlusDirection = true;
                                }
                                else
                                {
                                    MdpMap[x, y + range].InRangeOfBomb = true;
                                }
                            }
                            else
                                MdpMap[x, y + range].InRangeOfBomb = true;
                        }
                    }
                }
            }
        }

        private void assignBlockEntityValues(MdpBlock[,] MdpMap, GameBlock block, int x, int y)
        {
            if (block.Entity is DestructibleWallEntity)
            {
                MdpMap[x, y].Type = MdpTypes.Path;
                MdpMap[x, y].ItemOnBlockValue = WallValue;
                MdpMap[x, y].ValidItemOnBlockValue = true;
            }
            else if (block.Entity is IndestructibleWallEntity)
            {
                MdpMap[x, y].Type = MdpTypes.Indestructable;
            }
            else if (block.Entity is BombEntity)
            {
                MdpMap[x, y].Type = MdpTypes.Path;
                //MdpMap[x, y].value = BombValue;
                //MdpMap[x, y].validValue = true;
            }
            else if (block.Entity is BombBagPowerUpEntity)
            {
                MdpMap[x, y].Type = MdpTypes.Path;
                MdpMap[x, y].ItemOnBlockValue = PowerUpValue;
                MdpMap[x, y].ValidItemOnBlockValue = true;
            }
            else if (block.Entity is BombRaduisPowerUpEntity)
            {
                MdpMap[x, y].Type = MdpTypes.Path;
                MdpMap[x, y].ItemOnBlockValue = PowerUpValue;
                MdpMap[x, y].ValidItemOnBlockValue = true;
            }
            else if (block.Entity is SuperPowerUp)
            {
                MdpMap[x, y].Type = MdpTypes.Path;
                MdpMap[x, y].ItemOnBlockValue = SuperPowerUpValue;
                MdpMap[x, y].ValidItemOnBlockValue = true;
            }
            else if ( (block.Entity is BombBagPowerUpEntity) ||
                      (block.Entity is BombRaduisPowerUpEntity))
            {
                MdpMap[x, y].Type = MdpTypes.Path;
                MdpMap[x, y].ItemOnBlockValue = PowerUpValue;
                MdpMap[x, y].ValidItemOnBlockValue = true;
            }
            else
            {
                MdpMap[x, y].Type = MdpTypes.Path;
            }
        }

        private void assignBlockEntityValuesForEscape(MdpBlock[,] mdpMap, GameBlock block, int x, int y)
        {
            if (block.Entity is DestructibleWallEntity)
            {
                mdpMap[x, y].Type = MdpTypes.Indestructable;
            }
            else if (block.Entity is IndestructibleWallEntity)
            {
                mdpMap[x, y].Type = MdpTypes.Indestructable;
            }
            else if (block.Entity is BombEntity)
            {
                mdpMap[x, y].Type = MdpTypes.Path;
                //mdpMap[x, y].value = BombValue;
                //mdpMap[x, y].validValue = true;
            }
            else if (block.Entity is BombBagPowerUpEntity)
            {
                mdpMap[x, y].Type = MdpTypes.Path;
            }
            else if (block.Entity is BombRaduisPowerUpEntity)
            {
                mdpMap[x, y].Type = MdpTypes.Path;
            }
            else if (block.Entity is SuperPowerUp)
            {
                mdpMap[x, y].Type = MdpTypes.Path;
            }
            else
            {
                if (!mdpMap[x, y].InRangeOfBomb)
                {
                    mdpMap[x, y].Type = MdpTypes.Path;
                    mdpMap[x, y].ValidItemOnBlockValue = true;
                    mdpMap[x, y].ItemOnBlockValue = PathWhenBombValue;
                }
                else
                {
                    mdpMap[x, y].Type = MdpTypes.Path;
                };
            }
        }

        private void DrawMap(char playerKey)
        {
            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine(" 1234567890123456789012");
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                Debug.Write(y%9);
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
