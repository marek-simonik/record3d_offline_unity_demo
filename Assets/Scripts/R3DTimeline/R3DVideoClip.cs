using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


[Serializable]
public class R3DVideoClip : PlayableAsset, ITimelineClipAsset
{
    public R3DVideoBehaviour template = new R3DVideoBehaviour();
    public ExposedReference<Record3DPlayback> endLocation;

    public override double duration => playback == null ? 0 : playback.numberOfFrames / playback.fps;

    public ClipCaps clipCaps => ClipCaps.Blending;

    private Record3DPlayback playback = null;


    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var player = endLocation.Resolve(graph.GetResolver());

        var playable = ScriptPlayable<R3DVideoBehaviour>.Create(graph, template);
        R3DVideoBehaviour clone = playable.GetBehaviour();
        clone.endLocation = player;

        playback = player;

        return playable;
    }
}
