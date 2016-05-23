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
        public void ExecuteStrategyTest()
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
                "# #+#+#+#+$+#+#+#+#+#".ToCharArray(),
                "#  ++   +++++   ++  #".ToCharArray(),
                "# #!# #+#+#+#+# #+# #".ToCharArray(),
                "#    ++ +   + ++    #".ToCharArray(),
                "# # # # # # # #b# #+#".ToCharArray(),
                "#  + +  + + +       #".ToCharArray(),
                "# # #+#+#+#+#+# # # #".ToCharArray(),
                "#      + + +        #".ToCharArray(),
                "# # # #+# # #+# # # #".ToCharArray(),
                "#      ++ + ++      #".ToCharArray(),
                "#####################".ToCharArray()
            };
            var map = TestUtils.TestMap(charMap);
            var gameStrategy = new MdpStrategy();
            var command = gameStrategy.ExecuteStrategy(map, 'A');
            Assert.IsTrue(true);
        }
    }
}