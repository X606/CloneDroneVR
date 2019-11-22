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

        public float Scale
        {
            get
            {
                if(transform.localScale.x != transform.localScale.y || transform.localScale.y != transform.localScale.z)
                    throw new Exception("The player had a diffrent scale on at least one of its coordinates " + transform.localScale);

                return transform.localScale.x;
            }
            set
            {
                transform.localScale = Vector3.one * value;
            }
        }

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

            EVRCompositorError compositorError = OpenVR.Compositor.WaitGetPoses(VRManager.RenderPoses, VRManager.GamePoses);
            if(compositorError != EVRCompositorError.None)
                throw new Exception("OpenVr error: " +compositorError.ToString());

            OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, Utils.CalculatePredictedSecondsToPhotons(), VRManager.DevicePoses);
            SteamVR_Events.NewPoses.Send(VRManager.DevicePoses);
            
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

        void Awake()
        {
            NodeType = VRNodeType.Head;

            LeftCamera = new GameObject("LeftCamera").AddComponent<Camera>();
            LeftCamera.transform.parent = transform;
            VREye leftEye = LeftCamera.gameObject.AddComponent<VREye>();
            leftEye.EyeType = EVREye.Eye_Left;
            leftEye.Init(this);
            SteamVR_Utils.RigidTransform leftEyeTransform = new SteamVR_Utils.RigidTransform(OpenVR.System.GetEyeToHeadTransform(EVREye.Eye_Left));
            LeftCamera.transform.localPosition = leftEyeTransform.pos;
            LeftCamera.transform.localRotation = leftEyeTransform.rot;
            LeftCamera.targetTexture = VRManager.Instance.LeftEyeRenderTexture;
            LeftCamera.nearClipPlane = 0.01f;
            HmdMatrix44_t leftEyeProjectionMatrix = OpenVR.System.GetProjectionMatrix(EVREye.Eye_Left, LeftCamera.nearClipPlane, LeftCamera.farClipPlane);
            LeftCamera.projectionMatrix = Utils.Matrix4x4_OpenVr2UnityFormat(ref leftEyeProjectionMatrix);
            
            RightCamera = new GameObject("RightCamera").AddComponent<Camera>();
            RightCamera.transform.parent = transform;
            VREye rightEye = RightCamera.gameObject.AddComponent<VREye>();
            rightEye.EyeType = EVREye.Eye_Right;
            rightEye.Init(this);
            SteamVR_Utils.RigidTransform rightEyeTransform = new SteamVR_Utils.RigidTransform(OpenVR.System.GetEyeToHeadTransform(EVREye.Eye_Right));
            RightCamera.transform.localPosition = rightEyeTransform.pos;
            RightCamera.transform.localRotation = rightEyeTransform.rot;
            RightCamera.targetTexture = VRManager.Instance.RightEyeRenderTexture;
            RightCamera.nearClipPlane = 0.01f;
            HmdMatrix44_t rightEyeProjectionMatrix = OpenVR.System.GetProjectionMatrix(EVREye.Eye_Right, RightCamera.nearClipPlane, RightCamera.farClipPlane);
            RightCamera.projectionMatrix = Utils.Matrix4x4_OpenVr2UnityFormat(ref rightEyeProjectionMatrix);

            
        }

        int _eyesStartedRenderingThisFrame = 0;
        public void OnEyeStartedRendering()
        {
            _eyesStartedRenderingThisFrame++;
            if(_eyesStartedRenderingThisFrame <= 1)
            {
                if(VRManager.Instance.CurrentModeManager != null)
                    VRManager.Instance.CurrentModeManager.OnPreVRRender();

            }
            else
            {
                _eyesStartedRenderingThisFrame = 0;
            }
        }

        int _eyesRenderedThisFrame = 0;
        public void OnEyeFinishedRendering()
        {
            _eyesRenderedThisFrame++;
            if (_eyesRenderedThisFrame >= 2)
            {
                // [insert dark magic here]
                OpenVR.Compositor.PostPresentHandoff();

                if(VRManager.Instance.CurrentModeManager != null)
                    VRManager.Instance.CurrentModeManager.OnPostVRRender();

                _eyesRenderedThisFrame = 0;
            }
        }

    }
    public class VREye : MonoBehaviour
    {
        public EVREye EyeType;
        VRCamera _owner;
        Camera _camera;
        VRTextureBounds_t _bounds;

        public void Init(VRCamera owner)
        {
            _owner = owner;
            _camera = GetComponent<Camera>();
            _bounds = new VRTextureBounds_t();
            _bounds.uMin = 0.0f;
            _bounds.uMax = 1.0f;
            _bounds.vMin = 1.0f; // flip the vertical coordinate for some reason (no idea why but they did it in kerbal vr so im doing it too)
            _bounds.vMax = 0.0f;
        }

        void OnPreCull()
        {
            _owner.OnEyeStartedRendering();

            if(EyeType == EVREye.Eye_Left)
            {
                _camera.targetTexture = VRManager.Instance.LeftEyeRenderTexture;
            }
            else
            {
                _camera.targetTexture = VRManager.Instance.RightEyeRenderTexture;
            }
            
        }
        void OnPostRender()
        {
            if(EyeType == EVREye.Eye_Left)
            {
                VRManager.Instance.LeftEyeTexture.handle = VRManager.Instance.LeftEyeRenderTexture.GetNativeTexturePtr();

                EVRCompositorError error = OpenVR.Compositor.Submit(EVREye.Eye_Left, ref VRManager.Instance.LeftEyeTexture, ref _bounds, EVRSubmitFlags.Submit_Default);
                if(error != EVRCompositorError.None)
                    throw new Exception("OpenVR Sumbit error on left eye: " + error.ToString());
            }
            else
            {
                VRManager.Instance.RightEyeTexture.handle = VRManager.Instance.RightEyeRenderTexture.GetNativeTexturePtr();

                EVRCompositorError error = OpenVR.Compositor.Submit(EVREye.Eye_Right, ref VRManager.Instance.RightEyeTexture, ref _bounds, EVRSubmitFlags.Submit_Default);
                if(error != EVRCompositorError.None)
                    throw new Exception("OpenVR Sumbit error on right eye: " + error.ToString());
            }
            _owner.OnEyeFinishedRendering();
        }
    }

    public class VRController : VRNode
    {
        Renderer _renderer;

        void Start()
        {
            GameObject preview = GameObject.CreatePrimitive(PrimitiveType.Cube);
            preview.transform.parent = transform;
            preview.transform.localPosition = Vector3.zero;
            preview.transform.localRotation = Quaternion.identity;
            preview.transform.localScale = new Vector3(0.05f, 0.05f, 0.1f);

            _renderer = preview.GetComponent<Renderer>();
            if (NodeType == VRNodeType.RightHand)
            {
                _renderer.material.color = Color.blue;
            } else
            {
                _renderer.material.color = Color.red;
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

        public bool RendererActive
        {
            get
            {
                return _renderer.enabled;
            }
            set
            {
                _renderer.enabled = value;
            }
        }
        public bool ColliderActive
        {
            get
            {
                return _renderer.gameObject.GetComponent<Collider>().enabled;
            }
            set
            {
                _renderer.gameObject.GetComponent<Collider>().enabled = value;
            }
        }
    }
    
}
