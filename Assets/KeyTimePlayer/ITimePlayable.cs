/// <summary>
///時間での制御可能なインターフェース
/// </summary>
public interface ITimePlayable
{
    bool IsPlaying { get; }
    bool IsLoop { get; }
    // 経過時間
    float CurrentTime { get; }
    // 経過パーセンテージ
    float Progress { get; }
    // 全体の尺
    float Length { get; }

    void Process(float deltaTime);
    // 指定時間での更新。 Process内部でも使用される。Seekとして使う。
    void Evaluate(float timeSec);
    void Play();
    void Pause();
    void Stop();
    void Rewind();
}
