using GMDG.RainDrop.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GMDG.RainDrop.Scriptable
{
    [CreateAssetMenu(fileName = "Difficulty Asset", menuName = "ScriptableObjects/DifficultyAsset", order = 1)]
    public class DifficultyAsset : ScriptableObject
    {
        public int GoldenDropSpawnPercentage = 1;
        public List<OperationData> OperationsData = new List<OperationData>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (GoldenDropSpawnPercentage < 1 || GoldenDropSpawnPercentage > 100)
            {
                Debug.LogError("GoldenDropSpawnPercentage must be a value between 1 and 100");
            }

            for (int i = 0; i < OperationsData.Count; i++)
            {
                for (int j = i + 1; j < OperationsData.Count; j++)
                {
                    if (OperationsData[i].FirstOperand == OperationsData[j].FirstOperand &&
                        OperationsData[i].SecondOperand == OperationsData[j].SecondOperand &&
                        OperationsData[i].OperationType == OperationsData[j].OperationType)
                    {
                        Debug.LogError("Operation List contains duplicates!");
                        return;
                    }
                }
            }
        }
#endif

    }

    [Serializable]
    public struct OperationData
    {
        public int FirstOperand;
        public int SecondOperand;
        public EOperationType OperationType;
    }

    public enum EOperationType 
    { 
        Sum,
        Sub,
        Mul,
        Div,
    }
}
