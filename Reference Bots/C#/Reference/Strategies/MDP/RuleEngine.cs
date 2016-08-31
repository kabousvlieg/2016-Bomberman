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

        public bool OverrideMdpMoveWithRuleEngine(ref List<MdpTools.PlayersAndMoves> player, MdpTools mdp, bool endGame)
        {
            for (int i = 0; i < player.Count; i++)
            {
                //Check for survival
                //Will our mdpMove move into explosion
                //TODO Review if we shouldn't then rather take the second or third best move???
                if (WalkIntoExplosion(player[i].BestMove, player[i].playerEntity))
                    player[i].BestMove = GameCommand.DoNothing;   
                if (WalkIntoExplosion(player[i].SecondMove, player[i].playerEntity))
                    player[i].BestMove = GameCommand.DoNothing;
                if (WalkIntoExplosion(player[i].ThirdMove, player[i].playerEntity))
                    player[i].BestMove = GameCommand.DoNothing;
                //player[i] = MoveDedMovesDown(player[i]);
                    
                if (!mdp.areWeInRangeOfBomb(player[i].playerEntity))
                {
                    //If there is a better option, rather move to the better option
                    //TODO At some point powerups doesn't matter any more...Early game more important late game less so.
                    if (!CheckIfWeAreNextToAPowerUp(player[i]))
                    {
                        CheckIfWeShouldPlantABomb(player[i], mdp);
                    }                   
                    //Check if we can blow up the enemy
                    if (CanWeBlowAnEnemy(player[i].playerEntity, mdp))
                        player[i] = OverrideWithNewBestMove(player[i], GameCommand.TriggerBomb);
                    //Check if we can blow up the enemy
                    if (CanWeStealAWall(player[i].playerEntity, mdp))
                        player[i] = OverrideWithNewBestMove(player[i], GameCommand.TriggerBomb);

                    if (CanWeBlowABomb(player[i].BestMove, player[i].playerEntity))
                        player[i] = OverrideWithNewBestMove(player[i], GameCommand.TriggerBomb);
                    if (CanWeBlowABomb(player[i].SecondMove, player[i].playerEntity))
                        player[i] = OverrideWithNewSecondMove(player[i], GameCommand.TriggerBomb);
                    if (CanWeBlowABomb(player[i].ThirdMove, player[i].playerEntity))
                        player[i] = OverrideWithNewThirdMove(player[i], GameCommand.TriggerBomb);
                }
            }
        
            return Harikiri(player[0], mdp);
        }

        private bool Harikiri(MdpTools.PlayersAndMoves playersAndMoves, MdpTools mdp)
        {
            //I have only one bomb
            //I have more points than other players
            //All players are in the path of the bomb
            //All possible moves of enemy players keep them in the path of the bomb
            return false;   //cannot work...
        }

        private bool CheckIfWeAreNextToAPowerUp(MdpTools.PlayersAndMoves player)
        {
            if (player.BestMove == GameCommand.MoveDown &&
                player.playerEntity.Location.Y < _gameMap.MapHeight)
            {
                var block = _gameMap.GetBlockAtLocation(player.playerEntity.Location.X,
                    player.playerEntity.Location.Y + 1);
                if (block.PowerUp != null)
                    return true;
            }
            if (player.BestMove == GameCommand.MoveUp &&
                player.playerEntity.Location.Y > 1)
            {
                var block = _gameMap.GetBlockAtLocation(player.playerEntity.Location.X,
                    player.playerEntity.Location.Y - 1);
                if (block.PowerUp != null)
                    return true;
            }
            if (player.BestMove == GameCommand.MoveRight &&
                player.playerEntity.Location.X < _gameMap.MapWidth)
            {
                var block = _gameMap.GetBlockAtLocation(player.playerEntity.Location.X + 1,
                    player.playerEntity.Location.Y);
                if (block.PowerUp != null)
                    return true;
            }
            if (player.BestMove == GameCommand.MoveLeft &&
                player.playerEntity.Location.X > 1)
            {
                var block = _gameMap.GetBlockAtLocation(player.playerEntity.Location.X - 1,
                    player.playerEntity.Location.Y);
                if (block.PowerUp != null)
                    return true;
            }
            return false;
        }

        private void CheckIfWeShouldPlantABomb(MdpTools.PlayersAndMoves player, MdpTools mdp)
        {
            var bestMoveBombs = 0;
            if (player.BestMove == GameCommand.MoveDown &&
                player.playerEntity.Location.Y < _gameMap.MapHeight)
            {
                bestMoveBombs = WallsBombWillBlast(mdp, player.playerEntity.BombBag, player.playerEntity.BombRadius,
                    player.playerEntity.Location.X, player.playerEntity.Location.Y + 1, player.playerEntity.Key);
            }
            if (player.BestMove == GameCommand.MoveUp &&
                player.playerEntity.Location.Y > 1)
            {
                bestMoveBombs = WallsBombWillBlast(mdp, player.playerEntity.BombBag, player.playerEntity.BombRadius,
                    player.playerEntity.Location.X, player.playerEntity.Location.Y - 1, player.playerEntity.Key);
            }
            if (player.BestMove == GameCommand.MoveRight &&
                player.playerEntity.Location.X < _gameMap.MapWidth)
            {
                bestMoveBombs = WallsBombWillBlast(mdp, player.playerEntity.BombBag, player.playerEntity.BombRadius,
                    player.playerEntity.Location.X + 1, player.playerEntity.Location.Y, player.playerEntity.Key);
            }
            if (player.BestMove == GameCommand.MoveLeft &&
                player.playerEntity.Location.X > 1)
            {
                bestMoveBombs = WallsBombWillBlast(mdp, player.playerEntity.BombBag, player.playerEntity.BombRadius,
                    player.playerEntity.Location.X - 1, player.playerEntity.Location.Y, player.playerEntity.Key);
            }
            //Bomb planted here
            var wallsBombWillBlast = WallsBombWillBlast(mdp, player.playerEntity.BombBag, player.playerEntity.BombRadius,
                player.playerEntity.Location.X, player.playerEntity.Location.Y, player.playerEntity.Key);
            if ( (wallsBombWillBlast >= bestMoveBombs) &&
                 (wallsBombWillBlast > 0) )
            {
                player = OverrideWithNewBestMove(player, GameCommand.PlaceBomb);
            }
        }

        public void EliminateDuplicateMoves(ref List<MdpTools.PlayersAndMoves> playerMoves)
        {
            foreach (var player in playerMoves)
            {
                if (player.SecondMove == player.BestMove)
                {
                    if (player.ThirdMove != player.BestMove)
                    {
                        player.SecondMove = player.ThirdMove;
                        player.ThirdMove = GameCommand.DoNothing;
                    }
                    else
                    {
                        player.SecondMove = GameCommand.ImDed;
                    }
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
                case GameCommand.ImDed:
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
                        if (block.Bomb.IsExploding || block.Bomb.BombTimer <= 3)
                            continue;
                        //TODO review this decision
                        if ((mdpMove == GameCommand.DoNothing) || 
                            ( (player.BombBag == 0) && (mdpMove == GameCommand.PlaceBomb)) )
                            return true;
                    }
                }
            }
            return false;
        }

        private bool CanWeBlowAnEnemy(PlayerEntity player, MdpTools mdp)
        {
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    var block = _gameMap.GetBlockAtLocation(x, y);
                    if (block.Bomb?.Owner.Key == player.Key)
                    {
                        if (block.Bomb.IsExploding || block.Bomb.BombTimer <= 2) //It will explode in any case
                            continue;
                        if (EnemiesBombWillBlast(mdp, block.Bomb.BombRadius, block.Location.X, block.Location.Y, player.Key) > 0)
                            return true;
                    }
                }
            }
            return false;
        }

        private bool CanWeStealAWall(PlayerEntity player, MdpTools mdp)
        {
            for (var y = 1; y <= _gameMap.MapHeight; y++)
            {
                for (var x = 1; x <= _gameMap.MapWidth; x++)
                {
                    var block = _gameMap.GetBlockAtLocation(x, y);
                    if (block.Bomb?.Owner.Key == player.Key)
                    {
                        if (block.Bomb.IsExploding || block.Bomb.BombTimer <= 2) //It will explode in any case
                            continue;
                        if (EnemyBombWillBlast(mdp, block.Bomb.BombRadius, block.Location.X, block.Location.Y, player.Key) > 0)
                            return true;
                    }
                }
            }
            return false;
        }

        private int WallsBombWillBlast(MdpTools mdp, int bombBag, int bombRadius, int x, int y, char key)
        {
            var wallsInRange = 0;
            var bombBlockedXMinusDirection = false;
            var bombBlockedXPlusDirection = false;
            var bombBlockedYMinusDirection = false;
            var bombBlockedYPlusDirection = false;
            for (int range = 1; range <= bombRadius; range++)
            {
                if ((x - range > 1) && (!bombBlockedXMinusDirection))
                {
                    var xrange = x - range;
                    var yrange = y;
                    if (PlantBombUnlessBombBlocked(xrange, yrange, ref bombBlockedXMinusDirection, mdp, key))
                    {
                        wallsInRange++;
                    }
                }
                if ((x + range < _gameMap.MapWidth) && (!bombBlockedXPlusDirection))
                {
                    var xrange = x + range;
                    var yrange = y;
                    if (PlantBombUnlessBombBlocked(xrange, yrange, ref bombBlockedXPlusDirection, mdp, key))
                    {
                        wallsInRange++;
                    }
                }
                if ((y - range > 1) && (!bombBlockedYMinusDirection))
                {
                    var xrange = x;
                    var yrange = y - range;
                    if (PlantBombUnlessBombBlocked(xrange, yrange, ref bombBlockedYMinusDirection, mdp, key))
                    {
                        wallsInRange++;
                    }
                }
                if ((y + range < _gameMap.MapHeight) && (!bombBlockedYPlusDirection))
                {
                    var xrange = x;
                    var yrange = y + range;
                    if (PlantBombUnlessBombBlocked(xrange, yrange, ref bombBlockedYPlusDirection, mdp, key))
                    {
                        wallsInRange++;
                    }
                }
            }
            return wallsInRange;
        }

        private int EnemyBombWillBlast(MdpTools mdp, int bombRadius, int x, int y, char key)
        {
            var wallsInRange = 0;
            var bombBlockedXMinusDirection = false;
            var bombBlockedXPlusDirection = false;
            var bombBlockedYMinusDirection = false;
            var bombBlockedYPlusDirection = false;
            for (int range = 1; range <= bombRadius; range++)
            {
                if ((x - range > 1) && (!bombBlockedXMinusDirection))
                {
                    var xrange = x - range;
                    var yrange = y;
                    if (BlowBombForEnemyWallUnlessBombBlocked(xrange, yrange, ref bombBlockedXMinusDirection, mdp, key))
                    {
                        wallsInRange++;
                    }
                }
                if ((x + range < _gameMap.MapWidth) && (!bombBlockedXPlusDirection))
                {
                    var xrange = x + range;
                    var yrange = y;
                    if (BlowBombForEnemyWallUnlessBombBlocked(xrange, yrange, ref bombBlockedXPlusDirection, mdp, key))
                    {
                        wallsInRange++;
                    }
                }
                if ((y - range > 1) && (!bombBlockedYMinusDirection))
                {
                    var xrange = x;
                    var yrange = y - range;
                    if (BlowBombForEnemyWallUnlessBombBlocked(xrange, yrange, ref bombBlockedYMinusDirection, mdp, key))
                    {
                        wallsInRange++;
                    }
                }
                if ((y + range < _gameMap.MapHeight) && (!bombBlockedYPlusDirection))
                {
                    var xrange = x;
                    var yrange = y + range;
                    if (BlowBombForEnemyWallUnlessBombBlocked(xrange, yrange, ref bombBlockedYPlusDirection, mdp, key))
                    {
                        wallsInRange++;
                    }
                }
            }
            return wallsInRange;
        }

        private int EnemiesBombWillBlast(MdpTools mdp, int bombRadius, int x, int y, char key)
        {
            var enemiesInRange = 0;
            var bombBlockedXMinusDirection = false;
            var bombBlockedXPlusDirection = false;
            var bombBlockedYMinusDirection = false;
            var bombBlockedYPlusDirection = false;
            for (int range = 1; range <= bombRadius; range++)
            {
                if ((x - range > 1) && (!bombBlockedXMinusDirection))
                {
                    var xrange = x - range;
                    var yrange = y;
                    if (ExplodeBombUnlessBombBlocked(xrange, yrange, ref bombBlockedXMinusDirection, mdp, key))
                    {
                        enemiesInRange++;
                    }
                }
                if ((x + range < _gameMap.MapWidth) && (!bombBlockedXPlusDirection))
                {
                    var xrange = x + range;
                    var yrange = y;
                    if (ExplodeBombUnlessBombBlocked(xrange, yrange, ref bombBlockedXPlusDirection, mdp, key))
                    {
                        enemiesInRange++;
                    }
                }
                if ((y - range > 1) && (!bombBlockedYMinusDirection))
                {
                    var xrange = x;
                    var yrange = y - range;
                    if (ExplodeBombUnlessBombBlocked(xrange, yrange, ref bombBlockedYMinusDirection, mdp, key))
                    {
                        enemiesInRange++;
                    }
                }
                if ((y + range < _gameMap.MapHeight) && (!bombBlockedYPlusDirection))
                {
                    var xrange = x;
                    var yrange = y + range;
                    if (ExplodeBombUnlessBombBlocked(xrange, yrange, ref bombBlockedYPlusDirection, mdp, key))
                    {
                        enemiesInRange++;
                    }
                }
            }
            return enemiesInRange;
        }

        private bool PlantBombUnlessBombBlocked(int xrange, int yrange, ref bool bombBlockedDirection, MdpTools mdp, char key)
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
                bool inRangeOfEnemy = false;
                if (blockInRange.Entity is PlayerEntity)
                {
                    if ((blockInRange.Entity as PlayerEntity).Key != key)
                        return true;
                }
                if ( blockInRange.Entity is DestructibleWallEntity &&
                    (!mdpBlockInRange.InRangeOfMyBomb))/* &&
                    (!mdpBlockInRange.InRangeOfEnemyBomb))*/ //TODO Maak seker
                {
                    bombBlockedDirection = true;
                    return true;
                }
                    
            }
            return false;
        }

        private bool BlowBombForEnemyWallUnlessBombBlocked(int xrange, int yrange, ref bool bombBlockedDirection, MdpTools mdp, char key)
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
                bool inRangeOfEnemy = false;
                if (blockInRange.Entity is PlayerEntity)
                {
                    if ((blockInRange.Entity as PlayerEntity).Key != key)
                        return true;
                }
                if (blockInRange.Entity is DestructibleWallEntity &&
                    (mdpBlockInRange.InRangeOfEnemyBomb)) 
                {
                    bombBlockedDirection = true;
                    return true;
                }

            }
            return false;
        }

        private bool ExplodeBombUnlessBombBlocked(int xrange, int yrange, ref bool bombBlockedDirection, MdpTools mdp, char key)
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
                bool inRangeOfEnemy = false;
                if (blockInRange.Entity is PlayerEntity)
                {
                    if ((blockInRange.Entity as PlayerEntity).Key != key)
                        return true;
                }
                //if (blockInRange.Entity is DestructibleWallEntity &&
                //    (!mdpBlockInRange.InRangeOfMyBomb) &&
                //    (!mdpBlockInRange.InRangeOfEnemyBomb)) //TODO Maak seker
                //{
                //    bombBlockedDirection = true;
                //    return true;
                //}
            }
            return false;
        }
    }
}