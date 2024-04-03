using GMDG.RainDrop.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GMDG.RainDrop.Scriptable
{
    [CreateAssetMenu(fileName = "Difficulty Asset", menuName = "ScriptableObjects/DifficultyAsset", order = 1)]
    public class DifficultyAsset : ScriptableObject
    {
        [Range(1, 100)]
        public int GoldenDropSpawnPercentage = 1;

        [Range(0.5f, 10.0f)]
        public float DropsSpeed = 1.0f;

        [Range(0.1f, 4f)]
        public float SpawnCooldown = 2.0f;

        public int ScoreToReach = 1000;

        public List<OperationData> OperationsData = new List<OperationData>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (GoldenDropSpawnPercentage < 1 || GoldenDropSpawnPercentage > 100)
            {
                Debug.LogError("GoldenDropSpawnPercentage must be a value between 1 and 100!");
            }

            if (DropsSpeed < 0.5f || DropsSpeed > 10.0f)
            {
                Debug.LogError("DropSpeed must be a value between 1 and 10!");
            }

            if (SpawnCooldown < 0.1f || SpawnCooldown > 4.0f)
            {
                Debug.LogError("SpawnCooldown must be a value between 0.1 and 4!");
            }

            if (ScoreToReach <= 0)
            {
                Debug.LogError("ScoreToReach can't be less than equal of 0!");
            }

            for (int i = 0; i < OperationsData.Count; i++)
            {
                for (int j = i + 1; j < OperationsData.Count; j++)
                {
                    if (OperationsData[i].FirstOperand == OperationsData[j].FirstOperand &&
                        OperationsData[i].SecondOperand == OperationsData[j].SecondOperand &&
                        OperationsData[i].OperationType == OperationsData[j].OperationType)
                    {
                        Debug.LogError(string.Format("Operation List contains duplicates! 1st op: {0}, 2nd op: {1}, op type {2}", OperationsData[i].FirstOperand, OperationsData[i].SecondOperand, OperationsData[i].OperationType));
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
        And,
        Or,
    }
}
