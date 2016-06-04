using System;
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
        private readonly int SuperPowerUpValue = 200;
        private readonly int PenaltyValue = 3;
        //private readonly int BombValue = -100;
        private readonly int PathWhenMyBombValue = 50;
        private readonly int PathWhenEnemyBombValue = 500;


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

        private enum BombOwners
        {
            Me,
            Other
        }

        private struct MdpBlock
        {
            public MdpTypes Type;
            public int Value;
            public bool ValidValue;
            public int ItemOnBlockValue;
            public bool ValidItemOnBlockValue;
            public bool InRangeOfMyBomb;
            public bool InRangeOfEnemyBomb;
            public BombOwners BombOwner;
            public int BombCountDown;
        }

        private MdpBlock[,] _mdpMap;
        private GameMap _gameMap;
        private char _playerKey;
        private PlayerEntity _player;

        public MdpTools(GameMap gameMap, char playerKey, PlayerEntity player)
        {
            _mdpMap = new MdpBlock[gameMap.MapWidth + 1, gameMap.MapHeight + 1];
            _gameMap = gameMap;
            _playerKey = playerKey;
            _player = player;
        }

        #region bombs
        public void assignBombValues()
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
                    CalculateRangeOfBomb(x, y, block, myBomb);
                }
            }
        }

        private void CalculateRangeOfBomb(int x, int y, GameBlock block, bool myBomb)
        {
            if (myBomb)
                _mdpMap[x, y].InRangeOfMyBomb = true;
            else
                _mdpMap[x, y].InRangeOfEnemyBomb = true;

            bool bombBlockedXMinusDirection = false;
            bool bombBlockedXPlusDirection = false;
            bool bombBlockedYMinusDirection = false;
            bool bombBlockedYPlusDirection = false;
            for (int range = 1; range <= block.Bomb.BombRadius; range++)
            {
                if ((x - range > 1) && (!bombBlockedXMinusDirection))
                {
                    var xrange = x - range;
                    var yrange = y;
                    bombBlockedXMinusDirection = MarkUnlessBombBlocked(xrange, yrange, bombBlockedXMinusDirection, myBomb);
                }
                if ((x + range < _gameMap.MapWidth) && (!bombBlockedXPlusDirection))
                {
                    var xrange = x + range;
                    var yrange = y;
                    bombBlockedXPlusDirection = MarkUnlessBombBlocked(xrange, yrange, bombBlockedXMinusDirection, myBomb);
                }
                if ((y - range > 1) && (!bombBlockedYMinusDirection))
                {
                    var xrange = x;
                    var yrange = y - range;
                    bombBlockedXPlusDirection = MarkUnlessBombBlocked(xrange, yrange, bombBlockedXMinusDirection, myBomb);
                }
                if ((y + range < _gameMap.MapHeight) && (!bombBlockedYPlusDirection))
                {
                    var xrange = x;
                    var yrange = y + range;
                    bombBlockedXPlusDirection = MarkUnlessBombBlocked(xrange, yrange, bombBlockedXMinusDirection, myBomb);
                }
            }
        }

        private bool MarkUnlessBombBlocked(int xrange, int yrange, bool bombBlockedXMinusDirection, bool myBomb)
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
                    bombBlockedXMinusDirection = true;
                }
                else
                {
                    if (myBomb)
                        _mdpMap[xrange, yrange].InRangeOfMyBomb = true;
                    else
                        _mdpMap[xrange, yrange].InRangeOfEnemyBomb = true;
                }
            }
            else
            {
                if (myBomb)
                    _mdpMap[xrange, yrange].InRangeOfMyBomb = true;
                else
                    _mdpMap[xrange, yrange].InRangeOfEnemyBomb = true;
            }
            return bombBlockedXMinusDirection;
        }
        #endregion

        #region mdpgoalfunction
        public void AssignMdpGoals()
        {
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    var block = _gameMap.GetBlockAtLocation(x, y);
                    _mdpMap[x, y].ValidValue = false;
                    _mdpMap[x, y].Value = Int32.MinValue;
                    _mdpMap[x, y].ValidItemOnBlockValue = false;
                    _mdpMap[x, y].ItemOnBlockValue = Int32.MinValue;
                    if ((_mdpMap[_player.Location.X, _player.Location.Y].InRangeOfMyBomb) ||
                        (_mdpMap[_player.Location.X, _player.Location.Y].InRangeOfEnemyBomb))
                        AssignBlockEntityValuesForEscape(_mdpMap, block, x, y);
                    else
                        AssignBlockEntityValues(_mdpMap, block, x, y);
                }
            }
        }

        private void AssignBlockEntityValues(MdpBlock[,] MdpMap, GameBlock block, int x, int y)
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
                    //_mdpMap[x, y].value = BombValue;
                    //_mdpMap[x, y].validValue = true;
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
        }

        private void AssignBlockEntityValuesForEscape(MdpBlock[,] mdpMap, GameBlock block, int x, int y)
        {
            //TODO More advanced pathing in bomb zone, especially if the bomb is ours
            if (block.Entity != null)
            {
                if (block.Entity is DestructibleWallEntity)
                {
                    mdpMap[x, y].Type = MdpTypes.Indestructable;
                    return;
                }
                else if (block.Entity is IndestructibleWallEntity)
                {
                    mdpMap[x, y].Type = MdpTypes.Indestructable;
                    return;
                }
            }
            
            if (!mdpMap[x, y].InRangeOfMyBomb)
            {
                mdpMap[x, y].Type = MdpTypes.Path;
                mdpMap[x, y].ValidItemOnBlockValue = true;
                mdpMap[x, y].ItemOnBlockValue = PathWhenMyBombValue;
            }
            else if (!mdpMap[x, y].InRangeOfEnemyBomb)
            {
                mdpMap[x, y].Type = MdpTypes.Path;
                mdpMap[x, y].ValidItemOnBlockValue = true;
                mdpMap[x, y].ItemOnBlockValue = PathWhenEnemyBombValue;
            }
            else
            {
                mdpMap[x, y].Type = MdpTypes.Path;
            }
        }
        #endregion

        #region bestmove
        public GameCommand CalculateBestMoveFromMdp()
        {
            //TODO 
            //1 - We can still get stuck here...
            var largestMdpValue = int.MinValue;
            var bestMove = GameCommand.DoNothing;
            if (_player.Location.X > 1)
            {
                var xoffset = _player.Location.X - 1;
                var yoffset = _player.Location.Y;
                largestMdpValue = isBestMoveThisWay(xoffset, yoffset, largestMdpValue, ref bestMove, GameCommand.MoveLeft);
            }
            if (_player.Location.X < _gameMap.MapWidth)
            {
                var xoffset = _player.Location.X + 1;
                var yoffset = _player.Location.Y;
                largestMdpValue = isBestMoveThisWay(xoffset, yoffset, largestMdpValue, ref bestMove, GameCommand.MoveRight);
            }
            if (_player.Location.Y > 1)
            {
                var xoffset = _player.Location.X;
                var yoffset = _player.Location.Y - 1;
                largestMdpValue = isBestMoveThisWay(xoffset, yoffset, largestMdpValue, ref bestMove, GameCommand.MoveUp);
            }
            if (_player.Location.Y < _gameMap.MapHeight)
            {
                var xoffset = _player.Location.X;
                var yoffset = _player.Location.Y + 1;
                largestMdpValue = isBestMoveThisWay(xoffset, yoffset, largestMdpValue, ref bestMove, GameCommand.MoveDown);
            }
            return bestMove;
        }

        private int isBestMoveThisWay(int xoffset, int yoffset, int largestMdpValue, ref GameCommand bestMove, GameCommand thisWay)
        {
            if (_mdpMap[xoffset, yoffset].ValidValue &&
                _mdpMap[xoffset, yoffset].Type == MdpTypes.Path &&
                _mdpMap[xoffset, yoffset].Value > largestMdpValue)
            {
                if (_mdpMap[xoffset, yoffset].InRangeOfEnemyBomb &&
                    !_mdpMap[_player.Location.X, _player.Location.Y].InRangeOfEnemyBomb)
                {
                    //can't go that way...
                }
                //Todo what about my bombs
                else
                {
                    largestMdpValue = _mdpMap[xoffset, yoffset].Value;
                    bestMove = thisWay;
                }
            }
            return largestMdpValue;
        }
        #endregion

        #region CalculateMdp
        public void CalculateMdp()
        {
            //TODO notes:
            //1 - What about squares with multiple destructible walls, should be worth more?

            var stillNotDone = false;
            var stopwatch = Stopwatch.StartNew();
            do
            {
                if (stopwatch.ElapsedMilliseconds > 500)
                    break;
                stillNotDone = false;
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
                        stillNotDone = LargestNeighbourValid(largestNeigbourValid, x, y, largestNeighbour, stillNotDone);
                        //If difference is still too big, mark stillNotDone
                        //TODO limit the amount of iterations we will go through
                    }
                }
                //DrawMdpMap();
            } while (stillNotDone);
            DrawMdpMap();
        }

        private bool LargestNeighbourValid(bool largestNeigbourValid, int x, int y, int largestNeighbour, bool stillNotDone)
        {
            int calculatedValue;
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
                        calculatedValue = largestNeighbour - PenaltyValue;
                        if (Math.Abs(calculatedValue - _mdpMap[x, y].Value) > 10)
                            stillNotDone = true;
                        _mdpMap[x, y].Value = calculatedValue;
                        _mdpMap[x, y].ValidValue = true;
                    }
                }
                else
                {
                    calculatedValue = largestNeighbour - PenaltyValue;
                    if (Math.Abs(calculatedValue - _mdpMap[x, y].Value) > 10)
                        stillNotDone = true;
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
                stillNotDone = true;
            }
            return stillNotDone;
        }

        private int GetLargestNeighbour(int x, int y, out bool largestNeigbourValid)
        {
            var largestNeighbour = int.MinValue;
            largestNeigbourValid = false;
            if (x > 1)
            {
                var xoffset = x - 1;
                var yoffset = y;
                largestNeighbour = CheckLargestNeighbour(xoffset, yoffset, largestNeighbour, ref largestNeigbourValid);
            }
            if (x < _gameMap.MapWidth)
            {
                var xoffset = x + 1;
                var yoffset = y;
                largestNeighbour = CheckLargestNeighbour(xoffset, yoffset, largestNeighbour, ref largestNeigbourValid);
            }
            if (y > 1)
            {
                var xoffset = x;
                var yoffset = y - 1;
                largestNeighbour = CheckLargestNeighbour(xoffset, yoffset, largestNeighbour, ref largestNeigbourValid);
            }
            if (y < _gameMap.MapHeight)
            {
                var xoffset = x;
                var yoffset = y + 1;
                largestNeighbour = CheckLargestNeighbour(xoffset, yoffset, largestNeighbour, ref largestNeigbourValid);
            }
            return largestNeighbour;
        }

        private int CheckLargestNeighbour(int xoffset, int yoffset, int largestNeighbour, ref bool largestNeigbourValid)
        {
            if (_mdpMap[xoffset, yoffset].ValidValue)
            {
                if (_mdpMap[xoffset, yoffset].Value > largestNeighbour)
                {
                    largestNeighbour = _mdpMap[xoffset, yoffset].Value;
                    largestNeigbourValid = true;
                }
            }
            return largestNeighbour;
        }
        #endregion

        private void DrawMdpMap(bool printSign = false)
        {
            //Scale output to biggest value
            var largestValue = 1;
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
                }
            }

            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine(" 1234567890123456789012");
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                Debug.Write(y % 9);
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    if (_mdpMap[x, y].Type == MdpTypes.OtherPlayer)
                        Debug.Write(printSign ? "BB" : "B");
                    else if (_mdpMap[x, y].Type == MdpTypes.Indestructable)
                        Debug.Write(printSign ? "##" : "#");
                    else if (_mdpMap[x, y].ValidItemOnBlockValue)
                    {
                        if (_mdpMap[x, y].ItemOnBlockValue == PowerUpValue)
                            Debug.Write(printSign ? "!!" : "!");
                        else if (_mdpMap[x, y].ItemOnBlockValue == SuperPowerUpValue)
                            Debug.Write(printSign ? "$$" : "$");
                        else if (_mdpMap[x, y].ItemOnBlockValue == PathWhenMyBombValue)
                            Debug.Write(printSign ? ".." : ".");
                        else if (_mdpMap[x, y].ItemOnBlockValue == PathWhenEnemyBombValue)
                            Debug.Write(printSign ? ",," : ",");
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
                            Debug.Write(((int)(Math.Abs(_mdpMap[x, y].Value) * 9 / largestValue)).ToString());
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

        public bool areWeInRangeOfBomb()
        {
            return (_mdpMap[_player.Location.X, _player.Location.Y].InRangeOfMyBomb ||
                    _mdpMap[_player.Location.X, _player.Location.Y].InRangeOfEnemyBomb);
        }
    }
}