using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(92f/255f, 15f/255f, 91f/244f)]
[TrackClipType(typeof(R3DVideoClip))]
[TrackBindingType(typeof(Record3DPlayback))]
public class R3DVideoTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<R3DVideoBehaviour>.Create(graph, inputCount);
    }

    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
#if UNITY_EDITOR
        var comp = director.GetGenericBinding(this) as Record3DPlayback;
        if (comp == null)
            return;

        var so = new UnityEditor.SerializedObject(comp);
        var iter = so.GetIterator();
        while ( iter.NextVisible(true) )
        {
            if (iter.hasVisibleChildren)
                continue;

            driver.AddFromName<Record3DPlayback>(comp.gameObject, iter.propertyPath);
        }
#endif

        base.GatherProperties(director, driver);
    }
}
