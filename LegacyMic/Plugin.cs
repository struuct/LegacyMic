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

    internal static int Bitrate;
    internal static int SamplingRate;

    internal static Quality QualityMode;

    private void Awake()
    {
        Instance = this;

        QualityMode = Config.Bind("General",
            "Quality", Quality.Legacy,
            "The mode to set the mic quality to.").Value;

        Bitrate = Config.Bind("Microphone",
            "Bitrate", 20000,
            "Bitrate of the microphone. LegacyMic bitrate -> 20000, HQ bitrate -> 30000").Value;
        
        SamplingRate = Config.Bind("Microphone",
            "SamplingRate", 16000,
            "Sampling rate of the microphone. LegacyMic bitrate -> 16000, HQ bitrate -> 24000").Value;
        
        if (QualityMode == Quality.Legacy) {
            Bitrate = 20000;
            SamplingRate = 16000;
        }

        if (QualityMode == Quality.HQ) {
            Bitrate = 30000;
            SamplingRate = 24000;
        }

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

public enum Quality {
    Legacy = 0,
    HQ = 1,
    Custom = 2
}