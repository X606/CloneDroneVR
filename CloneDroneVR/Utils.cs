using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.ImageEffects;
using ModLibrary;

namespace CloneDroneVR
{
    public static class Utils
    {
        public static void ApplyPostProcessFiltersToCamera(Camera camera)
        {
            Camera mainCamera = Camera.main;

            Bloom bloom = camera.gameObject.AddComponent<Bloom>();
            bloom.tweakMode = Bloom.TweakMode.Basic;
            bloom.screenBlendMode = Bloom.BloomScreenBlendMode.Add;
            bloom.hdr = Bloom.HDRBloomMode.Auto;
            bloom.sepBlurSpread = 2.5f;
            bloom.quality = Bloom.BloomQuality.High;
            bloom.bloomIntensity = 0.5f;
            bloom.bloomThreshold = 0.9f;
            bloom.bloomThresholdColor = new Color(1f, 1f, 1f, 1f);
            bloom.bloomBlurIterations = 2;
            bloom.hollywoodFlareBlurIterations = 2;
            bloom.flareRotation = 0f;
            bloom.lensflareMode = Bloom.LensFlareStyle.Anamorphic;
            bloom.hollyStretchWidth = 2.5f;
            bloom.lensflareIntensity = 0f;
            bloom.lensflareThreshold = 0.3f;
            bloom.lensFlareSaturation = 0.75f;
            bloom.flareColorA = new Color(0.4f, 0.4f, 0.8f, 0.75f);
            bloom.flareColorB = new Color(0.4f, 0.8f, 0.8f, 0.75f);
            bloom.flareColorC = new Color(0.8f, 0.4f, 0.8f, 0.75f);
            bloom.flareColorD = new Color(0.8f, 0.4f, 0f, 0.75f);
            bloom.lensFlareVignetteMask = mainCamera.GetComponent<Bloom>().lensFlareVignetteMask;
            bloom.lensFlareShader = Shader.Find("Hidden/LensFlareCreate");
            bloom.screenBlendShader = Shader.Find("Hidden/BlendForBloom");
            bloom.blurAndFlaresShader = Shader.Find("Hidden/BlurAndFlares");
            bloom.brightPassFilterShader = Shader.Find("Hidden/BrightPassFilter2");

            AmplifyColorEffect mainCameraColorEffect = mainCamera.GetComponent<AmplifyColorEffect>();
            AmplifyColorEffect colorEffect = camera.gameObject.AddComponent<AmplifyColorEffect>();
            colorEffect.Tonemapper = AmplifyColor.Tonemapping.Disabled;
            colorEffect.Exposure = 1f;
            colorEffect.LinearWhitePoint = 11.2f;
            colorEffect.ApplyDithering = false;
            colorEffect.QualityLevel = AmplifyColor.Quality.Standard;
            colorEffect.BlendAmount = 0.33f;
            colorEffect.LutTexture = mainCameraColorEffect.LutTexture;
            colorEffect.LutBlendTexture = mainCameraColorEffect.LutBlendTexture;
            colorEffect.MaskTexture = mainCameraColorEffect.MaskTexture;
            colorEffect.UseDepthMask = false;
            colorEffect.DepthMaskCurve = mainCameraColorEffect.DepthMaskCurve;
            colorEffect.UseVolumes = false;
            colorEffect.ExitVolumeBlendTime = 1f;
            colorEffect.TriggerVolumeProxy = mainCameraColorEffect.TriggerVolumeProxy;
            colorEffect.VolumeCollisionMask = mainCameraColorEffect.VolumeCollisionMask;
            colorEffect.EffectFlags = mainCameraColorEffect.EffectFlags;

        }

        public static float CalculatePredictedSecondsToPhotons()
        {
            float secondsSinceLastVsync = 0f;
            ulong frameCounter = 0;
            OpenVR.System.GetTimeSinceLastVsync(ref secondsSinceLastVsync, ref frameCounter);

            float displayFrequency = GetFloatTrackedDeviceProperty(ETrackedDeviceProperty.Prop_DisplayFrequency_Float);
            float vsyncToPhotons = GetFloatTrackedDeviceProperty(ETrackedDeviceProperty.Prop_SecondsFromVsyncToPhotons_Float);
            float frameDuration = 1f / displayFrequency;

            return frameDuration - secondsSinceLastVsync + vsyncToPhotons;
        }

        public static float GetFloatTrackedDeviceProperty(ETrackedDeviceProperty property, uint device = OpenVR.k_unTrackedDeviceIndex_Hmd)
        {
            ETrackedPropertyError propertyError = ETrackedPropertyError.TrackedProp_Success;
            float value = OpenVR.System.GetFloatTrackedDeviceProperty(device, property, ref propertyError);
            if(propertyError != ETrackedPropertyError.TrackedProp_Success)
            {
                throw new Exception("Failed to obtain tracked device property \"" +
                    property + "\", error: (" + (int)propertyError + ") " + propertyError.ToString());
            }
            return value;
        }

