using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// Lightweight runtime performance logger.
/// Attach to an empty GameObject in the scene, enter Play mode (or run a build),
/// play for ~60 seconds, then stop. It writes a CSV of per-second samples and
/// prints an averaged summary to the console and to a _summary.txt file.
///
/// Output location:
///   Editor / standalone build -> next to the project (Application.dataPath/..)
///   The exact path is printed to the Console on start and on stop.
/// </summary>
public class PerfLogger : MonoBehaviour
{
    [Header("Capture settings")]
    [Tooltip("How often (seconds) to write one aggregated sample row.")]
    public float sampleInterval = 1f;

    [Tooltip("Optional label written into the CSV (e.g. 'Laptop-3050Ti' or 'normal-play').")]
    public string sessionLabel = "session";

    [Tooltip("Ignore the first N seconds so startup/JIT spikes don't skew averages.")]
    public float warmupSeconds = 3f;

    // --- internal state ---
    private float _sampleTimer;
    private float _warmupTimer;
    private bool _warmedUp;

    // per-interval accumulators
    private int _frames;
    private float _accumDeltaMs;          // sum of frame times this interval (ms)
    private float _worstFrameMs;          // worst (longest) frame this interval (ms)
    private float _bestFrameMs = float.MaxValue;

    // whole-session accumulators (post-warmup)
    private int _sessionFrames;
    private float _sessionTimeSec;
    private float _sessionWorstFrameMs;
    private long _gcAtStart;
    private int _gcCollectionsAtStart;

    private StringBuilder _csv;
    private string _csvPath;
    private string _summaryPath;

    void Awake()
    {
        // Don't cap our reading of the real frame rate.
        // (Comment these two lines out if your build intentionally locks FPS.)
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;

        _csv = new StringBuilder();
        _csv.AppendLine("label,t_sec,avg_fps,avg_frame_ms,best_frame_ms,worst_frame_ms,mono_used_mb,total_reserved_mb,gc_count_total");

        string dir = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string stamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        _csvPath = Path.Combine(dir, $"perf_{sessionLabel}_{stamp}.csv");
        _summaryPath = Path.Combine(dir, $"perf_{sessionLabel}_{stamp}_summary.txt");

        _gcAtStart = System.GC.GetTotalMemory(false);
        _gcCollectionsAtStart = System.GC.CollectionCount(0)
                              + System.GC.CollectionCount(1)
                              + System.GC.CollectionCount(2);

        Debug.Log($"[PerfLogger] Recording. CSV will be written to:\n{_csvPath}");
    }

    void Update()
    {
        float dtMs = Time.unscaledDeltaTime * 1000f;

        // --- warmup gate ---
        if (!_warmedUp)
        {
            _warmupTimer += Time.unscaledDeltaTime;
            if (_warmupTimer < warmupSeconds) return;
            _warmedUp = true;
        }

        // --- per-interval accumulation ---
        _frames++;
        _accumDeltaMs += dtMs;
        if (dtMs > _worstFrameMs) _worstFrameMs = dtMs;
        if (dtMs < _bestFrameMs) _bestFrameMs = dtMs;

        // --- session accumulation ---
        _sessionFrames++;
        _sessionTimeSec += Time.unscaledDeltaTime;
        if (dtMs > _sessionWorstFrameMs) _sessionWorstFrameMs = dtMs;

        // --- flush a row every sampleInterval seconds ---
        _sampleTimer += Time.unscaledDeltaTime;
        if (_sampleTimer >= sampleInterval)
        {
            float avgFrameMs = _accumDeltaMs / Mathf.Max(1, _frames);
            float avgFps = 1000f / Mathf.Max(0.0001f, avgFrameMs);

            float monoUsedMb = Profiler.GetMonoUsedSizeLong() / (1024f * 1024f);
            float totalReservedMb = Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f);
            int gcTotal = System.GC.CollectionCount(0)
                        + System.GC.CollectionCount(1)
                        + System.GC.CollectionCount(2);

            _csv.AppendLine(string.Join(",", new[]
            {
                sessionLabel,
                _sessionTimeSec.ToString("F1", CultureInfo.InvariantCulture),
                avgFps.ToString("F1", CultureInfo.InvariantCulture),
                avgFrameMs.ToString("F2", CultureInfo.InvariantCulture),
                _bestFrameMs.ToString("F2", CultureInfo.InvariantCulture),
                _worstFrameMs.ToString("F2", CultureInfo.InvariantCulture),
                monoUsedMb.ToString("F1", CultureInfo.InvariantCulture),
                totalReservedMb.ToString("F1", CultureInfo.InvariantCulture),
                gcTotal.ToString(CultureInfo.InvariantCulture)
            }));

            // reset interval accumulators
            _sampleTimer = 0f;
            _frames = 0;
            _accumDeltaMs = 0f;
            _worstFrameMs = 0f;
            _bestFrameMs = float.MaxValue;
        }
    }

    void OnApplicationQuit()  => WriteOut();
    void OnDisable()          => WriteOut();

    private bool _written;
    private void WriteOut()
    {
        if (_written) return;
        _written = true;

        try
        {
            File.WriteAllText(_csvPath, _csv.ToString());

            float avgFps = _sessionFrames / Mathf.Max(0.0001f, _sessionTimeSec);
            float avgFrameMs = (_sessionTimeSec * 1000f) / Mathf.Max(1, _sessionFrames);
            int gcDuring = (System.GC.CollectionCount(0)
                          + System.GC.CollectionCount(1)
                          + System.GC.CollectionCount(2)) - _gcCollectionsAtStart;
            float totalReservedMb = Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f);
            float monoUsedMb = Profiler.GetMonoUsedSizeLong() / (1024f * 1024f);

            var s = new StringBuilder();
            s.AppendLine("=== PerfLogger summary ===");
            s.AppendLine($"Label:               {sessionLabel}");
            s.AppendLine($"Duration (post-warmup): {_sessionTimeSec:F1} s");
            s.AppendLine($"Frames:              {_sessionFrames}");
            s.AppendLine($"Average FPS:         {avgFps:F1}");
            s.AppendLine($"Average frame time:  {avgFrameMs:F2} ms");
            s.AppendLine($"Worst frame:         {_sessionWorstFrameMs:F2} ms");
            s.AppendLine($"Mono used:           {monoUsedMb:F1} MB");
            s.AppendLine($"Total reserved:      {totalReservedMb:F1} MB");
            s.AppendLine($"GC collections:      {gcDuring}");
            s.AppendLine($"CSV:                 {_csvPath}");

            File.WriteAllText(_summaryPath, s.ToString());
            Debug.Log("[PerfLogger]\n" + s.ToString());
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PerfLogger] Failed to write output: {e.Message}");
        }
    }
}