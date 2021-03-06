<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reference.Serialization;

namespace Reference.Domain.Map
{
    [Serializable]
    public class GameMap : ICloneable
    {
        public int MapSeed { get; set; }
        public int MapHeight { get; set; }
        public int MapWidth { get; set; }
        public GameBlock[,] GameBlocks { get; set; }

        /// <summary>
        /// Retrieves the game block at the specified X and Y location.  Game locations start at 1 up to game width/height
        /// </summary>
        /// <param name="x">X coordinates</param>
        /// <param name="y">Y coordinates</param>
        /// <returns>The game block found at the specified location</returns>
        public GameBlock GetBlockAtLocation(int x, int y)
        {
            return GameBlocks[x - 1, y - 1];
        }

        public static GameMap FromJson(String map)
        {
            return JsonConvert.DeserializeObject<GameMap>(map, new JsonSerializerSettings()
            {
                Binder = new EntityTypeNameHandling()
            });
        }

        public object Clone()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, this);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(memoryStream) as GameMap;
            }
        }
    }
}
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reference.Serialization;

namespace Reference.Domain.Map
{
    public class GameMap
    {
        public int MapSeed { get; set; }
        public int MapHeight { get; set; }
        public int MapWidth { get; set; }
        public GameBlock[,] GameBlocks { get; set; }

        /// <summary>
        /// Retrieves the game block at the specified X and Y location.  Game locations start at 1 up to game width/height
        /// </summary>
        /// <param name="x">X coordinates</param>
        /// <param name="y">Y coordinates</param>
        /// <returns>The game block found at the specified location</returns>
        public GameBlock GetBlockAtLocation(int x, int y)
        {
            return GameBlocks[x - 1, y - 1];
        }

        public static GameMap FromJson(String map)
        {
            return JsonConvert.DeserializeObject<GameMap>(map, new JsonSerializerSettings()
            {
                Binder = new EntityTypeNameHandling()
            });
        }
    }
}
>>>>>>> 109822ff23a14212bfc6b41a11aec0ed2b9fb453
