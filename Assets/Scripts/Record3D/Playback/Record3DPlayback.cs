using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Record3D;
using System;
using System.IO;
using Unity.Collections;
using UnityEngine.VFX;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;


//[ExecuteInEditMode]
public partial class Record3DPlayback : MonoBehaviour
{
    public string r3dPath;

    public VisualEffect streamEffect;
    private Texture2D positionTex;
    private Texture2D colorTex;

    // Playback
    private int currentFrame_ = 0;
    private bool isPlaying_ = false;
    private Record3DVideo currentVideo_ = null;
    private Timer videoFrameUpdateTimer_ = null;
    private bool shouldRefresh_ = false;
    private string lastLoadedVideoPath_ = null;


    void ReinitializeTextures(int width, int height)
    {
        DestroyImmediate(positionTex);
        DestroyImmediate(colorTex);
        positionTex = null;
        colorTex = null;
        Resources.UnloadUnusedAssets();

        positionTex = new Texture2D(width, height, TextureFormat.RGBAFloat, false)
        {
            filterMode = FilterMode.Point
        };

        colorTex = new Texture2D(width, height, TextureFormat.RGB24, false)
        {
            filterMode = FilterMode.Point
        };

        int numParticles = width * height;
        if (streamEffect == null)
        {
            streamEffect = gameObject.GetComponent<VisualEffect>();
        }
        streamEffect.SetInt("Number of Particles", numParticles);
        streamEffect.SetTexture("Particle Position Texture", positionTex);
        streamEffect.SetTexture("Particle Color Texture", colorTex);
    }

    void Update()
    {
        if (isPlaying_ && (currentVideo_ != null) && shouldRefresh_)
        {
            shouldRefresh_ = false;
            LoadFrame(currentFrame_);
            currentFrame_ = (currentFrame_ + 1) % currentVideo_.numFrames;
        }
    }

    public void OnTimerTick(object sender, ElapsedEventArgs e)
    {
        shouldRefresh_ = true;
    }
}


public partial class Record3DPlayback
{
    public int numberOfFrames
    {
        get
        {
            ReloadVideoIfNeeded();
            return currentVideo_ == null ? 1 : currentVideo_.numFrames;
        }
    }

    public int fps
    {
        get
        {
            ReloadVideoIfNeeded();
            return currentVideo_ == null ? 1 : currentVideo_.fps;
        }
    }


    public void LoadVideo(string path, bool force = false)
    {
        if (!force && path == lastLoadedVideoPath_)
        {
            return;
        }

        var wasPlaying = isPlaying_;
        Pause();

        currentVideo_ = new Record3DVideo(path);
        ReinitializeTextures(currentVideo_.width, currentVideo_.height);

        // Reset the playback and load timer
        currentFrame_ = 0;
        videoFrameUpdateTimer_ = new Timer(1000.0 / currentVideo_.fps);
        videoFrameUpdateTimer_.AutoReset = true;
        videoFrameUpdateTimer_.Elapsed += this.OnTimerTick;

        if (wasPlaying)
        {
            Play();
        }

        lastLoadedVideoPath_ = path;
    }

    public void Pause()
    {
        isPlaying_ = false;
        if (videoFrameUpdateTimer_ != null) videoFrameUpdateTimer_.Enabled = false;
    }

    public void Play()
    {
        isPlaying_ = true;
        if (videoFrameUpdateTimer_ != null) videoFrameUpdateTimer_.Enabled = true;
    }

    private void ReloadVideoIfNeeded()
    {
        if (currentVideo_ == null)
        {
            LoadVideo(string.IsNullOrEmpty(lastLoadedVideoPath_) ? r3dPath : lastLoadedVideoPath_, force: true);
        }
    }

    public void LoadFrame(int frameNumber)
    {
        // Load the data from the archive
        ReloadVideoIfNeeded();

        if (streamEffect)

        currentVideo_.LoadFrameData(frameNumber);
        currentFrame_ = frameNumber;

        // Update the textures
        var positionTexBufferSize = positionTex.width * positionTex.height * 4;
        NativeArray<float>.Copy(currentVideo_.positionsBuffer, positionTex.GetRawTextureData<float>(), positionTexBufferSize);
        positionTex.Apply(false, false);

        const int numRGBChannels = 3;
        var colorTexBufferSize = colorTex.width * colorTex.height * numRGBChannels * sizeof(byte);

        NativeArray<byte>.Copy(currentVideo_.rgbBuffer, colorTex.GetRawTextureData<byte>(), colorTexBufferSize);
        colorTex.Apply(false, false);
    }
}