using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModLibrary;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using UnityEngine.EventSystems;

namespace CloneDroneVR.GameModeManagers
{
    public class GameModeNone : VRGameModeManager
    {
        public GameModeNone()
        {
            GameMode = GameMode.None;
        }

        Canvas _canvas;
        GameObject _ray;
        bool _hasClickedCurrentSelectable = false;
        EventSystem _eventSystem;

        public override void OnGameModeStarted()
        {
            VRManager.Instance.Player.transform.position = new Vector3(0f, 10f, 0f);

            _canvas = getTopParent(GameUIRoot.Instance.TitleScreenUI.transform).GetComponent<Canvas>();

            _canvas.renderMode = RenderMode.WorldSpace;

            _canvas.transform.position = new Vector3(0, 10, 5);
            _canvas.transform.localScale = Vector3.one * 0.01f;

            _eventSystem = GameObject.FindObjectOfType<EventSystem>();

            Component[] components = _canvas.GetComponentsInChildren<Component>(true);
            foreach(Component component in components)
            {
                if(!(component is IPointerClickHandler) && !(component is ISelectHandler))
                    continue;

                BoxCollider collider = component.gameObject.AddComponent<BoxCollider>();
                float width = component.GetComponent<RectTransform>().sizeDelta.x;
                float height = component.GetComponent<RectTransform>().sizeDelta.y;
                collider.size = new Vector3(width, height, 1f);
            }
        }
        public override void OnPreVRRender()
        {
            if (_ray == null)
            {
                _ray = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GameObject.Destroy(_ray.GetComponent<Collider>());
            }

            VRController controller = VRManager.Instance.Player.RightController;
            Ray ray = new Ray(controller.transform.position, controller.transform.forward);
            if (Physics.Raycast(ray,out RaycastHit hit, float.PositiveInfinity))
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
                    ((ISelectHandler)componentWithSelectHandaler).OnSelect(new BaseEventData(_eventSystem));

                if(controller.ControllerState.rAxis1.x > 0.5f)
                {
                    if(!_hasClickedCurrentSelectable)
                    {
                        Component componentWithClickHandaler = getInterfaceComponent<IPointerClickHandler>(hit.collider.gameObject);
                        if(componentWithClickHandaler != null)
                        {
                            ((IPointerClickHandler)componentWithClickHandaler).OnPointerClick(new PointerEventData(_eventSystem));
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

        public override void OnGameModeQuit()
        {
            GameObject.Destroy(_ray);
        }

        Transform getTopParent(Transform obj)
        {
            while(obj.parent != null)
            {
                obj = obj.parent;
            }
            return obj;
        }
        Component getInterfaceComponent<_interface>(GameObject _object)
        {
            Component[] components = _object.GetComponents<Component>();

            foreach(Component component in components)
            {
                if (component is _interface)
                {
                    return component;
                }
            }

            return null;
        }

    }
}
