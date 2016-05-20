using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Reference.Commands;
using Reference.Domain;
using Reference.Domain.Map;
using Reference.Domain.Map.Entities;
using Reference.Domain.Map.Entities.PowerUps;

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
                       
            utils.DrawMap();
            mdp.assignBombValues();
            mdp.AssignMdpGoals(); 
            mdp.CalculateMdp();
            var bestMove = mdp.CalculateBestMoveFromMdp();
            if (!mdp.areWeInRangeOfBomb())
                bestMove = ruleEngine.OverrideMdpMoveWithRuleEngine(bestMove);
            return bestMove;
        }
    }
}
