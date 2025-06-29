using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using MyGame.Data;
using MyGame.System;

namespace MyGame.Managers
{
    public class CGManager : Singleton<CGManager>
    {
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private VideoClip[] cgClips;

        private void OnEnable()
        {
            GameEvents.OnCGStart += PlayCG;
        }

        private void PlayCG(int cgId)
        {
            videoPlayer.clip = cgClips[cgId];
            videoPlayer.Play();
            videoPlayer.loopPointReached += (source) => OnCGComplete(source, cgId);
        }

        private void OnCGComplete(VideoPlayer source, int cgId)
        {
            GameManager.Instance.HandleCGComplete(cgId);
        }
    }
}