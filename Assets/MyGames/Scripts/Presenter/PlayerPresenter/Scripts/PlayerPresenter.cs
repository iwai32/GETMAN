using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;
using Zenject;
using PlayerModel;
using PlayerView;
using StateView;
using TriggerView;
using static StateType;

namespace PlayerPresenter
{
    public class PlayerPresenter : MonoBehaviour
    {
        #region//インスペクターから設定
        [SerializeField]
        [Header("プレイヤーの初期hpを設定")]
        int _initialHp = 3;

        [SerializeField]
        [Header("プレイヤーの武器の攻撃力を設定")]
        int _initialPower = 0;

        [SerializeField]
        [Header("プレイヤーの移動速度")]
        float _speed = 10.0f;

        [SerializeField]
        [Header("プレイヤーの点滅時間")]
        float _blinkTime = 3.0f;

        [SerializeField]
        [Header("ノックバック時の飛ぶ威力")]
        float _knockBackPower = 10.0f;

        [SerializeField]
        [Header("武器アイコンのUIを設定")]
        WeaponView _weaponView;

        [SerializeField]
        [Header("HPのUIを設定")]
        HpView _hpView;
        #endregion

        #region//フィールド
        ActionView _actionView;//プレイヤーのアクション用スクリプト
        WaitView _waitView;//待機状態のスクリプト
        RunView _runView;//移動状態のスクリプト
        DownView _downView;//ダウン状態のスクリプト
        DeadView _deadView;//デッド状態のスクリプト
        AttackView _attackView;//攻撃状態のスクリプト
        TriggerView.TriggerView _triggerView;//接触判定スクリプト
        CollisionView _collisionView;//衝突判定スクリプト
        InputView _inputView;//プレイヤーの入力取得スクリプト
        Rigidbody _rigidBody;
        Animator _animator;
        ObservableStateMachineTrigger _animTrigger;
        IWeaponModel _weaponModel;
        IHpModel _hpModel;
        IStateModel _stateModel;
        bool _isBlink;//点滅状態か
        bool _canStartGame;//ゲーム開始フラグ
        BoolReactiveProperty _isGameOver = new BoolReactiveProperty();//ゲームオーバー
        #endregion

        #region//プロパティ
        public bool CanStartGame => _canStartGame;
        public IReadOnlyReactiveProperty<bool> IsGameOver => _isGameOver;
        #endregion

        [Inject]
        public void Construct(
            IWeaponModel weapon,
            IHpModel hp,
            IStateModel state
        )
        {
            _weaponModel = weapon;
            _hpModel = hp;
            _stateModel = state;//todo不要？
        }

        /// <summary>
        /// プレハブのインスタンス直後の処理
        /// </summary>
        public void ManualAwake()
        {
            _actionView = GetComponent<ActionView>();
            _waitView = GetComponent<WaitView>();
            _runView = GetComponent<RunView>();
            _downView = GetComponent<DownView>();
            _deadView = GetComponent<DeadView>();
            _attackView = GetComponent<AttackView>();
            _triggerView = GetComponent<TriggerView.TriggerView>();
            _collisionView = GetComponent<CollisionView>();
            _inputView = GetComponent<InputView>();
            _rigidBody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _animTrigger = _animator.GetBehaviour<ObservableStateMachineTrigger>();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize()
        {
            InitializeModel();
            InitializeView();
            Bind();
        }

        /// <summary>
        /// モデルの初期化を行います
        /// </summary>
        void InitializeModel()
        {
            _weaponModel.SetPower(_initialPower);
            _hpModel.SetHp(_initialHp);
        }

        /// <summary>
        /// ビューの初期化を行います
        /// </summary>
        void InitializeView()
        {
            _runView.DelAction = Run;
            _actionView.State.Value = _waitView;
        }

        /// <summary>
        /// リセットします
        /// </summary>
        public void ResetData()
        {
            _actionView.State.Value = _waitView;
            _canStartGame = false;
            _isGameOver.Value = false;
            InitializeModel();
        }

        /// <summary>
        /// modelとviewの監視、処理
        /// </summary>
        void Bind()
        {
            //modelの監視
            _hpModel.Hp.Subscribe(hp => _hpView.SetHpGauge(hp));
            //_scoreModel.Score.Subscribe(score => CheckScore(score));
            //_pointModel.Point.Subscribe(point => _pointView.SetPointGauge(point));

            //trigger, collisionの取得
            _triggerView.OnTrigger()
                .Where(_ => CanGame())
                .Subscribe(collider => CheckCollider(collider));

            _collisionView.OnCollision()
                .Where(_ => CanGame())
                .Subscribe(collision => CheckCollision(collision));

            //viewの監視
            //状態の監視
            _actionView.State
                .Where(x => x != null)
                .Subscribe(x => {
                    _actionView.ChangeState(x.State);
            });

            //入力の監視
            _inputView.InputDirection
                .Where(_ => IsControllableState())
                .Subscribe(input => ChangeStateByInput(input));

            //攻撃入力
            _inputView.IsFired
                .Where(x => (x == true)
                && CanGame()
                && IsControllableState())
                .Subscribe(_ => ChangeAttack());

            //アニメーションの監視
            _animTrigger.OnStateUpdateAsObservable()
                .Where(s => s.StateInfo.IsName("Attack")
                || s.StateInfo.IsName("Down"))
                .Where(s => s.StateInfo.normalizedTime >= 1)
                .Subscribe(_ =>
                {
                    _actionView.State.Value = _waitView;
                })
                .AddTo(this);
        }

        /// <summary>
        /// ゲーム開始フラグの設定
        /// </summary>
        /// <param name="can"></param>
        public void SetCanStartGame(bool can)
        {
            _canStartGame = can;
        }

        /// <summary>
        /// ゲームオーバーフラグの設定
        /// </summary>
        public void SetIsGameOver(bool isGameOver)
        {
            _isGameOver.Value = isGameOver;
        }

        /// <summary>
        /// 操作可能な状態か
        /// </summary>
        bool IsControllableState()
        {
            return (_actionView.State.Value.State == RUN
                || _actionView.State.Value.State == WAIT);
        }

        /// <summary>
        /// ゲームができるか
        /// </summary>
        /// <returns></returns>
        bool CanGame()
        {
            return (_canStartGame && _isGameOver.Value == false);
        }

        /// <summary>
        /// fixedUpdate処理
        /// </summary>
        public void ManualFixedUpdate()
        {
            if (_canStartGame == false) return;
            _actionView.Action();
        }

        /// <summary>
        /// 接触したコライダーを確認します
        /// </summary>
        /// <param name="collider"></param>
        void CheckCollider(Collider collider)
        {
            TryGetPointItem(collider);
            //TryReceiveDamage(collider);todo ダメージ床用

            //プレイヤーがエネミーの接触し、攻撃中でhitしたとき
        }

        /// <summary>
        /// 衝突を確認します
        /// </summary>
        void CheckCollision(Collision collision)
        {
            TryCheckEnemyCollision(collision);
        }

        /// <summary>
        /// 敵の接触の確認を試みます
        /// </summary>
        void TryCheckEnemyCollision(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out IEnemy enemy))
            {
                TryReceiveDamage(collision.collider);
            }
        }

