using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reference.Domain.Map;
using Reference.Domain.Map.Entities;
using Reference.Domain.Map.Entities.PowerUps;

namespace Reference.Strategies.MDP.Tests
{
    public class TestUtils
    {
        public static GameMap TestMap(char[][] map)
        {
            Assert.IsTrue(map.Length == 21);
            for (var y = 0; y < map.Length; y++)
                Assert.IsTrue(map[y].Length == 21);
            var gm = new GameMap
            {
                MapWidth = 21,
                MapHeight = 21,
                GameBlocks = new GameBlock[21,21],
                MapSeed = 1
            };
            for (var y = 0; y < map.Length; y++)
            {
                for (var x = 0; x < 21; x++)
                {
                    switch (map[x][y])
                    {
                        case '#':
                            assignIndestructableWall(gm, x, y);
                            break;
                        case '+':
                            assignDestructableWall(gm, x, y);
                            break;
                        case '&':
                            assignBombBagPowerUp(gm, x, y);
                            break;
                        case '!':
                            assignBombRadiusPowerUp(gm, x, y);
                            break;
                        case '$':
                            assignSuperPowerUp(gm, x, y);
                            break;
                        case 'A':
                        case 'B':
                        case 'C':
                        case 'D':
                            assignPlayer(map, gm, x, y);
                            break;
                        case '*':
                            assignBombExplode(gm, x, y);
                            break;
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                            assignBombCountdown(map, gm, x, y);
                            break;
                        default:
                            assignPath(gm, x, y);
                            break;
                    } 
                }
            }
            return gm;
        }

        private static void assignPath(GameMap gm, int x, int y)
        {
            gm.GameBlocks[x, y] = new GameBlock()
            {
                Location = new Location()
                {
                    X = x + 1,
                    Y = y + 1
                }
            };
        }

        private static void assignBombCountdown(char[][] map, GameMap gm, int x, int y)
        {
            gm.GameBlocks[x, y] = new GameBlock()
            {
                Bomb = new BombEntity()
                {
                    BombRadius = 1,
                    BombTimer = (int) char.GetNumericValue(map[x][y]),
                    IsExploding = false,
                    Points = 0,
                    Location = new Location()
                    {
                        X = x,
                        Y = y
                    },
                    Owner = new PlayerEntity()
                    {
                        Location = new Location()
                        {
                            X = x+1,
                            Y = y+1
                        },
                        BombBag = 1,
                        BombRadius = 1,
                        Key = 'D',
                        Killed = false,
                        Name = "Player D",
                        Points = 1
                    }
                },
                Location = new Location()
                {
                    X = x+1,
                    Y = y+1
                }
            };
        }

        private static void assignBombExplode(GameMap gm, int x, int y)
        {
            gm.GameBlocks[x, y] = new GameBlock()
            {
                Bomb = new BombEntity()
                {
                    BombRadius = 0,
                    BombTimer = 0,
                    IsExploding = true,
                    Points = 0,
                    Location = new Location()
                    {
                        X = x+1,
                        Y = y+1
                    },
                    Owner = new PlayerEntity()
                    {
                        Location = new Location()
                        {
                            X = x+1,
                            Y = y+1
                        },
                        BombBag = 1,
                        BombRadius = 1,
                        Key = 'D',
                        Killed = false,
                        Name = "Player D",
                        Points = 1
                    }
                },
                Location = new Location()
                {
                    X = x+1,
                    Y = y+1
                }
            };
        }

        private static void assignPlayer(char[][] map, GameMap gm, int x, int y)
        {
            gm.GameBlocks[x, y] = new GameBlock()
            {
                Entity = new PlayerEntity()
                {
                    Location = new Location()
                    {
                        X = x+1,
                        Y = y+1
                    },
                    BombBag = 1,
                    BombRadius = 1,
                    Key = map[x][y],
                    Killed = false,
                    Name = "Player",
                    Points = 1
                },
                Location = new Location()
                {
                    X = x+1,
                    Y = y+1
                }
            };
        }

        private static void assignSuperPowerUp(GameMap gm, int x, int y)
        {
            gm.GameBlocks[x, y] = new GameBlock()
            {
                PowerUp = new SuperPowerUp()
                {
                    Location = new Location()
                    {
                        X = x+1,
                        Y = y+1
                    }
                },
                Location = new Location()
                {
                    X = x+1,
                    Y = y+1
                }
            };
        }

        private static void assignBombRadiusPowerUp(GameMap gm, int x, int y)
        {
            gm.GameBlocks[x, y] = new GameBlock()
            {
                PowerUp = new BombRaduisPowerUpEntity()
                {
                    Location = new Location()
                    {
                        X = x+1,
                        Y = y+1
                    }
                },
                Location = new Location()
                {
                    X = x+1,
                    Y = y+1
                }
            };
        }

        private static void assignBombBagPowerUp(GameMap gm, int x, int y)
        {
            gm.GameBlocks[x, y] = new GameBlock()
            {
                PowerUp = new BombBagPowerUpEntity()
                {
                    Location = new Location()
                    {
                        X = x+1,
                        Y = y+1
                    }
                },
                Location = new Location()
                {
                    X = x+1,
                    Y = y+1
                }
            };
        }

        private static void assignDestructableWall(GameMap gm, int x, int y)
        {
            gm.GameBlocks[x, y] = new GameBlock()
            {
                Entity = new DestructibleWallEntity()
                {
                    Location = new Location()
                    {
                        X = x+1,
                        Y = y+1
                    }
                },
                Location = new Location()
                {
                    X = x+1,
                    Y = y+1
                }
            };
        }

        private static void assignIndestructableWall(GameMap gm, int x, int y)
        {
            gm.GameBlocks[x, y] = new GameBlock()
            {
                Entity = new IndestructibleWallEntity()
                {
                    Location = new Location()
                    {
                        X = x+1,
                        Y = y+1
                    }
                },
                Location = new Location()
                {
                    X = x+1,
                    Y = y+1
                }
            };
        }
    }
}