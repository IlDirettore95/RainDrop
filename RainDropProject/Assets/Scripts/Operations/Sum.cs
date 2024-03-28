namespace GMDG.RainDrop.Entities
{
    public class Sum : Operation
    {
        public Sum(int firstOperand, int secondOperand, int result) : base(firstOperand, secondOperand, result) { }

        public override string ToString()
        {
            string text = string.Empty;

            text += _firstOperand + "+" + _secondOperand;

            return text;
        }
    }
}
