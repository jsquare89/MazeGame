using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace MazeGame
{
    public static class MusicPlayer
    {
        public static float masterDefaultVolume = 1.0f;
        private static float masterVolume = masterDefaultVolume;
        private static bool masterMuted = false;
        public static SoundEffectInstance currentSong;

        //Plays the provided song in a loop.
        public static void PlaySong(SoundEffect song)
        {
            Stop();
            currentSong = song.CreateInstance();
            currentSong.Volume = masterVolume;
            currentSong.IsLooped = true;
            currentSong.Play();
        }


        //Sets volume of player
        public static void SetVolume(float volume)
        {
            MusicPlayer.masterVolume = volume;

            if (currentSong != null && !currentSong.IsDisposed)
                currentSong.Volume = volume;
        }

        //Stops song currently playing
        public static void Stop()
        {
            if (currentSong != null && !currentSong.IsDisposed)
            {
                currentSong.Stop();
            }
        }

        public static void Mute()
        {
            if (currentSong != null && !currentSong.IsDisposed)
            {
                masterMuted = !masterMuted;
                if (masterMuted)
                    currentSong.Volume = 0;
                else
                    currentSong.Volume = masterVolume;
            }
        }
    }
}
