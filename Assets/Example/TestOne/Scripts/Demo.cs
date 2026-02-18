using DotsBridge;
using DotsBridge.Timeline;
using System;
using System.Collections;
using UnityEngine;

public class Demo : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private int Count = 10000;
    [SerializeField] private Vector3 OffsetForSpawn;
    private void Start() => StartCoroutine(Corutine());

    public IEnumerator Corutine()
    {
        yield return new WaitForSeconds(1);

        Dots.Spawn()
             .SetPrefab(Dots.GetPrefab(_prefab.name))
             .SetCount(100)
             .SetPosition(transform.position)
             .SetID("EnemyWave1")
             .SetDuration(1)
             .Build();

        Dots.Spawn()
            .SetCount(10)
            .SetID("Waypoints")
            .Build();

        yield return new WaitForSeconds(2f);

        Dots.Find("EnemyWave1")
            .Move(new Vector3(0, 0, 10), 1f, Ease.InOutQuad)
            .Build();

    }

}