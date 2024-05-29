using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

namespace Buddyworks.Scene 
{

    public class Compositor : MonoBehaviour
    {
        //I like fancy formatting, but I dont enjoy typing it all the time. Lets not do that.
        public static string logHeader = "<color=grey>[</color><color=#FDDA0D>BUDDYWORKS</color><color=grey>] </color>";
        public static string logAbort = logHeader + "<color=red>ERROR:</color> ";
        public static string logInfo = logHeader + "<color=white>Log:</color> ";
        public static string logSuccess = logHeader + "<color=green>OK:</color> ";

        //Relevant variables, writing the same paths all the time is eh...
        static string BuddyworksPath = "Assets/BUDDYWORKS";
        public static string SceneFolder = BuddyworksPath + "/Avatar Scene";
        public static string ScenePath = BuddyworksPath + "/Avatar Scene/Avatar Scene.unity";
        static string BuddyworksPackageID = "Packages/wtf.buddyworks.scene/Avatar Scene";

        //Prefab GUIDs
        public static string AudioLinkAvatarPrefab = "6e8e0ee5a3655884ea49447ae9e6e665";
        public static string LTCGIPrefab = "14b5e322766ebe0469a21d9898d446d9";
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
                Setup.OpenScene(); //Opens the newly created scene.

                //Configure the scene.
                Debug.Log(logInfo + "Setting up scene...");
                Setup.Skybox(); //Adjust Skybox

                //Creates the _System parent object.
                GameObject systemParent = new GameObject { name = "_System" };

                Setup.Floorplane(systemParent); //Loads Assets for the Floorplane and sets it up in the scene
                Setup.Light(systemParent, true); //Creates a directional light. Arg1 parent object, Arg2 whenever its active or not.
                Setup.Camera(systemParent, true); //Create the Orthographic Camera. Arg1 parent object, Arg2 whenever its active or not.
                Setup.PostProcessing(); //Adds a PostProcessing Layer to the Scene's camera for Anti Aliasing

                //Instantiates external prefabs. Arg1 = PrefabGUID, Arg2 = SetActive state, Arg3 = Parent.
                Spawn.Prefab(AudioLinkAvatarPrefab, false, systemParent);
                Spawn.Prefab(LTCGIPrefab, false, systemParent);
                Spawn.Prefab(GestureManagerPrefab, true, systemParent);
     
                Debug.Log(logSuccess + "Scene setup finished!");

                Setup.Save(false); //isUpgrade false.
                return;
            }
            Debug.Log(logAbort + "You already have the scene in your project! The sequence was aborted, no files were changed.");
        }

        [MenuItem("BUDDYWORKS/Avatar Scene/Upgrade Current Scene", priority = 0)]
        static void UpgradeBuddyworksScene() //Similiar to AddBuddyworksScene(), but upgrades the current scene instead of making a new one.
        {
            //Spawn. a dialog box, asking for confirmation.
            if(EditorUtility.DisplayDialog(upgradeWindowTitle, upgradeWindowContent, "Upgrade", "Abort")) {
                //Runs when "Upgrade" is selected.
                Debug.Log(logInfo + "Starting upgrade...");
                if (safetyCheck(SceneFolder)) { 
                    copySceneData(true); //Copies the scene files over. dropSceneFile true
                }

                EditorSceneManager.SaveOpenScenes();
                Debug.Log(logInfo + "Setting up scene...");
                Setup.Skybox(); //Adjust Skybox

                if (GameObject.Find("_System") is GameObject systemCheck) {systemCheck.SetActive(false);} //Disables existing _System GameObject, for when upgrade is applied to already upgraded scene.

                //Creates the _System parent object.
                GameObject systemParent = new GameObject { name = "_System" };
                systemParent.transform.SetSiblingIndex(0);

                Setup.Floorplane(systemParent); //Loads Assets for the Floorplane and sets it up in the scene
                Setup.Light(systemParent, false); //Creates a directional light. Arg1 parent object, Arg2 whenever its active or not.
                Setup.Camera(systemParent, false); //Create the Orthographic Camera. Arg1 parent object, Arg2 whenever its active or not.
                Setup.PostProcessing(); //Adds a PostProcessing Layer to the Scene's camera for Anti Aliasing

                //Instantiates external prefabs. Arg1 = PrefabGUID, Arg2 = SetActive state, Arg3 = Parent.
                Spawn.Prefab(AudioLinkAvatarPrefab, false, systemParent);
                Spawn.Prefab(LTCGIPrefab, false, systemParent);
                Spawn.Prefab(GestureManagerPrefab, true, systemParent);

                Debug.Log(logSuccess + "Scene setup finished!");
                Setup.Save(true); //isUpgrade true
                EditorUtility.DisplayDialog(upgradeWindowTitle, upgradeWindowContentCompleted, "Done");
                return;
            }
            //Runs when "Abort" is selected, or the window is closed.
            Debug.Log(logInfo + "Upgrade aborted, no files have been changed.");
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

            Setup.MetaCleanup(SceneFolder);
        }
    }
}