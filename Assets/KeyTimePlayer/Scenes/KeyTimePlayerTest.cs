using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTimePlayerTest : MonoBehaviour
{
    [System.Serializable]
    public class KeyTimeAction : KeyTimePlayer.KeyTime
    {
        [SerializeField] Transform targetTrans = null;
        [SerializeField] float scale = 1;
        public override void DoAct()
        {
            targetTrans.localScale = Vector3.one * scale;
        }
    }

    [SerializeField] KeyTimeAction[] keyTimeActions = null;
    [SerializeField] KeyTimePlayer keyTimePlayer = null;
    ITimePlayable timePlayable = null;

    // Start is called before the first frame update
    void Start()
    {
        keyTimePlayer.eventScenario += ScenarioPlayerBase_eventScenario;
        keyTimePlayer.eventScenarioComplete += ScenarioPlayerBase_eventScenarioComplete;

        keyTimePlayer.Init(keyTimeActions);

        timePlayable = keyTimePlayer.GetITimePlayable();
        timePlayable.Rewind();
    }

    private void ScenarioPlayerBase_eventScenarioComplete()
    {
        Debug.LogWarning("Complete");
    }

    private void ScenarioPlayerBase_eventScenario(KeyTimePlayer.KeyTime key, float timeSec)
    {
        Debug.LogWarning($"Action index: {key.timeSec} / timeSec: {timeSec}" );
        key.DoAct();
    }

    // Update is called once per frame
    void Update()
    {
        timePlayable.Process(Time.deltaTime);
    }

    [SerializeField] Rect drawRect = new Rect(10,10,400,400);
    private void OnGUI()
    {
        GUILayout.BeginArea(drawRect);
        if (GUILayout.Button("Rewind"))
        {
            timePlayable.Rewind();
        }
        if (GUILayout.Button("Play"))
        {
            timePlayable.Play();
        }
        if (GUILayout.Button("Pause"))
        {
            timePlayable.Pause();
        }
        if (GUILayout.Button("Stop"))
        {
            timePlayable.Stop();
        }
        GUILayout.Label($"IsPlaying: {timePlayable.IsPlaying}");

        float timeSec = timePlayable.CurrentTime;
        GUILayout.Label(timeSec.ToString("0.00"));
        timeSec = GUILayout.HorizontalSlider(timeSec, 0, timePlayable.Length);
        GUILayout.EndArea();
    }
}
