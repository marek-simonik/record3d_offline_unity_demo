using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoManager : MonoBehaviour
{
    // Starting frame (in percents)
    [Range(0.0f, 1.0f)]
    public float startingFramePercent;

    public string videoFilePath;

    private Record3DPlayback videoPlayer;

    // Start is called before the first frame update
    void Start()
    {
        // We add the Record3DPlayback script to the node to which this script is attached.
        // This allows us to load the video later.
        videoPlayer = gameObject.AddComponent<Record3DPlayback>();

        // We load the video from the user-specified filepath
        videoPlayer.LoadVideo(videoFilePath);

        // Here we compute which frame should be the starting one.
        int videoStartingFrame = (int)Mathf.Round(startingFramePercent * videoPlayer.numberOfFrames);

        // We load that particular frame.
        videoPlayer.LoadFrame(videoStartingFrame);

        // And then play the video after the user presses the Play button.
        videoPlayer.Play();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
