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


public class Record3DVideo
{
    private int numFrames_;
    public int numFrames { get { return numFrames_; } }

    private int fps_;
    public int fps { get { return fps_; } }

    private int width_;
    public int width { get { return width_; } }

    private int height_;
    public int height { get { return height_; } }

    /// <summary>
    /// The intrinsic matrix coefficients.
    /// </summary>
    private float fx_, fy_, tx_, ty_;

    private ZipArchive underlyingZip_;

    public byte[] rgbBuffer;

    public float[] positionsBuffer;

    [System.Serializable]
    public struct Record3DMetadata
    {
        public int w;
        public int h;
        public List<float> K;
        public int fps;
    }

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    private const string LIBRARY_NAME = "librecord3d_unity_playback.dylib";

#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private const string LIBRARY_NAME = "record3d_unity_playback.dll";

#else
#error "Unsupported platform!"
#endif

    [DllImport(LIBRARY_NAME)]
    private static extern void DecompressFrame(byte[] jpgBytes, UInt32 jpgBytesSize, byte[] lzfseDepthBytes, UInt32 lzfseBytesSize, byte[] rgbBuffer, float[] poseBuffer, Int32 width, Int32 height, float fx, float fy, float tx, float ty);


    public Record3DVideo(string filepath)
    {
        underlyingZip_ = ZipFile.Open(filepath, ZipArchiveMode.Read);

        // Load metadata (FPS, the intrinsic matrix, dimensions)
        using (var metadataStream = new StreamReader(underlyingZip_.GetEntry("metadata").Open()))
        {
            string jsonContents = metadataStream.ReadToEnd();
            Record3DMetadata parsedMetadata = (Record3DVideo.Record3DMetadata)JsonUtility.FromJson(jsonContents, typeof(Record3DMetadata));

            // Initialize properties
            this.fps_ = parsedMetadata.fps;
            this.width_ = parsedMetadata.w;
            this.height_ = parsedMetadata.h;

            // Init the intrinsic matrix coeffs
            this.fx_ = parsedMetadata.K[0];
            this.fy_ = parsedMetadata.K[4];
            this.tx_ = parsedMetadata.K[6];
            this.ty_ = parsedMetadata.K[7];
        }

        this.numFrames_ = underlyingZip_.Entries.Count(x => x.FullName.Contains(".depth"));
        //Debug.Log(String.Format("# Available Frames: {0}", this.numFrames_));

        rgbBuffer = new byte[width * height * 3];
        positionsBuffer = new float[width * height * 4];
    }

    public void LoadFrameData(int frameIdx)
    {
        if (frameIdx >= numFrames_)
        {
            return;
        }

        // Decompress the LZFSE depth data into a byte buffer
        byte[] lzfseDepthBuffer;
        using (var lzfseDepthStream = underlyingZip_.GetEntry(String.Format("rgbd/{0}.depth", frameIdx)).Open())
        {
            using (var memoryStream = new MemoryStream())
            {
                lzfseDepthStream.CopyTo(memoryStream);
                lzfseDepthBuffer = memoryStream.ToArray();
            }
        }

        // Decompress the JPG image into a byte buffer
        byte[] jpgBuffer;
        using (var jpgStream = underlyingZip_.GetEntry(String.Format("rgbd/{0}.jpg", frameIdx)).Open())
        {
            using (var memoryStream = new MemoryStream())
            {
                jpgStream.CopyTo(memoryStream);
                jpgBuffer = memoryStream.ToArray();
            }
        }

        // Decompress the LZFSE depth map archive, create point cloud and load the JPEG image
        DecompressFrame(jpgBuffer,
            (uint)jpgBuffer.Length,
            lzfseDepthBuffer,
            (uint)lzfseDepthBuffer.Length,
            this.rgbBuffer,
            this.positionsBuffer,
            this.width_, this.height_,
            this.fx_, this.fy_, this.tx_, this.ty_);
    }
}
