﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reference.Domain.Map.Entities.PowerUps
{
<<<<<<< HEAD
    [Serializable]
=======
>>>>>>> 109822ff23a14212bfc6b41a11aec0ed2b9fb453
    public class BombRaduisPowerUpEntity : BaseEntity, IPowerUp
    {
        public override int GetPossiblePoints()
        {
            return 50;
        }

        public override string ToString()
        {
            return String.Format("{0}(X:{1}, Y:{2})", GetType().Name, Location.X, Location.Y);
        }
    }
}
