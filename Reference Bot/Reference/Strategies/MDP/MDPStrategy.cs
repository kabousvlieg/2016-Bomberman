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
        private PlayerPosition _playerPosition;

        private readonly int WallValue = 10;
        private readonly int PowerUpValue = 100;
        private readonly int SuperPowerUpValue = 200;
        private readonly int PenaltyValue = 3;
        private readonly int BombValue = -100;


        private enum MdpTypes
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

        private struct MdpBlock
        {
            public MdpTypes type;
            public int value;
            public bool validValue;
        }

        struct PlayerPosition
        {
            public int x;
            public int y;
        }

        public GameCommand ExecuteStrategy(GameMap gameMap, char playerKey)
        {
            this._gameMap = gameMap;
            //var nodeMap = new NodeMap(gameMap, playerKey);
            MdpBlock[,] MdpMap = new MdpBlock[gameMap.MapWidth + 1, gameMap.MapHeight + 1];
            
            DrawMap(playerKey);
            AssignValues(playerKey, MdpMap);
            CalculateMdp(gameMap, MdpMap);
            var bestMove = CalculateBestMoveFromMdp(MdpMap);
            OverrideMdpMoveWithRuleEngine(gameMap, ref bestMove);
            DrawMdpMap(MdpMap);
            return bestMove;
        }

        private void OverrideMdpMoveWithRuleEngine(GameMap gameMap, ref GameCommand bestMove)
        {
            //Check for survival
                //Will our bestMdpMove move into explosion
                //Will our bestMdpMove move into range of bomb

            //Check if we can blow up the enemy
            //Check if we can plant a bomb

            //bestMove = GameCommand.DoNothing;
        }

        private GameCommand CalculateBestMoveFromMdp(MdpBlock[,] mdpMap)
        {
            var largestMdpValue = int.MinValue;
            var bestMove = GameCommand.DoNothing;
            if (_playerPosition.x > 1)
            {
                if (mdpMap[_playerPosition.x - 1, _playerPosition.y].validValue &&
                    mdpMap[_playerPosition.x - 1, _playerPosition.y].value > largestMdpValue)
                {
                    largestMdpValue = mdpMap[_playerPosition.x - 1, _playerPosition.y].value;
                    bestMove = GameCommand.MoveLeft;
                }
            }
            if (_playerPosition.x < _gameMap.MapWidth)
            {
                if (mdpMap[_playerPosition.x + 1, _playerPosition.y].validValue &&
                    mdpMap[_playerPosition.x + 1, _playerPosition.y].value > largestMdpValue)
                {
                    largestMdpValue = mdpMap[_playerPosition.x + 1, _playerPosition.y].value;
                    bestMove = GameCommand.MoveRight;
                }
            }
            if (_playerPosition.y > 1)
            {
                if (mdpMap[_playerPosition.x, _playerPosition.y - 1].validValue &&
                    mdpMap[_playerPosition.x, _playerPosition.y - 1].value > largestMdpValue)
                {
                    largestMdpValue = mdpMap[_playerPosition.x, _playerPosition.y - 1].value;
                    bestMove = GameCommand.MoveUp;
                }
            }
            if (_playerPosition.y < _gameMap.MapHeight)
            {
                if (mdpMap[_playerPosition.x, _playerPosition.y + 1].validValue &&
                    mdpMap[_playerPosition.x, _playerPosition.y + 1].value > largestMdpValue)
                {
                    largestMdpValue = mdpMap[_playerPosition.x, _playerPosition.y + 1].value;
                    bestMove = GameCommand.MoveDown;
                }
            }
            return bestMove;
        }

        private void CalculateMdp(GameMap gameMap, MdpBlock[,] MdpMap)
        {
            var stillNotDone = false;
            do
            {
                stillNotDone = false;
                for (var y = 1; y <= gameMap.MapHeight; y++)
                {
                    for (var x = 1; x <= gameMap.MapWidth; x++)
                    {
                        if (MdpMap[x, y].type == MdpTypes.Path)
                        {
                            //Get largest value neighbour
                            var largestNeighbour = -100;
                            var largestNeigbourValid = false;
                            if (x > 1)
                            {
                                if (MdpMap[x - 1, y].validValue)
                                {
                                    if (MdpMap[x - 1, y].value > largestNeighbour)
                                    {
                                        largestNeighbour = MdpMap[x - 1, y].value;
                                        largestNeigbourValid = true;
                                    }
                                }
                            }
                            if (x < gameMap.MapWidth)
                            {
                                if (MdpMap[x + 1, y].validValue)
                                {
                                    if (MdpMap[x + 1, y].value > largestNeighbour)
                                    {
                                        largestNeighbour = MdpMap[x + 1, y].value;
                                        largestNeigbourValid = true;
                                    }
                                }
                            }
                            if (y > 1)
                            {
                                if (MdpMap[x, y - 1].validValue)
                                {
                                    if (MdpMap[x, y - 1].value > largestNeighbour)
                                    {
                                        largestNeighbour = MdpMap[x, y - 1].value;
                                        largestNeigbourValid = true;
                                    }
                                }
                            }
                            if (y < gameMap.MapHeight)
                            {
                                if (MdpMap[x, y + 1].validValue)
                                {
                                    if (MdpMap[x, y + 1].value > largestNeighbour)
                                    {
                                        largestNeighbour = MdpMap[x, y + 1].value;
                                        largestNeigbourValid = true;
                                    }
                                }
                            }

                            int calculatedValue;
                            //Calculate our value
                            if (largestNeigbourValid)
                            {
                                calculatedValue = largestNeighbour - PenaltyValue;
                                MdpMap[x, y].value = calculatedValue;
                                MdpMap[x, y].validValue = true;
                            }
                            else
                                stillNotDone = true; //Not all blocks has a value yet, TODO check for getting stuck here
                            //If difference is still too big, mark stillNotDone
                            //TODO limit the amount of iterations we will go through
                        }
                    }
                }
                //DrawMdpMap(MdpMap);
            } while (stillNotDone);
        }

        private void DrawMdpMap(MdpBlock[,] MdpMap)
        {
            //Scale output to biggest value
            var largestValue = 1;
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    if ((MdpMap[x, y].validValue) &&
                        (MdpMap[x, y].type == MdpTypes.Path) &&
                        (MdpMap[x, y].value > largestValue))
                    {
                        largestValue = MdpMap[x, y].value;
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
                    if (MdpMap[x, y].type == MdpTypes.Me)
                        Debug.Write("A");
                    else if (MdpMap[x, y].type == MdpTypes.OtherPlayer)
                        Debug.Write("B");
                    else if (MdpMap[x, y].type == MdpTypes.Indestructable)
                        Debug.Write("#");
                    else if (MdpMap[x, y].type == MdpTypes.Wall)
                        Debug.Write("+");
                    else
                        Debug.Write(((int) (MdpMap[x, y].value*9/largestValue)).ToString());
                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("");
        }

        private void AssignValues(char playerKey, MdpBlock[,] MdpMap)
        {
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    var block = _gameMap.GetBlockAtLocation(x, y);
                    if (block.Entity != null)
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
                            MdpMap[x, y].validValue = false;
                        }
                        else if (block.Entity is PlayerEntity)
                        {
                            if ((block.Entity as PlayerEntity).Key == playerKey)
                            {
                                MdpMap[x, y].type = MdpTypes.Me;
                                _playerPosition.x = x;
                                _playerPosition.y = y;
                            }
                            else
                                MdpMap[x, y].type = MdpTypes.OtherPlayer;
                            MdpMap[x, y].validValue = false;
                        }
                        else if (block.Entity is BombEntity)
                        {
                            MdpMap[x, y].type = MdpTypes.Bomb;
                            MdpMap[x, y].value = BombValue;
                            MdpMap[x, y].validValue = true;
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
                            MdpMap[x, y].validValue = false;
                        }
                    }
                    else
                    {
                        MdpMap[x, y].type = MdpTypes.Path;
                        MdpMap[x, y].validValue = false;
                    }
                }
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
                    if (block.Entity != null)
                    {
                        if (block.Entity is DestructibleWallEntity)
                            Debug.Write("+");
                        else if (block.Entity is IndestructibleWallEntity)
                            Debug.Write("#");
                        else if (block.Entity is PlayerEntity)
                            Debug.Write((block.Entity as PlayerEntity).Key);
                        else if (block.Entity is BombEntity)
                            Debug.Write("?");
                        else if (block.Entity is BombBagPowerUpEntity)
                            Debug.Write("&");
                        else if (block.Entity is BombRaduisPowerUpEntity)
                            Debug.Write("&");
                        else if (block.Entity is SuperPowerUp)
                            Debug.Write("&");
                        else
                            Debug.Write(" ");
                    }
                    else Debug.Write(" ");
                }
                Debug.WriteLine("");
            }
        }
    }
}
