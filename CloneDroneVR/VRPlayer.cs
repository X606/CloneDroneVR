using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;
using UnityEngine.EventSystems;

namespace CloneDroneVR
{
    public class VRPlayer : MonoBehaviour
    {
        public VRCamera Head;
        public VRController LeftController;
        public VRController RightController;

        void Awake()
        {
            Head = new GameObject("Head").AddComponent<VRCamera>();
            Head.transform.parent = transform;

            LeftController = new GameObject("LeftController").AddComponent<VRController>();
            LeftController.transform.parent = transform;
            LeftController.NodeType = VRNodeType.LeftHand;

            RightController = new GameObject("RightController").AddComponent<VRController>();
            RightController.transform.parent = transform;
            RightController.NodeType = VRNodeType.RightHand;
        }

        public void OnLateUpdate()
        {
            dispatchOpenVREvents();
            
            OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0f, VRManager.DevicePoses);
            SteamVR_Events.NewPoses.Send(VRManager.DevicePoses);
            
            EVRCompositorError compositorError = OpenVR.Compositor.WaitGetPoses(VRManager.RenderPoses, VRManager.GamePoses);
            if(compositorError != EVRCompositorError.None)
                throw new Exception("OpenVr error: " +compositorError.ToString());
            
            for(int i = 0; i < VRManager.DevicePoses.Length; i++)
            {
                TrackedDevicePose_t devicePose = VRManager.DevicePoses[i];
                if(!devicePose.bDeviceIsConnected || !devicePose.bPoseIsValid)
                    continue;

                SteamVR_Utils.RigidTransform rigidTransform = new SteamVR_Utils.RigidTransform(devicePose.mDeviceToAbsoluteTracking);

                ETrackedDeviceClass deviceType = OpenVR.System.GetTrackedDeviceClass((uint)i);

                if (deviceType == ETrackedDeviceClass.HMD)
                {
                    Head.transform.localPosition = rigidTransform.pos;
                    Head.transform.localRotation = rigidTransform.rot;
                    Head.Velocity = fromHmdVector3_t(devicePose.vVelocity);
                    Head.AngularVelocity = fromHmdVector3_t(devicePose.vAngularVelocity);
                    Head.DeviceIndex = (uint)i;

                } else if (deviceType == ETrackedDeviceClass.Controller)
                {
                    ETrackedControllerRole role = OpenVR.System.GetControllerRoleForTrackedDeviceIndex((uint)i);
                    if (role == ETrackedControllerRole.LeftHand)
                    {
                        LeftController.transform.localPosition = rigidTransform.pos;
                        LeftController.transform.localRotation = rigidTransform.rot;
                        LeftController.Velocity = fromHmdVector3_t(devicePose.vVelocity);
                        LeftController.AngularVelocity = fromHmdVector3_t(devicePose.vAngularVelocity);
                        LeftController.DeviceIndex = (uint)i;
                    } else if (role == ETrackedControllerRole.RightHand)
                    {
                        RightController.transform.localPosition = rigidTransform.pos;
                        RightController.transform.localRotation = rigidTransform.rot;
                        RightController.Velocity = fromHmdVector3_t(devicePose.vVelocity);
                        RightController.AngularVelocity = fromHmdVector3_t(devicePose.vAngularVelocity);
                        RightController.DeviceIndex = (uint)i;
                    }
                }
                
            }
            
            //

            Head.Render();
            

