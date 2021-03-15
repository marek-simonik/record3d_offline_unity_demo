using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.VFX;

[Serializable]
public class R3DVideoBehaviour : PlayableBehaviour
{
    public Record3DPlayback endLocation;

    public override void OnGraphStart(Playable playable)
    {
        base.OnGraphStart(playable);
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // Use instead of the Play function
        var trackBinding = playerData as Record3DPlayback;

        if (trackBinding == null)
            return;

        int inputCount = playable.GetInputCount();

        for (int i = 0; i < inputCount; i++)
        {
            var playableInput = (ScriptPlayable<R3DVideoBehaviour>)playable.GetInput(i);
            R3DVideoBehaviour input = playableInput.GetBehaviour();

            if (input == null || input.endLocation == null)
                continue;

            int frameIdx = (int)Math.Round(playableInput.GetTime() * input.endLocation.fps);
            input.endLocation.LoadFrame(frameIdx);
        }
    }
}
