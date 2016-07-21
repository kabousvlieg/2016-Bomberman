using System;
using Reference.Commands;
using Reference.Domain.Map;
using Reference.Domain.Map.Entities;

namespace Reference.Strategies.MDP
{
    public class RuleEngine
    {
        private PlayerEntity[] _players;
        private GameMap _gameMap;

        public RuleEngine(GameMap gameMap, PlayerEntity[] players)
        {
            _players = players;
            _gameMap = gameMap;
        }

        public GameCommand OverrideMdpMoveWithRuleEngine(GameCommand bestMdpMove, MdpTools mdp, PlayerEntity player)
        {            
            //Check for survival
            //Will our bestMdpMove move into explosion
            if (WalkIntoExplosion(bestMdpMove, player))
                return GameCommand.DoNothing;
            if (!mdp.areWeInRangeOfBomb(player))
            {

                //Check if we can blow up the enemy
                //Check if we can plant a bomb
                if (CanWePlantABomb(mdp, player))
                    return GameCommand.PlaceBomb;
                if (CanWeBlowABomb(bestMdpMove, player))
                    return GameCommand.TriggerBomb;
            }
            return bestMdpMove;
        }

        private bool WalkIntoExplosion(GameCommand bestMdpMove, PlayerEntity player)
        {
            int x, y;
            switch (bestMdpMove)
            {
                case GameCommand.MoveUp:
                    x = player.Location.X;
                    y = player.Location.Y - 1;
                    break;
                case GameCommand.MoveLeft:
                    x = player.Location.X - 1;
                    y = player.Location.Y;
                    break;
                case GameCommand.MoveRight:
                    x = player.Location.X + 1;
                    y = player.Location.Y;
                    break;
                case GameCommand.MoveDown:
                    x = player.Location.X + 1;
                    y = player.Location.Y;
                    break;
                case GameCommand.PlaceBomb:
                    return false;
                case GameCommand.TriggerBomb:
                    return false;
                case GameCommand.DoNothing:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bestMdpMove), bestMdpMove, null);
            }
            var block = _gameMap.GetBlockAtLocation(x, y);
            return block.Exploding;
        }

        private bool CanWeBlowABomb(GameCommand bestMdpMove, PlayerEntity player)
        {
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    var block = _gameMap.GetBlockAtLocation(x, y);
                    if (block.Bomb?.Owner.Key == player.Key)
                    {
                        if (block.Bomb.IsExploding || block.Bomb.BombTimer == 1)
                            continue;
                        if (bestMdpMove == GameCommand.DoNothing || player.BombBag == 0)
                            return true;
                    }
                }
            }
            return false;
        }

        private bool CanWePlantABomb(MdpTools mdp, PlayerEntity player)
        {
            if (player.BombBag == 0)
                return false;

            var bombBlockedXMinusDirection = false;
            var bombBlockedXPlusDirection = false;
            var bombBlockedYMinusDirection = false;
            var bombBlockedYPlusDirection = false;
            for (int range = 1; range <= player.BombRadius; range++)
            {
                if ((player.Location.X - range > 1) && (!bombBlockedXMinusDirection))
                {
                    var xrange = player.Location.X - range;
                    var yrange = player.Location.Y;
                    if (PlantBombUnlessBombBlocked(xrange, yrange, ref bombBlockedXMinusDirection, mdp))
                    {
                        return true;
                    }
                }
                if ((player.Location.X + range < _gameMap.MapWidth) && (!bombBlockedXPlusDirection))
                {
                    var xrange = player.Location.X + range;
                    var yrange = player.Location.Y;
                    if (PlantBombUnlessBombBlocked(xrange, yrange, ref bombBlockedXPlusDirection, mdp))
                    {
                        return true;
                    }
                }
                if ((player.Location.Y - range > 1) && (!bombBlockedYMinusDirection))
                {
                    var xrange = player.Location.X;
                    var yrange = player.Location.Y - range;
                    if (PlantBombUnlessBombBlocked(xrange, yrange, ref bombBlockedYMinusDirection, mdp))
                    {
                        return true;
                    }
                }
                if ((player.Location.Y + range < _gameMap.MapHeight) && (!bombBlockedYPlusDirection))
                {
                    var xrange = player.Location.X;
                    var yrange = player.Location.Y + range;
                    if (PlantBombUnlessBombBlocked(xrange, yrange, ref bombBlockedYPlusDirection, mdp))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool PlantBombUnlessBombBlocked(int xrange, int yrange, ref bool bombBlockedDirection, MdpTools mdp)
        {
            GameBlock blockInRange;
            MdpTools.MdpBlock mdpBlockInRange;
            blockInRange = _gameMap.GetBlockAtLocation(xrange, yrange);
            mdpBlockInRange = mdp._mdpMap[xrange, yrange];
            if (blockInRange.Entity == null) return false;
            if (blockInRange.Entity is IndestructibleWallEntity)
            {
                bombBlockedDirection = true;
                return false;
            }
            else
            {
                if ((blockInRange.Entity is DestructibleWallEntity) && (!mdpBlockInRange.InRangeOfMyBomb) && (!mdpBlockInRange.InRangeOfEnemyBomb))
                    return true;
            }
            return false;
        }
    }
}