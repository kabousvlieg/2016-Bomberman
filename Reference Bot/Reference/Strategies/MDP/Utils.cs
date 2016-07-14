using System;
using System.Diagnostics;
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

        private PlayerEntity _player = null;
        public PlayerEntity Player => _player ?? (_player = getPlayer());

        public PlayerEntity getPlayer()
        {
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
                            return block.Entity as PlayerEntity;
                        }
                    }
                }
            }
            return null;
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

        public void tickTheMap(ref GameMap gameMap, GameCommand bestMove)
        {
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    var block = _gameMap.GetBlockAtLocation(x, y);
                    if (block.Bomb != null)
                    {
                        if (block.Bomb.IsExploding)
                        {
                            _gameMap.GameBlocks[x - 1, y - 1].Bomb = null;
                        }
                        else if (block.Bomb.BombTimer == 1)
                        {
                            _gameMap.GameBlocks[x - 1, y - 1].Bomb.IsExploding = true;
                            _gameMap.GameBlocks[x - 1, y - 1].Bomb.BombTimer--;
                        }
                        else
                        {
                            _gameMap.GameBlocks[x - 1, y - 1].Bomb.BombTimer--;
                        }
                    }

                }
            }
        }
    }
}