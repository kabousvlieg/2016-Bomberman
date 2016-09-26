<<<<<<< HEAD
﻿using System;
using Newtonsoft.Json;
=======
﻿using Newtonsoft.Json;
>>>>>>> 109822ff23a14212bfc6b41a11aec0ed2b9fb453
using Reference.Domain.Map.Entities;
using Reference.Domain.Map.Entities.PowerUps;

namespace Reference.Domain.Map
{
<<<<<<< HEAD
    [Serializable]
=======
>>>>>>> 109822ff23a14212bfc6b41a11aec0ed2b9fb453
    public class GameBlock
    {
        public Location Location { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
        public IEntity Entity { get; set; }
        public BombEntity Bomb { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
        public IPowerUp PowerUp { get; set; }
        public bool Exploding { get; set; }
    }
}
