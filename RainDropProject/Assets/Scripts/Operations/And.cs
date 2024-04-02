using System;

namespace GMDG.RainDrop.Entities
{
    public class And : Operation
    {
        public And(int firstOperand, int secondOperand) : base(firstOperand, secondOperand)
        {
            // TODO
            // Result
        }

        public override string ToString()
        {
            string text = string.Empty;

            text += _firstOperand + Environment.NewLine + "&" + Environment.NewLine + _secondOperand;

            return text;
        }
    }
}
