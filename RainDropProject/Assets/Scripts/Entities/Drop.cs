using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GMDG.RainDrop.Entities
{
    public class Drop : MonoBehaviour
    {
        [SerializeField] TMP_Text OperationText;

        private Operation _operation;
        public Operation Operation
        {
            get
            { 
                return _operation;
            }
            set 
            { 
                _operation = value;
                OperationText.text = _operation.ToString();
            }
        }

        #region Unity_Messages

        private void Awake()
        {
            Debug.Assert(OperationText != null, "Drop spawned without a TMP_Text attached!");
        }

        #endregion

        public int GetOperationResult()
        {
            Debug.Assert(_operation != null, "Operation has not been set on this drop!");
            return _operation.Result;
        }
    }
}
