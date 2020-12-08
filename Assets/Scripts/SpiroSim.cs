using System;
using System.Collections;
using UnityEngine;

public class SpiroSim : MonoBehaviour
{
    private int[] _volList;
    private int[] _flowList;

    private const int NumDataPoints = 21;

    /// <summary>
    /// There are two types of tests in here and either are valid starting points for projects. The hardcoded
    /// breaths in here are 10 second challenges. The patient would have 10 seconds once a breath is detected
    /// to finish their breath. The quality of the breath would then be calculated after the 10 seconds are completed.
    /// This challenge is a remnant of a time where we could not read flow data. The volume piston takes a while to fall
    /// back down to zero where the flow gauge will zero immediately. This has since been fixed and we can now read flow.
    /// This makes detecting the end of a breath much easier as you can detect an end of flow. The second type of challenge
    /// in here will start when a flow is detected and end when two 0 values for flow are read in a row. A value is received
    /// from a Spirometer device every 0.5 seconds.
    /// </summary>


    // 1) 10 second breath challenges (flow level and number of flow drops determine quality)
    public void StartBadBreath()
    {
        StartVariableLengthTest(10);
        return;
        // Breath is bad because flow drops to zero several times. This could lead to hyperventilation
        _volList = new int[] { 0, 1, 2, 3, 4, 6, 6, 5, 5, 7, 9, 8, 7, 7, 9, 10, 9, 9, 9, 10, 10 };
        _flowList = new int[] { 0, 2, 2, 2, 3, 3, 0, 0, 3, 3, 3, 0, 0, 3, 3, 2, 0, 0, 3, 2, 0 };
        StartChallenge1();
    }

    public void StartPoorerBreath()
    {
        // Breath is poor because they  have a large gap in the center and ended test early.
        _volList = new int[] { 0, 3, 4, 5, 6, 7, 9, 10, 9, 9, 8, 7, 6, 6, 8, 9, 10, 10, 10, 10, 9};
        _flowList = new int[] { 0, 3, 2, 2, 2, 3, 3, 2, 0, 0, 0, 0, 0, 3, 3, 3, 2, 2, 2, 0, 0 };
        StartChallenge1();
    }

    public void StartPoorBreath()
    {
        // Breath is poor. It looks as if the patient had too high of a flow and caused the flow to read 0.
        _volList = new int[] { 0, 2, 3, 4, 6, 7, 9, 8, 7, 6, 6, 8, 9, 9, 10, 10, 10, 10, 10, 10, 10 };
        _flowList = new int[] { 0, 3, 3, 3, 3, 3, 0, 0, 0, 0, 3, 2, 2, 2, 2, 3, 2, 2, 3, 2, 3 };
        StartChallenge1();
    }

    public void StartGoodBreath()
    {
        // Breath levels are mostly 3s which is within GOOD range. It looks like the breath couldn't be
        // sustained and they needed to breath in to finish test.
        _volList = new int[] { 0, 2, 3, 4, 5, 6, 8, 9, 10, 10, 9, 8, 9, 10, 10, 10, 10, 10, 10, 10, 10};
        _flowList = new int[] { 0, 3, 2, 2, 2, 3, 3, 2, 2, 0, 0, 0, 3, 3, 3, 2, 3, 3, 3, 3, 3 };
        StartChallenge1();
    }

    public void StartBetterBreath()
    {
        // Breath levels are mostly 2/3s which indicates great flow. It looks like they needed to take
        // a breath intake to finish test
        _volList = new int[] { 0, 2, 3, 4, 5, 5, 6, 7, 8, 9, 9, 10, 9, 8, 8, 9, 10, 10, 10, 10, 10 };
        _flowList = new int[] { 0, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 2 };
        StartChallenge1();
    }

    public void StartBestBreath()
    {
        // Patient kept breath in GREAT flow range throughout the entire test
        _volList = new int[] { 0, 2, 2, 3, 3, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 10, 10, 10 };
        _flowList = new int[] { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        StartChallenge1();
    }

    // 2) Variable length 1 breath challenges (flow level and hold length determine quality)
    public void StartVariableLengthTest(int seconds)
    {
        // This test will give a random breath. We recommend altering it to consistently provide
        // deterministic poor/good/better/best breaths by playing with the probability.
        var rand = new System.Random();
        _volList = new int[seconds * 2 + 3];
        _flowList = new int[seconds * 2 + 3];
        _volList[0] = 0;
        _flowList[0] = 0;
        var currentVol = 1f;
        var lastVal = 0;
        for (var i = 1; i <= seconds * 2; ++i)
        {
            // Rand int determines flow. Its smart in its generation to ensure flow
            // does not jump around erratically
            var rInt = 1;
            if (lastVal == 1)
            {
                rInt = rand.Next(1, 3);
            }
            else if (lastVal == 2)
            {
                rInt = rand.Next(1, 4);
            }
            else if (lastVal == 3)
            {
                rInt = rand.Next(2, 4);
            }
            lastVal = rInt;
            _flowList[i] = rInt;

            // Volume read increases based on flow reading
            if (rInt == 1)
            {
                currentVol += 0.4f;
            }
            else if (rInt == 2)
            {
                currentVol += 0.6f;
            }
            else
            {
                currentVol += 1f;
            }
            // Volume floored to an int as spirometer can only return ints
            _volList[i] = Math.Floor(currentVol) > 10 ? 10 : (int) Math.Floor(currentVol);
        }

        // Two zeros here in flow indicate end of breath. (1 second of no flow)
        _flowList[seconds * 2 + 1] = 0;
        _flowList[seconds * 2 + 2] = 0;
        _volList[seconds * 2 + 1] = _volList[seconds * 2];
        _volList[seconds * 2 + 2] = _volList[seconds * 2];

        StartChallenge2(seconds);
    }

    // Start script has a check to ensure that only 1 co-routine runs at a time
    private void StartChallenge1()
    {
        StopAllCoroutines();
        StartCoroutine(RunChallenge1());
    }

    private void StartChallenge2(int seconds)
    {
        StopAllCoroutines();
        StartCoroutine(RunChallenge2(seconds));
    }

    private IEnumerator RunChallenge1()
    {
        // Sends an element of each array every second for a total of 10 seconds
        for (var i = 0; i < NumDataPoints; ++i)
        {
            HandleInput(i, _volList[i], _flowList[i]);
            yield return new WaitForSeconds(0.5f);
        }
        // Send a 0/0 signal at the end. (Can remove if game code doesn't rely on it)
        HandleInput(NumDataPoints + 1, 0, 0);
    }

    private IEnumerator RunChallenge2(int seconds)
    {
        // Sends an element of each array every second for a variable number of seconds
        for (var i = 0; i < seconds * 2 + 3; ++i)
        {
            HandleInput(i, _volList[i], _flowList[i]);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void HandleInput(int dataPos, int vol, int flow)
    {
        // Call Game Function Here
        // Game.sendInput(vol, flow)
        Debug.Log("Vol " + (dataPos + 1) + " Received: " + vol);
        Debug.Log("Flow " + (dataPos + 1) + " Received: " + flow);
    }
}
