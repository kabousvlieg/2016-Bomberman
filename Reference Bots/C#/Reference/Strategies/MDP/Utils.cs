﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Reference.Commands;
using Reference.Domain.Map;
using Reference.Domain.Map.Entities;
using Reference.Domain.Map.Entities.PowerUps;

namespace Reference.Strategies.MDP
{


    public class Utils
    {
        private GameMap _gameMap;
        private char _playerKey;

        public Utils(GameMap gameMap, char playerKey)
        {
            _gameMap = gameMap;
            _playerKey = playerKey;
        }

        private PlayerEntity[] _players = new PlayerEntity[4];

        public void getPlayers(ref PlayerEntity[] players)
        {
            //TODO Not guaranteed 4 players always
            var count = 1;
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    var block = _gameMap.GetBlockAtLocation(x, y);
                    if (block == null) continue;
                    if (block.Entity == null) continue;
                    if (block.Entity is PlayerEntity)
                    {
                        if ((block.Entity as PlayerEntity).Key == _playerKey)
                        {
                            players[0] = block.Entity as PlayerEntity;
                        }
                        else
                        {
                            players[count++] = block.Entity as PlayerEntity;
                        }
                    }
                }
            }
            _players = players;
        }

        public void DrawMap()
        {
            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine(" 1234567890123456789012");
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                Debug.Write(y%10);
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
                            Debug.Write("!");
                        else if (block.Entity is SuperPowerUp)
                            Debug.Write("$");
                        else
                            Debug.Write(" ");
                    }
                    else if (block.Exploding)
                        Debug.Write("*");
                    else if (block.PowerUp != null)
                    {
                        if (block.PowerUp is BombBagPowerUpEntity)
                            Debug.Write("!");
                        else if (block.PowerUp is BombRaduisPowerUpEntity)
                            Debug.Write("&");
                        else if (block.PowerUp is SuperPowerUp)
                            Debug.Write("$");
                    }
                    else Debug.Write(" ");
                }
                Debug.WriteLine("");
            }
        }

        public static void DrawMap(GameMap gameMap)
        {
            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine(" 1234567890123456789012");
            for (var y = 1; y <= gameMap.MapHeight; y++)
            {
                Debug.Write(y % 10);
                for (var x = 1; x <= gameMap.MapWidth; x++)
                {
                    var block = gameMap.GetBlockAtLocation(x, y);
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
                            Debug.Write("!");
                        else if (block.Entity is SuperPowerUp)
                            Debug.Write("$");
                        else
                            Debug.Write(" ");
                    }
                    else if (block.Exploding)
                        Debug.Write("*");
                    else if (block.PowerUp != null)
                    {
                        if (block.PowerUp is BombBagPowerUpEntity)
                            Debug.Write("!");
                        else if (block.PowerUp is BombRaduisPowerUpEntity)
                            Debug.Write("&");
                        else if (block.PowerUp is SuperPowerUp)
                            Debug.Write("$");
                    }
                    else Debug.Write(" ");
                }
                Debug.WriteLine("");
            }
        }

        public static GameMap tickTheMap(GameMap gameMap, List<MdpTools.PlayersAndMoves> playerMoves)
        {
            //Debug.WriteLine("Before tick:");
            //DrawMap(gameMap);
            var nextMap = gameMap;
            foreach (var player in playerMoves)
            {
                //TODO Still need to do collision detection and mitigation here
                if ( (player.BestMove == GameCommand.MoveUp) ||
                     (player.BestMove == GameCommand.MoveDown) ||
                     (player.BestMove == GameCommand.MoveLeft) ||
                     (player.BestMove == GameCommand.MoveRight) )
                {
                    var x = player.playerEntity.Location.X;
                    var y = player.playerEntity.Location.Y;
                    if (player.BestMove == GameCommand.MoveUp)
                    {
                        if (player.playerEntity.Location.Y > 1)
                            y = player.playerEntity.Location.Y - 1;
                    }
                    if (player.BestMove == GameCommand.MoveDown)
                    {
                        if (player.playerEntity.Location.Y < nextMap.MapHeight)
                            y = player.playerEntity.Location.Y + 1;
                    }
                    if (player.BestMove == GameCommand.MoveLeft)
                    {
                        if (player.playerEntity.Location.X > 1)
                            x = player.playerEntity.Location.X - 1;
                    }
                    if (player.BestMove == GameCommand.MoveRight)
                    {
                        if (player.playerEntity.Location.X < nextMap.MapWidth)
                            x = player.playerEntity.Location.X + 1;
                    }
                    nextMap.GameBlocks[player.playerEntity.Location.X - 1, player.playerEntity.Location.Y - 1].Entity = null;
                    int bombBagPowerUpAdd = 0;
                    int bombRadiusPowerUpAdd = 0;
                    if (nextMap.GameBlocks[x - 1, y - 1].PowerUp != null)
                    {
                        if (nextMap.GameBlocks[x - 1, y - 1].PowerUp is BombBagPowerUpEntity)
                        {
                            bombBagPowerUpAdd = 1;
                        }
                        if (nextMap.GameBlocks[x - 1, y - 1].PowerUp is BombRaduisPowerUpEntity)
                        {
                            bombRadiusPowerUpAdd = 1;
                        }
                        if (nextMap.GameBlocks[x - 1, y - 1].PowerUp is SuperPowerUp)
                        {
                            bombBagPowerUpAdd = 1;
                            bombRadiusPowerUpAdd = 1;
                        }
                        nextMap.GameBlocks[x - 1, y - 1].PowerUp = null;
                    }
                    nextMap.GameBlocks[x - 1, y - 1]
                        .Entity = new PlayerEntity()
                        {
                            Location = new Location()
                            {
                                X = x,
                                Y = y
                            },
                            BombBag = player.playerEntity.BombBag + bombBagPowerUpAdd,
                            BombRadius = player.playerEntity.BombRadius + bombRadiusPowerUpAdd,
                            Key = player.playerEntity.Key,
                            Killed = player.playerEntity.Killed,
                            Name = player.playerEntity.Name,
                            Points = player.playerEntity.Points
                        };
                }
                if (player.BestMove == GameCommand.PlaceBomb)
                {
                    nextMap.GameBlocks[player.playerEntity.Location.X - 1, player.playerEntity.Location.Y - 1]
                        .Bomb = new BombEntity()
                        {
                            BombRadius = player.playerEntity.BombRadius,
                            BombTimer = 5, // player.playerEntity.,  Hmmm problem
                            IsExploding = false,
                            Points = 1,
                            Location = new Location()
                            {
                                X = player.playerEntity.Location.X,
                                Y = player.playerEntity.Location.Y
                            },
                            Owner = new PlayerEntity()
                            {
                                Location = new Location()
                                {
                                    X = player.playerEntity.Location.X,
                                    Y = player.playerEntity.Location.Y
                                },
                                BombBag = player.playerEntity.BombBag,
                                BombRadius = player.playerEntity.BombRadius,
                                Key = player.playerEntity.Key,
                                Killed = player.playerEntity.Killed,
                                Name = player.playerEntity.Name,
                                Points = player.playerEntity.Points
                            }
                        };
                }
            }
            for (var y = 1; y <= nextMap.MapHeight; y++)
            {
                for (var x = 1; x <= nextMap.MapWidth; x++)
                {
                    var block = nextMap.GetBlockAtLocation(x, y);
                    if (block.Bomb != null)
                    {
                        if (block.Bomb.IsExploding)
                        {
                            //TODO Calculate range of bomb
                            nextMap.GameBlocks[x - 1, y - 1].Bomb = null;
                            foreach (var player in playerMoves)
                            {
                                if ((player.playerEntity.Location.X == x) &&
                                    (player.playerEntity.Location.Y == y))
                                {
                                    player.playerEntity.Killed = true;
                                }
                            }
                        }
                        else if (block.Bomb.BombTimer == 1)
                        {
                            nextMap.GameBlocks[x - 1, y - 1].Bomb.IsExploding = true;
                            nextMap.GameBlocks[x - 1, y - 1].Bomb.BombTimer--;
                            foreach (var player in playerMoves) //You will be dead the next round
                            {
                                //TODO Calculate range of bomb
                                if ((player.playerEntity.Location.X == x) &&
                                    (player.playerEntity.Location.Y == y))
                                {
                                    player.playerEntity.Killed = true;
                                }
                            }
                        }
                        else
                        {
                            nextMap.GameBlocks[x - 1, y - 1].Bomb.BombTimer--;
                        }
                    }
                }
            }
            //Debug.WriteLine("After tick:");
            //DrawMap(nextMap);
            return nextMap;
        }


        private void CalculateRangeOfBomb(int x, int y, GameBlock block, ref bool done)
        {
            var bombBlockedXMinusDirection = false;
            var bombBlockedXPlusDirection = false;
            var bombBlockedYMinusDirection = false;
            var bombBlockedYPlusDirection = false;
            for (int range = 1; range <= block.Bomb.BombRadius; range++)
            {
                if ((x - range > 1) && (!bombBlockedXMinusDirection))
                {
                    var xrange = x - range;
                    var yrange = y;
                    bombBlockedXMinusDirection = MarkUnlessBombBlocked(xrange, yrange,
                                                                       bombBlockedXMinusDirection,
                                                                       ref done);
                }
                if ((x + range < _gameMap.MapWidth) && (!bombBlockedXPlusDirection))
                {
                    var xrange = x + range;
                    var yrange = y;
                    bombBlockedXPlusDirection = MarkUnlessBombBlocked(xrange, yrange,
                                                                      bombBlockedXPlusDirection,
                                                                      ref done);
                }
                if ((y - range > 1) && (!bombBlockedYMinusDirection))
                {
                    var xrange = x;
                    var yrange = y - range;
                    bombBlockedYMinusDirection = MarkUnlessBombBlocked(xrange, yrange,
                                                                       bombBlockedYMinusDirection,
                                                                       ref done);
                }
                if ((y + range < _gameMap.MapHeight) && (!bombBlockedYPlusDirection))
                {
                    var xrange = x;
                    var yrange = y + range;
                    bombBlockedYPlusDirection = MarkUnlessBombBlocked(xrange, yrange,
                                                                      bombBlockedYPlusDirection,
                                                                      ref done);
                }
            }
        }

        private bool MarkUnlessBombBlocked(int xrange, int yrange, bool bombBlockedDirection, ref bool done)
        {
            GameBlock blockInRange;
            blockInRange = _gameMap.GetBlockAtLocation(xrange, yrange);
            if (blockInRange.Entity != null)
            {
                if ((blockInRange.Entity is DestructibleWallEntity) ||
                    (blockInRange.Entity is IndestructibleWallEntity) ||
                    (blockInRange.Entity is DestructibleWallEntity) /* ||
                                    (blockInRange.Entity is PlayerEntity)*/)
                {
                    bombBlockedDirection = true;
                }
                else
                {
                    if (myBomb)
                        _mdpMap[xrange, yrange].InRangeOfMyBomb = true;
                    else
                        _mdpMap[xrange, yrange].InRangeOfEnemyBomb = true;

                    if (_mdpMap[xrange, yrange].BombCountDown > bombTimer)
                    {
                        _mdpMap[xrange, yrange].BombCountDown = bombTimer;
                        done = false;
                    }
                }
            }
            else
            {
                if (myBomb)
                    _mdpMap[xrange, yrange].InRangeOfMyBomb = true;
                else
                    _mdpMap[xrange, yrange].InRangeOfEnemyBomb = true;

                if (_mdpMap[xrange, yrange].BombCountDown > bombTimer)
                {
                    _mdpMap[xrange, yrange].BombCountDown = bombTimer;
                    done = false;
                }
            }
            return bombBlockedDirection;
        }
    }
}