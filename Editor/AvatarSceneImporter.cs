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

    //GUI Strings
    static string upgradeWindowTitle = "BUDDYWORKS Avatar Scene Upgrader";
    static string upgradeWindowContent = "This will change the currently opened scene to add the BUDDYWORKS Avatar Scene features, do you want to proceed?\n\nThis operation can not be reversed.";
    static string upgradeWindowContentCompleted = "The scene has been upgraded successfully!\n\nIt will still use your existing lights and cameras, you can find the new ones in _System.";

    [MenuItem("BUDDYWORKS/Avatar Scene/Generate New Scene", priority = 0)]
    static void AddBuddyworksScene() //The main function, copying the scene date over to /Assets/ and generating the various objects.
    {
        Debug.Log(logInfo + "Starting safety check...");
        if (safetyCheck(SceneFolder)) //Ensures that Avatar Scene is not already in the project at the specified location.
        {
            copySceneData(false); //Copies the scene files over. dropSceneFile false

            EditorSceneManager.SaveOpenScenes(); //Saves the current scene, if needed.
            openBuddyworksScene(); //Opens the newly created scene.

            //Configure the scene.
            Debug.Log(logInfo + "Setting up scene...");
            setupSkybox(); //Adjust Skybox

            //Creates the _System parent object.
            GameObject systemParent = new GameObject { name = "_System" };

            setupFloorplane(systemParent); //Loads Assets for the Floorplane and sets it up in the scene
            setupLight(systemParent, true); //Creates a directional light. Arg1 parent object, Arg2 whenever its active or not.
            setupCamera(systemParent, true); //Create the Orthographic Camera. Arg1 parent object, Arg2 whenever its active or not.
            setupPostProcessing(); //Adds a PostProcessing Layer to the Scene's camera for Anti Aliasing

            //Instantiates external prefabs. Arg1 = PrefabGUID, Arg2 = SetActive state, Arg3 = Parent.
            spawnPrefab(AudioLinkAvatarPrefab, false, systemParent);
            spawnPrefab(LTCGIPrefab, false, systemParent);
            spawnPrefab(GestureManagerPrefab, true, systemParent);
 
            Debug.Log(logSuccess + "Scene setup finished!");

            setupSave(false); //isUpgrade false.
            return;
        }
        Debug.Log(logAbort + "You already have the scene in your project! The sequence was aborted, no files were changed.");
    }

    [MenuItem("BUDDYWORKS/Avatar Scene/Upgrade Current Scene", priority = 0)]
    static void UpgradeBuddyworksScene() //Similiar to AddBuddyworksScene(), but upgrades the current scene instead of making a new one.
    {
        //Spawn a dialog box, asking for confirmation.
        if(EditorUtility.DisplayDialog(upgradeWindowTitle, upgradeWindowContent, "Upgrade", "Abort")) {
            //Runs when "Upgrade" is selected.
            Debug.Log(logInfo + "Starting upgrade...");
            if (safetyCheck(SceneFolder)) { 
                copySceneData(true); //Copies the scene files over. dropSceneFile true
            }

            EditorSceneManager.SaveOpenScenes();
            Debug.Log(logInfo + "Setting up scene...");
            setupSkybox(); //Adjust Skybox

            if (GameObject.Find("_System") is GameObject systemCheck) {systemCheck.SetActive(false);} //Disables existing _System GameObject, for when upgrade is applied to already upgraded scene.

            //Creates the _System parent object.
            GameObject systemParent = new GameObject { name = "_System" };
            systemParent.transform.SetSiblingIndex(0);

            setupFloorplane(systemParent); //Loads Assets for the Floorplane and sets it up in the scene
            setupLight(systemParent, false); //Creates a directional light. Arg1 parent object, Arg2 whenever its active or not.
            setupCamera(systemParent, false); //Create the Orthographic Camera. Arg1 parent object, Arg2 whenever its active or not.
            setupPostProcessing(); //Adds a PostProcessing Layer to the Scene's camera for Anti Aliasing

            //Instantiates external prefabs. Arg1 = PrefabGUID, Arg2 = SetActive state, Arg3 = Parent.
            spawnPrefab(AudioLinkAvatarPrefab, false, systemParent);
            spawnPrefab(LTCGIPrefab, false, systemParent);
            spawnPrefab(GestureManagerPrefab, true, systemParent);

            Debug.Log(logSuccess + "Scene setup finished!");
            setupSave(true); //isUpgrade true
            EditorUtility.DisplayDialog(upgradeWindowTitle, upgradeWindowContentCompleted, "Done");
            return;
        }
        //Runs when "Abort" is selected, or the window is closed.
        Debug.Log(logInfo + "Upgrade aborted, no files have been changed.");
    }

    [MenuItem("BUDDYWORKS/Avatar Scene/Spawn AudioLink Prefab...", priority = 23)]
    private static void AudioLinkAvatarPrefabSpawn() //AudioLink Spawner for PostInstallation
    {
        spawnPrefabPostSetup(AudioLinkAvatarPrefab);
    }
    [MenuItem("BUDDYWORKS/Avatar Scene/Spawn AudioLink Prefab...", true)]
    private static bool ValidateAudioLinkAvatarPrefabSpawn()
    {
        return AssetDatabase.IsValidFolder("Packages/com.llealloo.audiolink") != false;
    }

    [MenuItem("BUDDYWORKS/Avatar Scene/Spawn LTCGI Prefab...", priority = 24)]
    private static void LTCGIPrefabSpawn() //LTCGI Spawner for PostInstallation
    {
        spawnPrefabPostSetup(LTCGIPrefab);
    }
    [MenuItem("BUDDYWORKS/Avatar Scene/Spawn LTCGI Prefab...", true)]
    private static bool ValidateLTCGIPrefabSpawn()
    {
        return AssetDatabase.IsValidFolder("Packages/at.pimaker.ltcgi") != false;
    }

    private static bool safetyCheck(string path) //Made to avoid calls that would overwrite data or end scripts in errors.
    {
        if (System.IO.Directory.Exists(path)) { //Checks whenever the input path exists.
            return false;
        }
        Debug.Log(logSuccess + "No existing scene data found, proceeding...");
        return true;
    }

    private static void copySceneData(bool dropSceneFile) //Copies scene files, ARG1 drops the shell .scene file on upgrades.
    {
        Debug.Log(logInfo + "Copying assets...");
        System.IO.Directory.CreateDirectory(BuddyworksPath);
        FileUtil.CopyFileOrDirectory(BuddyworksPackageID, SceneFolder);

        if(dropSceneFile) {
            System.IO.File.Delete(ScenePath);
        }

        DeleteMetaFilesRecursively(SceneFolder);
    }

    static void setupSave(bool isUpgrade) //Workarounds to REALLY make sure the scene is actually saved.
    {
        Debug.Log(logInfo + "Saving scene...");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); //Marks the scene as dirty, making sure that the scene is saved on the next SaveScene call.
        if(isUpgrade) { //Is upgrading existing scene?
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log(logSuccess + "Scene successfully upgraded!");
            return;
        }
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath, false);
        Debug.Log(logSuccess + "Avatar Scene successfully imported!");
    }
    
    private static void openBuddyworksScene() //Opens the scene. Is only called on AddBuddyworksScene().
    {
        if (System.IO.File.Exists(ScenePath)) {
            Debug.Log(logInfo + "Opening Scene...");
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene(ScenePath);
            return;
        }
        Debug.LogError(logAbort + "The scene could not be found, something went wrong.");
    }

    private static void DeleteMetaFilesRecursively(string folderPath) //Ensures metafiles are removed in the newly copied folder, avoids conflicts.
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

    private static void spawnPrefab(string guid, bool isActive, GameObject systemParent) //Contextual prefab spawning function.
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

    private static void spawnPrefabPostSetup(string guid) //Non-contextual prefab spawning function, can be called through menus.
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

    private static void setupFloorplane(GameObject systemParent) //Spawns the signature floorplane.
    {
        Material floorplaneMaterial = (Material)AssetDatabase.LoadAssetAtPath(SceneFolder + "/Materials/Floorplane_Material.mat", typeof(Material));
        Texture2D floorplaneTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(SceneFolder + "/Textures/Floorplane_Texture.png", typeof(Texture2D));
        GameObject floorplane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        floorplane.transform.SetParent(systemParent.transform);
        floorplane.transform.Rotate(90.0f, 180.0f, 0.0f, Space.Self);
        Vector3 floorplaneScale = new Vector3(2.0327f, 2.0327f, 2.0327f);
        floorplane.transform.localScale = floorplaneScale;
        floorplaneMaterial.mainTexture = floorplaneTexture;
        floorplane.GetComponent<MeshRenderer>().material = floorplaneMaterial;
        floorplane.name = "Floorplane";
    }

    private static void setupLight(GameObject systemParent, bool isActive) //Sets up a neutral directional light.
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
        scenelight.SetActive(isActive);
    }

    private static void setupCamera(GameObject systemParent, bool isActive) //Sets up a orthographic camera for the front view Game Window.
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
        orthoCamera.SetActive(isActive);
    }

    private static void setupSkybox() //Sets up the skybox of the scene.
    {
        Debug.Log(logInfo + "Setting up skybox...");
        Material skybox = (Material)AssetDatabase.LoadAssetAtPath(SceneFolder + "/Materials/Skybox_Material.mat", typeof(Material));
        Texture2D skyboxTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(SceneFolder + "/Textures/Skybox_Texture.psd", typeof(Texture2D));
        RenderSettings.skybox = skybox;

        string[] keywords = { "_FrontTex", "_BackTex", "_LeftTex", "_RightTex", "_UpTex", "_DownTex" };
        foreach (string keyword in keywords)
        {
            skybox.EnableKeyword(keyword);
            skybox.SetTexture(keyword, skyboxTexture);
        }
        Debug.Log(logSuccess + "Skybox setup finished!");
    }

    [MenuItem("BUDDYWORKS/Avatar Scene/Enable Scene Anti-Aliasing", priority = 12)]
    private static void setupPostProcessing() //Sets up a PostProcessing Layer for all Cameras in the scene that are MainCamera and not part of a prefab.
    {
        GameObject[] rootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects(); //Get all root game objects in the scene

        foreach (GameObject rootObject in rootObjects)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(rootObject)) continue; //Skips prefabbed entries.
            Camera[] cameras = rootObject.GetComponentsInChildren<Camera>(true); //Find cameras attached to the root object and its children.

            foreach (Camera camera in cameras)
            {
                //Debug.Log(logInfo + "Found: " + camera.name);
                if (camera.GetComponent<PostProcessLayer>() == null && camera.CompareTag("MainCamera")) // Add PostProcessLayer component if it doesn't exist, and Camera is MainCamera.
                {
                    PostProcessLayer postProcessLayer = camera.gameObject.AddComponent<PostProcessLayer>();
                    Debug.Log(logInfo + "Added PostProcessing to camera: " + camera.name);
                }
            }
        }
        Debug.Log(logInfo + "Post-process Layer added to all cameras in the scene.");
    }
}