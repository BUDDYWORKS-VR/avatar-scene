using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class BuddyworksScene : MonoBehaviour
{
    //I like fancy formatting, but I dont enjoy typing it all the time. Lets not do that.
    static string logHeader = "<color=grey>[</color><color=#FDDA0D>BUDDYWORKS</color><color=grey>] </color>";
    static string logAbort = logHeader + "<color=red>ERROR:</color> ";
    static string logInfo = logHeader + "<color=white>Log:</color> ";
    static string logSuccess = logHeader + "<color=green>OK:</color> ";

    //Relevant variables, writing the same paths all the time is eh...
    static string BuddyworksPath = "Assets/BUDDYWORKS";
    static string SceneFolder = BuddyworksPath + "/Avatar Scene";
    static string ScenePath = BuddyworksPath + "/Avatar Scene/Avatar Scene.unity";
    static string BuddyworksPackageID = "Packages/wtf.buddyworks.scene/Avatar Scene";

    //Prefab GUIDs
    static string AudioLinkAvatarPrefab = "6e8e0ee5a3655884ea49447ae9e6e665";
    static string LTCGIPrefab = "14b5e322766ebe0469a21d9898d446d9";
    static string GestureManagerPrefab = "2cd7c2d73a12a214b930125a1ca4ed33";

    //The main function, copying the scene date over to /Assets/ and generating the various objects.
    [MenuItem("BUDDYWORKS/Avatar Scene/Add Scene Data", priority = 0)]
    static void AddBuddyworksScene()
    {
        Debug.Log(logInfo + "Starting safety check...");
        if (safetyCheck(ScenePath)) //Ensures that Avatar Scene is not already in the project at the specified location.
        {
            copySceneData(); //Copies the scene files over.

            EditorSceneManager.SaveOpenScenes(); //Saves the current scene, if needed.
            openBuddyworksScene(); //Opens the newly created scene.

            //Configure the scene.
            Debug.Log(logInfo + "Setting up scene...");
            setupSkybox(); //Adjust Skybox

            //Creates the _System parent object.
            Debug.Log(logInfo + "Setting up GameObjects and Prefabs...");
            GameObject systemParent = new GameObject();
            systemParent.name = "_System";

            setupFloorplane(systemParent); //Loads Assets for the Floorplane and sets it up in the scene
            setupLight(systemParent); //Creates a directional light
            setupCamera(systemParent); //Create the Orthographic Camera
            setupPostProcessing(); //Adds a PostProcessing Layer to the Scene's camera for Anti Aliasing

            //Instantiate some prefabs, easy enough eh?
            spawnPrefab(AudioLinkAvatarPrefab, false, systemParent);
            spawnPrefab(LTCGIPrefab, false, systemParent);
            spawnPrefab(GestureManagerPrefab, true, systemParent);
 
            Debug.Log(logSuccess + "Scene setup finished!");

            //Workarounds to REALLY make sure the scene is actually saved.
            Debug.Log(logInfo + "Saving scene...");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath, false);
            Debug.Log(logSuccess + "Avatar Scene successfully imported!");
        }
    }

    //Lets you manually open the scene, if it is still at the specified path. Else its disabled.
    [MenuItem("BUDDYWORKS/Avatar Scene/Open Scene...", priority = 1)]
    private static void openBuddyworksScene()
    {
        Debug.Log(logInfo + "Opening Scene...");
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene(ScenePath);
        return;
    }
    [MenuItem("BUDDYWORKS/Avatar Scene/Open Scene...", true)]
    private static bool ValidateopenBuddyworksScene()
    {
        //Checks whenever the scene exists.
        if (System.IO.File.Exists(ScenePath)) {
            return true;
        }
        return false;
    }

    //Ensures metafiles are removed in the newly copied folder, avoids conflicts.
    private static void DeleteMetaFilesRecursively(string folderPath)
    {
        string[] files = Directory.GetFiles(folderPath, "*.meta");
        foreach (string file in files)
        {
            File.Delete(file);
        }

        string[] subDirectories = Directory.GetDirectories(folderPath);
        foreach (string directory in subDirectories)
        {
            DeleteMetaFilesRecursively(directory);
        }
        AssetDatabase.Refresh();
    }

    private static void spawnPrefab(string guid, bool isDefaultActive, GameObject systemParent)
    {
        string prefabPath = AssetDatabase.GUIDToAssetPath(guid);

        if (string.IsNullOrEmpty(prefabPath)) {
            Debug.LogError(logAbort + "Prefab with GUID " + guid + " not found.");
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject selectedObject = Selection.activeGameObject;

        if (prefab == null) {
            Debug.LogError(logAbort + "Failed to load prefab with GUID " + guid + " at path " + prefabPath);
            return;
        }

        GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab, systemParent.transform);
        instantiatedPrefab.SetActive(isDefaultActive);

        if (instantiatedPrefab = null) Debug.LogError("Failed to instantiate prefab with GUID " + guid);
    }

    private static void spawnPrefabPostSetup(string guid)
    {
        string prefabPath = AssetDatabase.GUIDToAssetPath(guid);

        if (string.IsNullOrEmpty(prefabPath)) {
            Debug.LogError(logAbort + "Prefab with GUID " + guid + " not found.");
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject selectedObject = Selection.activeGameObject;

        if (prefab == null) {
            Debug.LogError(logAbort + "Failed to load prefab with GUID " + guid + " at path " + prefabPath);
            return;
        }

        GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        if (selectedObject != null) instantiatedPrefab.transform.parent = selectedObject.transform; 
        EditorGUIUtility.PingObject(instantiatedPrefab);

        if (instantiatedPrefab = null) Debug.LogError("Failed to instantiate prefab with GUID " + guid);
    }

    [MenuItem("BUDDYWORKS/Avatar Scene/Enable Scene Anti-Aliasing", priority = 12)]
    private static void setupPostProcessing()
    {
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera camera in cameras)
        {
            if (camera.GetComponent<PostProcessLayer>() == null) {
                PostProcessLayer postProcessLayer = camera.gameObject.AddComponent<PostProcessLayer>();
            }
            else {
                Debug.LogWarning(logInfo + "Camera '{camera.name}' already has a Post-process Layer component attached.");
            }
        }
        Debug.Log(logInfo + "Post-process Layer added to all cameras in the scene.");
    }

    //AudioLink Spawner for PostInstallation
    [MenuItem("BUDDYWORKS/Avatar Scene/Spawn AudioLink Prefab...", priority = 23)]
    private static void AudioLinkAvatarPrefabSpawn()
    {
        spawnPrefabPostSetup(AudioLinkAvatarPrefab);
    }
    [MenuItem("BUDDYWORKS/Avatar Scene/Spawn AudioLink Prefab...", true)]
    private static bool ValidateAudioLinkAvatarPrefabSpawn()
    {
        return AssetDatabase.IsValidFolder("Packages/com.llealloo.audiolink") != false;
    }

    //LTCGI Spawner for PostInstallation
    [MenuItem("BUDDYWORKS/Avatar Scene/Spawn LTCGI Prefab...", priority = 24)]
    private static void LTCGIPrefabSpawn()
    {
        spawnPrefabPostSetup(LTCGIPrefab);
    }
    [MenuItem("BUDDYWORKS/Avatar Scene/Spawn LTCGI Prefab...", true)]
    private static bool ValidateLTCGIPrefabSpawn()
    {
        return AssetDatabase.IsValidFolder("Packages/at.pimaker.ltcgi") != false;
    }

    private static void setupSkybox()
    {
        Debug.Log(logInfo + "Setting up skybox...");
        Material skybox = (Material)AssetDatabase.LoadAssetAtPath("Assets/BUDDYWORKS/Avatar Scene/Materials/Skybox_Material.mat", typeof(Material));
        Texture2D skyboxTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/BUDDYWORKS/Avatar Scene/Textures/Skybox_Texture.psd", typeof(Texture2D));
        RenderSettings.skybox = skybox;

        string[] keywords = { "_FrontTex", "_BackTex", "_LeftTex", "_RightTex", "_UpTex", "_DownTex" };
        foreach (string keyword in keywords)
        {
            skybox.EnableKeyword(keyword);
            skybox.SetTexture(keyword, skyboxTexture);
        }
        Debug.Log(logSuccess + "Skybox setup finished!");
    }

    private static bool safetyCheck(string path)
    {
        //Checks whenever the scene already exists and aborts execution if it is.
        if (System.IO.File.Exists(path)) {
            Debug.Log(logAbort + "You already have the scene in your project! The sequence was aborted, no files were changed.");
            return false;
        }
        Debug.Log(logSuccess + "No existing scene data found, proceeding...");
        return true;
    }

    private static void setupFloorplane(GameObject systemParent)
    {
        Material floorplaneMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/BUDDYWORKS/Avatar Scene/Materials/Floorplane_Material.mat", typeof(Material));
        Texture2D floorplaneTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/BUDDYWORKS/Avatar Scene/Textures/Floorplane_Texture.png", typeof(Texture2D));
        GameObject floorplane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        floorplane.transform.SetParent(systemParent.transform);
        floorplane.transform.Rotate(90.0f, 180.0f, 0.0f, Space.Self);
        Vector3 floorplaneScale = new Vector3(2.0327f, 2.0327f, 2.0327f);
        floorplane.transform.localScale = floorplaneScale;
        floorplaneMaterial.mainTexture = floorplaneTexture;
        floorplane.GetComponent<MeshRenderer>().material = floorplaneMaterial;
        floorplane.name = "Floorplane";
    }

    private static void setupLight(GameObject systemParent)
    {
        GameObject scenelight = new GameObject("Directional Light");
        Light lightComp = scenelight.AddComponent<Light>();
        lightComp.color = Color.white;
        lightComp.type = LightType.Directional;
        lightComp.shadows = LightShadows.Soft;
        lightComp.shadowStrength = 0.443f;
        lightComp.intensity = 0.72f;
        scenelight.transform.Rotate(50.0f, -200.0f, 0.0f, Space.Self);
        scenelight.transform.position = new Vector3(0f, 4f, 0f);
        scenelight.transform.SetParent(systemParent.transform);
    }

    private static void setupCamera(GameObject systemParent)
    {
        GameObject orthoCamera = new GameObject("Main Camera");
        Camera cameraComponent = orthoCamera.AddComponent<Camera>();
        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = 0.8f;
        orthoCamera.transform.position = new Vector3(0f, 0.72f, 3.26f);
        orthoCamera.transform.rotation = Quaternion.Euler(0f, -180f, 0f);
        orthoCamera.transform.SetParent(systemParent.transform);
        orthoCamera.AddComponent<AudioListener>();
        orthoCamera.tag = "MainCamera";
    }

    private static void copySceneData()
    {
        Debug.Log(logInfo + "Copying assets...");
        System.IO.Directory.CreateDirectory(BuddyworksPath);
        FileUtil.CopyFileOrDirectory(BuddyworksPackageID, SceneFolder);
        DeleteMetaFilesRecursively(SceneFolder);
    }
}