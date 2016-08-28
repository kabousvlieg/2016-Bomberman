using System;
using System.Collections.Generic;
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

        public void OverrideMdpMoveWithRuleEngine(ref List<MdpTools.PlayersAndMoves> player, MdpTools mdp)
        {
            for (int i = 0; i < player.Count; i++)
            {
                //Check for survival
                //Will our mdpMove move into explosion
                if (WalkIntoExplosion(player[i].BestMove, player[i].playerEntity))
                    player[i].BestMove = GameCommand.DoNothing;   
                if (WalkIntoExplosion(player[i].SecondMove, player[i].playerEntity))
                    player[i].BestMove = GameCommand.DoNothing;
                if (WalkIntoExplosion(player[i].ThirdMove, player[i].playerEntity))
                    player[i].BestMove = GameCommand.DoNothing;
                //player[i] = MoveDedMovesDown(player[i]);
                    
                if (!mdp.areWeInRangeOfBomb(player[i].playerEntity))
                {
                    //Check if we can blow up the enemy
                    //Check if we can plant a bomb
                    if (CanWePlantABomb(mdp, player[i].playerEntity))
                        player[i] = OverrideWithNewBestMove(player[i], GameCommand.PlaceBomb);
                    if (CanWeBlowABomb(player[i].BestMove, player[i].playerEntity))
                        player[i] = OverrideWithNewBestMove(player[i], GameCommand.TriggerBomb);
                    if (CanWeBlowABomb(player[i].SecondMove, player[i].playerEntity))
                        player[i] = OverrideWithNewSecondMove(player[i], GameCommand.TriggerBomb);
                    if (CanWeBlowABomb(player[i].ThirdMove, player[i].playerEntity))
                        player[i] = OverrideWithNewThirdMove(player[i], GameCommand.TriggerBomb);
                }
            }
        }

        public void EliminateDuplicateMoves(ref List<MdpTools.PlayersAndMoves> playerMoves)
        {
            foreach (var player in playerMoves)
            {
                if (player.SecondMove == player.BestMove)
                {
                    if (player.ThirdMove != player.BestMove)
                        player.SecondMove = player.ThirdMove;
                    else
                        player.SecondMove = GameCommand.ImDed;
                }
                    
                if (player.ThirdMove == player.SecondMove)
                    player.ThirdMove = GameCommand.ImDed;
                if (player.ThirdMove == player.BestMove)
                    player.ThirdMove = GameCommand.ImDed;
            }
        }

        private MdpTools.PlayersAndMoves OverrideWithNewBestMove(MdpTools.PlayersAndMoves player, GameCommand command)
        {
            player.ThirdMove = player.SecondMove;
            player.SecondMove = player.BestMove;
            player.BestMove = command;
            return player;
        }

        private MdpTools.PlayersAndMoves OverrideWithNewSecondMove(MdpTools.PlayersAndMoves player, GameCommand command)
        {
            player.ThirdMove = player.SecondMove;
            player.SecondMove = command;
            return player;
        }

        private MdpTools.PlayersAndMoves OverrideWithNewThirdMove(MdpTools.PlayersAndMoves player, GameCommand command)
        {
            player.ThirdMove = command;
            return player;
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

        private bool CanWeBlowABomb(GameCommand mdpMove, PlayerEntity player)
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
                        //TODO review this decision
                        if (mdpMove == GameCommand.DoNothing || player.BombBag == 0)
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