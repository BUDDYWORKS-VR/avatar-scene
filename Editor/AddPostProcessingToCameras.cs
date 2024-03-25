using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;

public class AddPostProcessLayerToCameras : Editor
{
    [MenuItem("BUDDYWORKS/Avatar Scene/Enable Scene Anti-Aliasing")]
    static void AddPostProcessLayerToAllCameras()
    {
        // Get all cameras in the active scene
        Camera[] cameras = FindObjectsOfType<Camera>();

        foreach (Camera camera in cameras)
        {
            // Check if the camera already has a Post-process Layer component
            if (camera.GetComponent<PostProcessLayer>() == null)
            {
                // Add a Post-process Layer component to the camera
                PostProcessLayer postProcessLayer = camera.gameObject.AddComponent<PostProcessLayer>();

                // You can configure the Post-process Layer settings here if needed
                // For example:
                // postProcessLayer.volumeLayer = LayerMask.GetMask("PostProcessing");
                // postProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                // postProcessLayer.finalBlitToCameraTarget = false;
            }
            else
            {
                Debug.LogWarning($"Camera '{camera.name}' already has a Post-process Layer component attached.");
            }
        }
        Debug.Log("Post-process Layer added to all cameras in the scene.");
    }
}
