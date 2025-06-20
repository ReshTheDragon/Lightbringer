using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Materials.Main_menu_with_parallax_FREE.Scripts
{
    [Serializable]
    public class GameData
    {
        public int currentSceneIndex;
        public string sceneName;

        public float playerPosX;
        public float playerPosY;
        public float playerPosZ;

        public void SetPlayerPosition(Vector3 position)
        {
            playerPosX = position.x;
            playerPosY = position.y;
            playerPosZ = position.z;
        }

        public Vector3 GetPlayerPosition()
        {
            return new Vector3(playerPosX, playerPosY, playerPosZ);
        }
    }
}
