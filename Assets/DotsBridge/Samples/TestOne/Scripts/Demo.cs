using DotsBridge;
using DotsBridge.Modules.Movement;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class InputContext
{
    public float3 CurrentDirection;
}

public class Demo : MonoBehaviour
{
    [SerializeField] private int Count = 10000;

    private bool _isStart;

    private InputContext _inputContext = new InputContext();

    private DotsCommand _moveCommand;

    private void Start()
    {
        StartCoroutine(Corutine());
    }

    public IEnumerator Corutine()
    {
        yield return new WaitForSeconds(1);


        EntityBridge
            .BeginSpawn("Cub")
            .SetCount(10)
            .SetPosition(new Vector3(0, 5, 0))
            .Spawn("EnemyWave1")
            .SetData(new MoveTransformSpeed { Value = 5 })
            .Do(batch => Debug.Log($"Волна появилась! Юнитов: {batch.Entities.Length}"))
            .SetDestroyTimer(3)
            .Execute();

        _moveCommand = EntityBridge.Command("MoveWave")
            .GetById("EnemyWave1")
            .Move(() => _inputContext.CurrentDirection);

        yield return new WaitForSeconds(5);
        EntityBridge.GetById("Cub")
            .Move(new Vector3(0, 1, 0));
        _isStart = true;
    }

    private void Update()
    {
        if (!_isStart)
            return;

        float3 currentInput = new float3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);

        if (!currentInput.Equals(_inputContext.CurrentDirection))
        {
            _inputContext.CurrentDirection = currentInput;
            _moveCommand.Execute();
        }
    }
}