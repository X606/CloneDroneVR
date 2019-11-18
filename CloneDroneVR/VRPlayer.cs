using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace CloneDroneVR
{
    public class VRPlayer : MonoBehaviour
    {
        public VRCamera Head;
        public VRController LeftController;
        public VRController RightController;

        void Start()
        {
            Head = new GameObject("Head").AddComponent<VRCamera>();
            Head.transform.parent = transform;

            LeftController = new GameObject("LeftController").AddComponent<VRController>();
            LeftController.transform.parent = transform;

            RightController = new GameObject("RightController").AddComponent<VRController>();
            RightController.transform.parent = transform;
        }

        void LateUpdate()
        {
            OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0f, VRManager.DevicePoses);
            SteamVR_Events.NewPoses.Send(VRManager.DevicePoses);

            for(int i = 0; i < VRManager.DevicePoses.Length; i++)
            {
                TrackedDevicePose_t devicePose = VRManager.DevicePoses[i];
                if(!devicePose.bDeviceIsConnected || !devicePose.bPoseIsValid)
                    continue;

                SteamVR_Utils.RigidTransform rigidTransform = new SteamVR_Utils.RigidTransform(devicePose.mDeviceToAbsoluteTracking);

                VRNodeType currentType = (VRNodeType)i;
                
                VRNode currentNode = null;
                switch(currentType)
                {
                    case VRNodeType.Head:
                        currentNode = Head;
                    break;
                    case VRNodeType.LeftHand:
                        currentNode = LeftController;
                    break;
                    case VRNodeType.RightHand:
                        currentNode = RightController;
                    break;
                    default:
                    continue;
                }

                currentNode.transform.localPosition = rigidTransform.pos;
                currentNode.transform.localRotation = rigidTransform.rot;
                currentNode.Velocity = fromHmdVector3_t(devicePose.vVelocity);
                currentNode.AngularVelocity = fromHmdVector3_t(devicePose.vAngularVelocity);
            }

            Head.Render();
        }


        Vector3 fromHmdVector3_t(HmdVector3_t hmdVector3_T)
        {
            return new Vector3(hmdVector3_T.v0, hmdVector3_T.v1, hmdVector3_T.v2);
        }

    }

    public enum VRNodeType
    {
        Head = 0,
        LeftHand = 1,
        RightHand = 2
    }

    public class VRNode : MonoBehaviour
    {
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
            LeftCamera.enabled = false;
            SteamVR_Utils.RigidTransform leftEye = new SteamVR_Utils.RigidTransform(OpenVR.System.GetEyeToHeadTransform(EVREye.Eye_Left));
            LeftCamera.transform.localPosition = leftEye.pos;
            LeftCamera.transform.localRotation = leftEye.rot;

            RightCamera = new GameObject("RightCamera").AddComponent<Camera>();
            RightCamera.transform.parent = transform;
            RightCamera.enabled = false;
            SteamVR_Utils.RigidTransform rightEye = new SteamVR_Utils.RigidTransform(OpenVR.System.GetEyeToHeadTransform(EVREye.Eye_Right));
            RightCamera.transform.localPosition = leftEye.pos;
            RightCamera.transform.localRotation = leftEye.rot;

        }

        public void Render()
        {
            RightCamera.Render();
            LeftCamera.Render();

            OpenVR.Compositor.Submit(EVREye.Eye_Left,)
        }
    }

    public class VRController : VRNode
    {

    }
}
