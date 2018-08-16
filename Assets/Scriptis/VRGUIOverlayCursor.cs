using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using sh_akira.VROverlay;

namespace sh_akira.VROverlay
{
    public class VRGUIOverlayCursor : MonoBehaviour
    {

        public Texture CursorTexture;

        public VRGUIOverlay guiOverlay;
        public float WidthInMeters = 0.1f;
        
        private Transform cursorPosition = null;

        private void Log(params object[] values)
        {
            var list = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                list[i] = values[i].ToString();
            }
            Debug.Log(string.Join("", list));
        }

        //private void OnRenderObject()
        //{
        //    if (guiCamera == null || guiOverlay == null || CursorTexture == null) return;

        //    var target = guiCamera.targetTexture;
        //    if (target != null & guiOverlay.showCursor)
        //    {
        //        //var prevActive = RenderTexture.active;
        //        //RenderTexture.active = target;

        //        float x = guiOverlay.cursorPosition.x * target.width, y = guiOverlay.cursorPosition.y * target.height;
        //        float w = CursorTexture.width, h = CursorTexture.height;
        //        float ratio = target.height / target.width;

        //        Log("Cursor| showCursor:", guiOverlay.showCursor, " cursorPosition x:", guiOverlay.cursorPosition.x, " y:", guiOverlay.cursorPosition.y, " real x:", x, " y:", y, " w:", w, " h:", h, " ratio:", ratio);
        //        Graphics.DrawTexture(new Rect(0, 0, w * ratio, h), CursorTexture);

        //        //RenderTexture.active = prevActive;
        //    }
        //}


        private string overlayKey;

        private ulong handle = OpenVR.k_ulOverlayHandleInvalid;

        void OnEnable()
        {
            overlayKey = "unity:" + Application.companyName + "." + Application.productName + "/" + System.Guid.NewGuid().ToString();
            var overlay = OpenVR.Overlay;
            if (overlay != null)
            {
                var error = overlay.CreateOverlay(overlayKey, gameObject.name, ref handle);
                if (error != EVROverlayError.None)
                {
                    Debug.Log(overlay.GetOverlayErrorNameFromEnum(error));
                    enabled = false;
                    return;
                }
            }
        }

        void OnDisable()
        {
            if (handle != OpenVR.k_ulOverlayHandleInvalid)
            {
                var overlay = OpenVR.Overlay;
                if (overlay != null)
                {
                    overlay.DestroyOverlay(handle);
                }

                handle = OpenVR.k_ulOverlayHandleInvalid;
            }
        }

        public void UpdateCursorOverlay()
        {
            var overlay = OpenVR.Overlay;
            if (overlay == null || guiOverlay == null)
                return;

            if (CursorTexture != null && guiOverlay.showCursor && guiOverlay.cursorTargetTransform != null)
            {
                var error = overlay.ShowOverlay(handle);
                if (error == EVROverlayError.InvalidHandle || error == EVROverlayError.UnknownOverlay)
                {
                    if (overlay.FindOverlay(overlayKey, ref handle) != EVROverlayError.None)
                        return;
                }

                var tex = new Texture_t();
                tex.handle = CursorTexture.GetNativeTexturePtr();
                tex.eType = SteamVR.instance.textureType;
                tex.eColorSpace = EColorSpace.Auto;
                overlay.SetOverlayTexture(handle, ref tex);

                overlay.SetOverlayAlpha(handle, 1.0f);
                overlay.SetOverlayWidthInMeters(handle, WidthInMeters);
                var curvedRange = new Vector2(1, 2);
                overlay.SetOverlayAutoCurveDistanceRangeInMeters(handle, curvedRange.x, curvedRange.y);

                var uvOffset = new Vector4(0, 0, 1, 1);
                var textureBounds = new VRTextureBounds_t();
                textureBounds.uMin = (0 + uvOffset.x) * uvOffset.z;
                textureBounds.vMin = (1 + uvOffset.y) * uvOffset.w;
                textureBounds.uMax = (1 + uvOffset.x) * uvOffset.z;
                textureBounds.vMax = (0 + uvOffset.y) * uvOffset.w;
                overlay.SetOverlayTextureBounds(handle, ref textureBounds);

                var vecMouseScale = new HmdVector2_t();
                var mouseScale = new Vector2(1, 1);
                vecMouseScale.v0 = mouseScale.x;
                vecMouseScale.v1 = mouseScale.y;
                overlay.SetOverlayMouseScale(handle, ref vecMouseScale);

                if (cursorPosition == null)
                {
                    cursorPosition = (new GameObject()).transform;
                    cursorPosition.SetParent(guiOverlay.cursorTargetTransform);
                    cursorPosition.localEulerAngles = Vector3.zero;
                }
                var offset = new SteamVR_Utils.RigidTransform();

                var texWidth = guiOverlay.cursorTargetTransform.localScale.x;
                var texHeight = guiOverlay.cursorTargetTransform.localScale.y;
                var curWidth = WidthInMeters;
                var curHeight = (WidthInMeters / CursorTexture.width) * CursorTexture.height;
                var scaleWidth = curWidth / texWidth;
                var scaleHeight = curHeight / texHeight;

                cursorPosition.localPosition = new Vector3(guiOverlay.cursorPosition.x - 0.5f + (scaleWidth / 2), 0.5f - guiOverlay.cursorPosition.y - (scaleHeight / 2), -0.0f);
                offset.pos = cursorPosition.position;
                offset.rot = cursorPosition.rotation;

                var t = offset.ToHmdMatrix34();
                overlay.SetOverlayTransformAbsolute(handle, SteamVR_Render.instance.trackingSpace, ref t);

                overlay.SetOverlaySortOrder(handle, 1); //SortOrderは数字が大きいほうが後からレンダリングされる

                overlay.SetOverlayInputMethod(handle, VROverlayInputMethod.Mouse);

            }
            else
            {
                overlay.HideOverlay(handle);
            }
        }


        private void Update()
        {
            UpdateCursorOverlay();
        }
    }
}