namespace GMDG.RainDrop.Entities
{
    public class Mul : Operation
    {
        public Mul(int firstOperand, int secondOperand) : base(firstOperand, secondOperand)
        {
            Result = firstOperand * secondOperand;
        }

        public override string ToString()
        {
            string text = string.Empty;

            text += _firstOperand + "X" + _secondOperand;

            return text;
        }
    }
}
