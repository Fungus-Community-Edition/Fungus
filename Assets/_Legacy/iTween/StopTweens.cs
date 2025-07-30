


﻿using UnityEngine;

namespace Amanita
{
    /// <summary>
    /// Stop all active iTweens in the current scene.
    /// </summary>
    [CommandInfo("iTween", 
                 "Stop Tweens", 
                 "Stop all active iTweens in the current scene.")]
    [AddComponentMenu("")]
    public class StopTweens : Command
    {
        #region Public members

        public override void OnEnter()
        {
            iTween.Stop();
            Continue();
        }

        #endregion
    }
}