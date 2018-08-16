using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using sh_akira.VROverlay;

namespace sh_akira.VROverlay
{
    public class VROverlay : MonoBehaviour
    {
        public Texture texture;
        
        public float WidthInMeters = 1.0f;
        private float lastWidthInMeters = 0.0f;

        public float alpha = 1.0f;

        public Transform RenderPosition;
        private Transform lastRenderPosition = null;

        [System.NonSerialized]
        public GameObject RayTargetQuad = null;

        public Vector3 PositionOffset = new Vector3(0, 0, 0);
        private Vector3 lastPositionOffset = new Vector3(0, 0, 0);

        public Vector4 uvOffset = new Vector4(0, 0, 1, 1);

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

        public void UpdateOverlay()
        {
            var overlay = OpenVR.Overlay;
            if (overlay == null)
                return;

            if (texture != null)
            {
                var error = overlay.ShowOverlay(handle);
                if (error == EVROverlayError.InvalidHandle || error == EVROverlayError.UnknownOverlay)
                {
                    if (overlay.FindOverlay(overlayKey, ref handle) != EVROverlayError.None)
                        return;
                }

                var tex = new Texture_t();
                tex.handle = texture.GetNativeTexturePtr();
                switch (SystemInfo.graphicsDeviceType)
                {
                    case UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore:
                    case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2:
                    case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3:
                        tex.eType = ETextureType.OpenGL;
                        break;
                    case UnityEngine.Rendering.GraphicsDeviceType.Vulkan:
                        tex.eType = ETextureType.Vulkan;
                        break;
                    default:
                        tex.eType = ETextureType.DirectX;
                        break;
                }
                tex.eColorSpace = EColorSpace.Auto;
                overlay.SetOverlayTexture(handle, ref tex);

                overlay.SetOverlayAlpha(handle, alpha);
                overlay.SetOverlayWidthInMeters(handle, WidthInMeters);
                var curvedRange = new Vector2(1, 2);
                overlay.SetOverlayAutoCurveDistanceRangeInMeters(handle, curvedRange.x, curvedRange.y);

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

                if ((lastRenderPosition != RenderPosition || lastWidthInMeters != WidthInMeters || lastPositionOffset != PositionOffset))
                {
                    if (RayTargetQuad != null) Destroy(RayTargetQuad);
                    //Ray飛ばし座標取得用のQuadを配置
                    RayTargetQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    RayTargetQuad.GetComponent<MeshRenderer>().enabled = false; // 非表示にする
                    RayTargetQuad.transform.SetParent(RenderPosition);
                    RayTargetQuad.transform.localPosition = PositionOffset;
                    RayTargetQuad.transform.localEulerAngles = Vector3.zero;
                    RayTargetQuad.transform.localScale = new Vector3(WidthInMeters, (WidthInMeters / texture.width) * texture.height, 1.0f);
                    lastRenderPosition = RenderPosition;
                    lastWidthInMeters = WidthInMeters;
                    lastPositionOffset = PositionOffset;
                }

                var offset = new SteamVR_Utils.RigidTransform();
                if (RayTargetQuad != null)
                {
                    offset.pos = RayTargetQuad.transform.position;
                    offset.rot = RayTargetQuad.transform.rotation;
                }
                else
                {
                    offset.pos = new Vector3(0, 0, 0);
                    offset.rot = Quaternion.Euler(0, 0, 0);
                }
                var t = offset.ToHmdMatrix34();
                overlay.SetOverlayTransformAbsolute(handle, SteamVR_Render.instance.trackingSpace, ref t);


                overlay.SetOverlayInputMethod(handle, VROverlayInputMethod.Mouse);

            }
            else
            {
                overlay.HideOverlay(handle);
            }
        }

        public bool PollNextEvent(ref VREvent_t pEvent)
        {
            var overlay = OpenVR.Overlay;
            if (overlay == null)
                return false;

            var size = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Valve.VR.VREvent_t));
            return overlay.PollNextOverlayEvent(handle, ref pEvent, size);
        }

        public struct IntersectionResults
        {
            public Vector3 point;
            public Vector3 normal;
            public Vector2 UVs;
            public float distance;
        }

        public bool ComputeIntersection(Vector3 source, Vector3 direction, ref IntersectionResults results)
        {
            var overlay = OpenVR.Overlay;
            if (overlay == null)
                return false;

            var input = new VROverlayIntersectionParams_t();
            input.eOrigin = SteamVR_Render.instance.trackingSpace;
            input.vSource.v0 = source.x;
            input.vSource.v1 = source.y;
            input.vSource.v2 = -source.z;
            input.vDirection.v0 = direction.x;
            input.vDirection.v1 = direction.y;
            input.vDirection.v2 = -direction.z;

            var output = new VROverlayIntersectionResults_t();
            if (!overlay.ComputeOverlayIntersection(handle, ref input, ref output))
                return false;

            results.point = new Vector3(output.vPoint.v0, output.vPoint.v1, -output.vPoint.v2);
            results.normal = new Vector3(output.vNormal.v0, output.vNormal.v1, -output.vNormal.v2);
            results.UVs = new Vector2(output.vUVs.v0, output.vUVs.v1);
            results.distance = output.fDistance;
            return true;
        }

        void Update()
        {

            UpdateOverlay();
        }
    }
}