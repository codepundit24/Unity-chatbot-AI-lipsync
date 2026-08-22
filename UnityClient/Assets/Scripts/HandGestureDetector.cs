using Mediapipe.Tasks.Vision.HandLandmarker;
using TMPro;
using UnityEngine;

public class HandGestureDetector : MonoBehaviour
{
    [SerializeField] private VoiceClient voiceClient;
    [SerializeField] private GameObject gestureIcon;
    [SerializeField] private TMP_Text gestureStatusText;

    private bool isRecording, startRequested, stopRequested;

    // Frame stability counter (accidental gestures ignore)
    private int openPalmFrames , fistFrames ;
    private const int REQUIRED_FRAMES = 5;

    // Landmark Indices: Tip & MCP (Knuckle) pairs for Index, Middle, Ring, Pinky
    private static readonly (int tip, int mcp)[] FingerJoints = { (8, 5), (12, 9), (16, 13), (20, 17) };

    public void ProcessHandResult(HandLandmarkerResult result)
    {
        if (result.handLandmarks == null || result.handLandmarks.Count == 0)
        {
            ResetCounters();
            return;
        }

        var hand = result.handLandmarks[0].landmarks;
        if (hand == null || hand.Count < 21) return;

        Vector2 wrist = new Vector2(hand[0].x, hand[0].y);
        int openCount = 0, closedCount = 0;

        foreach (var (tip, mcp) in FingerJoints)
        {
            float tipDist = Vector2.Distance(new Vector2(hand[tip].x, hand[tip].y), wrist);
            float mcpDist = Vector2.Distance(new Vector2(hand[mcp].x, hand[mcp].y), wrist);

            if (tipDist > mcpDist * 1.3f) openCount++;
            else if (tipDist <= mcpDist * 1.1f) closedCount++;
        }

        // Thumb check (Tip 4 separated from Pinky Base 17)
        Vector2 pinkyBase = new(hand[17].x, hand[17].y);
        bool thumbOpen = Vector2.Distance(new Vector2(hand[4].x, hand[4].y), pinkyBase) >
                         Vector2.Distance(new Vector2(hand[2].x, hand[2].y), pinkyBase);

        bool isOpenPalm = openCount == 4 && thumbOpen;
        bool isFist = closedCount == 4;

        if (isOpenPalm && UpdateFrames(ref openPalmFrames, ref fistFrames, !isRecording))
            startRequested = true;
        else if (isFist && UpdateFrames(ref fistFrames, ref openPalmFrames, isRecording))
            stopRequested = true;
        else if (!isOpenPalm && !isFist)
            ResetCounters();
    }
    private void Update()
    {
        if (startRequested && !isRecording)
        {
            ToggleState(true, "Listening...", () => voiceClient.StartRecording());
            startRequested = false;
        }
        else if (stopRequested && isRecording)
        {
            ToggleState(false, "Processing...", () => voiceClient.StopRecording());
            stopRequested = false;
            Invoke(nameof(HideGestureFeedback), 2f);
        }
    }

    private bool UpdateFrames(ref int matchFrames, ref int otherFrames, bool condition)
    {
        otherFrames = 0;
        if (condition && ++matchFrames >= REQUIRED_FRAMES)
        {
            matchFrames = 0;
            return true;
        }
        return false;
    }

    private void ToggleState(bool recording, string status, System.Action action)
    {
        isRecording = recording;
        if (gestureIcon) gestureIcon.SetActive(recording);
        if (gestureStatusText) gestureStatusText.text = status;
        action?.Invoke();
    }

