using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ModLibrary;
using RootMotion.FinalIK;
using Valve.VR;

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
            _timeToActivateInput = Time.time + 0.5f;

            _owner = GetComponent<FirstPersonMover>();
            if(_owner == null)
                throw new MissingComponentException("VR player must be on the same GameObject as a FirstPersonMover");

            _head = _owner.GetBodyPart(MechBodyPartType.Head).transform.parent.gameObject;
            _head.SetActive(false);
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

            VRManager.Instance.Player.Scale = transform.localScale.y * 1.4f;
        }
        void Update()
        {
            handleVRInput();
            handleWeaponsActive();

            PlayerInputController nonVRInputController = _owner.GetComponent<PlayerInputController>();
            if(nonVRInputController.enabled)
                nonVRInputController.enabled = false;

            VRManager.Instance.Player.transform.position = transform.position;

            Vector3 playerHeadEularAngles = VRManager.Instance.Player.Head.transform.eulerAngles;

            Vector3 myEularAngles = transform.eulerAngles;
            myEularAngles.y = playerHeadEularAngles.y;
            transform.eulerAngles = myEularAngles;
            
            _leftArmIK.solver.IKPosition = VRManager.Instance.Player.LeftController.transform.position;
            _leftArmIK.solver.IKRotation = VRManager.Instance.Player.LeftController.transform.rotation * Quaternion.Euler(0f, -90f, 0f);

            _rightArmIK.solver.IKPosition = VRManager.Instance.Player.RightController.transform.position;
            _rightArmIK.solver.IKRotation = VRManager.Instance.Player.RightController.transform.rotation * Quaternion.Euler(0f, -90f, 0f);

            CharacterModel characterModel = _owner.GetCharacterModel();
            if (characterModel != null)
            {
                Vector3 worldOffset = VRManager.Instance.Player.Head.transform.localPosition;
                worldOffset.y = 0f;
                
                Vector3 characterModelPosition = Quaternion.Euler(0f, -VRManager.Instance.Player.Head.transform.eulerAngles.y, 0f) * worldOffset;
                characterModel.transform.localPosition = characterModelPosition;
            }

        }

        float _timeToActivateInput;
        bool _damageKeyDown = false;
        void handleVRInput()
        {
            if(Time.time < _timeToActivateInput)
                return;

            VRControllerState_t leftControllerState = VRManager.Instance.Player.LeftController.ControllerState;
            VRControllerState_t rightControllerState = VRManager.Instance.Player.RightController.ControllerState;

            Vector2 movmentVector = leftControllerState.GetJoystick();

            _owner.SetHorizontalMovement(movmentVector.x);
            _owner.SetVerticalMovement(movmentVector.y);

            bool joystickDown = (rightControllerState.ulButtonPressed & 4294967296) != 0;

            _owner.SetJumpKeyDown(joystickDown);

            if (!_damageKeyDown && rightControllerState.GetFrontTriggerValue() > 0.8f)
            {
                WeaponModel weaponModel = Accessor.GetPrivateField<FirstPersonMover, WeaponModel>("_currentWeaponModel", _owner);
                weaponModel.SetWeaponDamageActive(true);
                _damageKeyDown = true;
            }
            if(_damageKeyDown && rightControllerState.GetFrontTriggerValue() <= 0.8f)
            {
                WeaponModel weaponModel = Accessor.GetPrivateField<FirstPersonMover, WeaponModel>("_currentWeaponModel", _owner);
                weaponModel.SetWeaponDamageActive(false);
                _damageKeyDown = false;
            }

            if (_damageKeyDown)
            {
                OpenVR.System.TriggerHapticPulse(VRManager.Instance.Player.RightController.DeviceIndex, 0, 50000);
            }
        }
        
        void handleWeaponsActive()
        {
            
            
            /*
            bool isFastEnough = _isWithinSpeedsToCut;

            debug.Log(isFastEnough + " : " + _wasDamageActive);
            if(!_wasDamageActive && isFastEnough)
            {
                debug.Log("a");
                weaponModel.SetWeaponDamageActive(true);
                
                _wasDamageActive = true;
            }
            else if(!isFastEnough && _wasDamageActive)
            {
                debug.Log("b");
                weaponModel.SetWeaponDamageActive(false);
                _wasDamageActive = false;
            }
            */
        }

        public const float MinSpeedToCut = 0.5f;
        bool _isWithinSpeedsToCut
        {
            get
            {
                return VRManager.Instance.Player.RightController.Velocity.magnitude > 1 ||  VRManager.Instance.Player.RightController.AngularVelocity.magnitude > 1;
            }
        }

        void OnPlayerDeath()
        {
            if(VRManager.Instance.CurrentModeManager != null)
                VRManager.Instance.CurrentModeManager.OnVRPlayerDeath(this);

            Destroy(this);
        }
    }
}
