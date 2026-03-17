using ModbusBridge;
using System;
using System.Collections;
using UnityEngine;

public class PumpSimulationBridge : MonoBehaviour
{
    [Header("bool")]
    [SerializeField] private ushort AddresBool = 6;
    [SerializeField] private bool ValueBool = true;
    [Header("Int")]
    [SerializeField] private ushort AddresInt = 4;
    [SerializeField] private short ValueInt = 52;
    private void Start()
    {
        ModbusService.CreateConnection("TestCabinet")
          .SetIp("127.0.0.1")
          .SetPort(502)
          .SetUnitId(1)
          .Build();

        var connection = ModbusService.GetConnection("TestCabinet");

        connection.InputInt(2).OnChangeSubscribe(value =>
        {
            Debug.Log($"<color=green>[SUCCESS]</color> Данные получены! Значение в ПЛК: {value}");
        });

        ModbusService.ActivateAll();
    }

    [ContextMenu("OpenValve")]
    public void OpenValve()
    {
        var conn = ModbusService.GetConnection("TestCabinet");

        conn.WriteBool(AddresBool, ValueBool);

        conn.WriteInt16(AddresInt, ValueInt);
    }
}
