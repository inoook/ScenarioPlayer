using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 時間の経過で登録されたイベントを発行する
/// </summary>
[System.Serializable]
public class KeyTimePlayer : ITimePlayable
{
    [System.Serializable]
    public class KeyTime
    {
        public float timeSec = -1;

        public virtual void DoAct()
        {
            Debug.LogWarning("Act: "+this);
        }

        public override string ToString()
        {
            return $"KeyTime: {timeSec:0.00}";
        }
    }


    public delegate void ScenarioHandler(KeyTime key, float timeSec);
    public event ScenarioHandler eventScenario;

    public delegate void ScenarioEventHandler();
    public event ScenarioEventHandler eventScenarioComplete;

    [SerializeField] bool isPlay = false;
    [SerializeField] bool isLoop = false;
    [SerializeField, Range(0, 1)] float midPer = 0; // 今と次のScenarioの間 per(0 ~ 1.0f)
    [SerializeField, Range(0, 1)] float progress = 0;

    KeyTime[] keyList = null;

    [SerializeField] int scenarioIndex = -1;
    [SerializeField] float currentTimeSec = 0;

    public void Init(KeyTime[] keyList)
    {
        this.keyList = keyList;
    }

    public float Progress => progress;
    public float Length
    {
        get { return keyList[ScenarioCount - 1].timeSec; }
    }
    public float CurrentTime => currentTimeSec;
    public bool IsPlaying => isPlay;
    public bool IsLoop => isLoop;


    public void Play()
    {
        isPlay = true;
    }
    public void Pause()
    {
        isPlay = false;
    }
    public void Stop()
    {
        isPlay = false;
        Rewind();
    }

    public void Rewind()
    {
        scenarioIndex = -1;
        currentTimeSec = 0;

        Evaluate(currentTimeSec);
    }

    /// <summary>
    /// Updateで呼び出す
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Process(float deltaTime)
    {
        if (!isPlay) { return; }

        if (!IsComplete())
        {
            currentTimeSec += deltaTime;
            Evaluate(currentTimeSec);
        }
    }

    public void Evaluate(float timeSec)
    {
        KeyTime end = keyList[keyList.Length - 1];
        progress = Mathf.InverseLerp(0, end.timeSec, timeSec);

        float deltaTime = timeSec - currentTimeSec;
        currentTimeSec = Mathf.Clamp(timeSec, 0, end.timeSec);

        int index = GetIndex(timeSec);

        int currentIndex = Mathf.Clamp(index, 0, keyList.Length - 1);
        int nextIndex = Mathf.Clamp(index + 1, 0, keyList.Length - 1);
        KeyTime current = keyList[currentIndex];
        KeyTime next = keyList[nextIndex];
        midPer = Mathf.InverseLerp(current.timeSec, next.timeSec, timeSec);// 今と次のScenarioの間 per(0 ~ 1.0f)

        // event
        if (scenarioIndex != index)
        {
            int delta = index - scenarioIndex;
            if (delta > 1)
            {
                Debug.LogWarning("error: Scenarioのdeltaが2以上です。シナリオがスキップされた可能性があります。scenarioIndex: " + scenarioIndex + " / index: " + index);
            }
            //
            for (int i = 0; i < delta; i++)
            {
                int _index = scenarioIndex + 1 + i;
                eventScenario?.Invoke(keyList[_index], timeSec);
            }
            //
            if (index == keyList.Length - 1)
            {
                if (isLoop)
                {
                    // 頭から再生
                    Rewind();
                }
                else
                {
                    // 終了
                    isPlay = false;
                }
                eventScenarioComplete?.Invoke();
            }
            //
            scenarioIndex = index;
        }
    }


    bool IsComplete()
    {
        return scenarioIndex == keyList.Length - 1;
    }

    int GetIndex(float timeSec)
    {
        int index = -1;
        for (int i = 0; i < keyList.Length; i++)
        {
            KeyTime s = keyList[i];
            if (s.timeSec <= timeSec)
            {
                index = i;
            } else {
                return index;
            }
        }
        return index;
    }

    public int ScenarioCount => keyList.Length;

    public ITimePlayable GetITimePlayable()
    {
        return this;
    }
}
