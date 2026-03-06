using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    [DefaultExecutionOrder(-100)] // Должен просыпаться раньше других скриптов
    public class DotsBridgeBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
        }
    }
}