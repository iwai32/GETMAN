using UnityEngine;
using Zenject;
using Fade;
using Dialog;
using CustomSceneManager;
using SaveDataManager;
using SoundManager;
using UIUtility;

namespace GlobalInstaller
{
    public class ProjectPrefabInstaller : MonoInstaller
    {
        [SerializeField]
        [Header("フェードのプレハブを設定")]
        Fade.Fade _fadePrefab;

        [SerializeField]
        [Header("シーンマネージャーのプレハブを設定")]
        CustomSceneManager.CustomSceneManager _customSceneManagerPrefab;

        [SerializeField]
        [Header("セーブデータマネージャーのプレハブを設定")]
        SaveDataManager.SaveDataManager _saveDataManagerPrefab;

        [SerializeField]
        [Header("サウンドマネージャーのプレハブを設定")]
        SoundManager.SoundManager _soundManagerPrefab;

        [SerializeField]
        [Header("ダイアログのプレハブを設定")]
        Dialog.Dialog _dialogPrefab;

        /// <summary>
		/// シーンの切り替えを行っても破棄しないインスタンスを生成します
		/// </summary>
        public override void InstallBindings()
        {
            Container.Bind<ICustomSceneManager>()
               .FromComponentInNewPrefab(_customSceneManagerPrefab)
               .AsSingle()
               .NonLazy();

            Container.Bind<ISaveDataManager>()
                .FromComponentInNewPrefab(_saveDataManagerPrefab)
                .AsSingle()
                .NonLazy();

            Container.Bind<ISoundManager>()
                .FromComponentInNewPrefab(_soundManagerPrefab)
                .AsSingle()
                .NonLazy();

            //ダイアログ
            Container.Bind<IDialog>()
                .FromComponentInNewPrefab(_dialogPrefab)
                .AsSingle()
                .NonLazy();

            //フェード
            Container.Bind<IFade>()
                .FromComponentInNewPrefab(_fadePrefab)
                .AsSingle()
                .NonLazy();

            //セーブデータ
            Container.Bind<ISaveData>()
                .To<SaveData>()
                .AsSingle()
                .NonLazy();

            //クリックボタン
            Container.Bind<IObservableClickButton>()
                .To<ObservableClickButton>()
                .AsSingle()
                .NonLazy();

            //UIの表示非表示処理
            Container.Bind<IToggleableUI>()
                .To<ToggleableUI>()
                .AsSingle()
                .NonLazy();
        }
    }
}