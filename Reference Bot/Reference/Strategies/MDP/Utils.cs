using System;
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

        public static void tickTheMap(ref GameMap gameMap, List<MdpTools.PlayersAndMoves> playerMoves)
        {
            for (var i = 0; i < playerMoves.Count; i++)
            {
                //TODO Move the player
                //Bombs kills player
                //Player picks up powerup
                //Player plants bomb
                //Player detonates bomb
            }
            for (var y = 1; y <= gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= gameMap.MapWidth; x++)
                {
                    var block = gameMap.GetBlockAtLocation(x, y);
                    if (block.Bomb != null)
                    {
                        if (block.Bomb.IsExploding)
                        {
                            gameMap.GameBlocks[x - 1, y - 1].Bomb = null;
                        }
                        else if (block.Bomb.BombTimer == 1)
                        {
                            gameMap.GameBlocks[x - 1, y - 1].Bomb.IsExploding = true;
                            gameMap.GameBlocks[x - 1, y - 1].Bomb.BombTimer--;
                        }
                        else
                        {
                            gameMap.GameBlocks[x - 1, y - 1].Bomb.BombTimer--;
                        }
                    }

                }
            }
        }
    }
}