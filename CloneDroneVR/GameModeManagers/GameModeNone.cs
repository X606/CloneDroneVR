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

        Canvas[] _canvases;
        GameObject _ray;
        bool _hasClickedCurrentSelectable = false;
        EventSystem _eventSystem;

        List<Component> _selectedObjects = new List<Component>();
        void DeselectEverything()
        {
            foreach(Component @object in _selectedObjects)
            {
                IDeselectHandler deselect = @object as IDeselectHandler;
                if (deselect != null)
                {
                    deselect.OnDeselect(new BaseEventData(_eventSystem));
                }
            }
            _selectedObjects.Clear();
        }

        public override void OnGameModeStarted()
        {

            VRManager.Instance.Player.LeftController.ColliderActive = true;
            VRManager.Instance.Player.RightController.ColliderActive = true;
            VRManager.Instance.Player.LeftController.RendererActive = true;
            VRManager.Instance.Player.RightController.RendererActive = true;

            VRManager.Instance.Player.transform.position = new Vector3(0f, 40f, 0f);

            RefreshCanvases();
            
            _eventSystem = GameObject.FindObjectOfType<EventSystem>();

            
        }

        void RefreshCanvases()
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
                {
                    if(!_selectedObjects.Contains(componentWithSelectHandaler))
                    {
                        DeselectEverything();
                        ((ISelectHandler)componentWithSelectHandaler).OnSelect(new BaseEventData(_eventSystem));
                        _selectedObjects.Add(componentWithSelectHandaler);
                    }
                }
                    

                if(controller.ControllerState.rAxis1.x > 0.5f)
                {
                    if(!_hasClickedCurrentSelectable)
                    {
                        Component componentWithClickHandaler = getInterfaceComponent<IPointerClickHandler>(hit.collider.gameObject);
                        if(componentWithClickHandaler != null)
                        {
                            ((IPointerClickHandler)componentWithClickHandaler).OnPointerClick(new PointerEventData(_eventSystem));
                            DelegateScheduler.Instance.Schedule(RefreshCanvases, 0f);
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
