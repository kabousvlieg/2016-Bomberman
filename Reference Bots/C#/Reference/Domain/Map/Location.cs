﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reference.Domain.Map
{
<<<<<<< HEAD
    [Serializable]
=======
>>>>>>> 109822ff23a14212bfc6b41a11aec0ed2b9fb453
    public class Location
    {
        public int X { get; set; }
        public int Y { get; set; }

        public bool IsSameLocation(Location other)
        {
            return X == other.X && Y == other.Y;
        }

        public override string ToString()
        {
            return String.Format("Location({0},{1})", X, Y);
        }
    }
}
