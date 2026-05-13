using System.Collections;
using BepInEx;
using LegacyMic.Models;
using Photon.Voice.Unity;
using POpusCodec.Enums;
using UnityEngine;

namespace LegacyMic;

[BepInPlugin(Constants.Guid, Constants.ModName, Constants.Version)]
public sealed class Plugin : BaseUnityPlugin
{
    internal static Plugin? Instance { get; private set; }
    private static bool Initialized;

    private void Awake()
    {
        Instance = this;
        
        GorillaTagger.OnPlayerSpawned(() =>
        {
            if (Initialized) return;

            gameObject.AddComponent<Callbacks>();
            Initialized = true;
        });
    }
    
    // s/o kingofnetflix
    internal void SetQuality(int bitrate = 20000, int samplingRate = 16000)
    {
        var network = NetworkSystem.Instance;
        if (!network.InRoom || !network.LocalRecorder) return;

        var recorder = network.VoiceConnection.PrimaryRecorder;
        if (recorder.SamplingRate == (SamplingRate)samplingRate && recorder.Bitrate == bitrate) return;

        recorder.SamplingRate = (SamplingRate)samplingRate;
        recorder.Bitrate = bitrate;
        StartCoroutine(ReloadMicrophone(recorder));
    }

    private static IEnumerator ReloadMicrophone(Recorder recorder)
    {
        yield return new WaitForSeconds(0.25f);
        if (recorder.SourceType != Recorder.InputSourceType.Microphone)
            recorder.SourceType = Recorder.InputSourceType.Microphone;
        recorder.RestartRecording(true);
    }
}