    private void ResetCounters() => openPalmFrames = fistFrames = 0;
    private void HideGestureFeedback()
    {
        if (gestureIcon) gestureIcon.SetActive(false);
        if (gestureStatusText) gestureStatusText.text = "";
    }
}


    //// Receives hand tracking results from MediaPipe worker thread
    //public void ProcessHandResult(HandLandmarkerResult result)
    //{
    //    if (result.handLandmarks == null || result.handLandmarks.Count == 0)
    //    {
    //        openPalmFrames = 0;
    //        fistFrames = 0;
    //        return;
    //    }

    //    var hand = result.handLandmarks[0].landmarks;

    //    if (hand == null || hand.Count < 21)
    //        return;

    //    var wrist = new Vector2(hand[0].x, hand[0].y);

    //    // Distances from wrist to tips
    //    float indexDist = Vector2.Distance(new Vector2(hand[8].x, hand[8].y), wrist);
    //    float middleDist = Vector2.Distance(new Vector2(hand[12].x, hand[12].y), wrist);
    //    float ringDist = Vector2.Distance(new Vector2(hand[16].x, hand[16].y), wrist);
    //    float pinkyDist = Vector2.Distance(new Vector2(hand[20].x, hand[20].y), wrist);

    //    // Distances from wrist to MCP joints (knuckles)
    //    float indexMcpDist = Vector2.Distance(new Vector2(hand[5].x, hand[5].y), wrist);
    //    float middleMcpDist = Vector2.Distance(new Vector2(hand[9].x, hand[9].y), wrist);
    //    float ringMcpDist = Vector2.Distance(new Vector2(hand[13].x, hand[13].y), wrist);
    //    float pinkyMcpDist = Vector2.Distance(new Vector2(hand[17].x, hand[17].y), wrist);

    //    // Open Palm Checks (Tips extended far beyond knuckles)
    //    bool indexOpen = indexDist > indexMcpDist * 1.3f;
    //    bool middleOpen = middleDist > middleMcpDist * 1.3f;
    //    bool ringOpen = ringDist > ringMcpDist * 1.3f;
    //    bool pinkyOpen = pinkyDist > pinkyMcpDist * 1.3f;

    //    // Thumb check (Tip 4 separated from Pinky Base 17)
    //    bool thumbOpen = Vector2.Distance(new Vector2(hand[4].x, hand[4].y), new Vector2(hand[17].x, hand[17].y)) >
    //                     Vector2.Distance(new Vector2(hand[2].x, hand[2].y), new Vector2(hand[17].x, hand[17].y));

    //    bool isOpenPalm = indexOpen && middleOpen && ringOpen && pinkyOpen && thumbOpen;

    //    // Fist Checks (All 4 fingers curled down towards palm/wrist)
    //    bool indexClosed = indexDist <= indexMcpDist * 1.1f;
    //    bool middleClosed = middleDist <= middleMcpDist * 1.1f;
    //    bool ringClosed = ringDist <= ringMcpDist * 1.1f;
    //    bool pinkyClosed = pinkyDist <= pinkyMcpDist * 1.1f;

    //    bool isFist = indexClosed && middleClosed && ringClosed && pinkyClosed;

    //    // Consecutive frames debounce
    //    if (isOpenPalm)
    //    {
    //        openPalmFrames++;
    //        fistFrames = 0;
    //        if (openPalmFrames >= REQUIRED_FRAMES && !isRecording)
    //        {
    //            startRequested = true;
    //            openPalmFrames = 0;
    //        }
    //    }
    //    else if (isFist)
    //    {
    //        fistFrames++;
    //        openPalmFrames = 0;
    //        // FIX: Must check 'isRecording' when stopping!
    //        if (fistFrames >= REQUIRED_FRAMES && isRecording)
    //        {
    //            stopRequested = true;
    //            fistFrames = 0;
    //        }
    //    }
    //    else
    //    {
    //        openPalmFrames = 0;
    //        fistFrames = 0;
    //    }
    //}

    //// Update runs on Unity's main thread
    //private void Update()
    //{
    //    if (startRequested && !isRecording)
    //    {
    //        startRequested = false;
    //        isRecording = true;

    //        if (gestureIcon != null) gestureIcon.SetActive(true);
    //        if (gestureStatusText != null) gestureStatusText.text = "Listening...";

    //        Debug.Log("Open Palm detected - Start Recording");
    //        voiceClient.StartRecording();
    //    }

    //    if (stopRequested && isRecording)
    //    {
    //        stopRequested = false;
    //        isRecording = false;

    //        if (gestureStatusText != null) gestureStatusText.text = "Processing...";

    //        Debug.Log("Fist detected - Stop Recording");
    //        voiceClient.StopRecording();

    //        Invoke(nameof(HideGestureFeedback), 2f);
    //    }
    //}

    //private void HideGestureFeedback()
    //{
    //    if (gestureIcon != null) gestureIcon.SetActive(false);
    //    if (gestureStatusText != null) gestureStatusText.text = "";
    //}