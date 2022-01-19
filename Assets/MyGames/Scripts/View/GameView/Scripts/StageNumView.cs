using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameView
{
    public class StageNumView : MonoBehaviour
    {
        [SerializeField]
        [Header("ステージ番号表示用テキストを設定")]
        Text stageNumText;

        /// <summary>
        /// ステージ番号の更新
        /// </summary>
        public void SetStageNum(int stageNum)
        {
            stageNumText.text = stageNum.ToString();
        }
    }
}