            // [insert dark magic here]
            OpenVR.Compositor.PostPresentHandoff();
            

        }

        
        void dispatchOpenVREvents()
        {
            // copied from SteamVR_Render
            var vrEvent = new VREvent_t();
            uint size = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t));
            for(int i = 0; i < 64; i++)
            {
                if(!OpenVR.System.PollNextEvent(ref vrEvent, size))
                    break;

                switch((EVREventType)vrEvent.eventType)
                {
                    case EVREventType.VREvent_InputFocusCaptured: // another app has taken focus (likely dashboard)
                    if(vrEvent.data.process.oldPid == 0)
                    {
                        SteamVR_Events.InputFocus.Send(false);
                    }
                    break;
                    case EVREventType.VREvent_InputFocusReleased: // that app has released input focus
                    if(vrEvent.data.process.pid == 0)
                    {
                        SteamVR_Events.InputFocus.Send(true);
                    }
                    break;
                    case EVREventType.VREvent_ShowRenderModels:
                    SteamVR_Events.HideRenderModels.Send(false);
                    break;
                    case EVREventType.VREvent_HideRenderModels:
                    SteamVR_Events.HideRenderModels.Send(true);
                    break;
                    default:
                    SteamVR_Events.System((EVREventType)vrEvent.eventType).Send(vrEvent);
                    break;
                }
            }
        }

        Vector3 fromHmdVector3_t(HmdVector3_t hmdVector3_T)
        {
            return new Vector3(hmdVector3_T.v0, hmdVector3_T.v1, hmdVector3_T.v2);
        }
    }

    public enum VRNodeType
    {
        Head,
        LeftHand,
        RightHand
    }

    public class VRNode : MonoBehaviour
    {
        public uint DeviceIndex;
        public VRNodeType NodeType;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        
        
    }

    public class VRCamera : VRNode
    {
        public Camera LeftCamera;
        public Camera RightCamera;

        VRTextureBounds_t _bounds;

        void Awake()
        {
            NodeType = VRNodeType.Head;

            LeftCamera = new GameObject("LeftCamera").AddComponent<Camera>();
            LeftCamera.transform.parent = transform;
            LeftCamera.enabled = false;
            SteamVR_Utils.RigidTransform leftEye = new SteamVR_Utils.RigidTransform(OpenVR.System.GetEyeToHeadTransform(EVREye.Eye_Left));
            LeftCamera.transform.localPosition = leftEye.pos;
            LeftCamera.transform.localRotation = leftEye.rot;
            LeftCamera.targetTexture = VRManager.Instance.LeftEyeRenderTexture;
            LeftCamera.nearClipPlane = 0.01f;
            HmdMatrix44_t leftEyeProjectionMatrix = OpenVR.System.GetProjectionMatrix(EVREye.Eye_Left, LeftCamera.nearClipPlane, LeftCamera.farClipPlane);
            LeftCamera.projectionMatrix = matrix4x4_OpenVr2UnityFormat(ref leftEyeProjectionMatrix);
            
            RightCamera = new GameObject("RightCamera").AddComponent<Camera>();
            RightCamera.transform.parent = transform;
            RightCamera.enabled = false;
            SteamVR_Utils.RigidTransform rightEye = new SteamVR_Utils.RigidTransform(OpenVR.System.GetEyeToHeadTransform(EVREye.Eye_Right));
            RightCamera.transform.localPosition = rightEye.pos;
            RightCamera.transform.localRotation = rightEye.rot;
            RightCamera.targetTexture = VRManager.Instance.RightEyeRenderTexture;
            RightCamera.nearClipPlane = 0.01f;
            HmdMatrix44_t rightEyeProjectionMatrix = OpenVR.System.GetProjectionMatrix(EVREye.Eye_Right, RightCamera.nearClipPlane, RightCamera.farClipPlane);
            RightCamera.projectionMatrix = matrix4x4_OpenVr2UnityFormat(ref rightEyeProjectionMatrix);

            _bounds = new VRTextureBounds_t();
            _bounds.uMin = 0.0f;
            _bounds.uMax = 1.0f;
            _bounds.vMin = 1.0f; // flip the vertical coordinate for some reason (no idea why but they did it in kerbal vr so im doing it too)
            _bounds.vMax = 0.0f;
        }



        public void Render()
        {
            Console.WriteLine("starting render");
            if(VRManager.Instance.CurrentModeManager != null)
                VRManager.Instance.CurrentModeManager.OnPreVRRender();

            //RightCamera.targetTexture = VRManager.Instance.RightEyeRenderTexture;
            //LeftCamera.targetTexture = VRManager.Instance.LeftEyeRenderTexture;

            Console.WriteLine("starting render.2");

            RightCamera.Render();
            Console.WriteLine("starting render.3");
            LeftCamera.Render();

            Console.WriteLine("render0.5");

            VRManager.Instance.RightEyeTexture.handle = VRManager.Instance.RightEyeRenderTexture.GetNativeTexturePtr();
            VRManager.Instance.LeftEyeTexture.handle = VRManager.Instance.LeftEyeRenderTexture.GetNativeTexturePtr();

            Console.WriteLine("render1");

            EVRCompositorError error = OpenVR.Compositor.Submit(EVREye.Eye_Left, ref VRManager.Instance.LeftEyeTexture, ref _bounds, EVRSubmitFlags.Submit_Default);
            if(error != EVRCompositorError.None)
                throw new Exception("OpenVR Sumbit error on left eye: " + error.ToString());

            error = OpenVR.Compositor.Submit(EVREye.Eye_Right, ref VRManager.Instance.RightEyeTexture, ref _bounds, EVRSubmitFlags.Submit_Default);
            if(error != EVRCompositorError.None)
                throw new Exception("OpenVR Sumbit error on right eye: " + error.ToString());

            Console.WriteLine("render2");

            if (VRManager.Instance.CurrentModeManager != null)
                VRManager.Instance.CurrentModeManager.OnPostVRRender();

            Console.WriteLine("end of render");
        }

        Matrix4x4 matrix4x4_OpenVr2UnityFormat(ref HmdMatrix44_t mat44_openvr)
        {
            Matrix4x4 mat44_unity = Matrix4x4.identity;
            mat44_unity.m00 = mat44_openvr.m0;
            mat44_unity.m01 = mat44_openvr.m1;
            mat44_unity.m02 = mat44_openvr.m2;
            mat44_unity.m03 = mat44_openvr.m3;
            mat44_unity.m10 = mat44_openvr.m4;
            mat44_unity.m11 = mat44_openvr.m5;
            mat44_unity.m12 = mat44_openvr.m6;
            mat44_unity.m13 = mat44_openvr.m7;
            mat44_unity.m20 = mat44_openvr.m8;
            mat44_unity.m21 = mat44_openvr.m9;
            mat44_unity.m22 = mat44_openvr.m10;
            mat44_unity.m23 = mat44_openvr.m11;
            mat44_unity.m30 = mat44_openvr.m12;
            mat44_unity.m31 = mat44_openvr.m13;
            mat44_unity.m32 = mat44_openvr.m14;
            mat44_unity.m33 = mat44_openvr.m15;
            return mat44_unity;
        }

    }

    public class VRController : VRNode
    {
        void Start()
        {
            GameObject preview = GameObject.CreatePrimitive(PrimitiveType.Cube);
            preview.transform.parent = transform;
            preview.transform.localPosition = Vector3.zero;
            preview.transform.localRotation = Quaternion.identity;
            preview.transform.localScale = new Vector3(0.05f, 0.05f, 0.1f);

            Renderer renderer = preview.GetComponent<Renderer>();
            if (NodeType == VRNodeType.RightHand)
            {
                renderer.material.color = Color.blue;
            } else
            {
                renderer.material.color = Color.red;
            }

        }
        /// <summary>
        /// axis0: joystick (both x and y)
        /// axis1: front trigger(x only)
        /// axis2: side trigger(x only)
        /// axis3: unused
        /// axis4: unused
        /// </summary>
        public unsafe VRControllerState_t ControllerState
        {
            get
            {
                VRControllerState_t controllerState = new VRControllerState_t();
                uint controllerSize = (uint)sizeof(VRControllerState_t);
                OpenVR.System.GetControllerState(DeviceIndex, ref controllerState, controllerSize);

                return controllerState;
            }
        }
    }
}
