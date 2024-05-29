using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

namespace Buddyworks.Scene 
{

    public class Setup : MonoBehaviour
    {

        static string logHeader = Compositor.logHeader;
        static string logAbort = Compositor.logAbort;
        static string logInfo = Compositor.logInfo;
        static string logSuccess = Compositor.logSuccess;

        static string SceneFolder = Compositor.SceneFolder;
        static string ScenePath = Compositor.ScenePath;


        public static void Floorplane(GameObject systemParent) //Spawns the signature floorplane.
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

        public static void Light(GameObject systemParent, bool isActive) //Sets up a neutral directional light.
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

        public static void Camera(GameObject systemParent, bool isActive) //Sets up a orthographic camera for the front view Game Window.
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

        public static void Skybox() //Sets up the skybox of the scene.
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

        public static void OpenScene() //Opens the scene. Is only called on AddBuddyworksScene().
        {
            if (System.IO.File.Exists(ScenePath)) {
                Debug.Log(logInfo + "Opening Scene...");
                EditorSceneManager.SaveOpenScenes();
                EditorSceneManager.OpenScene(ScenePath);
                return;
            }
            Debug.LogError(logAbort + "The scene could not be found, something went wrong.");
        }

        [MenuItem("BUDDYWORKS/Avatar Scene/Enable Scene Anti-Aliasing", priority = 12)]
        public static void PostProcessing() //Sets up a PostProcessing Layer for all Cameras in the scene that are MainCamera and not part of a prefab.
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

        public static void MetaCleanup(string folderPath) //Ensures metafiles are removed in the newly copied folder, avoids conflicts.
        {
            string[] files = Directory.GetFiles(folderPath, "*.meta");
            foreach (string file in files)
            {
                File.Delete(file);
            }

            string[] subDirectories = Directory.GetDirectories(folderPath);
            foreach (string directory in subDirectories)
            {
                MetaCleanup(directory);
            }
            AssetDatabase.Refresh();
        }

        public static void Save(bool isUpgrade) //Workarounds to REALLY make sure the scene is actually saved.
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
    }
}