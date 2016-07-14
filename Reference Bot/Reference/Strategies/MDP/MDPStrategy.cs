using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Reference.Commands;
using Reference.Domain.Map;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reference.Strategies.MDP
{
    public class MdpStrategy : IStrategy
    {
        public GameCommand ExecuteStrategy(GameMap gameMap, char playerKey)
        {
            const int rounds = 2;
            var bestMoves = new MdpTools.Moves[rounds];
            for (var i = 0; i < rounds; i++)
            {
                var utils = new Utils(gameMap, playerKey);
                var player = utils.getPlayer();
                var mdp = new MdpTools(gameMap, playerKey, player);
                var ruleEngine = new RuleEngine(gameMap, player);
#if (DEBUG)
                var stopwatch = Stopwatch.StartNew();
#endif
                utils.DrawMap();
                //TODO recursive breadth search until I am out of time
                //TODO calculate enemy best moves and take into account
                while (!mdp.AssignBombValues())
                {
                } //while not done
                mdp.AssignMdpGoals();
                mdp.CalculateMdp();
                bestMoves[i] = mdp.CalculateBestMoveFromMdp();
                bestMoves[i] = ruleEngine.OverrideMdpMoveWithRuleEngine(bestMoves[i], mdp);

                utils.tickTheMap(ref gameMap, bestMoves[i]);
            }

            if (bestMoves[0] == GameCommand.ImDed)
                bestMoves[0] = GameCommand.TriggerBomb; //TODO last ditch effort, place bomb or trigger
#if (DEBUG)
            //if ((stopwatch.ElapsedMilliseconds > 2000))
            //    Assert.Fail("Code overran time of 2 seconds");
#endif
            return bestMoves[0];
        }
    }
}
