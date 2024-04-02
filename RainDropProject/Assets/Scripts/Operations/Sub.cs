namespace GMDG.RainDrop.Entities
{
    public class Sub : Operation
    {
        public Sub(int firstOperand, int secondOperand) : base(firstOperand, secondOperand)
        {
            Result = firstOperand - secondOperand;
        }

        public override string ToString()
        {
            string text = string.Empty;

            text += _firstOperand + "-" + _secondOperand;

            return text;
        }
    }
}
