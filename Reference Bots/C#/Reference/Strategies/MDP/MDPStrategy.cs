using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Reference.Commands;
using Reference.Domain.Map;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reference.Domain.Map.Entities;

namespace Reference.Strategies.MDP
{
    public class MdpStrategy : IStrategy
    {
        struct GameRound
        {
            public GameMap Map;
            public bool FirstRun;
            public List<MdpTools.PlayersAndMoves> PlayersMoves;

            public GameRound(GameMap map, List<MdpTools.PlayersAndMoves> playersMoves)
            {
                Map = map;
                FirstRun = false;
                PlayersMoves = playersMoves;
            }
        }

        public GameCommand ExecuteStrategy(GameMap gameMap, char playerKey)
        {
            var stopwatch = Stopwatch.StartNew();
            const int rounds = 10;
            int currentRound = 0;
            var round = new GameRound[rounds];
            //List<MdpTools.PlayersAndMoves> playersMoves;

            round[0] = new GameRound(gameMap.Clone() as GameMap, null);
            var bestMoveThoughDead = GameCommand.TriggerBomb;
            var initialBestMove = true;
            while (currentRound < rounds)
            {
                if ((stopwatch.ElapsedMilliseconds > 2000))
                {
                    round[0].PlayersMoves[0] = MoveDedMovesDown(round[0].PlayersMoves[0]);
                    return round[0].PlayersMoves[0].SecondMove;
                }
                if (round[currentRound].PlayersMoves == null)
                {
                    Utils utils;
                    utils = new Utils(round[currentRound].Map, playerKey);
                    PlayerEntity[] players = new PlayerEntity[4]; //Maximum of 4 players
                    players[0] = null;
                    players[1] = null;
                    players[2] = null;
                    players[3] = null;
                    utils.getPlayers(ref players);
                    var mdp = new MdpTools(round[currentRound].Map, playerKey, players);
                    var ruleEngine = new RuleEngine(round[currentRound].Map, players);

                    //utils.DrawMap();
                    while (!mdp.AssignBombValues())
                    {
                    } //while not done
                    var endGame = utils.EndGame();
                    var inRangeOfBomb = mdp.areWeInRangeOfBomb(players[0]);
                    mdp.AssignMdpGoals(endGame, playerKey, false, players[0]);
                    mdp.CalculateMdp();
                    //mdp.DrawMdpMap();
                    var playerMoves = mdp.CalculateBestMoveFromMdp(endGame, Utils.FightOrNotFlight(players));
                    if (inRangeOfBomb)
                    {
                        var nonBombMdp = new MdpTools(round[currentRound].Map, playerKey, players);
                        var nonBombruleEngine = new RuleEngine(round[currentRound].Map, players);

                        //utils.DrawMap();
                        while (!nonBombMdp.AssignBombValues())
                        {
                        } //while not done
                        var nonBombEndGame = utils.EndGame();
                        nonBombMdp.AssignMdpGoals(nonBombEndGame, playerKey, inRangeOfBomb, players[0]);
                        nonBombMdp.CalculateMdp();
                        //nonBombMdp.DrawMdpMap();
                        var nonBombplayerMoves = nonBombMdp.CalculateBestMoveFromMdp(nonBombEndGame, Utils.FightOrNotFlight(players));
                        //TODO needs work
                        if ( (playerMoves[0].BestMove != GameCommand.DoNothing) && 
                            ((playerMoves[0].BestMove == nonBombplayerMoves[0].BestMove) ||
                             (playerMoves[0].BestMove == nonBombplayerMoves[0].SecondMove) ||
                             (playerMoves[0].BestMove == nonBombplayerMoves[0].ThirdMove)) )
                        {
                            //Do nothing
                        }
                        else
                        {
                            if ((playerMoves[0].SecondMove != GameCommand.DoNothing) &&
                                ( (playerMoves[0].SecondMove == nonBombplayerMoves[0].BestMove) ||
                                  (playerMoves[0].SecondMove == nonBombplayerMoves[0].SecondMove) ||
                                  (playerMoves[0].SecondMove == nonBombplayerMoves[0].ThirdMove)))
                            {
                                playerMoves[0].BestMove = playerMoves[0].SecondMove;
                            }
                            else
                            {
                                if ((playerMoves[0].BestMove != GameCommand.DoNothing) &&
                                    ((playerMoves[0].SecondMove == nonBombplayerMoves[0].BestMove) ||
                                    (playerMoves[0].SecondMove == nonBombplayerMoves[0].SecondMove) ||
                                    (playerMoves[0].SecondMove == nonBombplayerMoves[0].ThirdMove)) )
                                {
                                    playerMoves[0].BestMove = playerMoves[0].ThirdMove;
                                }
                                else
                                {
                                    playerMoves[0].BestMove = nonBombplayerMoves[0].BestMove;
                                    playerMoves[0].SecondMove = nonBombplayerMoves[0].SecondMove;
                                    playerMoves[0].ThirdMove = nonBombplayerMoves[0].ThirdMove;
                                }
                            }
                        }
                    }
                    ruleEngine.EliminateDuplicateMoves(ref playerMoves);
                    var harikiri = ruleEngine.OverrideMdpMoveWithRuleEngine(ref playerMoves, mdp, endGame);
                    if (harikiri)
                        return GameCommand.TriggerBomb;
                    ruleEngine.EliminateDuplicateMoves(ref playerMoves);
                    if ((currentRound == 0) && (initialBestMove))
                    {
                        if (playerMoves[0].BestMove != GameCommand.ImDed)
                            bestMoveThoughDead = playerMoves[0].BestMove;
                    }
                    if ((playerMoves[0].BestMove == GameCommand.PlaceBomb) &&
                        (playerMoves[0].playerEntity.BombBag == 0)) //Bomb will explode soon
                    {
                        playerMoves[0].BestMove = GameCommand.DoNothing; //So wait it out
                    }
                    //TODO Eliminate same moves
                    round[currentRound].PlayersMoves = playerMoves;             
                }

                //Debug.Print("Round " + currentRound + "\n" +
                //    round[currentRound].PlayersMoves[0].BestMove.ToString() + "\n" +
                //    round[currentRound].PlayersMoves[0].SecondMove + "\n" +
                //    round[currentRound].PlayersMoves[0].ThirdMove);

                //if all moves is imded
                if ( (round[currentRound].PlayersMoves[0].BestMove == GameCommand.ImDed) &&
                     (round[currentRound].PlayersMoves[0].SecondMove == GameCommand.ImDed) &&
                     (round[currentRound].PlayersMoves[0].ThirdMove == GameCommand.ImDed) )
                {
                    if (currentRound > 0)
                    {
                        round[--currentRound].PlayersMoves[0].BestMove = GameCommand.ImDed;
                        continue; //Repeat the last round with the next best move
                    }
                    else
                    {
                        //What the hell now? Game check mate?
                        //Dis nie te se nie
                        return bestMoveThoughDead; //TODO last ditch effort, place bomb or trigger
                    } 
                }
                
                //Get best non ded move
                if (round[currentRound].PlayersMoves[0].BestMove == GameCommand.ImDed)
                {
                    round[currentRound].PlayersMoves[0] = MoveDedMovesDown(round[currentRound].PlayersMoves[0]);
                }

                var nextmap = Utils.tickTheMap(round[currentRound].Map.Clone() as GameMap, round[currentRound].PlayersMoves);

                if (round[currentRound].PlayersMoves[0].playerEntity.Killed)
                {
                    round[currentRound].PlayersMoves[0].BestMove = GameCommand.ImDed;
                }
                else
                {
                    if (currentRound + 1 >= rounds) //Yay we survived
                    {
                        break;  
                    }
                    round[currentRound + 1] = new GameRound(nextmap.Clone() as GameMap, null);
                    currentRound++;
                }
            }

            if (round[0].PlayersMoves[0].BestMove == GameCommand.ImDed)
                round[0].PlayersMoves[0].BestMove = GameCommand.PlaceBomb; //TODO last ditch effort, place bomb or trigger
#if (DEBUG)
            //if ((stopwatch.ElapsedMilliseconds > 2000))
            //    Assert.Fail("Code overran time of 2 seconds");
#endif
            return round[0].PlayersMoves[0].BestMove;
        }

        private MdpTools.PlayersAndMoves MoveDedMovesDown(MdpTools.PlayersAndMoves player)
        {
            for (int i = 0; i < 3; i++)
            {
                if (player.BestMove == GameCommand.ImDed)
                {
                    player.BestMove = player.SecondMove;
                    player.SecondMove = player.ThirdMove;
                    player.ThirdMove = GameCommand.ImDed;
                }
                if (player.SecondMove == GameCommand.ImDed)
                {
                    player.SecondMove = player.ThirdMove;
                    player.ThirdMove = GameCommand.ImDed;
                }
            }
            return player;
        }
    }
}
