using Reference.Commands;
using Reference.Domain.Map;
using Reference.Domain.Map.Entities;

namespace Reference.Strategies.MDP
{
    public class RuleEngine
    {
        private PlayerEntity _player;
        private GameMap _gameMap;

        public RuleEngine(GameMap gameMap, PlayerEntity player)
        {
            _player = player;
            _gameMap = gameMap;
        }

        public GameCommand OverrideMdpMoveWithRuleEngine(GameCommand bestMdpMove, MdpTools mdp)
        {            
            //Check for survival
            //Will our bestMdpMove move into explosion
            //Will our bestMdpMove move into range of bomb

            //Check if we can blow up the enemy
            //Check if we can plant a bomb
            if (CanWePlantABomb(mdp))
                return GameCommand.PlaceBomb;
            if (CanWeBlowABomb(bestMdpMove))
                return GameCommand.TriggerBomb;
            return bestMdpMove;
        }

        private bool CanWeBlowABomb(GameCommand bestMdpMove)
        {
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    var block = _gameMap.GetBlockAtLocation(x, y);
                    if (block.Bomb?.Owner.Key == _player.Key)
                    {
                        if (block.Bomb.IsExploding ||
                            block.Bomb.BombTimer == 1)
                            continue;
                        if (bestMdpMove == GameCommand.DoNothing ||
                            _player.BombBag == 0)
                            return true;
                    }
                }
            }
            return false;
        }

        private bool CanWePlantABomb(MdpTools mdp)
        {
            if (_player.BombBag == 0)
                return false;

            var bombBlockedXMinusDirection = false;
            var bombBlockedXPlusDirection = false;
            var bombBlockedYMinusDirection = false;
            var bombBlockedYPlusDirection = false;
            for (int range = 1; range <= _player.BombRadius; range++)
            {
                if ((_player.Location.X - range > 1) && (!bombBlockedXMinusDirection))
                {
                    var xrange = _player.Location.X - range;
                    var yrange = _player.Location.Y;
                    if (PlantBombUnlessBombBlocked(xrange, yrange,
                                                   ref bombBlockedXMinusDirection,
                                                   mdp))
                    {
                        return true;
                    }

                }
                if ((_player.Location.X + range < _gameMap.MapWidth) && (!bombBlockedXPlusDirection))
                {
                    var xrange = _player.Location.X + range;
                    var yrange = _player.Location.Y;
                    if (PlantBombUnlessBombBlocked(xrange, yrange,
                                                   ref bombBlockedXPlusDirection,
                                                   mdp))
                    {
                        return true;
                    }
                }
                if ((_player.Location.Y - range > 1) && (!bombBlockedYMinusDirection))
                {
                    var xrange = _player.Location.X;
                    var yrange = _player.Location.Y - range;
                    if (PlantBombUnlessBombBlocked(xrange, yrange,
                                                   ref bombBlockedYMinusDirection,
                                                   mdp))
                    {
                        return true;
                    }
                }
                if ((_player.Location.Y + range < _gameMap.MapHeight) && (!bombBlockedYPlusDirection))
                {
                    var xrange = _player.Location.X;
                    var yrange = _player.Location.Y + range;
                    if (PlantBombUnlessBombBlocked(xrange, yrange,
                                                   ref bombBlockedYPlusDirection,
                                                   mdp))
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
                if ((blockInRange.Entity is DestructibleWallEntity) &&
                    (!mdpBlockInRange.InRangeOfMyBomb) &&
                    (!mdpBlockInRange.InRangeOfEnemyBomb))
                    return true;
            }
            return false;
        }
    }
}