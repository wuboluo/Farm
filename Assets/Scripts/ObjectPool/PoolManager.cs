using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


// https://docs.unity.cn/cn/2021.2/ScriptReference/Pool.ObjectPool_1.html
public class PoolManager : MonoBehaviour
{
    public List<GameObject> poolPrefabs;
    public List<ObjectPool<GameObject>> poolEffectList = new();

    public Queue<GameObject> soundQueue = new();

    private void Start()
    {
        CreatePool();
    }

    private void OnEnable()
    {
        EventHandler.ParticleEffectEvent += OnParticleEffect;
        EventHandler.InitSoundEffectEvent += InitSoundEffect;
    }

    private void OnDisable()
    {
        EventHandler.ParticleEffectEvent -= OnParticleEffect;
        EventHandler.InitSoundEffectEvent -= InitSoundEffect;
    }

    private void OnParticleEffect(ParticleEffectType effectType, Vector3 pos)
    {
        var objPool = effectType switch
        {
            ParticleEffectType.LeavesFalling01 => poolEffectList[0],
            ParticleEffectType.LeavesFalling02 => poolEffectList[1],
            ParticleEffectType.Rock => poolEffectList[2],
            ParticleEffectType.ReapableScenery => poolEffectList[3],
            _ => null
        };

        if (objPool != null)
        {
            var obj = objPool.Get();
            obj.transform.position = pos;
            StartCoroutine(ReleaseRoutine(objPool, obj));
        }
    }

    /// 生成对象池
    private void CreatePool()
    {
        foreach (var item in poolPrefabs)
        {
            // 为每一种对象创建一个父节点，并同意放至 PoolMgr下
            var parent = new GameObject(item.name).transform;
            parent.SetParent(transform);

            // 使用官方提供的 objectPool 对象，并设置其事件
            var newPool = new ObjectPool<GameObject>
            (
                () => Instantiate(item, parent),
                e => e.SetActive(true),
                e => e.SetActive(false),
                Destroy
            );

            poolEffectList.Add(newPool);
        }
    }

    private IEnumerator ReleaseRoutine(IObjectPool<GameObject> pool, GameObject obj)
    {
        yield return new WaitForSeconds(1.5f);
        pool.Release(obj);
    }


    // private void InitSoundEffect(SoundDetails soundDetails)
    // {
    //     var pool = poolEffectList[4];
    //     var obj = pool.Get();
    //
    //     obj.GetComponent<Sound>().SetSound(soundDetails);
    //
    //     StartCoroutine(DisableSound(pool, obj, soundDetails));
    // }
    //
    // private IEnumerator DisableSound(IObjectPool<GameObject> pool, GameObject obj, SoundDetails soundDetails)
    // {
    //     yield return new WaitForSeconds(soundDetails.soundClip.length);
    //     pool.Release(obj);
    // }

    private void CreateSoundPool()
    {
        var parent = new GameObject(poolPrefabs[4].name).transform;
        parent.SetParent(transform);

        for (var i = 0; i < 20; i++)
        {
            var newObj = Instantiate(poolPrefabs[4], parent);
            newObj.SetActive(false);
            soundQueue.Enqueue(newObj);
        }
    }

    private GameObject GetPoolObject()
    {
        if (soundQueue.Count < 2) CreateSoundPool();
        return soundQueue.Dequeue();
    }

    private void InitSoundEffect(SoundDetails soundDetails)
    {
        var obj = GetPoolObject();
        obj.GetComponent<Sound>().SetSound(soundDetails);
        obj.SetActive(true);

        StartCoroutine(DisableSound(obj, soundDetails.soundClip.length));
    }

    private IEnumerator DisableSound(GameObject obj, float duration)
    {
        yield return new WaitForSeconds(duration);
        obj.SetActive(false);
        soundQueue.Enqueue(obj);
    }
}