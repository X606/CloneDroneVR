using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ModLibrary;
using RootMotion.FinalIK;

namespace CloneDroneVR
{
    public class PhysicalVRPlayer : MonoBehaviour
    {
        FirstPersonMover _owner;

        GameObject _head;
        GameObject _rightHand;
        GameObject _leftHand;

        LimbIK _leftArmIK;
        LimbIK _rightArmIK;

        void Awake()
        {


            _owner = GetComponent<FirstPersonMover>();
            if(_owner == null)
                throw new MissingComponentException("VR player must be on the same GameObject as a FirstPersonMover");

            _head = _owner.GetBodyPart(MechBodyPartType.Head).transform.parent.gameObject;
            _rightHand = _owner.GetBodyPart(MechBodyPartType.RightArm).transform.parent.gameObject;
            _leftHand = _owner.GetBodyPart(MechBodyPartType.LeftArm).transform.parent.gameObject;

            _owner.AddDeathListener(OnPlayerDeath);

            if(_rightHand == null)
                throw new Exception("Right hand is null");
            if(_leftHand == null)
                throw new Exception("Left hand is null");

            Transform armLowerL = _leftHand.transform.parent;
            Transform armUpperL = armLowerL.parent;

            Transform armLowerR = _rightHand.transform.parent;
            Transform armUpperR = armLowerR.parent;

            Transform ArmsRoot = armUpperL.parent;


            _leftArmIK = armUpperL.gameObject.AddComponent<LimbIK>();
            _leftArmIK.solver.SetChain(armUpperL, armLowerL, _leftHand.transform, ArmsRoot);

            _rightArmIK = armUpperR.gameObject.AddComponent<LimbIK>();
            _rightArmIK.solver.SetChain(armUpperR, armLowerR, _rightHand.transform, ArmsRoot);

            _leftArmIK.solver.target = VRManager.Instance.Player.LeftController.transform;
            _rightArmIK.solver.target = VRManager.Instance.Player.RightController.transform;

        }
        public void OnPreVRRender()
        {
            VRManager.Instance.Player.transform.position = transform.position;
        }

        void OnPlayerDeath()
        {
            if(VRManager.Instance.CurrentModeManager != null)
                VRManager.Instance.CurrentModeManager.OnVRPlayerDeath(this);

            Destroy(this);
        }
    }
}
