using System.Diagnostics;
using Reference.Commands;
using Reference.Domain.Map;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reference.Strategies.MDP
{
    public class MdpStrategy : IStrategy
    {
        public GameCommand ExecuteStrategy(GameMap gameMap, char playerKey)
        {
            var utils = new Utils(gameMap, playerKey);
            var player = utils.getPlayer();
            var mdp = new MdpTools(gameMap, playerKey, player);
            var ruleEngine = new RuleEngine(gameMap, player);
#if (DEBUG)
            var stopwatch = Stopwatch.StartNew();
#endif

            utils.DrawMap();
            while (!mdp.AssignBombValues()) {} //while not done
            mdp.AssignMdpGoals(); 
            mdp.CalculateMdp();
            var bestMove = mdp.CalculateBestMoveFromMdp();
           
            bestMove = ruleEngine.OverrideMdpMoveWithRuleEngine(bestMove, mdp);
#if (DEBUG)
            //if ((stopwatch.ElapsedMilliseconds > 2000))
            //    Assert.Fail("Code overran time of 2 seconds");
#endif
            return bestMove;
        }
    }
}
