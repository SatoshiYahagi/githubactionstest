using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;

//[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(CharacterAnimation))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputDriver : NetworkBehaviour
{
    #region Types.
    public struct MoveInputData : IReplicateData
    {
        public Vector2 moveVector;
        public bool jump;
        public bool grounded;

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    public struct ReconcileData : IReconcileData
    {
        public Vector3 Position;
        public Quaternion Rotation;

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    #endregion

    #region Fields
    //private CharacterController _characterController;
    private Vector2 _moveInput;
    //  private Vector3 _moveDirection;
    private bool _isGrounded;
    private bool _jump;
    private CapsuleCollider _capsuleCollider;
    private Rigidbody _rigidbody;
    private CharacterAnimation _characterAnimation; // Animationを制御するスクリプト

    [SerializeField] public float jumpSpeed = 6f;
    [SerializeField] public float speed = 4f;
    //[SerializeField] public float gravity = -9.8f;
    public float checkGroundByRayDistance = 0.2f;

    #endregion

    private void Start()
    {
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        //_characterController = GetComponent(typeof(CharacterController)) as CharacterController;
        _capsuleCollider = GetComponent(typeof(CapsuleCollider)) as CapsuleCollider;
        _rigidbody = GetComponent(typeof(Rigidbody)) as Rigidbody;
        _characterAnimation = GetComponent(typeof(CharacterAnimation)) as CharacterAnimation;
        _jump = false;   
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    private void OnDestroy()
    {
        if (InstanceFinder.TimeManager != null)
            InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
    }

    /*private void Update()
    {
        if (!base.IsOwner)
            return;
        if (_characterController.isGrounded)
        {
            _moveDirection = new Vector3(_moveInput.x, 0.0f, _moveInput.y);
            _moveDirection *= speed;

            if (_jump)
            {
                _moveDirection.y = jumpSpeed;
                _jump = false;
            }
        }
        _moveDirection.y += gravity * Time.deltaTime;
        _characterController.Move(_moveDirection * Time.deltaTime);
    }*/

    #region Movement Processing
    private void GetInputData(out MoveInputData moveData)
    {
        moveData = new MoveInputData
        {
            jump = _jump,
            grounded = _isGrounded,
            moveVector = _moveInput
        };
    }
    private void TimeManager_OnTick()
    {
        if (base.IsOwner)
        {
            Reconciliation(default, false);
            GetInputData(out MoveInputData md);
            Move(md, false);
        }

        if (base.IsServer)
        {
            Move(default, true);
            ReconcileData rd = new ReconcileData {
                Position = transform.position,
                Rotation = transform.rotation
            };
            Reconciliation(rd, true);
        }
        //Debug.Log("IsSever:" + base.IsServer);
    }

    /// <summary>
    /// 地面に垂直なカプセルコライダーの底から出すRayに何かが当たっていたら_isGroundedをtrueにする
    /// </summary>
    private void CheckGroundByRay()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        _isGrounded = Physics.Raycast(ray, checkGroundByRayDistance + _capsuleCollider.height / 2);

    }
    #endregion

    #region Prediction Methods
    [Replicate]
    private void Move(MoveInputData md, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
    {
        Vector3 move = new Vector3();

        // Rigidbody版
        move.x = md.moveVector.x;
        move.y = Physics.gravity.y;
        move.z = md.moveVector.y;
        // AddForceが適切かは要検討
        _rigidbody.AddForce(move * speed * (float)base.TimeManager.TickDelta);

        //
        if (md.jump)
        {
            _characterAnimation.Jump();
        }
        else
        {
            _characterAnimation.Grounded();
        }
        // CharcterContoroller版
        //if (md.grounded)
        //{
        //    move.x = md.moveVector.x;
        //    //move.y = gravity;
        //    move.z = md.moveVector.y;
        //    if (md.jump)
        //    {
        //        move.y = jumpSpeed;
        //        _characterAnimation.Jump();
        //    }
        //    else
        //    {
        //        _characterAnimation.Grounded();
        //    }
        //}
        //else
        //{
        //    move.x = md.moveVector.x;
        //    move.z = md.moveVector.y;
        //}
        //move.y += gravity * (float)base.TimeManager.TickDelta; // gravity is negative...
        //_characterController.Move(move * speed * (float)base.TimeManager.TickDelta);

        Debug.Log("Sever:" + asServer + " MoveData:" + move);
    }

    [Reconcile]
    private void Reconciliation(ReconcileData rd, bool asServer, Channel channel = Channel.Unreliable)
    {

        //Debug.Log("Sever:" + asServer + " ReconsileData:" + rd);

        transform.position = rd.Position;
        transform.rotation = rd.Rotation;
    }

    #endregion

    #region UnityEventCallbacks
    public void OnMovement(InputAction.CallbackContext context)
    {
        if (!base.IsOwner)
            return;
        _moveInput = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!base.IsOwner)
            return;
        if (context.started || context.performed)
        {
            //　アクションとの相互作用を開始する入力を受け取った、または相互作用が完了した場合
            _jump = true;
        }
        else if (context.canceled)
        {
            // アクションがキャンセルされた場合
            _jump = false;
        }
    }

    #endregion
}