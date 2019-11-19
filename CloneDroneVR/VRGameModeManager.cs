using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneDroneVR
{
    public abstract class VRGameModeManager
    {
        public GameMode GameMode;

        public virtual void OnGameModeStarted()
        {
        }
        public virtual void OnGameModeUpdate()
        {
        }
        public virtual void OnGameModeLateUpdate()
        {
        }
        public virtual void OnGameModeFixedUpdate()
        {
        }
        public virtual void OnGameModeQuit()
        {
        }
        public virtual void OnPostVRRender()
        {
        }
        public virtual void OnPreVRRender()
        {
        }

    }
}
