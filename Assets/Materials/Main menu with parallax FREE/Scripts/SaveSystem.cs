using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;

namespace Assets.Materials.Main_menu_with_parallax_FREE.Scripts
{
    public class SaveSystem
    {
        private static string savePath = Application.persistentDataPath + "/savegame.json";

        public static void SaveGame(int sceneIndex, string sceneName, Vector3 playerPosition)
        {
            GameData data = new GameData
            {
                currentSceneIndex = sceneIndex,
                sceneName = sceneName
            };
            data.SetPlayerPosition(playerPosition);

            string json = JsonUtility.ToJson(data);
            File.WriteAllText(savePath, json);
        }

        public static GameData LoadGame()
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                return JsonUtility.FromJson<GameData>(json);
            }
            return null;
        }

        public static bool HasSave()
        {
            return File.Exists(savePath);
        }

        public static void DeleteSave()
        {
            if (File.Exists(savePath))
                File.Delete(savePath);
        }
    }
}
