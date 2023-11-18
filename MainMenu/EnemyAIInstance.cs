using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace NIKO_Menu_V2.MainMenu
{
    internal class EnemyAI : MonoBehaviour
    {
        public static EnemyAI Instance { get; private set; }
        private void Awake()
        {
            EnemyAI enemyAI = UnityEngine.Object.FindObjectOfType<EnemyAI>();
            if (Instance == null && enemyAI != null)
            {
                Instance = enemyAI;
            }
            else
            {
                Debug.LogWarning("PlayerControllerB not found or multiple instances detected.");
                UnityEngine.Object.Destroy(base.gameObject);
            }
        }
    }
}