using MoreMountains.Tools;
using UnityEngine;

namespace Domains.Audio
{
    /// <summary>
    ///     Add this class to a GameObject to have it play a background music when instanciated.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Sound/Background Music")]
    public class BackgroundMusic : MonoBehaviour
    {
        /// the background music
        [Tooltip("the audio clip to use as background music")]
        public AudioClip SoundClip;

        /// whether or not the music should loop
        [Tooltip("whether or not the music should loop")]
        public bool Loop = true;

        /// the ID to create this background music with
        [Tooltip("the ID to create this background music with")]
        public int ID = 255;

        // Volume in range of 0-1
        [Tooltip("Volume of the background music")] [Range(0f, 1f)]
        public float volume = 1f;


        /// <summary>
        ///     Gets the AudioSource associated to that GameObject, and asks the GameManager to play it.
        /// </summary>
        protected virtual void Start()
        {
            var options = MMSoundManagerPlayOptions.Default;
            options.ID = ID;
            options.Loop = Loop;
            options.Location = Vector3.zero;
            options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;
            options.Volume = volume;

            MMSoundManagerSoundPlayEvent.Trigger(SoundClip, options);
        }
    }
}