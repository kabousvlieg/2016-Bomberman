﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameEngine.Exceptions
{
    public class InvalidCommandException : Exception
    {
        public InvalidCommandException(String message) :
            base(message)
        {
            
        }
    }
}
