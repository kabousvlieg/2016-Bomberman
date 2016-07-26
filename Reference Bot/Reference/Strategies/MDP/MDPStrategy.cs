using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Reference.Commands;
using Reference.Domain.Map;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
#if (DEBUG)
            var stopwatch = Stopwatch.StartNew();
#endif
            const int rounds = 2;
            List<GameRound> round = new List<GameRound>(0);
            //List<MdpTools.PlayersAndMoves> playersMoves;
            for (var i = 0; i < rounds; i++)
            {
                if ((round == null) || (round.Count - 1 < i))
                {
                    var utils = new Utils(gameMap, playerKey);
                    PlayerEntity[] players = new PlayerEntity[4];
                    utils.getPlayers(ref players);
                    var mdp = new MdpTools(gameMap, playerKey, players);
                    var ruleEngine = new RuleEngine(gameMap, players);

                    utils.DrawMap();
                    while (!mdp.AssignBombValues())
                    {
                    } //while not done
                    mdp.AssignMdpGoals();
                    mdp.CalculateMdp();
                    var playerMoves = mdp.CalculateBestMoveFromMdp();
                    ruleEngine.OverrideMdpMoveWithRuleEngine(ref playerMoves, mdp);
                    round.Add(new GameRound(gameMap, playerMoves));             
                }            
                //if move is imded
                //if all moves is imded
                //rollback gamemap, mark bestmove there as imded
                //if all previous moves is imded
                //rollback twice 
                    
                Utils.tickTheMap(ref gameMap, round[i].PlayersMoves);
            }

            if (round[0].PlayersMoves[0].BestMove == GameCommand.ImDed)
                round[0].PlayersMoves[0].BestMove = GameCommand.TriggerBomb; //TODO last ditch effort, place bomb or trigger
#if (DEBUG)
            //if ((stopwatch.ElapsedMilliseconds > 2000))
            //    Assert.Fail("Code overran time of 2 seconds");
#endif
            return round[0].PlayersMoves[0].BestMove;
        }
    }
}
