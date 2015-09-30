using DeviceIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keithley24xx.CALCulate.Commands
{
    public class MATH_Command : CALCulate
    {
        protected string _Command { get; private set; }

        public MATH_Command(ref IDeviceIO Driver)
            : base(ref Driver)
        {
            _Command = string.Format("{0}{1}", SubsystemIdentifier, ":MATH");

            EXPRession = new Expression(ref Driver);
        }

        public Expression EXPRession { get; private set; }

        /// <summary>
        /// Define units name for math expression (3ASCII characters).
        /// Query math expression units name.
        /// </summary>
        public string UNITs
        {
            get
            {
                return _driver.RequestQuery(string.Format("{0}{1}", _Command, ":UNIT?"));
            }
            set
            {
                _driver.SendCommandRequest(string.Format("{0}{1} {2}", _Command, ":UNIT", value));
            }
        }
    }

    public class Expression : MATH_Command
    {
        protected string _ExpressionCommand { get; private set; }

        public Expression(ref IDeviceIO Driver)
            : base(ref Driver)
        {
            _ExpressionCommand = string.Format("{0}{1}", _Command, ":EXPR");
        }

        /// <summary>
        /// Define or gets math expression using standard math operator symbols.
        /// </summary>
        public string EXPRession
        {
            get
            {
                return _driver.RequestQuery(string.Format("{0}{1}", _ExpressionCommand, "?"));
            }
            set
            {
                _driver.SendCommandRequest(string.Format("{0} {1}", _ExpressionCommand, value));
            }
        }

        /// <summary>
        /// Query the list of expression names
        /// </summary>
        public string CATalog
        {
            get
            {
                return _driver.RequestQuery(string.Format("{0}{1}", _ExpressionCommand, ":CAT?"));
            }
        }

        /// <summary>
        /// Path to delete user-defined expressions.
        /// </summary>
        /// <param name="name">Delete specified expression or delete all expressions by default</param>
        public void DeleteEXPRession(string name = "All")
        {
            if (name != "All")
                _driver.SendCommandRequest(string.Format("{0}{1} {2}", _ExpressionCommand, ":DEL:SEL", name));
            else
                _driver.SendCommandRequest(string.Format("{0}{1}", _ExpressionCommand, ":ALL"));
        }


    }
}
