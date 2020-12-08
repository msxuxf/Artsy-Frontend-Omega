using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


 public static class GlobalTimer 
 {
    static DateTime EndTime = DateTime.MaxValue;
    
    public static void SetEndTime(DateTime endTime)
    {
        // Debug.Log("End Time is set.");
        // Debug.Log(endTime.ToString("yyyy-MM-dd-HH-mm-ss"));
        EndTime = endTime;
    }

    public static TimeSpan TimeLeft
    {
        get
        {
            // Debug.Log(EndTime.ToString("yyyy-MM-dd-HH-mm-ss"));
            var result = EndTime - DateTime.UtcNow;
            if (result.TotalSeconds <= 0)
            {
                PlayerPrefs.SetInt("UNLIMITED_STATUS", 0);
                return TimeSpan.Zero;
            }
            return result;
        }
    }
 }