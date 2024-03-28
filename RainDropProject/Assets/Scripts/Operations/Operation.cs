
namespace GMDG.RainDrop.Entities
{
    public abstract class Operation
    {
        public int Result { get; protected set; }
        
        protected int _firstOperand;
        protected int _secondOperand;

        protected Operation(int firstOperand, int secondOperand, int result) 
        { 
            _firstOperand = firstOperand;
            _secondOperand = secondOperand;
            Result = result;
        }

        public abstract override string ToString();
    }
}
