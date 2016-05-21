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

        public GameCommand OverrideMdpMoveWithRuleEngine(GameCommand bestMdpMove)
        {
            
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
    }
}