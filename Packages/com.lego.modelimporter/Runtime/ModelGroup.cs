﻿// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace LEGOModelImporter
{

    [SelectionBase]
    public class ModelGroup : MonoBehaviour
    {
        [Flags]
        public enum ViewBasedOptimizations
        {
            None = 0,
            RemoveInvisibleInsides = 1 << 0,
            RemoveInvisibleOutsides = 1 << 1,
            BackfaceCulling = 1 << 2,
            SortFrontToBack = 1 << 3,
            Everything = ~0,
        }

        public static ViewBasedOptimizations ViewBasedGeometryRemovalOptimizations = ViewBasedOptimizations.RemoveInvisibleInsides | ViewBasedOptimizations.RemoveInvisibleOutsides | ViewBasedOptimizations.BackfaceCulling;

        /*    [Flags]
            public enum Imperfections
            {
                None = 0,
                RandomizeNormals = 1 << 0,
                UVDegradation = 1 << 1,
                Scratches = 1 << 2,
                Everything = ~0,
            }*/

        public string groupName;
        public string parentName;
        public int number;

        public ModelGroupImportSettings importSettings;

        public ViewBasedOptimizations viewBasedOptimizations = ViewBasedOptimizations.None;
        public bool randomizeNormals = false;
        //public Imperfections imperfections = Imperfections.Everything;
        public bool processed;

        public string absoluteFilePath;
        public string relativeFilePath;

        public bool autoGenerated;

        public List<CullingCameraConfig> views = new List<CullingCameraConfig>();

        public void DisconnectGroup()
        {
            var bricksInGroup = GetComponentsInChildren<Brick>();
            foreach (var brick in bricksInGroup)
            {
                brick.DisconnectInverse(bricksInGroup);
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!processed && viewBasedOptimizations != 0)
            {
                Gizmos.color = Color.cyan;
                int index = 0;
                foreach (var view in views)
                {
                    Gizmos.matrix = Matrix4x4.TRS(view.position, view.rotation, Vector3.one);
                    if (view.perspective)
                    {
#if UNITY_EDITOR
                        Handles.Label(view.position, string.IsNullOrEmpty(view.name) ? "View " + index : view.name);
#endif
                        Gizmos.DrawFrustum(Vector3.zero, view.fov, view.maxRange, view.minRange, view.aspect);
                    }
                    else
                    {
#if UNITY_EDITOR
                        Handles.Label(view.position, string.IsNullOrEmpty(view.name) ? "View " + index : view.name);
#endif
                        var center = Vector3.forward * (view.minRange + view.maxRange) * 0.5f;
                        var size = new Vector3(view.size * 2.0f * view.aspect, view.size * 2.0f, view.maxRange - view.minRange);
                        Gizmos.DrawWireCube(center, size);
                    }
                    index++;
                }
            }
        }
    }

}