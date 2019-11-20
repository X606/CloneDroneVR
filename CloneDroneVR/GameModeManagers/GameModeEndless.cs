using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using ModLibrary;
using ModLibrary.YieldInstructions;

namespace CloneDroneVR.GameModeManagers
{
    public class GameModeEndless : VRGameModeManager
    {
        public GameModeEndless()
        {
            GameMode = GameMode.Endless;
        }
        bool _hasPlayerSpawned = false;

        public override void OnGameModeStarted()
        {
            WaitForThenCall.Schedule(delegate
            {
                _hasPlayerSpawned = true;
                FirstPersonMover player = CharacterTracker.Instance.GetPlayer();
                


            }, delegate { return CharacterTracker.Instance.GetPlayer() != null; });
        }

        public override void OnPreVRRender()
        {
            if(!_hasPlayerSpawned)
                return;

            FirstPersonMover player = CharacterTracker.Instance.GetPlayer();
            VRManager.Instance.Player.transform.position = player.transform.position;
        }

    }

}
