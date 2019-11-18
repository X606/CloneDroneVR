using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ModLibrary;
using UnityEngine;

namespace CloneDroneVR
{
    public static class ExtenFunctions
    {
        // This function allows us to import DLLs from a given path.
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetDllDirectory(string lpPathName);

    }

    public class Main : Mod
    {
        public override string GetModName() => "CloneDroneVr";
        public override string GetUniqueID() => "c4515f4b-a493-49c5-a0e8-78026c7972ba";

        public override void OnModEnabled()
        {
            if(VRStartupManager.Instance == null)
                new GameObject("VRStartupManager").AddComponent<VRStartupManager>();

            if(VRManager.Instance == null)
                new GameObject("VRManager").AddComponent<VRManager>();
            
            VRStartupManager.Instance.InitOpenVR();
            
        }
        
    }
}
