using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reference.Strategies.MDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reference.Domain.Map;

namespace Reference.Strategies.MDP.Tests
{
    [TestClass()]
    public class MdpStrategyTests
    {
        [TestMethod()]
        public void CheckIfWeCanParseAMap()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#      *& + ++      #".ToCharArray(),
                "# # # #*# # #+# # # #".ToCharArray(),
                "# !  ***** + +6D    #".ToCharArray(),
                "# # #A#*#+#+#+# # # #".ToCharArray(),
                "#  + +  + + +  &    #".ToCharArray(),
                "#+# # # # # # # # #+#".ToCharArray(),
                "#C   ++ +   + ++    #".ToCharArray(),
                "# #+# #+#+#+#+# #+# #".ToCharArray(),
                "#  ++   +++++   ++  #".ToCharArray(),
                "#3#+#+#+#+$+#+#+#+#+#".ToCharArray(),
                "#  ++   +++++   ++  #".ToCharArray(),
                "# #!# #+#+#+#+# #+# #".ToCharArray(),
                "#    ++ +   + ++    #".ToCharArray(),
                "# # # # # # # #b# #+#".ToCharArray(),
                "#  + +  + + +       #".ToCharArray(),
                "# # #+#+#+#+#+#9# # #".ToCharArray(),
                "#      + + +        #".ToCharArray(),
                "# # # #+# # #+# #1# #".ToCharArray(),
                "#      ++ + ++      #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 9);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void DontStepIntoEnemyBomb()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "# 9                 #".ToCharArray(),
                "#A# ####            #".ToCharArray(),
                "#z#                 #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 9);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.MoveDown);
        }

        [TestMethod()]
        public void AvoidMyBomb()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#Az                 #".ToCharArray(),
                "# ######            #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 9);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.MoveDown);
        }

        [TestMethod()]
        public void AvoidBombChain()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#     +       + +   #".ToCharArray(),
                "# # # # #+#+#+#+# # #".ToCharArray(),
                "#       + + ++      #".ToCharArray(),
                "# # # # # # # # # # #".ToCharArray(),
                "#                   #".ToCharArray(),
                "# # # # # # # # # #+#".ToCharArray(),
                "#        A         +#".ToCharArray(),
                "# # # # # # # # #+# #".ToCharArray(),
                "#       + u      +  #".ToCharArray(),
                "# # # # #z  # # # #+#".ToCharArray(),
                "#              + +  #".ToCharArray(),
                "# # # # # # # # # # #".ToCharArray(),
                "#    C  B     D    +#".ToCharArray(),
                "# # # # #1# #+# # #+#".ToCharArray(),
                "#        +    +     #".ToCharArray(),
                "# # # # # # # #+# #+#".ToCharArray(),
                "#    + +++++++ +    #".ToCharArray(),
                "# # #+#+#+#+#+#+# # #".ToCharArray(),
                "#  ++ +       + +   #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 4);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.MoveLeft || 
                          command == Commands.GameCommand.MoveRight);
        }

        [TestMethod()]
        public void AvoidMyBombCloseToExploding()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#A  t               #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "#z#                 #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 4);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.MoveDown);
        }

        [TestMethod()]
        public void DontChooseTheEasierRouteLitteredWithBombs()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#A  v               #".ToCharArray(),
                "# ###               #".ToCharArray(),
                "#z#                 #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 4);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.MoveDown);
        }

        [TestMethod()]
        public void EnemyBombChainNoGo()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#A  r               #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "#r#                 #".ToCharArray(),
                "# # 9               #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 4);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.MoveDown);
        }

        [TestMethod()]
        public void BombChainZerosMyBombsTimer()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#A  z               #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "#z#                 #".ToCharArray(),
                "# # r               #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "# #                 #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 4);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.MoveDown);
        }

        [TestMethod()]
        public void Gotcha1()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#             +     #".ToCharArray(),
                "# # # # # # #+# # # #".ToCharArray(),
                "#                  +#".ToCharArray(),
                "#+# # # # # # # # #+#".ToCharArray(),
                "#                 + #".ToCharArray(),
                "# # # # # # # # #+# #".ToCharArray(),
                "#               A3  #".ToCharArray(),
                "# # # # # # # # # # #".ToCharArray(),
                "# +             + + #".ToCharArray(),
                "# # # # #   # # #+#+#".ToCharArray(),
                "#               + + #".ToCharArray(),
                "# # # # # # # # # # #".ToCharArray(),
                "#   +          1++  #".ToCharArray(),
                "# # # # # # # # #&# #".ToCharArray(),
                "#               D   #".ToCharArray(),
                "# # # # # # # # # #+#".ToCharArray(),
                "#    +  + + +       #".ToCharArray(),
                "# # # #+# # #+# # # #".ToCharArray(),
                "#     ++     ++     #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 4);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.MoveRight);
        }
    }
}