        /// <summary>
        /// ポイントアイテムの取得を試みます
        /// </summary>
        void TryGetPointItem(Collider collider)
        {
            if (collider.TryGetComponent(out IPointItem pointItem))
            {
                //_pointModel.AddPoint(pointItem.Point);
                //_scoreModel.AddScore(pointItem.Score);
                pointItem.Destroy();
            }
        }

        /// <summary>
        /// ダメージを受けるか確認します
        /// </summary>
        void TryReceiveDamage(Collider collider)
        {
            if (_isBlink) return;
            if (collider.TryGetComponent(out IAttacker attacker))
            {
                _hpModel.ReduceHp(attacker.Power);
                ChangeStateByDamage();
                KnockBack(collider?.gameObject);
            }
        }

        /// <summary>
        /// 入力の有無でプレイヤーの状態を切り替えます
        /// </summary>
        /// <param name="input"></param>
        void ChangeStateByInput(Vector2 input)
        {
            if (input.magnitude != 0)
                _actionView.State.Value = _runView;
            else
                _actionView.State.Value = _waitView;
        }

        /// <summary>
        /// 攻撃状態に切り替えます
        /// </summary>
        void ChangeAttack()
        {
            //todo 武器があれば攻撃する

            _actionView.State.Value = _attackView;
        }

        /// <summary>
        /// ダメージによってプレイヤーの状態を切り替えます
        /// </summary>
        void ChangeStateByDamage()
        {
            if (_hpModel.Hp.Value > 0)
                ChangeDown();
            else ChangeDead();
        }

        void ChangeDown()
        {
            _actionView.State.Value = _downView;
            //点滅処理
            PlayerBlinks().Forget();
        }

        public void ChangeDead()
        {
            _actionView.State.Value = _deadView;
            _isGameOver.Value = true;
        }

        /// <summary>
        /// ノックバックします
        /// </summary>
        void KnockBack(GameObject target)
        {
            //ノックバック方向を取得
            Vector3 knockBackDirection = (transform.position - target.transform.position).normalized;

            //速度ベクトルをリセット
            _rigidBody.velocity = Vector3.zero;
            _rigidBody.AddForce(knockBackDirection * _knockBackPower, ForceMode.VelocityChange);
        }

        /// <summary>
        /// プレイヤーの点滅
        /// </summary>
        async UniTask PlayerBlinks()
        {
            bool isActive = false;
            float elapsedBlinkTime = 0.0f;

            _isBlink = true;
            while (elapsedBlinkTime <= _blinkTime)
            {
                SetActiveToAllChild(isActive);
                isActive = !isActive;
                await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
                elapsedBlinkTime += 0.2f;
            }

            SetActiveToAllChild(true);
            _isBlink = false;
        }

        /// <summary>
        /// 子要素を全てアクティブ・非アクティブにする
        /// </summary>
        /// <param name="isActive"></param>
        void SetActiveToAllChild(bool isActive)
        {
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetActive(isActive);
            }
        }

        /// <summary>
        /// 走ります
        /// </summary>
        void Run()
        {
            Vector2 input = _inputView.InputDirection.Value;
            Move(input);
            Rotation(input);
        }

        /// <summary>
        /// 移動します
        /// </summary>
        /// <param name="input"></param>
        void Move(Vector2 input)
        {
            //入力があった場合
            if (input != Vector2.zero)
            {
                Vector3 movePos = new Vector3(input.x, 0, input.y);
                _rigidBody.velocity = movePos * _speed;
            }
        }

        /// <summary>
        /// 回転します
        /// </summary>
        /// <param name="input"></param>
        void Rotation(Vector2 input)
        {
            _rigidBody.rotation = Quaternion.LookRotation(new Vector3(input.x, 0, input.y));
        }
    }
}