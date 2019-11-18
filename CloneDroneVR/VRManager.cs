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
        public VRPlayer Player;

        public void InitPlayer()
        {
            Player = new GameObject("Player").AddComponent<VRPlayer>();
            Player.transform.parent = transform;
            Player.transform.localPosition = Vector3.zero;
            Player.transform.localRotation = Quaternion.identity;
        }

        public void InitEyeTextures(int resolutionWidth, int resolutionHeight)
        {
            RightEyeRenderTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24, RenderTextureFormat.ARGB32);
            RightEyeRenderTexture.Create();
            RightEyeTexture.handle = RightEyeRenderTexture.GetNativeTexturePtr();
            RightEyeTexture.eColorSpace = EColorSpace.Auto;
            RightEyeTexture.eType = ETextureType.DirectX;

            LeftEyeRenderTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24, RenderTextureFormat.ARGB32);
            LeftEyeRenderTexture.Create();
            LeftEyeTexture.handle = LeftEyeRenderTexture.GetNativeTexturePtr();
            LeftEyeTexture.eColorSpace = EColorSpace.Auto;
            LeftEyeTexture.eType = ETextureType.DirectX;
        }

        public RenderTexture RightEyeRenderTexture;
        public Texture_t RightEyeTexture;

        public RenderTexture LeftEyeRenderTexture;
        public Texture_t LeftEyeTexture;

        public static TrackedDevicePose_t[] DevicePoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];


    }
}
