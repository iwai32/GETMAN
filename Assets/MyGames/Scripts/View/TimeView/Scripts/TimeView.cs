using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TimeView
{
    public class TimeView : MonoBehaviour
    {
        [SerializeField]
        [Header("timer表示用テキストを設定")]
        TextMeshProUGUI _timeText;

        /// <summary>
        /// timeをテキストに設定します
        /// </summary>
        public void SetTimeText(int time)
        {
            _timeText.text = time.ToString();
        }
    }
}
