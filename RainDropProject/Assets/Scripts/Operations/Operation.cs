using System;

namespace GMDG.RainDrop.Entities
{
    [Serializable]
    public abstract class Operation
    {
        public int x;
        public int Result { get; protected set; }
        
        protected int _firstOperand;
        protected int _secondOperand;

        protected Operation(int firstOperand, int secondOperand) 
        { 
            _firstOperand = firstOperand;
            _secondOperand = secondOperand;
        }

        public abstract override string ToString();
    }
}
