using System.Diagnostics;

/// <summary>
/// 알고리즘 성능 측정을 위한 Benchmark 타이머 관리자 클래스
/// </summary>
public class BenchmarkManager
{
    private Stopwatch _stopwatch; // 실행 시간 측정을 위한 Stopwatch

    public BenchmarkManager()
    {
        _stopwatch = new Stopwatch();
    }

    public void Start()
    {
        _stopwatch.Restart();
    }

    public long Stop()
    {
        _stopwatch.Stop();
        return _stopwatch.ElapsedMilliseconds;
    }
}