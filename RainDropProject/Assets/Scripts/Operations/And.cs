using System;

namespace GMDG.RainDrop.Entities
{
    public class And : Operation
    {
        // Operands are expected to represent binary numbers
        public And(int firstOperand, int secondOperand) : base(firstOperand, secondOperand)
        {
            int firstOperandDecimal = Convert.ToInt32(firstOperand.ToString(), 2);
            int secondOperandDecimal = Convert.ToInt32(secondOperand.ToString(), 2);
            
            Result = int.Parse(Convert.ToString(firstOperandDecimal & secondOperandDecimal, 2));
        }

        public override string ToString()
        {
            string text = string.Empty;
            string firstOperandText = _firstOperand.ToString();
            string secondOperandText = _secondOperand.ToString();

            if (firstOperandText.Length > secondOperandText.Length) 
            {
                string textToAdd = string.Empty;
                for (int i = 0; i < firstOperandText.Length - secondOperandText.Length; i++)
                {
                    textToAdd += "0";
                }
                secondOperandText = secondOperandText.Insert(0, textToAdd);
            }
            else if (secondOperandText.Length > firstOperandText.Length)
            {
                string textToAdd = string.Empty;
                for (int i = 0; i < secondOperandText.Length - firstOperandText.Length; i++)
                {
                    textToAdd += "0";
                }
                firstOperandText = firstOperandText.Insert(0, textToAdd);
            }

            text += firstOperandText + Environment.NewLine + "&" + Environment.NewLine + secondOperandText;

            return text;
        }
    }
}
