using System;
using System.Collections.Generic;
using System.Diagnostics;
using Reference.Commands;
using Reference.Domain.Map;
using Reference.Domain.Map.Entities;
using Reference.Domain.Map.Entities.PowerUps;

namespace Reference.Strategies.MDP
{
    public class MdpTools
    {
        private readonly int WallValue = 10;
        private readonly int PowerUpValue = 100;
        private readonly int SuperPowerUpValue = 500;
        private readonly int PenaltyValue = 30;
        private readonly int EnemyBombPenalty = 90;
        //private readonly int BombValue = -100;
        private readonly int PathWhenBombValue = 5000;
        private readonly int PathWhenMyBombValue = 500;
        private readonly int PathWhenEnemyBombValue = 50;

        private readonly int EndGameOtherPlayerValue = 200;


        public enum MdpTypes
        {
            Bomb,
            //Me,
            OtherPlayer,
            //PowerUp,
            //SuperPowerUp,
            Indestructable,
            //Wall,
            Path,
            //PathAsGoal
            Destructable
        }

        public class PlayersAndMoves
        {
            public PlayerEntity playerEntity;
            public GameCommand BestMove;
            public GameCommand SecondMove;
            public GameCommand ThirdMove;

            public PlayersAndMoves(PlayerEntity entity)
            {
                playerEntity = entity;
                BestMove = new GameCommand();
                SecondMove = new GameCommand();
                ThirdMove = new GameCommand();
            }
        }

        public struct MdpBlock
        {
            public MdpTypes Type;
            public int Value;
            public bool ValidValue;
            public int ItemOnBlockValue;
            public bool ValidItemOnBlockValue;
            public bool InRangeOfMyBomb;
            public bool InRangeOfEnemyBomb;
            public int BombCountDown;
        }

        public MdpBlock[,] _mdpMap;
        private GameMap _gameMap;
        private char _playerKey;
        private List<PlayersAndMoves> _players = new List<PlayersAndMoves>();

