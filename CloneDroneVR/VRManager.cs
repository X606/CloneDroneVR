using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ModLibrary;
using Valve.VR;

namespace CloneDroneVR
{
    public class VRManager : Singleton<VRManager>
    {
        List<VRGameModeManager> _gameModeManagers = new List<VRGameModeManager>();
        
        public VRPlayer Player;

        public void AddGameModeManager(VRGameModeManager gameModeManager)
        {
            for(int i = 0; i < _gameModeManagers.Count; i++)
            {
                if(_gameModeManagers[i].GameMode == gameModeManager.GameMode)
                    throw new InvalidOperationException("There is already a manager for the GameMode \"" + gameModeManager.GameMode.ToString() + "\"");
            }

            _gameModeManagers.Add(gameModeManager);
        }

        GameMode _oldGameMode = (GameMode)int.MaxValue;
        public VRGameModeManager CurrentModeManager { get; private set; }
        void Update()
        {
            PointerRay.Update();

            GameMode currentGameMode = GameFlowManager.Instance.GetCurrentGameMode();
            if (currentGameMode != _oldGameMode)
            {
                if(CurrentModeManager != null)
                    CurrentModeManager.OnGameModeQuit();

                CurrentModeManager = null;
                foreach(VRGameModeManager manager in _gameModeManagers)
                {
                    if(manager.GameMode == currentGameMode)
                    {
                        manager.OnGameModeStarted();
                        CurrentModeManager = manager;
                        break;
                    }
                }
                if (CurrentModeManager == null)
                {
                    throw new NotImplementedException("Manager for GameMode: " + currentGameMode + " could not be found");
                }
            }

            CurrentModeManager.OnGameModeUpdate();


            _oldGameMode = currentGameMode;
        }
        void LateUpdate()
        {
            if (Player != null)
                Player.OnLateUpdate();

            if(CurrentModeManager != null)
                CurrentModeManager.OnGameModeLateUpdate();
        }
        void FixedUpdate()
        {
            if(CurrentModeManager != null)
            {
                CurrentModeManager.OnGameModeFixedUpdate();
            }
        }

        public void InitPlayer()
        {
            Player = new GameObject("Player").AddComponent<VRPlayer>();
            Player.transform.parent = transform;
            Player.transform.localPosition = new Vector3(0, 10, 0);
            Player.transform.localRotation = Quaternion.identity;
        }

        public void InitEyeTextures(int resolutionWidth, int resolutionHeight)
        {
            ETextureType textureType = ETextureType.DirectX;
            switch(SystemInfo.graphicsDeviceType)
            {
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore:
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2:
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3:
                case UnityEngine.Rendering.GraphicsDeviceType.Direct3D9:
                    throw new InvalidOperationException(SystemInfo.graphicsDeviceType.ToString() + " does not support VR. You must use -force-d3d12");

                case UnityEngine.Rendering.GraphicsDeviceType.Direct3D11:
                case UnityEngine.Rendering.GraphicsDeviceType.Direct3D12:
                    textureType = ETextureType.DirectX;
                    break;

                default:
                    throw new InvalidOperationException(SystemInfo.graphicsDeviceType.ToString() + " not supported");
            }

            RightEyeRenderTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24, RenderTextureFormat.ARGB32);
            RightEyeRenderTexture.Create();
            RightEyeTexture.handle = RightEyeRenderTexture.GetNativeTexturePtr();
            RightEyeTexture.eColorSpace = EColorSpace.Auto;
            RightEyeTexture.eType = textureType;

            LeftEyeRenderTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24, RenderTextureFormat.ARGB32);
            LeftEyeRenderTexture.Create();
            LeftEyeTexture.handle = LeftEyeRenderTexture.GetNativeTexturePtr();
            LeftEyeTexture.eColorSpace = EColorSpace.Auto;
            LeftEyeTexture.eType = textureType;
        }

        public RenderTexture RightEyeRenderTexture;
        public Texture_t RightEyeTexture;

        public RenderTexture LeftEyeRenderTexture;
        public Texture_t LeftEyeTexture;

        public static TrackedDevicePose_t[] DevicePoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
        public static TrackedDevicePose_t[] RenderPoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
        public static TrackedDevicePose_t[] GamePoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];


    }
}
