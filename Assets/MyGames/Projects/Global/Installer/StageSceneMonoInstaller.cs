using UnityEngine;
using Zenject;
using CountDownTimer;
using EnemyPool;
using EnemyWeaponPool;
using PointItemPool;
using PlayerWeaponPool;
using SPWIP = SpPlayerWeaponInvokerPool;
using CharacterState;

namespace GlobalInstaller
{
    /// <summary>
    /// ステージシーンで使用する汎用クラスのDI設定
    /// </summary>
    public class StageSceneMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            //カウントダウン
            Container.Bind<IObservableCountDownTimer>()
                .To<ObservableCountDownTimer>().AsTransient().NonLazy();

            //----オブジェクトプール
            //エネミー
            Container.Bind<IEnemyPool>()
                .To<EnemyPool.EnemyPool>().AsSingle().NonLazy();
            //エネミーの武器
            Container.Bind<IEnemyWeaponPool>()
                .To<EnemyWeaponPool.EnemyWeaponPool>().AsSingle().NonLazy();
            //ポイントアイテム
            Container.Bind<IPointItemPool>()
                .To<PointItemPool.PointItemPool>().AsSingle().NonLazy();
            //SP武器呼び出し用
            Container.Bind<SPWIP.ISpPlayerWeaponInvokerPool>()
                .To<SPWIP.SpPlayerWeaponInvokerPool>().AsSingle().NonLazy();
            //SP武器
            Container.Bind<ISpPlayerWeaponPool>()
                .To<SpWeaponPool>().AsSingle().NonLazy();

            //----キャラクターステート
            Container.Bind<ICharacterAttackState>()
                .To<AttackState>().AsTransient().NonLazy();
            Container.Bind<ICharacterDeadState>()
                .To<DeadState>().AsTransient().NonLazy();
            Container.Bind<ICharacterDownState>()
                .To<DownState>().AsTransient().NonLazy();
            Container.Bind<ICharacterRunState>()
                .To<RunState>().AsTransient().NonLazy();
            Container.Bind<ICharacterJoyState>()
                .To<JoyState>().AsTransient().NonLazy();
            Container.Bind<ICharacterWaitState>()
                .To<WaitState>().AsTransient().NonLazy();
            Container.Bind<ICharacterTrackState>()
                .To<TrackState>().AsTransient().NonLazy();
        }
    }
}