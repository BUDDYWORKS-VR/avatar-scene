using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;

public class BuddyworksScene : MonoBehaviour
{

    [MenuItem("BUDDYWORKS/Avatar Scene/Add Scene Data")]
    static void CopyBuddyworksScene()
    {
        
        //I like fancy formatting, but I dont enjoy typing it all the time. Lets not do that.
        string logHeader = "<color=grey>[</color><color=#FDDA0D>BUDDYWORKS</color><color=grey>] </color>";
        string logAbort = "<color=red>ERROR:</color> ";
        string logInfo = "<color=white>Log:</color> ";
        string logSuccess = "<color=green>OK:</color> ";

        Debug.Log(logHeader + logInfo + "Starting safety check...");

        //Checks whenever the scene already exists and aborts execution if it is.
        if (System.IO.File.Exists("Assets/BUDDYWORKS/Avatar Scene/Avatar Scene.unity"))
        {
            Debug.Log(logHeader + logAbort + "You already have the scene in your project! The sequence was aborted, no files were changed.");
            return;
        }
        Debug.Log(logHeader + logSuccess + "No existing scene data found, proceeding...");

        //Copies the scene files over.
        Debug.Log(logHeader + logInfo + "Copying assets...");
        System.IO.Directory.CreateDirectory("Assets/BUDDYWORKS");
        FileUtil.CopyFileOrDirectory("Packages/wtf.buddyworks.scene/Avatar Scene", "Assets/BUDDYWORKS/Avatar Scene");
        //Clean up .meta files in target.
        string clearMetaPath = "Assets/BUDDYWORKS/Avatar Scene";
        DeleteMetaFilesRecursively(clearMetaPath);
        AssetDatabase.Refresh();

        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene("Assets/BUDDYWORKS/Avatar Scene/Avatar Scene.unity");

        //Configure the scene.
        Debug.Log(logHeader + logInfo + "Setting up scene...");

        //Adjust Skybox
        Debug.Log(logHeader + logInfo + "Setting up skybox...");
        Material skybox = (Material)AssetDatabase.LoadAssetAtPath("Assets/BUDDYWORKS/Avatar Scene/Materials/Skybox_Material.mat", typeof(Material));
        Texture2D skyboxTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/BUDDYWORKS/Avatar Scene/Textures/Skybox_Texture.psd", typeof(Texture2D));
        RenderSettings.skybox = skybox;

        skybox.EnableKeyword ("_FrontTex");
        skybox.EnableKeyword ("_BackTex");
        skybox.EnableKeyword ("_LeftTex");
        skybox.EnableKeyword ("_RightTex");
        skybox.EnableKeyword ("_UpTex");
        skybox.EnableKeyword ("_DownTex");

        skybox.SetTexture("_FrontTex", skyboxTexture);
        skybox.SetTexture("_BackTex", skyboxTexture);
        skybox.SetTexture("_LeftTex", skyboxTexture);
        skybox.SetTexture("_RightTex", skyboxTexture);
        skybox.SetTexture("_UpTex", skyboxTexture);
        skybox.SetTexture("_DownTex", skyboxTexture);
        Debug.Log(logHeader + logSuccess + "Skybox setup finished!");

        //Creates the _System parent object.
        Debug.Log(logHeader + logInfo + "Setting up GameObjects and Prefabs...");
        GameObject systemparent = new GameObject();
        systemparent.name = "_System";

        //Loads Assets for the Floorplane and sets it up in the scene
        Material floorplaneMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/BUDDYWORKS/Avatar Scene/Materials/Floorplane_Material.mat", typeof(Material));
        Texture2D floorplaneTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/BUDDYWORKS/Avatar Scene/Textures/Floorplane_Texture.png", typeof(Texture2D));
        GameObject floorplane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        floorplane.transform.SetParent(systemparent.transform);
        floorplane.transform.Rotate(90.0f, 180.0f, 0.0f, Space.Self);
        Vector3 floorplaneScale = new Vector3(2.0327f, 2.0327f, 2.0327f);
        floorplane.transform.localScale = floorplaneScale;
        floorplaneMaterial.mainTexture = floorplaneTexture;
        floorplane.GetComponent<MeshRenderer>().material = floorplaneMaterial;
        floorplane.name = "Floorplane";

        //Creates a directional light
        GameObject scenelight = new GameObject("Directional Light");
        Light lightComp = scenelight.AddComponent<Light>();
        lightComp.color = Color.white;
        lightComp.type = LightType.Directional;
        lightComp.shadows = LightShadows.Soft;
        lightComp.shadowStrength = 0.443f;
        lightComp.intensity = 0.72f;
        scenelight.transform.Rotate(50.0f, -200.0f, 0.0f, Space.Self);
        scenelight.transform.SetParent(systemparent.transform);

        //Create the Orthographic Camera
        GameObject orthoCamera = new GameObject("Main Camera");
        Camera cameraComponent = orthoCamera.AddComponent<Camera>();
        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = 0.8f;
        orthoCamera.transform.position = new Vector3(0f, 0.72f, 3.26f);
        orthoCamera.transform.rotation = Quaternion.Euler(0f, -180f, 0f);
        orthoCamera.transform.SetParent(systemparent.transform);
        orthoCamera.AddComponent<AudioListener>();

        //Instantiate some prefabs, easy enough eh?

        //AudioLink
        string AudioLinkAvatarPrefab = "6e8e0ee5a3655884ea49447ae9e6e665";
        string AudioLinkAvatarPrefabPath = AssetDatabase.GUIDToAssetPath(AudioLinkAvatarPrefab);
        GameObject AudioLinkprefab = AssetDatabase.LoadAssetAtPath<GameObject>(AudioLinkAvatarPrefabPath);
        GameObject AudioLinkInstantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(AudioLinkprefab, systemparent.transform);
        AudioLinkprefab.SetActive(false);

        //LTCGI
        string LTCGIPrefabGUID = "14b5e322766ebe0469a21d9898d446d9";
        string LTCGIPrefabPath = AssetDatabase.GUIDToAssetPath(LTCGIPrefabGUID);
        GameObject LTCGIprefab = AssetDatabase.LoadAssetAtPath<GameObject>(LTCGIPrefabPath);
        GameObject LTCGIInstantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(LTCGIprefab, systemparent.transform);
        LTCGIprefab.SetActive(false);
 
        Debug.Log(logHeader + logSuccess + "Scene setup finished!");

        Debug.Log(logHeader + logInfo + "Saving scene...");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/BUDDYWORKS/Avatar Scene/Avatar Scene.unity", false);
        Debug.Log(logHeader + logSuccess + "Avatar Scene successfully imported!");
        return;
    }

    [MenuItem("BUDDYWORKS/Avatar Scene/Open Scene...")]
    static void OpenBuddyworksScene()
    {
        string logHeader = "<color=grey>[</color><color=#FDDA0D>BUDDYWORKS</color><color=grey>] </color>";
        string logInfo = "<color=white>Log:</color> ";
        Debug.Log(logHeader + logInfo + "Opening Scene...");
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene("Assets/BUDDYWORKS/Avatar Scene/Avatar Scene.unity");
        return;
    }
    
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
    }
}