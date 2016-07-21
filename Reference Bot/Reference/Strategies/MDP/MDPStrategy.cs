using System.Collections.Generic;
using System.Diagnostics;
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
            List<MdpTools.PlayersAndMoves> playersMoves;
        }

        public GameCommand ExecuteStrategy(GameMap gameMap, char playerKey)
        {
            const int rounds = 2;
            for (var i = 0; i < rounds; i++)
            {
                List<MdpTools.PlayersAndMoves> playersMoves;
                var utils = new Utils(gameMap, playerKey);
                PlayerEntity[] players = new PlayerEntity[4];
                utils.getPlayers(ref players);
                var mdp = new MdpTools(gameMap, playerKey, players);
                var ruleEngine = new RuleEngine(gameMap, players);
#if (DEBUG)
                var stopwatch = Stopwatch.StartNew();
#endif
                utils.DrawMap();
                while (!mdp.AssignBombValues())
                {
                } //while not done
                mdp.AssignMdpGoals();
                mdp.CalculateMdp();
                playersMoves = mdp.CalculateBestMoveFromMdp();
                playersMoves[0].BestMove = ruleEngine.OverrideMdpMoveWithRuleEngine(playersMoves[0].BestMove, mdp, playersMoves[0].playerEntity);

                utils.tickTheMap(ref gameMap, playersMoves);
            }

            if (playersMoves[0].BestMove == GameCommand.ImDed)
                playersMoves[0].BestMove = GameCommand.TriggerBomb; //TODO last ditch effort, place bomb or trigger
#if (DEBUG)
            //if ((stopwatch.ElapsedMilliseconds > 2000))
            //    Assert.Fail("Code overran time of 2 seconds");
#endif
            return playersMoves[0].BestMove;
        }
    }
}
