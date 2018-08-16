using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.VR;
using sh_akira.VROverlay;

namespace sh_akira.VROverlay
{
    public class VRGUIInputModule : BaseInputModule
    {
        public OVRControllerAction controllerAction;
        public List<VRGUIOverlay> guiOverlays = new List<VRGUIOverlay>();

        private bool initialized = false;
        private bool IsDrag = false;
        private bool IsKeyUp = false;
        /// <summary>
        /// Update
        /// </summary>
        public override void Process()
        {
            if (controllerAction == null) return;

            if (initialized == false)
            {
                initialized = true;
                controllerAction.KeyDownEvent += Controller_KeyDown;
                controllerAction.KeyUpEvent += Controller_KeyUp;
            }
            var pointerData = GetVRPointerEventData();
            if (pointerData.pointerCurrentRaycast.gameObject != null)
            {
                BaseEventData data = GetBaseEventData();

                if (IsKeyUp)
                {
                    ExecuteEvents.Execute(pointerData.pointerCurrentRaycast.gameObject, data, ExecuteEvents.submitHandler);
                }
                if (IsDrag)
                {
                    ExecuteEvents.Execute(pointerData.pointerCurrentRaycast.gameObject, data, ExecuteEvents.dragHandler);
                }
            }
            IsKeyUp = false;
        }

        private void Controller_KeyDown(object sender, OVRKeyEventArgs args)
        {
            if (args.ButtonId == Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)
            {
                IsDrag = true;
            }
        }

        private void Controller_KeyUp(object sender, OVRKeyEventArgs args)
        {
            if (args.ButtonId == Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)
            {
                IsDrag = false;
                IsKeyUp = true;
            }
        }
        
        /// <summary>
        /// VRController pointer data(Get fitst hit object by raycaster)
        /// </summary>
        /// <returns></returns>
        protected virtual PointerEventData GetVRPointerEventData()
        {
            PointerEventData pointerData = new PointerEventData(eventSystem);

            pointerData.Reset();
            foreach (var guiOverlay in guiOverlays)
            {
                if (guiOverlay.showCursor == false) continue;

                var oldPos = pointerData.position;
                pointerData.position = new Vector2(guiOverlay.overlay.texture.width * guiOverlay.cursorPosition.x, guiOverlay.overlay.texture.height * (1.0f - guiOverlay.cursorPosition.y));
                pointerData.delta = pointerData.position - oldPos;
                pointerData.scrollDelta = Vector3.zero;
                eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
                var raycast = FindFirstRaycast(m_RaycastResultCache);
                pointerData.pointerCurrentRaycast = raycast;
                m_RaycastResultCache.Clear();
            }
            return pointerData;
        }

    }
}