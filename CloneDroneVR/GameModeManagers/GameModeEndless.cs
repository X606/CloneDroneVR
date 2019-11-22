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

        public override void OnVRPlayerDeath(PhysicalVRPlayer player)
        {
            if(player != _player)
                throw new Exception("The passed player is not the same as the saved player");

            _player = null;
            
            if (CloneManager.Instance.GetNumClones() == 0) // this is a game over, go to UI
            {
                TransportUtils.TransportTo(VRManager.Instance.Player.transform, VRManager.Instance.Player.transform.position, VRManager.Instance.Player.transform.rotation, new Vector3(0f, 40f, 0f), Quaternion.identity, 5f, delegate
                {
                    PointerRay.SetEnabled(true);
                    PointerRay.OnAnyButtonClicked = delegate
                    {
                        FindPlayer();

                        PointerRay.SetEnabled(false);
                        PointerRay.OnAnyButtonClicked = null;
                    };
                });
                return;
            }

            FindPlayer();
        }

        void FindPlayer()
        {
            if(_player != null)
                return;

            float lastTime = Time.time + 5f;

            WaitForThenCall.Schedule(delegate
            {
                if(Time.time >= lastTime)
                    return;

                FirstPersonMover player = CharacterTracker.Instance.GetPlayer();

                if(player.gameObject.GetComponent<PhysicalVRPlayer>() != null)
                    throw new Exception("There was already a PhysicalVrPlayer on the player");

                _player = player.gameObject.AddComponent<PhysicalVRPlayer>();



            }, delegate {

                if(Time.time >= lastTime)
                    return true;

                FirstPersonMover player = CharacterTracker.Instance.GetPlayer();
                return player != null && player.IsAttachedAndAlive();
            });
        }

    }

}
