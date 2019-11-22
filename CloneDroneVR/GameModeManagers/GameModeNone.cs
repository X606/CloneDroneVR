using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModLibrary;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using UnityEngine.EventSystems;

namespace CloneDroneVR.GameModeManagers
{
    public class GameModeNone : VRGameModeManager
    {
        public GameModeNone()
        {
            GameMode = GameMode.None;
        }
        
        public override void OnGameModeStarted()
        {
            VRManager.Instance.Player.Scale = 1f;
            VRManager.Instance.Player.LeftController.ColliderActive = true;
            VRManager.Instance.Player.RightController.ColliderActive = true;
            VRManager.Instance.Player.LeftController.RendererActive = true;
            VRManager.Instance.Player.RightController.RendererActive = true;

            VRManager.Instance.Player.transform.position = new Vector3(0f, 40f, 0f);
            
            PointerRay.SetEnabled(true);
        }

        public override void OnGameModeQuit()
        {
            PointerRay.SetEnabled(false);
        }

        

    }
}
