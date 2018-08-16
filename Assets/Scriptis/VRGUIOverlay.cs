using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using sh_akira.VROverlay;

namespace sh_akira.VROverlay
{
    public class VRGUIOverlay : MonoBehaviour
    {

        public Transform LeftController;
        public Transform RightController;

        [System.NonSerialized]
        public VROverlay overlay;

        [System.NonSerialized]
        public bool showCursor = false;
        [System.NonSerialized]
        public Vector2 cursorPosition = new Vector2(0, 0);
        [System.NonSerialized]
        public Transform cursorTargetTransform = null;

        private GameObject rayTargetQuad;

        private void Start()
        {
            if (overlay == null) overlay = GetComponent<VROverlay>();
        }

        private void Log(params object[] values)
        {
            var list = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                list[i] = values[i].ToString();
            }
            Debug.Log(string.Join("", list));
        }

        private void Update()
        {
            if (overlay != null)
            {
                bool hitted = false;
                Vector2 coord = Vector2.zero;
                //UIカーソル
                foreach (var controller in new Transform[] { LeftController, RightController })
                {
                    var t = controller;

                    //var results = new VROverlay.IntersectionResults();
                    //showCursor = overlay.ComputeIntersection(t.position, t.rotation * Vector3.forward, ref results);
                    //if (showCursor)
                    //{
                    //    Debug.Log("Point:" + results.point.ToString() + " normal:" + results.normal.ToString() + " UVs:" + results.UVs.ToString() +" distance:" + results.distance.ToString());
                    //    cursorPosition = results.UVs;
                    //    //localRotation = Quaternion.LookRotation(results.normal);
                    //}

                    //OpenVRの当たり判定が1:1の領域しか取得してくれないためRayを飛ばす方法を使う
                    var ray = new Ray(controller.position, controller.forward);
                    RaycastHit hit;
                    float distance = 10.0f;
                    if (Physics.Raycast(ray, out hit, distance))
                    {
                        if (hit.transform == overlay.RayTargetQuad.transform)
                        {
                            hitted = true;
                            //Log("RayHit|barycentricCoordinate:", hit.barycentricCoordinate, " distance:", hit.distance, " lightmapCoord:", hit.lightmapCoord, " normal:", hit.normal, " point:", hit.point, " textureCoord:", hit.textureCoord, " x:", hit.textureCoord.x, " y:", hit.textureCoord.y);
                            coord = hit.textureCoord;
                            cursorTargetTransform = hit.transform;
                        }
                    }
                }
                showCursor = hitted;
                if (hitted)
                {
                    cursorPosition.x = coord.x;
                    cursorPosition.y = 1.0f - coord.y;
                }
            }
        }
    }
}