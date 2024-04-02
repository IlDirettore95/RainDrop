namespace GMDG.RainDrop.Entities
{
    public class Div : Operation
    {
        public Div(int firstOperand, int secondOperand) : base(firstOperand, secondOperand)
        {
            Result = firstOperand / secondOperand;
        }

        public override string ToString()
        {
            string text = string.Empty;

            text += _firstOperand + "/" + _secondOperand;

            return text;
        }
    }
}
