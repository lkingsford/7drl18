using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    /// <summary>
    /// Exception to throw when rules violated
    /// </summary>
    public class BrokenRulesException : Exception
    {
        public BrokenRulesException()
        {

        }

        public BrokenRulesException(string message) : base(message)
        {

        }

        public BrokenRulesException(string message, Exception inner) : base (message, inner)
        {

        }
    }
}
