using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModLibrary;
using UnityEngine;
using System.IO;
using Valve.VR;

namespace CloneDroneVR
{
    public class VRStartupManager : Singleton<VRStartupManager>
    {
        public void InitOpenVR()
        {
            ExtenFunctions.SetDllDirectory(OpenVRDllPath);

            if (!OpenVR.IsHmdPresent())
                throw new InvalidOperationException("Could not find vr headset");

            if(!OpenVR.IsRuntimeInstalled())
                throw new InvalidOperationException("Could not find openVr runtime");

            EVRInitError hmdInitErrorCode = EVRInitError.None;
            OpenVR.Init(ref hmdInitErrorCode, EVRApplicationType.VRApplication_Scene);
            if(hmdInitErrorCode != EVRInitError.None)
                throw new Exception("OpenVR error: " + OpenVR.GetStringForHmdError(hmdInitErrorCode));

            OpenVR.System.ResetSeatedZeroPose();

            uint renderTextureWidth = 0;
            uint renderTextureHeight = 0;
            OpenVR.System.GetRecommendedRenderTargetSize(ref renderTextureWidth, ref renderTextureHeight);

            VRManager.Instance.InitEyeTextures((int)renderTextureWidth, (int)renderTextureHeight);
        }
        
        public string OpenVRDllPath
        {
            get
            {
                string basePath = AssetLoader.GetModsFolderDirectory();
                string path = Path.Combine(basePath, "openvr");
                return Path.Combine(path, Is64BitProcess ? "win64" : "win32");
            }
        }

        public bool Is64BitProcess
        {
            get { return (IntPtr.Size == 8); }
        }
    }
}
