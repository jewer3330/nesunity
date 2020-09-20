using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using dotNES;
using dotNES.Controllers;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


public class NesManager : MonoBehaviour
{

    private UnityController unityController;
    private Emulator emulator;
    private Thread _renderThread;
    public const int GameWidth = 256;
    public const int GameHeight = 240;
    private Texture2D texture2D;
    public RawImage rawImage;
    private Color32[] colors;
    private bool gameStarted;
    private bool _rendererRunning = true;
    private bool suspended;
    private int activeSpeed = 1;
    private uint[] rawBitmap;
    public string rom = "[054]  智力类 - 吞食天地1.NES";
    private void Awake()
    {
        unityController = new UnityController();
        texture2D = new Texture2D(GameWidth, GameHeight);
        rawImage.texture = texture2D;
        colors = new Color32[GameWidth * GameHeight];
        
        BootCartridge(Application.dataPath + "/Resources/rom/" + rom);
        
    }

    private void OnDestroy()
    {
        if (texture2D)
            Destroy(texture2D);
        _rendererRunning = false;
    }


    private void BootCartridge(string rom)
    {
        emulator = new Emulator(rom, unityController);
        _renderThread = new Thread(() =>
        {
            gameStarted = true;
            Debug.Log(emulator.Cartridge);
            Stopwatch s = new Stopwatch();
            Stopwatch s0 = new Stopwatch();
            while (_rendererRunning)
            {
                if (suspended)
                {
                    Thread.Sleep(100);
                    continue;
                }

                s.Restart();
                for (int i = 0; i < 60 && !suspended; i++)
                {
                    s0.Restart();
                    emulator.PPU.ProcessFrame();
                    rawBitmap = emulator.PPU.RawBitmap;
                    Draw();
                    s0.Stop();
                    Thread.Sleep(Math.Max((int)(980 / 60.0 - s0.ElapsedMilliseconds), 0) / activeSpeed);
                }
                s.Stop();
                Debug.Log($"60 frames in {s.ElapsedMilliseconds}ms");
            }
        });
        _renderThread.Start();
    }

    public void Draw()
    {
        for (int y = 0; y < GameHeight; y++)
        {
            for (int x = 0; x < GameWidth; x++)
            {
                var v = (int)(rawBitmap[y * GameWidth + x]);//argb
                var t = new Color32((byte)(v >> 16 & 0xff), (byte)(v >> 8 & 0xff), (byte)(v & 0xff), 255);
                colors[y * GameWidth + x] = t;
            }
        }
        
    }

    // Update is called once per frame
    void Update () {
        

        foreach (var k in unityController._keyMapping)
        {
            if (Input.GetKeyDown(k.Key))
                unityController.PressKey(k.Key);
            if (Input.GetKeyUp(k.Key))
                unityController.ReleaseKey(k.Key);
        }

        texture2D.SetPixels32(colors);
        texture2D.Apply();

        
    }

    private void OnApplicationPause(bool pause)
    {
        suspended = pause;
    }

}