        public static Matrix4x4 Matrix4x4_OpenVr2UnityFormat(ref HmdMatrix44_t mat44_openvr)
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

        public static float Pythagoras(float x, float y)
        {
            return Mathf.Sqrt(x*x+y*y);
        }
    }


    public static class PointerRay
    {
        public static Action OnAnyButtonClicked = null;

        static GameObject _ray;

        static bool _active;
        public static void SetEnabled(bool state)
        {
            if (_active && !state)
            {
                onDisable();
            }
            if (!_active && state)
            {
                onEnable();
            }

            _active = state;

            
        }

        static Canvas[] _canvases;
        static bool _hasClickedCurrentSelectable = false;
        static EventSystem _eventSystem;

        static List<Component> _selectedObjects = new List<Component>();
        static void deselectEverything()
        {
            foreach(Component @object in _selectedObjects)
            {
                IDeselectHandler deselect = @object as IDeselectHandler;
                if(deselect != null)
                {
                    deselect.OnDeselect(new BaseEventData(_eventSystem));
                }
            }
            _selectedObjects.Clear();
        }

        static void refreshCanvases()
        {
            RectTransform mainCanvas = getTopParent(GameUIRoot.Instance.TitleScreenUI.transform).GetComponent<RectTransform>();

            _canvases = GameObject.FindObjectsOfType<Canvas>();
            for(int i = 0; i < _canvases.Length; i++)
            {
                _canvases[i].renderMode = RenderMode.WorldSpace;

                float offset = i / 25f;

                _canvases[i].transform.position = new Vector3(0f, 40f, 5f + offset);
                _canvases[i].transform.localScale = Vector3.one * 0.01f;
                _canvases[i].GetComponent<RectTransform>().sizeDelta = mainCanvas.sizeDelta;
                Component[] components = _canvases[i].GetComponentsInChildren<Component>(true);
                foreach(Component component in components)
                {
                    if(!(component is IPointerClickHandler) && !(component is ISelectHandler))
                        continue;

                    if(component.GetComponent<BoxCollider>() != null) // we have already created the collider
                        continue;

                    BoxCollider collider = component.gameObject.AddComponent<BoxCollider>();
                    float width = component.GetComponent<RectTransform>().sizeDelta.x;
                    float height = component.GetComponent<RectTransform>().sizeDelta.y;
                    collider.size = new Vector3(width, height, 1f);
                }
            }
        }

        public static void Update()
        {
            if(!_active)
                return;


            if(_ray == null)
            {
                _ray = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GameObject.Destroy(_ray.GetComponent<Collider>());
            }

            VRController controller = VRManager.Instance.Player.RightController;
            Ray ray = new Ray(controller.transform.position, controller.transform.forward);
            if(Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity))
            {
                if(!_ray.activeSelf)
                    _ray.SetActive(true);

                Vector3 offset = hit.point - controller.transform.position;
                float length = offset.magnitude;
                _ray.transform.localScale = new Vector3(0.02f, 0.02f, length);
                _ray.transform.localPosition = offset/2f + controller.transform.position;
                _ray.transform.forward = offset.normalized;

                Component componentWithSelectHandaler = getInterfaceComponent<ISelectHandler>(hit.collider.gameObject);
                if(componentWithSelectHandaler != null)
                {
                    if(!_selectedObjects.Contains(componentWithSelectHandaler))
                    {
                        deselectEverything();
                        ((ISelectHandler)componentWithSelectHandaler).OnSelect(new BaseEventData(_eventSystem));
                        _selectedObjects.Add(componentWithSelectHandaler);
                    }
                }


                if(controller.ControllerState.GetFrontTriggerValue() > 0.5f)
                {
                    if(!_hasClickedCurrentSelectable)
                    {
                        Component componentWithClickHandaler = getInterfaceComponent<IPointerClickHandler>(hit.collider.gameObject);
                        if(componentWithClickHandaler != null)
                        {
                            ((IPointerClickHandler)componentWithClickHandaler).OnPointerClick(new PointerEventData(_eventSystem));
                            DelegateScheduler.Instance.Schedule(refreshCanvases, 0f);
                            if(OnAnyButtonClicked != null)
                                OnAnyButtonClicked();
                            _hasClickedCurrentSelectable = true;
                        }
                    }
                }
                else
                {
                    _hasClickedCurrentSelectable = false;
                }

            }
            else
            {
                if(_ray.activeSelf)
                    _ray.SetActive(false);
            }
        }
        static void onEnable()
        {
            refreshCanvases();

            _eventSystem = GameObject.FindObjectOfType<EventSystem>();
        }
        static void onDisable()
        {
            GameObject.Destroy(_ray);
        }

        static Transform getTopParent(Transform obj)
        {
            while(obj.parent != null)
            {
                obj = obj.parent;
            }
            return obj;
        }
        static Component getInterfaceComponent<_interface>(GameObject _object)
        {
            Component[] components = _object.GetComponents<Component>();

            foreach(Component component in components)
            {
                if(component is _interface)
                {
                    return component;
                }
            }

            return null;
        }

    }

}
