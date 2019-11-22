using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;
using UnityEngine;

namespace CloneDroneVR
{
    public static class VrControllerStateExtensionMethods
    {
        public static Vector2 GetJoystick(this VRControllerState_t me)
        {
            return new Vector2(me.rAxis0.x, me.rAxis0.y);
        }
        public static float GetFrontTriggerValue(this VRControllerState_t me)
        {
            return me.rAxis1.x;
        }
        public static float GetSideTriggerValue(this VRControllerState_t me)
        {
            return me.rAxis2.x;
        }
    }
}
