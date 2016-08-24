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
        public void Gotcha1_MoveOverBombWhenDangerClose()
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
            var map = TestUtils.TestMap(charMap, 9);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.MoveRight);
        }

        [TestMethod()]
        public void Gotcha4_CalculateIfSafeDistanceCanBeReached()
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
                "#              *A3  #".ToCharArray(),
                "# # # # # # # #*# # #".ToCharArray(),
                "# +            *+ + #".ToCharArray(),
                "# # # # #   # #*#+#+#".ToCharArray(),
                "#              *+ + #".ToCharArray(),
                "# # # # # # # #*# # #".ToCharArray(),
                "#   +          *++  #".ToCharArray(),
                "# # # # # # # # #&# #".ToCharArray(),
                "#               D   #".ToCharArray(),
                "# # # # # # # # # #+#".ToCharArray(),
                "#    +  + + +       #".ToCharArray(),
                "# # # #+# # #+# # # #".ToCharArray(),
                "#     ++     ++     #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 9);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.DoNothing);
        }

        [TestMethod()]
        public void Gotcha2_DontPlantABomb()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#      ++   ++ ++   #".ToCharArray(),
                "# # # # # # # #+# # #".ToCharArray(),
                "#          +        #".ToCharArray(),
                "# # # # # # # # # # #".ToCharArray(),
                "#      + + + +  +   #".ToCharArray(),
                "# # # # # # # # # # #".ToCharArray(),
                "#4+    +  +  + +D   #".ToCharArray(),
                "# #+# # #+#+# # #+# #".ToCharArray(),
                "# A+ + +++++++ + +  #".ToCharArray(),
                "# #+#+#+#+$+#+#+#+# #".ToCharArray(),
                "#  + + +++++++ + +  #".ToCharArray(),
                "# #+# # #+#+# # #+# #".ToCharArray(),
                "#C++ + +  +  +   ++ #".ToCharArray(),
                "# # # # # # #6# # # #".ToCharArray(),
                "#   +  + + + B      #".ToCharArray(),
                "# #+# # # # # # # # #".ToCharArray(),
                "#  !     + +        #".ToCharArray(),
                "# # #+# # # # #+# # #".ToCharArray(),
                "#   ++ ++   ++ ++   #".ToCharArray(),
                "#####################".ToCharArray(),
            };
            var map = TestUtils.TestMap(charMap, 4);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.DoNothing);
        }

        [TestMethod()]
        public void Gotcha3_DontBlockYourselfIn()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#      ++   ++ ++   #".ToCharArray(),
                "# # # # # # # #+# # #".ToCharArray(),
                "#          +        #".ToCharArray(),
                "# # # # # # # # # # #".ToCharArray(),
                "#      + + + +  +   #".ToCharArray(),
                "# # # # # # # # # # #".ToCharArray(),
                "#4+    +  +  + +D   #".ToCharArray(),
                "# #+# # #+#+# # #+# #".ToCharArray(),
                "#A + + +++++++ + +  #".ToCharArray(),
                "# #+#+#+#+$+#+#+#+# #".ToCharArray(),
                "#  + + +++++++ + +  #".ToCharArray(),
                "# #+# # #+#+# # #+# #".ToCharArray(),
                "#C++ + +  +  +   ++ #".ToCharArray(),
                "# # # # # # #6# # # #".ToCharArray(),
                "#   +  + + + B      #".ToCharArray(),
                "# #+# # # # # # # # #".ToCharArray(),
                "#  !     + +        #".ToCharArray(),
                "# # #+# # # # #+# # #".ToCharArray(),
                "#   ++ ++   ++ ++   #".ToCharArray(),
                "#####################".ToCharArray(),
            };
            var map = TestUtils.TestMap(charMap, 4);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.MoveDown);
        }

        [TestMethod()]
        public void PlantBombAtRange()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#A  +               #".ToCharArray(),
                "# #                 #".ToCharArray(),
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
                "#                   #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 4);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.PlaceBomb);
        }

        [TestMethod()]
        public void PlantBombAtRangeButNotThroughIndestructableWalls()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#   A               #".ToCharArray(),
                "#   #               #".ToCharArray(),
                "#   +               #".ToCharArray(),
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
                "#                   #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 4);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command != Commands.GameCommand.PlaceBomb);
        }

        [TestMethod()]
        public void GetPowerUp1()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#                   #".ToCharArray(),
                "# # # # #+#+#+# # # #".ToCharArray(),
                "#  A          +   + #".ToCharArray(),
                "#!#3# #+#+#+#+# # #+#".ToCharArray(),
                "#++++   ++ ++   ++++#".ToCharArray(),
                "#+#+# # # # # # #+#+#".ToCharArray(),
                "#+  + + ++ ++ !    +#".ToCharArray(),
                "# #+#+#+#+#+#+# #+# #".ToCharArray(),
                "#      +++++++      #".ToCharArray(),
                "#+#+#+# #+$+# #+#+#+#".ToCharArray(),
                "#      +++++++      #".ToCharArray(),
                "# #+#+#+#+#+#+#+#+# #".ToCharArray(),
                "#+  + + ++ ++ + +  +#".ToCharArray(),
                "#+#+# # # # # # #+#+#".ToCharArray(),
                "# + +   ++ ++   B6++#".ToCharArray(),
                "# #C# #+#+#+#+# # #+#".ToCharArray(),
                "#     +       +   + #".ToCharArray(),
                "# # # #+#+#+#+# # # #".ToCharArray(),
                "#   +               #".ToCharArray(),
                "#####################".ToCharArray(),
            };
            var map = TestUtils.TestMap(charMap, 4);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.MoveLeft);
        }

        [TestMethod()]
        public void GetPowerUp2()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#     A    +  +++   #".ToCharArray(),
                "# # #!# # # #+#+# # #".ToCharArray(),
                "#     +++   +++     #".ToCharArray(),
                "#+# # # #+#+# # # #+#".ToCharArray(),
                "#     ++  +  &      #".ToCharArray(),
                "# # #+# #+#+# # # # #".ToCharArray(),
                "#    + ++   +    +  #".ToCharArray(),
                "# # # #+#+#+#+#D# # #".ToCharArray(),
                "#   + +++++++++6+   #".ToCharArray(),
                "# # #+# #+$+# #+# # #".ToCharArray(),
                "#   + +++++++++ +   #".ToCharArray(),
                "# # # #+#+#+#+# # # #".ToCharArray(),
                "#    + ++   +  & +  #".ToCharArray(),
                "# # #+# #+#+# # # # #".ToCharArray(),
                "#C    ++  +b        #".ToCharArray(),
                "#!# # # #+#+# # # # #".ToCharArray(),
                "#     +++   ++      #".ToCharArray(),
                "# # #+#+# # #+# # # #".ToCharArray(),
                "#    ++  + +  +++   #".ToCharArray(),
                "#####################".ToCharArray(),
            };
            var map = TestUtils.TestMap(charMap, 4);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.MoveLeft);
        }

        [TestMethod()]
        public void KillEnemyPlayer()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
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
                "#                 ! #".ToCharArray(),
                "#                  A#".ToCharArray(),
                "#                 # #".ToCharArray(),
                "#                 # #".ToCharArray(),
                "#                 # #".ToCharArray(),
                "#                 #B#".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 8);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.PlaceBomb);
        }

        [TestMethod()]
        public void Harikiri()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
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
                "#                 ! #".ToCharArray(),
                "#                 Az#".ToCharArray(),
                "#                 # #".ToCharArray(),
                "#                 # #".ToCharArray(),
                "#                 # #".ToCharArray(),
                "#                 #B#".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 8);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.TriggerBomb);
        }

        [TestMethod()]
        public void GettingStarted()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#A  +    +++    +  D#".ToCharArray(),
                "# # #+# #+#+# #+# # #".ToCharArray(),
                "#  +++  + + +  +++  #".ToCharArray(),
                "#+#+# #+# # #+# #+#+#".ToCharArray(),
                "# ++ +  +++++  + ++ #".ToCharArray(),
                "# #+#+# # # # #+#+# #".ToCharArray(),
                "#+ +   +++++++   + +#".ToCharArray(),
                "#+# # #+#+#+#+# # #+#".ToCharArray(),
                "#+ ++ + +++++ + ++ +#".ToCharArray(),
                "# # # # #+$+# # # # #".ToCharArray(),
                "#+ ++ + +++++ + ++ +#".ToCharArray(),
                "#+# # #+#+#+#+# # #+#".ToCharArray(),
                "#+ +   +++++++   + +#".ToCharArray(),
                "# #+#+# # # # #+#+# #".ToCharArray(),
                "# ++ +  +++++  + ++ #".ToCharArray(),
                "#+#+# #+# # #+# #+#+#".ToCharArray(),
                "#  +++  + + +  +++  #".ToCharArray(),
                "# # #+# #+#+# #+# # #".ToCharArray(),
                "#C  +    +++    +  B#".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 1);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.MoveDown);
        }

        [TestMethod()]
        public void DontGetStuckHere()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),          
                "#  ++ +++ + +++ ++3D#".ToCharArray(),          
                "#A# # #+# # #+# # # #".ToCharArray(),         
                "#++++  +  +  +  ++++#".ToCharArray(),          
                "#+#+# # #+#+# # #+#+#".ToCharArray(),          
                "#+   +  ++ ++  +   +#".ToCharArray(),          
                "# # # # # # # # # # #".ToCharArray(),          
                "#+    ++  +  ++    +#".ToCharArray(),          
                "# # #+# #+#+# #+# # #".ToCharArray(),          
                "#    ++ +++++ ++    #".ToCharArray(),          
                "# #+# # #+$+# # #+# #".ToCharArray(),          
                "#    ++ +++++ ++    #".ToCharArray(),          
                "# # #+# #+#+# #+# # #".ToCharArray(),          
                "#+    ++  +  ++    +#".ToCharArray(),          
                "# # # # # # # # # # #".ToCharArray(),          
                "#+   +  ++ ++  +   +#".ToCharArray(),          
                "#+#+# # #+#+# # #+#+#".ToCharArray(),          
                "#++++  +  +  +  ++++#".ToCharArray(),          
                "#3# # #+# # #+# # # #".ToCharArray(),          
                "#C ++ +++ + +++ ++3B#".ToCharArray(),
                "#####################".ToCharArray()    
            };
            var map = TestUtils.TestMap(charMap, 1);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.PlaceBomb);
        }

        [TestMethod()]
        public void DontMoveUp()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#                6  #".ToCharArray(),
                "# # # # # # # # # #a#".ToCharArray(),
                "#              1    #".ToCharArray(),
                "# # # # # # # # # #+#".ToCharArray(),
                "#+++              ++#".ToCharArray(),
                "#+# #+# # # # #+# #+#".ToCharArray(),
                "#  ++1  D       &+  #".ToCharArray(),
                "# # #+# # # # #+# # #".ToCharArray(),
                "#    +  +++ +  +    #".ToCharArray(),
                "# # #+# #+$+# #+# # #".ToCharArray(),
                "#    + 1+++++  +    #".ToCharArray(),
                "# # #+# # # #!#+# # #".ToCharArray(),
                "#       C       &+  #".ToCharArray(),
                "#+# # # # # # #+# #+#".ToCharArray(),
                "#++ B            +++#".ToCharArray(),
                "#+# # # # # # # # #+#".ToCharArray(),
                "#                +  #".ToCharArray(),
                "# # # #+# # # # # # #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 1);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command != Commands.GameCommand.MoveUp);
        }

        [TestMethod()]
        public void MoveDownFromBomb()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#                6  #".ToCharArray(),
                "# # # # # # # # # #a#".ToCharArray(),
                "#                   #".ToCharArray(),
                "# # # # # # # # # #+#".ToCharArray(),
                "#+++              ++#".ToCharArray(),
                "#+# #+# # # # #+# #+#".ToCharArray(),
                "#  ++1  D       &+  #".ToCharArray(),
                "# # #+# # # # #+# # #".ToCharArray(),
                "#    +  +++ +  +    #".ToCharArray(),
                "# # #+# #+$+# #+# # #".ToCharArray(),
                "#    + 1+++++  +    #".ToCharArray(),
                "# # #+# # # #!#+# # #".ToCharArray(),
                "#       C       &+  #".ToCharArray(),
                "#+# # # # # # #+# #+#".ToCharArray(),
                "#++ B            +++#".ToCharArray(),
                "#+# # # # # # # # #+#".ToCharArray(),
                "#                +  #".ToCharArray(),
                "# # # #+# # # # # # #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 1);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.MoveDown);
        }

        [TestMethod()]
        public void DontMoveIntoAnExplodingBomb()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#      +++ ++       #".ToCharArray(),
                "# # # # #+# # # # # #".ToCharArray(),
                "#    +  +++         #".ToCharArray(),
                "# # # # # # # # # # #".ToCharArray(),
                "#                   #".ToCharArray(),
                "# # # #&# # #B# # # #".ToCharArray(),
                "#             1++   #".ToCharArray(),
                "# # # # # # # # # # #".ToCharArray(),
                "#        1          #".ToCharArray(),
                "#+# #+# #+a+# # # #+#".ToCharArray(),
                "# + +   +++++       #".ToCharArray(),
                "# # #+# #+# # #+# # #".ToCharArray(),
                "#   +++  +      +   #".ToCharArray(),
                "# # # # # # # # # # #".ToCharArray(),
                "#    + + ++         #".ToCharArray(),
                "# # #+# # # # # # #+#".ToCharArray(),
                "#   ++  +++     + + #".ToCharArray(),
                "# # # # #+#+# # # # #".ToCharArray(),
                "#  ++  +++ ++   +   #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 1);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.DoNothing);
        }

        [TestMethod()]
        public void Gotcha4_DontWalkIntoExplodingBomb()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#      +     +++    #".ToCharArray(),
                "# # # # # # # # # # #".ToCharArray(),
                "#           + ++    #".ToCharArray(),
                "# # # # #+#+# # # # #".ToCharArray(),
                "#    1!++   ++++++ +#".ToCharArray(),
                "# # # # # # # # # # #".ToCharArray(),
                "#  6+  +  +  +  + + #".ToCharArray(),
                "# # # # #+#+# # #+# #".ToCharArray(),
                "# + a +++++++++  ++ #".ToCharArray(),
                "# # # # #+$+# # # #+#".ToCharArray(),
                "#C +  +++++++++  ++ #".ToCharArray(),
                "# #!# # #+# #9# #+# #".ToCharArray(),
                "#      +     B    + #".ToCharArray(),
                "# # # # # # # # # # #".ToCharArray(),
                "#   +++++   +      +#".ToCharArray(),
                "# # # # #+# # # # # #".ToCharArray(),
                "#    ++ +   + +     #".ToCharArray(),
                "# # # # # # # # # # #".ToCharArray(),
                "#    +++     ++     #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 1);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command != Commands.GameCommand.MoveRight);
        }

        [TestMethod()]
        public void Gotcha5_EvadeImmediatelyExplodingBomb()
        {
            var charMap = new char[][]
            {
                "#####################".ToCharArray(),
                "#                   #".ToCharArray(),
                "# # # # # # # # # # #".ToCharArray(),
                "#                   #".ToCharArray(),
                "# # # # # # # # # #+#".ToCharArray(),
                "#    D     A  1  + +#".ToCharArray(),
                "# # # # # #9#4# # # #".ToCharArray(),
                "#        ++ ++   +++#".ToCharArray(),
                "# # # # #+#+# # #+# #".ToCharArray(),
                "#   B    ++++  + +  #".ToCharArray(),
                "# # # # #+$+#+#+#+#+#".ToCharArray(),
                "#       C  ++  + +  #".ToCharArray(),
                "# # # # # #+# # #+# #".ToCharArray(),
                "#          ++    +++#".ToCharArray(),
                "# # #+# # # # #+# # #".ToCharArray(),
                "#             +  + +#".ToCharArray(),
                "# # # # # # # # #+#+#".ToCharArray(),
                "#     +       ++    #".ToCharArray(),
                "# # #+# # #+# # # # #".ToCharArray(),
                "#                   #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap, 1);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(command == Commands.GameCommand.MoveUp);
        }
    }
}