using System;
using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// One approved main sound per character, heard only when the player asks for it with the Play screen's
    /// "Sound" control. Touching the toy never plays it. The clip is a local resource (no network), follows the
    /// global Sound switch and volume, and stops when the toy, the page or the app's focus changes.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        /// <summary>Resources folder holding one clip per character id, for example "Audio/Main/pip".</summary>
        public const string MainSoundRoot = "Audio/Main/";
        private const float MainSoundLevel = .65f; // same headroom as the collection voices
        private readonly Dictionary<string, AudioClip> mainSounds = new Dictionary<string, AudioClip>(StringComparer.Ordinal);
        private Button stageSound;
        private AudioSource mainSoundSource;
        private string mainSoundCharacter = "";

        /// <summary>Raised when a main sound starts: the collectible on stage and the clip length in seconds.</summary>
        public event Action<string, float> MainSoundPlayed;
        /// <summary>The clip most recently started, or null once it has been stopped.</summary>
        public AudioClip CurrentMainSound => mainSoundSource == null ? null : mainSoundSource.clip;

        public static string MainSoundResource(string characterId) => string.IsNullOrEmpty(characterId) ? "" : MainSoundRoot + characterId;

        private AudioClip MainSoundFor(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return null;
            if (!mainSounds.TryGetValue(characterId, out var clip)) { clip = Resources.Load<AudioClip>(MainSoundResource(characterId)); mainSounds.Add(characterId, clip); }
            return clip;
        }

        /// <summary>A fifth labelled control on the play rail. It plays a sound; it does not open a card.</summary>
        private void BuildStageSound()
        {
            stageSound = CreateButton("Sound", stageRail, Vector2.zero, new Vector2(RailButton, RailButton), new Color(1, 1, 1, .8f),
                PlayMainSound, out _, PockleTheme.SmallSize);
            stageSound.name = "Stage sound";
            stageSound.gameObject.SetActive(false);
        }

        /// <summary>Shows the control only for a toy whose character has a clip, and dims it while Sound is off.</summary>
        private void RefreshStageSound(bool onStage, CollectibleDefinition toy)
        {
            if (stageSound == null) return;
            string character = onStage && toy != null ? toy.CharacterId : "";
            bool shown = character.Length > 0 && MainSoundFor(character) != null;
            if (!shown || character != mainSoundCharacter) StopMainSound();
            mainSoundCharacter = shown ? character : "";
            stageSound.gameObject.SetActive(shown);
            stageSound.interactable = soundEnabled;
        }

        /// <summary>Plays the staged toy's main sound from the start. Pressing again restarts it; sounds never stack.</summary>
        public void PlayMainSound()
        {
            if (!soundEnabled || page != AppPage.Play || !StageSelection(out var toy, out _)) return;
            var clip = MainSoundFor(toy.CharacterId);
            float volume = PlayerPrefs.GetFloat("pockle.prototype.soundVolume", 1);
            volume = float.IsNaN(volume) || float.IsInfinity(volume) ? 0 : Mathf.Clamp01(volume);
            if (clip == null || volume <= 0) return;
            if (mainSoundSource == null)
            {
                mainSoundSource = gameObject.AddComponent<AudioSource>();
                mainSoundSource.playOnAwake = false; mainSoundSource.loop = false; mainSoundSource.spatialBlend = 0;
            }
            mainSoundSource.Stop();
            mainSoundSource.clip = clip; mainSoundSource.volume = MainSoundLevel * volume;
            mainSoundSource.Play();
            MainSoundPlayed?.Invoke(toy.Id, clip.length);
        }

        public void StopMainSound()
        {
            if (mainSoundSource == null) return;
            mainSoundSource.Stop(); mainSoundSource.clip = null;
        }
    }
}
