using UnityEngine;
using System.Collections.Generic;

public class DiagnosticChronometer
{
    private double startTimeSeconds;
    private double endTimeSeconds;
    private bool started = false;
    private double addedRecordedTimeSeconds;
    private int count;

    public void Start()
    {
        if (started)
        {
            UnityEngine.Debug.LogWarning("Timer already started");
            return;
        }
        started = true;
        startTimeSeconds = Time.realtimeSinceStartup;
    }
    public void Stop()
    {
        if (!started)
        {
            UnityEngine.Debug.LogWarning("Timer not started");
            return;
        }
        started = false;
        endTimeSeconds = Time.realtimeSinceStartup;
        addedRecordedTimeSeconds += endTimeSeconds - startTimeSeconds;
        ++count;
    }
    public double GetElapsedTimeMiliseconds()
    {
        return (endTimeSeconds-startTimeSeconds) * 1000;
    }
    public double GetMeanTimeMiliseconds()
    {
        return addedRecordedTimeSeconds/count * 1000;
    }
    public void Clear()
    {
        startTimeSeconds = 0;
        endTimeSeconds = 0;
        addedRecordedTimeSeconds = 0;
        count = 0;
        started = false;
    }
}
