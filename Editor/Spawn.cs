using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

namespace Buddyworks.Scene 
{

    public class Spawn : MonoBehaviour
    {

        static string logHeader = Compositor.logHeader;
        static string logAbort = Compositor.logAbort;
        static string logInfo = Compositor.logInfo;
        static string logSuccess = Compositor.logSuccess;

        static string AudioLinkAvatarPrefab = Compositor.AudioLinkAvatarPrefab;
        static string LTCGIPrefab = Compositor.LTCGIPrefab;

        [MenuItem("BUDDYWORKS/Avatar Scene/Spawn. AudioLink Prefab...", priority = 23)]
        private static void AudioLinkAvatarPrefabSpawn() //AudioLink Spawn.er for PostInstallation
        {
            Spawn.PrefabPostSetup(AudioLinkAvatarPrefab);
        }
        [MenuItem("BUDDYWORKS/Avatar Scene/Spawn. AudioLink Prefab...", true)]
        private static bool ValidateAudioLinkAvatarPrefabSpawn()
        {
            return AssetDatabase.IsValidFolder("Packages/com.llealloo.audiolink") != false;
        }

        [MenuItem("BUDDYWORKS/Avatar Scene/Spawn. LTCGI Prefab...", priority = 24)]
        private static void LTCGIPrefabSpawn() //LTCGI Spawn.er for PostInstallation
        {
            Spawn.PrefabPostSetup(LTCGIPrefab);
        }
        [MenuItem("BUDDYWORKS/Avatar Scene/Spawn. LTCGI Prefab...", true)]
        private static bool ValidateLTCGIPrefabSpawn()
        {
            return AssetDatabase.IsValidFolder("Packages/at.pimaker.ltcgi") != false;
        }

        public static void Prefab(string guid, bool isActive, GameObject systemParent) //Contextual prefab spawning function.
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(prefabPath)) { Debug.LogError(logAbort + "Prefab with GUID " + guid + " not found."); return; }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject selectedObject = Selection.activeGameObject;

            if (prefab == null) { Debug.LogError(logAbort + "Failed to load prefab with GUID " + guid + " at path " + prefabPath); return; }

            GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab, systemParent.transform);
            instantiatedPrefab.SetActive(isActive);

            if (instantiatedPrefab = null) Debug.LogError(logAbort + "Failed to instantiate prefab with GUID " + guid);
        }

        public static void PrefabPostSetup(string guid) //Non-contextual prefab spawning function, can be called through menus.
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(prefabPath)) { Debug.LogError(logAbort + "Prefab with GUID " + guid + " not found."); return; }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject selectedObject = Selection.activeGameObject;

            if (prefab == null) { Debug.LogError(logAbort + "Failed to load prefab with GUID " + guid + " at path " + prefabPath); return; }

            GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            if (selectedObject != null) instantiatedPrefab.transform.parent = selectedObject.transform; 
            EditorGUIUtility.PingObject(instantiatedPrefab);

            if (instantiatedPrefab = null) Debug.LogError(logAbort + "Failed to instantiate prefab with GUID " + guid);
        }
    }
}