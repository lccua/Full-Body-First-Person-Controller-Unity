using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    #if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
    #endif
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Player")]
        public float MoveSpeed = 4.0f;
        public float SprintSpeed = 6.0f;
        public float RotationSpeed = 1.0f;
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        public float Gravity = -15.0f;
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.5f;
        public LayerMask GroundLayers;

        [Header("Player Look")]
        public float mouseSensitivity = 100f;
        public Transform cameraTransform;

        [Header("Animator")]
        private Animator _playerAnimator;
		public float dampTime = 0.1f; 

        [Header("Footsteps")]
		public AudioClip[] FootstepAudioClips;
		[Range(0, 1)] public float FootstepAudioVolume = 0.5f;




        private float _speed;
        private Vector3 _velocity;
        private float _xRotation = 0f;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private float _fallTimeoutDelta;

        #if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
        #endif
        private CharacterController _controller;
        private StarterAssetsInputs _input;

        private const float _threshold = 0.01f;

        private bool IsCurrentDeviceMouse
        {
            get
            {
                #if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
                #else
                return false;
                #endif
            }
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
			_playerAnimator = GetComponent<Animator>();

            #if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
            #else
            Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
            #endif

            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            GravityCheck();
            GroundedCheck();
            Move();
            HandleAnimations();
        }

        private void LateUpdate()
        {
            HandleLook();
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        private void HandleAnimations()
        {
            // Calculate velocity components for the blend tree
            float velocityX = _input.move.x * _speed / MoveSpeed;
            float velocityY = _input.move.y * _speed / MoveSpeed;



            // Pass these values to the Animator
			_playerAnimator.SetFloat("VelocityZ", velocityX, dampTime, Time.deltaTime);
			_playerAnimator.SetFloat("VelocityY", velocityY, dampTime, Time.deltaTime);


        }

        private void HandleLook()
        {
            // Get mouse input for looking around
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Horizontal rotation (player body)
            transform.Rotate(Vector3.up * mouseX);

            // Vertical rotation (camera)
            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f); // Limit look up/down angle

            cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        }
        private void Move()
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            if (_input.move == Vector2.zero)
            {
                targetSpeed = 0.0f;
            }

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero)
            {
                inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
            }

            _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        private void GravityCheck()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

		private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }
    }
}
