using System.IO;
using static System.Text.Encoding;
using UnityEngine;
/// <summary>
/// セーブデータ保存クラス
/// </summary>
namespace SaveData
{
    [System.Serializable]
    public class SaveData : ISaveData
    {
        [SerializeField]
        int _stageNum;
        [SerializeField]
        int _highScore;
        string _savePath = Application.dataPath + "/playerData.json";
        //float _bgmVolume;
        //float _seVolume;

        public int StageNum => _stageNum;
        public int HighScore => _highScore;
        

        public void SetStageNum(int stageNum)
        {
            _stageNum = stageNum;
        }

        public void SetHighScore(int highScore)
        {
            _highScore = highScore;
        }

        public void Save()
        {
            string jsonStr =  JsonUtility.ToJson(this);

            //ファイルに出力
            using (StreamWriter sw = new StreamWriter(_savePath, false, UTF8))
            {
                try
                {
                    sw.Write(jsonStr);
                    sw.Flush();
                    sw.Close();//念の為明記
                }
                catch
                {
                    Debug.Log("データを保存できませんでした。");
                }
            }
        }

        public void Load()
        {
            //ファイルの読み込み
            //オブジェクトに反映
        }
    }
}
