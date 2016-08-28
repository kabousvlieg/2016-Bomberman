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
            while (currentRound < rounds)
            {
                //if ((stopwatch.ElapsedMilliseconds > 2000))
                //{
                //    round[0].PlayersMoves[0] = MoveDedMovesDown(round[0].PlayersMoves[0]);
                //    break;
                //}
                if (round[currentRound].PlayersMoves == null)
                {
                    Utils utils;
                    utils = new Utils(round[currentRound].Map, playerKey);
                    PlayerEntity[] players = new PlayerEntity[4]; //Maximum of 4 players
                    utils.getPlayers(ref players);
                    var mdp = new MdpTools(round[currentRound].Map, playerKey, players);
                    var ruleEngine = new RuleEngine(round[currentRound].Map, players);

                    //utils.DrawMap();
                    while (!mdp.AssignBombValues())
                    {
                    } //while not done
                    mdp.AssignMdpGoals();
                    mdp.CalculateMdp();
                    //mdp.DrawMdpMap();
                    var playerMoves = mdp.CalculateBestMoveFromMdp();
                    ruleEngine.OverrideMdpMoveWithRuleEngine(ref playerMoves, mdp);
                    ruleEngine.EliminateDuplicateMoves(ref playerMoves);
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
                        return GameCommand.TriggerBomb; //TODO last ditch effort, place bomb or trigger
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
                round[0].PlayersMoves[0].BestMove = GameCommand.TriggerBomb; //TODO last ditch effort, place bomb or trigger

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
