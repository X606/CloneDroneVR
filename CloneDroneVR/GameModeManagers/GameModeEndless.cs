using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using ModLibrary;
using ModLibrary.YieldInstructions;
using RootMotion.FinalIK;

namespace CloneDroneVR.GameModeManagers
{
    public class GameModeEndless : VRGameModeManager
    {
        public GameModeEndless()
        {
            GameMode = GameMode.Endless;
        }
        PhysicalVRPlayer _player;

        public override void OnGameModeStarted()
        {
            VRManager.Instance.Player.LeftController.ColliderActive = false;
            VRManager.Instance.Player.RightController.ColliderActive = false;
            VRManager.Instance.Player.LeftController.RendererActive = true;
            VRManager.Instance.Player.RightController.RendererActive = true;

            FindPlayer();
        }

        public override void OnPreVRRender()
        {
            if(_player == null)
                return;

            _player.OnPreVRRender();
        }

        public override void OnVRPlayerDeath(PhysicalVRPlayer player)
        {
            if(player != _player)
                throw new Exception("The passed player is not the same as the saved player");

            _player = null;

            FindPlayer();
        }

        void FindPlayer()
        {
            if(_player != null)
                return;

            WaitForThenCall.Schedule(delegate
            {
                FirstPersonMover player = CharacterTracker.Instance.GetPlayer();

                if(player.gameObject.GetComponent<PhysicalVRPlayer>() != null)
                    throw new Exception("There was already a PhysicalVrPlayer on the player");

                _player = player.gameObject.AddComponent<PhysicalVRPlayer>();



            }, delegate { return CharacterTracker.Instance.GetPlayer() != null; });
        }

    }

}