        public MdpTools(GameMap gameMap, char playerKey, PlayerEntity[] players)
        {
            _mdpMap = new MdpBlock[gameMap.MapWidth + 1, gameMap.MapHeight + 1];
            _gameMap = gameMap;
            _playerKey = playerKey;
            foreach (var player in players)
            {
                if (player != null)
                    _players.Add(new PlayersAndMoves(player));     
            }
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    _mdpMap[x, y].BombCountDown = int.MaxValue;
                }
            }
        }

        #region bombs
        public bool AssignBombValues()
        {
            var done = true;
            //Enemy bombs
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
                            if (entity.Key == _playerKey)
                            {
                                _mdpMap[x, y].Type = MdpTypes.Path;
                            }
                            else
                                _mdpMap[x, y].Type = MdpTypes.OtherPlayer;
                        }
                    }
                    if (block.Bomb == null) continue;
                    var myBomb = block.Bomb.Owner.Key == _playerKey;
                    if (!myBomb)
                        CalculateRangeOfBomb(x, y, block, myBomb, ref done);
                }
            }

            //My bombs
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
                            if (entity.Key == _playerKey)
                            {
                                _mdpMap[x, y].Type = MdpTypes.Path;
                            }
                            else
                                _mdpMap[x, y].Type = MdpTypes.OtherPlayer;
                        }
                    }
                    if (block.Bomb == null) continue;
                    var myBomb = block.Bomb.Owner.Key == _playerKey;
                    if (!myBomb) continue;
                    if (_mdpMap[x, y].InRangeOfEnemyBomb) //My bomb is part of an enemy bomb chain
                        CalculateRangeOfBomb(x, y, block, false, ref done);
                    else
                        CalculateRangeOfBomb(x, y, block, true, ref done);
                }
            }
            return done;
        }

        private void CalculateRangeOfBomb(int x, int y, GameBlock block, bool myBomb, ref bool done)
        {
            if (myBomb)
                _mdpMap[x, y].InRangeOfMyBomb = true;
            else
                _mdpMap[x, y].InRangeOfEnemyBomb = true;

            var bombBlockedXMinusDirection = false;
            var bombBlockedXPlusDirection = false;
            var bombBlockedYMinusDirection = false;
            var bombBlockedYPlusDirection = false;
            var bombTimer = block.Bomb.BombTimer;
            if (_mdpMap[x, y].BombCountDown < block.Bomb.BombTimer)
                bombTimer = _mdpMap[x, y].BombCountDown;
            for (int range = 1; range <= block.Bomb.BombRadius; range++)
            {
                if ((x - range > 1) && (!bombBlockedXMinusDirection))
                {
                    var xrange = x - range;
                    var yrange = y;
                    bombBlockedXMinusDirection = MarkUnlessBombBlocked(xrange, yrange, 
                                                                       bombBlockedXMinusDirection, myBomb,
                                                                       bombTimer, ref done);
                }
                if ((x + range < _gameMap.MapWidth) && (!bombBlockedXPlusDirection))
                {
                    var xrange = x + range;
                    var yrange = y;
                    bombBlockedXPlusDirection = MarkUnlessBombBlocked(xrange, yrange, 
                                                                      bombBlockedXPlusDirection, myBomb,
                                                                      bombTimer, ref done);
                }
                if ((y - range > 1) && (!bombBlockedYMinusDirection))
                {
                    var xrange = x;
                    var yrange = y - range;
                    bombBlockedYMinusDirection = MarkUnlessBombBlocked(xrange, yrange, 
                                                                       bombBlockedYMinusDirection, myBomb,
                                                                       bombTimer, ref done);
                }
                if ((y + range < _gameMap.MapHeight) && (!bombBlockedYPlusDirection))
                {
                    var xrange = x;
                    var yrange = y + range;
                    bombBlockedYPlusDirection = MarkUnlessBombBlocked(xrange, yrange, 
                                                                      bombBlockedYPlusDirection, myBomb,
                                                                      bombTimer, ref done);
                }
            }
        }

        private bool MarkUnlessBombBlocked(int xrange, int yrange, bool bombBlockedDirection, bool myBomb, int bombTimer, ref bool done)
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
        #endregion

        #region mdpgoalfunction
        public void AssignMdpGoals(bool endGame, char playerKey)
        {
            //TODO If no more walls and powerups, assign goals to corners based on player key
            //TODO or implement an already explored map writing it to disk
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    var block = _gameMap.GetBlockAtLocation(x, y);
                    _mdpMap[x, y].ValidValue = false;
                    _mdpMap[x, y].Value = Int32.MinValue;
                    _mdpMap[x, y].ValidItemOnBlockValue = false;
                    _mdpMap[x, y].ItemOnBlockValue = Int32.MinValue;
                    if ((_mdpMap[_players[0].playerEntity.Location.X, _players[0].playerEntity.Location.Y].InRangeOfMyBomb) ||
                        (_mdpMap[_players[0].playerEntity.Location.X, _players[0].playerEntity.Location.Y].InRangeOfEnemyBomb))
                        AssignBlockEntityValues(_mdpMap, block, x, y, true, endGame, playerKey);
                    else
                        AssignBlockEntityValues(_mdpMap, block, x, y, false, endGame, playerKey);
                }
            }
        }

        private void AssignBlockEntityValues(MdpBlock[,] MdpMap, GameBlock block, int x, int y, bool escape, bool endGame, char playerKey)
        {
            if ((block.Entity == null) &&
                (block.PowerUp == null))
            {
                MdpMap[x, y].Type = MdpTypes.Path;
            }
            if (block.Entity != null)
            {
                if (block.Entity is DestructibleWallEntity)
                {
                    if (escape)
                    {
                        MdpMap[x, y].Type = MdpTypes.Indestructable;
                        MdpMap[x, y].ItemOnBlockValue = WallValue;
                        MdpMap[x, y].Value = WallValue;
                        MdpMap[x, y].ValidItemOnBlockValue = true;
                        MdpMap[x, y].ValidValue = true;
                        return;
                    }
                    else
                    {
                        MdpMap[x, y].Type = MdpTypes.Destructable;
                        MdpMap[x, y].ItemOnBlockValue = WallValue;
                        MdpMap[x, y].Value = WallValue;
                        MdpMap[x, y].ValidItemOnBlockValue = true;
                        MdpMap[x, y].ValidValue = true;
                    }                   
                }
                else if (block.Entity is IndestructibleWallEntity)
                {
                    if (escape)
                    {
                        MdpMap[x, y].Type = MdpTypes.Indestructable;
                        return;
                    }
                    else
                    {
                        MdpMap[x, y].Type = MdpTypes.Indestructable;
                    }
                }                   
                else if (block.Entity is BombEntity)
                {
                    MdpMap[x, y].Type = MdpTypes.Bomb;
                    //_mdpMap[x, y].value = BombValue;
                    //_mdpMap[x, y].validValue = true;
                    if(escape)
                        return;
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
                else if ((block.Entity is BombBagPowerUpEntity) ||
                         (block.Entity is BombRaduisPowerUpEntity))
                {
                    MdpMap[x, y].Type = MdpTypes.Path;
                    MdpMap[x, y].ItemOnBlockValue = PowerUpValue;
                    MdpMap[x, y].ValidItemOnBlockValue = true;
                }
                else if (block.Entity is PlayerEntity)
                {
                    if ((endGame) && (block.Entity as PlayerEntity).Key != playerKey)
                    {
                        MdpMap[x, y].Type = MdpTypes.Path;
                        MdpMap[x, y].ItemOnBlockValue = EndGameOtherPlayerValue;
                        MdpMap[x, y].ValidItemOnBlockValue = true;
                    }
                }
            }
            if (block.PowerUp != null)
            {
                if ((block.PowerUp is BombBagPowerUpEntity) ||
                    (block.PowerUp is BombRaduisPowerUpEntity))
                {
                    MdpMap[x, y].Type = MdpTypes.Path;
                    MdpMap[x, y].ItemOnBlockValue = PowerUpValue;
                    MdpMap[x, y].ValidItemOnBlockValue = true;
                }
                else if (block.PowerUp is SuperPowerUp)
                {
                    MdpMap[x, y].Type = MdpTypes.Path;
                    MdpMap[x, y].ItemOnBlockValue = SuperPowerUpValue;
                    MdpMap[x, y].ValidItemOnBlockValue = true;
                }
            }
            if (block.Bomb != null)
            {
                MdpMap[x, y].Type = MdpTypes.Bomb;
                MdpMap[x, y].ValidValue = false;
                if (escape)
                    return;
            }
            if (escape)
            {
                MdpMap[x, y].Type = MdpTypes.Path;
                if ((!MdpMap[x, y].InRangeOfMyBomb) && (!MdpMap[x, y].InRangeOfEnemyBomb))
                {
                    if (!MdpMap[x, y].ValidItemOnBlockValue)
                    {
                        MdpMap[x, y].ValidItemOnBlockValue = true;
                        MdpMap[x, y].ItemOnBlockValue = PathWhenBombValue;
                    }
                    else
                    {
                        MdpMap[x, y].ItemOnBlockValue += PathWhenBombValue;
                    }
                }
                else if (MdpMap[x, y].InRangeOfMyBomb)
                {
                    if (!MdpMap[x, y].ValidItemOnBlockValue)
                    {
                        MdpMap[x, y].ValidItemOnBlockValue = true;
                        MdpMap[x, y].ItemOnBlockValue = PathWhenMyBombValue * MdpMap[x, y].BombCountDown;
                    }
                    else
                    {
                        MdpMap[x, y].ItemOnBlockValue += PathWhenMyBombValue * MdpMap[x, y].BombCountDown;
                    }
                }
                else if (MdpMap[x, y].InRangeOfEnemyBomb)
                {
                    if (!MdpMap[x, y].ValidItemOnBlockValue)
                    {
                        MdpMap[x, y].ValidItemOnBlockValue = true;
                        MdpMap[x, y].ItemOnBlockValue = PathWhenEnemyBombValue * MdpMap[x, y].BombCountDown;
                    }
                    else
                    {
                        MdpMap[x, y].ItemOnBlockValue += PathWhenEnemyBombValue * MdpMap[x, y].BombCountDown;
                    }
                }
            }
        }
        #endregion

        #region bestmove

        public struct ValuesAndMoves
        {
            public int Value;
            public GameCommand Move;

            public ValuesAndMoves(int value, GameCommand move)
            {
                Value = value;
                Move = move;
            }
        }

        public List<PlayersAndMoves> CalculateBestMoveFromMdp(bool endGame, bool fightOrNotFlight)
        {
            //TODO 
            //1 - We can still get stuck here...
            List<ValuesAndMoves> largestMdpValues;
            foreach (var player in _players)
            {
                largestMdpValues = new List<ValuesAndMoves> {
                    new ValuesAndMoves(int.MinValue, GameCommand.MoveLeft),
                    new ValuesAndMoves(int.MinValue, GameCommand.MoveRight),
                    new ValuesAndMoves(int.MinValue, GameCommand.MoveUp),
                    new ValuesAndMoves(int.MinValue, GameCommand.MoveDown)
                };
                if (player.playerEntity.Location.X > 1)
                {
                    var xoffset = player.playerEntity.Location.X - 1;
                    var yoffset = player.playerEntity.Location.Y;
                    largestMdpValues[0] = new ValuesAndMoves(isBestMoveThisWay(xoffset, yoffset, player.playerEntity), GameCommand.MoveLeft);
                }
                if (player.playerEntity.Location.X < _gameMap.MapWidth)
                {
                    var xoffset = player.playerEntity.Location.X + 1;
                    var yoffset = player.playerEntity.Location.Y;
                    largestMdpValues[1] = new ValuesAndMoves(isBestMoveThisWay(xoffset, yoffset, player.playerEntity), GameCommand.MoveRight);
                }
                if (player.playerEntity.Location.Y > 1)
                {
                    var xoffset = player.playerEntity.Location.X;
                    var yoffset = player.playerEntity.Location.Y - 1;
                    largestMdpValues[2] = new ValuesAndMoves(isBestMoveThisWay(xoffset, yoffset, player.playerEntity), GameCommand.MoveUp);
                }
                if (player.playerEntity.Location.Y < _gameMap.MapHeight)
                {
                    var xoffset = player.playerEntity.Location.X;
                    var yoffset = player.playerEntity.Location.Y + 1;
                    largestMdpValues[3] = new ValuesAndMoves(isBestMoveThisWay(xoffset, yoffset, player.playerEntity), GameCommand.MoveDown);
                }
                removeInvalidMoves(largestMdpValues, player.playerEntity.Location.X, player.playerEntity.Location.Y);
                if (!endGame)
                {
                    player.BestMove = GetLargestAndRemove(largestMdpValues);
                    player.SecondMove = GetLargestAndRemove(largestMdpValues);
                    player.ThirdMove = GetLargestAndRemove(largestMdpValues);
                }
                else 
                {
                    if (fightOrNotFlight) //Go toward enemy player
                    {
                        player.BestMove = GetLargestAndRemove(largestMdpValues);
                        player.SecondMove = GetLargestAndRemove(largestMdpValues);
                        player.ThirdMove = GetLargestAndRemove(largestMdpValues);
                    }
                    else //Run away from enemy player
                    {
                        player.BestMove = GetSmallestAndRemove(largestMdpValues);
                        player.SecondMove = GetSmallestAndRemove(largestMdpValues);
                        player.ThirdMove = GetSmallestAndRemove(largestMdpValues);
                    }
                }
            }
            return _players;
        }

        private void removeInvalidMoves(List<ValuesAndMoves> largestMdpValues, int x, int y)
        {
            for (int i = largestMdpValues.Count - 1; i >= 0; i--)
            {
                if (largestMdpValues[i].Move == GameCommand.MoveDown)
                {
                    if (y >= _gameMap.MapHeight)
                    {
                        largestMdpValues.RemoveAt(i);
                        continue;
                    }
                    var block = _gameMap.GetBlockAtLocation(x, y + 1);
                    if (block.Entity != null)
                    {
                        largestMdpValues.RemoveAt(i);
                        continue;
                    }

                }
                if (largestMdpValues[i].Move == GameCommand.MoveUp)
                {
                    if (y <= 1)
                    {
                        largestMdpValues.RemoveAt(i);
                        continue;
                    }
                    var block = _gameMap.GetBlockAtLocation(x, y - 1);
                    if (block.Entity != null)
                    {
                        largestMdpValues.RemoveAt(i);
                        continue;
                    }
                }
                if (largestMdpValues[i].Move == GameCommand.MoveLeft)
                {
                    if (x <= 1)
                    {
                        largestMdpValues.RemoveAt(i);
                        continue;
                    }
                    var block = _gameMap.GetBlockAtLocation(x - 1, y);
                    if (block.Entity != null)
                    {
                        largestMdpValues.RemoveAt(i);
                        continue;
                    }
                }
                if (largestMdpValues[i].Move == GameCommand.MoveRight)
                {
                    if (x >= _gameMap.MapWidth)
                    {
                        largestMdpValues.RemoveAt(i);
                        continue;
                    }
                    var block = _gameMap.GetBlockAtLocation(x + 1, y);
                    if (block.Entity != null)
                    {
                        largestMdpValues.RemoveAt(i);
                        continue;
                    }
                }
            }
        }

        private static GameCommand GetLargestAndRemove(List<ValuesAndMoves> largestMdpValues)
        {
            var largest = int.MinValue;
            var pos = 0;
            if (largestMdpValues.Count == 0)
                return GameCommand.DoNothing;
            for (int i = largestMdpValues.Count - 1; i >= 0; i--)
            {
                if (largestMdpValues[i].Value > largest)
                {
                    largest = largestMdpValues[i].Value;
                    pos = i;
                }
            }
            var move = largestMdpValues[pos].Move;
            largestMdpValues.RemoveAt(pos);
            if (largest == int.MinValue)
                move = GameCommand.DoNothing;
            return move;
        }

        private static GameCommand GetSmallestAndRemove(List<ValuesAndMoves> largestMdpValues)
        {
            var smallest = int.MaxValue;
            var pos = 0;
            if (largestMdpValues.Count == 0)
                return GameCommand.DoNothing;
            for (int i = largestMdpValues.Count - 1; i >= 0; i--)
            {
                if (largestMdpValues[i].Value < smallest)
                {
                    smallest = largestMdpValues[i].Value;
                    pos = i;
                }
            }
            var move = largestMdpValues[pos].Move;
            largestMdpValues.RemoveAt(pos);
            if (smallest == int.MaxValue)
                move = GameCommand.DoNothing;
            return move;
        }

        private int isBestMoveThisWay(int xoffset, int yoffset, PlayerEntity player)
        {
            var largestMdpValue = int.MinValue;
            if (_mdpMap[xoffset, yoffset].ValidValue &&
                _mdpMap[xoffset, yoffset].Type == MdpTypes.Path)
            {
                if (_mdpMap[xoffset, yoffset].InRangeOfEnemyBomb &&
                    !_mdpMap[player.Location.X, player.Location.Y].InRangeOfEnemyBomb)
                {
                    //can't go that way...
                }
                else if (_mdpMap[xoffset, yoffset].InRangeOfMyBomb &&
                    !_mdpMap[player.Location.X, player.Location.Y].InRangeOfMyBomb)
                {
                    //can't go that way...
                    //TODO can improve this if we can determine it is safe to step into a blast zone
                }
                else
                {
                    largestMdpValue = _mdpMap[xoffset, yoffset].Value;
                }
            }
            return largestMdpValue;
        }
        #endregion

        #region CalculateMdp
        public void CalculateMdp()
        {
            var done = false;
            for (int i = 0; i < 10; i++) //Only parse a maximum of ten times through the map
            {
                done = false;
                for (var y = 1; y <= _gameMap.MapHeight; y++)
                {
                    for (var x = 1; x <= _gameMap.MapWidth; x++)
                    {
                        if (_mdpMap[x, y].Type != MdpTypes.Path)
                        {
                            continue;
                        }
                        bool largestNeigbourValid;
                        var largestNeighbour = GetLargestNeighbour(x, y, out largestNeigbourValid);
                        int largestDifference = 0;
                        done = LargestNeighbourValid(largestNeigbourValid, x, y, largestNeighbour, done, ref largestDifference);
                        
                        //If difference is still too big, mark done
                        //TODO limit the amount of iterations we will go through
                    }
                }
                //DrawMdpMap();
                if (done)
                    break;
            }
        }

        private bool LargestNeighbourValid(bool largestNeigbourValid, int x, int y, int largestNeighbour, bool done, ref int largestDifference)
        {
            int calculatedValue;
            largestDifference = int.MinValue;
            //Calculate our value
            if (largestNeigbourValid)
            {
                if (_mdpMap[x, y].ValidItemOnBlockValue)
                {
                    if (_mdpMap[x, y].ItemOnBlockValue > largestNeighbour)
                    {
                        _mdpMap[x, y].Value = _mdpMap[x, y].ItemOnBlockValue;
                        _mdpMap[x, y].ValidValue = true;
                    }
                    else
                    {
                        if (_mdpMap[x, y].InRangeOfEnemyBomb)
                        {
                            var countDown = _mdpMap[x, y].BombCountDown;
                            if (_mdpMap[x, y].BombCountDown > 9)
                                countDown = 9;
                            var penalty = EnemyBombPenalty*(10 - countDown);
                            calculatedValue = largestNeighbour - penalty;
                        }
                        else
                        {
                            var countDown = _mdpMap[x, y].BombCountDown;
                            if (_mdpMap[x, y].BombCountDown > 9)
                                countDown = 9;
                            var penalty = PenaltyValue * (10 - countDown);
                            calculatedValue = largestNeighbour - penalty;
                        }
                        if (Math.Abs(calculatedValue - _mdpMap[x, y].Value) > 10)
                            done = false;
                        if (Math.Abs(calculatedValue - _mdpMap[x, y].Value) > largestDifference)
                            largestDifference = Math.Abs(calculatedValue - _mdpMap[x, y].Value);
                        _mdpMap[x, y].Value = calculatedValue;
                        _mdpMap[x, y].ValidValue = true;
                    }
                }
                else
                {
                    if (_mdpMap[x, y].InRangeOfEnemyBomb)
                    {
                        var countDown = _mdpMap[x, y].BombCountDown;
                        if (_mdpMap[x, y].BombCountDown > 9)
                            countDown = 9;
                        var penalty = EnemyBombPenalty * (10 - countDown);
                        calculatedValue = largestNeighbour - penalty;
                    }
                    else
                    {
                        var countDown = _mdpMap[x, y].BombCountDown;
                        if (_mdpMap[x, y].BombCountDown > 9)
                            countDown = 9;
                        var penalty = PenaltyValue * (10 - countDown);
                        calculatedValue = largestNeighbour - penalty;
                    }
                    if (Math.Abs(calculatedValue - _mdpMap[x, y].Value) > 10)
                        done = false;
                    if (Math.Abs(calculatedValue - _mdpMap[x, y].Value) > largestDifference)
                        largestDifference = Math.Abs(calculatedValue - _mdpMap[x, y].Value);
                    _mdpMap[x, y].Value = calculatedValue;
                    _mdpMap[x, y].ValidValue = true;
                }
            }
            else if (_mdpMap[x, y].ValidItemOnBlockValue)
            {
                _mdpMap[x, y].Value = _mdpMap[x, y].ItemOnBlockValue;
                _mdpMap[x, y].ValidValue = true;
            }
            else
            {
                done = false;
            }
            return done;
        }

        private int GetLargestNeighbour(int x, int y, out bool largestNeigbourValid)
        {
            var largestNeighbour = int.MinValue;
            var neighboursContribute = 0;
            largestNeigbourValid = false;
            if (x > 1)
            {
                var xoffset = x - 1;
                var yoffset = y;
                if (_mdpMap[xoffset, yoffset].ValidValue)
                {
                    if (_mdpMap[xoffset, yoffset].Value > largestNeighbour)
                    {
                        largestNeighbour = _mdpMap[xoffset, yoffset].Value;
                        largestNeigbourValid = true;
                        neighboursContribute += _mdpMap[xoffset, yoffset].Value/100;
                    }
                }
            }
            if (x < _gameMap.MapWidth)
            {
                var xoffset = x + 1;
                var yoffset = y;
                if (_mdpMap[xoffset, yoffset].ValidValue)
                {
                    if (_mdpMap[xoffset, yoffset].Value > largestNeighbour)
                    {
                        largestNeighbour = _mdpMap[xoffset, yoffset].Value;
                        largestNeigbourValid = true;
                        neighboursContribute += _mdpMap[xoffset, yoffset].Value / 100;
                    }
                }
            }
            if (y > 1)
            {
                var xoffset = x;
                var yoffset = y - 1;
                if (_mdpMap[xoffset, yoffset].ValidValue)
                {
                    if (_mdpMap[xoffset, yoffset].Value > largestNeighbour)
                    {
                        largestNeighbour = _mdpMap[xoffset, yoffset].Value;
                        largestNeigbourValid = true;
                        neighboursContribute += _mdpMap[xoffset, yoffset].Value / 100;
                    }
                }
            }
            if (y < _gameMap.MapHeight)
            {
                var xoffset = x;
                var yoffset = y + 1;
                if (_mdpMap[xoffset, yoffset].ValidValue)
                {
                    if (_mdpMap[xoffset, yoffset].Value > largestNeighbour)
                    {
                        largestNeighbour = _mdpMap[xoffset, yoffset].Value;
                        largestNeigbourValid = true;
                        neighboursContribute += _mdpMap[xoffset, yoffset].Value / 100;
                    }
                }
            }
            return largestNeighbour;// + neighboursContribute;
        }


        #endregion

        public void DrawMdpMap(bool printSign = false)
        {
            //Scale output to biggest value
            var largestValue = int.MinValue;
            var smallestValue = int.MaxValue;
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    if ((_mdpMap[x, y].ValidValue) &&
                        (_mdpMap[x, y].Type == MdpTypes.Path) &&
                        (Math.Abs(_mdpMap[x, y].Value) > largestValue))
                    {
                        largestValue = Math.Abs(_mdpMap[x, y].Value);
                    }
                    if ((_mdpMap[x, y].ValidValue) &&
                        (_mdpMap[x, y].Type == MdpTypes.Path) &&
                        (Math.Abs(_mdpMap[x, y].Value) < smallestValue))
                    {
                        smallestValue = Math.Abs(_mdpMap[x, y].Value);
                    }
                }
            }

            Debug.WriteLine("");
            Debug.WriteLine("");
            if (printSign)
                Debug.WriteLine(" 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2");
            else
                Debug.WriteLine(" 1234567890123456789012");
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                Debug.Write(y % 9);
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    var blk = _mdpMap[x, y];
                    if (_mdpMap[x, y].Type == MdpTypes.OtherPlayer)
                        Debug.Write(printSign ? "BB" : "B");
                    else if (_mdpMap[x, y].Type == MdpTypes.Indestructable)
                        Debug.Write(printSign ? "##" : "#");
                    else if ((_mdpMap[x, y].ValidItemOnBlockValue) && (_mdpMap[x, y].Type != MdpTypes.Path))
                    {
                        if (_mdpMap[x, y].ItemOnBlockValue == PowerUpValue)
                            Debug.Write(printSign ? "!!" : "!");
                        else if (_mdpMap[x, y].ItemOnBlockValue == SuperPowerUpValue)
                            Debug.Write(printSign ? "$$" : "$");
                        else if (_mdpMap[x, y].ItemOnBlockValue == PathWhenBombValue)
                            Debug.Write(printSign ? ".." : ".");
                        else if (_mdpMap[x, y].ItemOnBlockValue == WallValue)
                            Debug.Write(printSign ? "++" : "+");
                    }
                    else if (_mdpMap[x, y].ValidValue == false)
                        Debug.Write(printSign ? "??" : "?");
                    else
                    {
                        if (Math.Abs(_mdpMap[x, y].Value) <= largestValue)
                        {
                            //Console.ForegroundColor = _mdpMap[x, y].value > 0 ? ConsoleColor.Red : ConsoleColor.Yellow;
                            if (printSign)
                                Debug.Write(_mdpMap[x, y].Value > 0 ? "-" : "=");
                            var largestDiff = Math.Abs(largestValue) - Math.Abs(smallestValue);
                            var thisValueOverSmallest = Math.Abs(_mdpMap[x, y].Value) - Math.Abs(smallestValue);
                            if (largestDiff != 0)
                                Debug.Write(((int)(Math.Abs(thisValueOverSmallest) * 9 / Math.Abs(largestDiff))).ToString());
                            else
                                Debug.Write(printSign ? "??" : "?");
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

        public bool areWeInRangeOfBomb(PlayerEntity player)
        {
            return (_mdpMap[player.Location.X, player.Location.Y].InRangeOfMyBomb ||
                    _mdpMap[player.Location.X, player.Location.Y].InRangeOfEnemyBomb);
        }
    }
}