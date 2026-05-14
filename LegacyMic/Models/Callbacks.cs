using Photon.Pun;

namespace LegacyMic.Models;

internal sealed class Callbacks : MonoBehaviourPunCallbacks
{
    public override void OnJoinedRoom() {
        Plugin.Instance?.SetQuality(Plugin.Bitrate, Plugin.SamplingRate);
    }
}