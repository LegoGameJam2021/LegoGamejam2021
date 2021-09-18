using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace LEGOMinifig
{

    public class MinifigController : MonoBehaviour
    {
        // Constants.
        const float stickyTime = 0.05f;
        const float stickyForce = 9.6f;
        const float coyoteDelay = 0.1f;

        const float distanceEpsilon = 0.1f;
        const float angleEpsilon = 0.1f;

        // Input type for controlling the minifig.
        enum InputType
        {
            Tank,
            Direct
        }

        // Internal classes used to define targets when automatically animating.
        class MoveTarget
        {
            public Vector3 destination;
            public float minDistance;
            public Action onComplete;
            public float onCompleteDelay;
            public float moveDelay;
            public bool cancelSpecial;
            public float speedMultiplier;
            public float rotationSpeedMultiplier;
            public Vector3? turnToWhileCompleting;
        }

        class FollowTarget
        {
            public Transform target;
            public float minDistance;
            public Action onComplete;
            public float onCompleteDelay;
            public float followDelay;
            public bool cancelSpecial;
            public float speedMultiplier;
            public float rotationSpeedMultiplier;
            public Vector3? turnToWhileCompleting;
        }

        class TurnTarget
        {
            public Vector3 target;
            public float minAngle;
            public Action onComplete;
            public float onCompleteDelay;
            public float turnDelay;
            public bool cancelSpecial;
            public float rotationSpeedMultiplier;
        }

        // State used when automatically animating.
        enum State
        {
            Idle,
            Moving,
            CompletingMove,

            Turning,
            CompletingTurn,

            Following,
            CompletingFollow,
        }

        [Header("Movement")]

        [Range(5, 30)]
        public float maxForwardSpeed = 5f;
        [Range(4, 8)]
        public float maxBackwardSpeed = 4f;
        [Range(1, 60)]
        public float acceleration = 20.0f;
        // TODO Add sensible range when animations are fixed.
        [Range(0, 500)]
        public float maxRotateSpeed = 150f;
        [Range(0, 2500)]
        public float rotateAcceleration = 600f;
        public float jumpSpeed = 20f;
        public float gravity = 40f;

        [Header("Audio")]

        public List<AudioClip> stepAudioClips = new List<AudioClip>();
        public List<AudioClip> landAudioClips = new List<AudioClip>();
        public AudioClip jumpAudioClip;
        public AudioClip doubleJumpAudioClip;
        public AudioClip explodeAudioClip;

        [Header("Controls")]
        [SerializeField]
        VirtualJoystick virtualJoystick;
        [SerializeField]
        InputType inputType = InputType.Tank;
        [SerializeField]
        bool inputEnabled = true;
        [SerializeField, Range(0, 10)]
        int maxJumpsInAir = 1;

        public enum SpecialAnimation
        {
            AirGuitar = 0,
            Ballerina = 1,
            Crawl = 2,
            CrawlOnWallLeft = 3,
            CrawlOnWallRight = 4,
            Crawl_Tilt_Left = 5,
            Crawl_Tilt_Right = 6,
            Dance = 7,
            Flexing = 8,
            HatSwap = 9,
            HatSwap2 = 10,
            Idle_Light = 11,
            IdleHandsOnSides = 12,
            IdleHeavy = 13,
            IdleImpatient = 14,
            Jump = 15,
            Jump_BackflipNoYAxis = 16,
            Jump_FlipNoYAxis = 17,
            Jump_GoingDown = 18,
            Jump_GoingUp = 19,
            Jump_LandingBounce = 20,
            Jump_Midair = 21,
            Jump_NoLanding_No_Y_Axis = 22,
            Jump_wBounce = 23,
            KickRightFoot = 24,
            Laughing = 25,
            LeftHandReaction = 26,
            LegReactive = 27,
            LookingAbove = 28,
            LookingAround = 29,
            LookingDown = 30,
            LookingUp = 31,
            MoonWalk = 32,
            PantsSwap = 33,
            RightHandReactive = 34,
            Run = 35,
            Showering = 36,
            Skipping = 37,
            Sleeping = 38,
            Slide = 39,
            Sneezing = 40,
            Snoozing = 41,
            Spiderman = 42,
            Spin = 43,
            Stretching = 44,
            Superman_Flying = 45,
            Superman_Setoff = 46,
            Torso_Reactive = 47,
            Turn_90_Left = 48,
            Turn_90_Right = 49,
            Walk = 50,
            WalkBackwards = 51,
            Walk_Sneaking = 52,
            Wave = 53
        }

        #region Rigging

        Rig leftArmRig;
        Rig rightArmRig;
        Rig leftLegRig;
        Rig rightLegRig;
        Rig headRig;

        Transform leftArmEffector;
        Transform leftArmRoot;
        Transform rightArmEffector;
        Transform rightArmRoot;
        Transform leftLegEffector;
        Transform leftLegRoot;
        Transform rightLegEffector;
        Transform rightLegRoot;
        Transform headEffector;

        float leftHandReachWeight;
        float leftHandReachSpeed;
        float rightHandReachWeight;
        float rightHandReachSpeed;
        float leftFootReachWeight;
        float leftFootReachSpeed;
        float rightFootReachWeight;
        float rightFootReachSpeed;
        float headLookAtWeight;
        float headLookAtSpeed;

        bool leftHandReaching;
        bool rightHandReaching;
        bool leftFootReaching;
        bool rightFootReaching;
        bool headLooking;

        AnimationCurve rigEasing = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

        #endregion

        Minifig minifig;
        CharacterController controller;
        Animator animator;
        AudioSource audioSource;

        bool airborne;
        float airborneTime;
        int jumpsInAir;
        Vector3 directSpeed;
        bool exploded;
        bool stepped;

        List<MoveTarget> moves = new List<MoveTarget>();
        MoveTarget currentMove;
        FollowTarget currentFollowTarget;
        TurnTarget currentTurnTarget;
        State state;
        float waitedTime = 0.0f;

        float speed;
        float rotateSpeed;
        Vector3 moveDelta = Vector3.zero;
        bool stopSpecial;
        bool cancelSpecial;

        float externalRotation;
        Vector3 externalMotion;

        Transform groundedTransform;
        Vector3 groundedLocalPosition;
        Vector3 oldGroundedPosition;
        Quaternion oldGroundedRotation;

        int speedHash = Animator.StringToHash("Speed");
        int rotateSpeedHash = Animator.StringToHash("Rotate Speed");
        int groundedHash = Animator.StringToHash("Grounded");
        int jumpHash = Animator.StringToHash("Jump");
        int playSpecialHash = Animator.StringToHash("Play Special");
        int cancelSpecialHash = Animator.StringToHash("Cancel Special");
        int specialIdHash = Animator.StringToHash("Special Id");

        Action<bool> onSpecialComplete;

        void Awake()
        {
            minifig = GetComponent<Minifig>();
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();

            // Setup rigs.
            leftArmRig = transform.Find("Left Arm Rig").GetComponent<Rig>();
            rightArmRig = transform.Find("Right Arm Rig").GetComponent<Rig>();
            leftLegRig = transform.Find("Left Leg Rig").GetComponent<Rig>();
            rightLegRig = transform.Find("Right Leg Rig").GetComponent<Rig>();
            headRig = transform.Find("Head Rig").GetComponent<Rig>();

            SetupRig(leftArmRig, ref leftArmEffector, ref leftArmRoot);
            SetupRig(rightArmRig, ref rightArmEffector, ref rightArmRoot);
            SetupRig(leftLegRig, ref leftLegEffector, ref leftLegRoot);
            SetupRig(rightLegRig, ref rightLegEffector, ref rightLegRoot);
            headEffector = headRig.transform.GetChild(0).GetChild(0);

            // Initialise animation.
            animator.SetBool(groundedHash, true);

            // Make sure the Character Controller is grounded if starting on the ground.
            controller.Move(Vector3.down * 0.01f);
        }

        void Update()
        {
            if (exploded)
            {
                return;
            }

            // Handle input.
            if (inputEnabled)
            {
                switch (inputType)
                {
                    case InputType.Tank:
                        {
                            // Calculate speed.
                            var targetSpeed = GetAxisInput("Vertical");
                            targetSpeed *= targetSpeed > 0 ? maxForwardSpeed : maxBackwardSpeed;
                            if (targetSpeed > speed)
                            {
                                speed = Mathf.Min(targetSpeed, speed + acceleration * Time.deltaTime);
                            }
                            else if (targetSpeed < speed)
                            {
                                speed = Mathf.Max(targetSpeed, speed - acceleration * Time.deltaTime);
                            }

                            // Calculate rotation speed.
                            var targetRotateSpeed = GetAxisInput("Horizontal");
                            targetRotateSpeed *= maxRotateSpeed;
                            if (targetRotateSpeed > rotateSpeed)
                            {
                                rotateSpeed = Mathf.Min(targetRotateSpeed, rotateSpeed + rotateAcceleration * Time.deltaTime);
                            }
                            else if (targetRotateSpeed < rotateSpeed)
                            {
                                rotateSpeed = Mathf.Max(targetRotateSpeed, rotateSpeed - rotateAcceleration * Time.deltaTime);
                            }

                            // Calculate move delta.
                            moveDelta = new Vector3(0, moveDelta.y, speed);
                            moveDelta = transform.TransformDirection(moveDelta);
                            break;
                        }
                    case InputType.Direct:
                        {
                            // Calculate direct speed and speed.
                            var right = Vector3.right;
                            var forward = Vector3.forward;
                            if (Camera.main)
                            {
                                right = Camera.main.transform.right;
                                right.y = 0.0f;
                                right.Normalize();
                                forward = Camera.main.transform.forward;
                                forward.y = 0.0f;
                                forward.Normalize();
                            }

                            var targetSpeed = right * GetAxisInput("Horizontal");
                            targetSpeed += forward * GetAxisInput("Vertical");
                            if (targetSpeed.sqrMagnitude > 0.0f)
                            {
                                targetSpeed.Normalize();
                            }
                            targetSpeed *= maxForwardSpeed;

                            var speedDiff = targetSpeed - directSpeed;
                            if (speedDiff.sqrMagnitude < acceleration * acceleration * Time.deltaTime * Time.deltaTime)
                            {
                                directSpeed = targetSpeed;
                            }
                            else if (speedDiff.sqrMagnitude > 0.0f)
                            {
                                speedDiff.Normalize();

                                directSpeed += speedDiff * acceleration * Time.deltaTime;
                            }
                            speed = directSpeed.magnitude;

                            // Calculate rotation speed - ignore rotate acceleration.
                            rotateSpeed = 0.0f;
                            if (targetSpeed.sqrMagnitude > 0.0f)
                            {
                                var localTargetSpeed = transform.InverseTransformDirection(targetSpeed);
                                var angleDiff = Vector3.SignedAngle(Vector3.forward, localTargetSpeed.normalized, Vector3.up);

                                if (angleDiff > 0.0f)
                                {
                                    rotateSpeed = maxRotateSpeed;
                                }
                                else if (angleDiff < 0.0f)
                                {
                                    rotateSpeed = -maxRotateSpeed;
                                }

                                // Assumes that x > NaN is false - otherwise we need to guard against Time.deltaTime being zero.
                                if (Mathf.Abs(rotateSpeed) > Mathf.Abs(angleDiff) / Time.deltaTime)
                                {
                                    rotateSpeed = angleDiff / Time.deltaTime;
                                }
                            }

                            // Calculate move delta.
                            moveDelta = new Vector3(directSpeed.x, moveDelta.y, directSpeed.z);
                            break;
                        }
                }

                // Check if player is grounded.
                if (!airborne)
                {
                    jumpsInAir = maxJumpsInAir;
                }

                // Check if player is jumping.
                if (GetJumpInput())
                {
                    if (!airborne || jumpsInAir > 0)
                    {
                        if (airborne)
                        {
                            jumpsInAir--;

                            if (doubleJumpAudioClip)
                            {
                                audioSource.PlayOneShot(doubleJumpAudioClip);
                            }
                        }
                        else
                        {
                            if (jumpAudioClip)
                            {
                                audioSource.PlayOneShot(jumpAudioClip);
                            }
                        }

                        moveDelta.y = jumpSpeed;
                        animator.SetTrigger(jumpHash);

                        airborne = true;
                        airborneTime = coyoteDelay;
                    }
                }

                // Cancel special.
                cancelSpecial = !Mathf.Approximately(GetAxisInput("Vertical"), 0) || !Mathf.Approximately(GetAxisInput("Horizontal"), 0) || GetJumpInput();

            }
            else
            {
                // Handle automatic animation.

                waitedTime += Time.deltaTime;

                switch (state)
                {
                    case State.Idle:
                        {
                            // Stop moving.
                            MoveInDirection(transform.forward, 0.0f, false, 0.0f, 0.0f, 0.0f, false);
                            break;
                        }
                    case State.Moving:
                        {
                            if (waitedTime > currentMove.moveDelay)
                            {
                                var direction = currentMove.destination - transform.position;

                                // Neutralize y component.
                                direction.y = 0.0f;

                                if (direction.magnitude > currentMove.minDistance + distanceEpsilon)
                                {
                                    var shouldBreak = currentMove.onCompleteDelay > 0.0f || moves.Count == 1 || (moves.Count > 1 && moves[1].moveDelay > 0.0f);
                                    MoveInDirection(direction, currentMove.minDistance, shouldBreak, currentMove.speedMultiplier, 0.0f, currentMove.rotationSpeedMultiplier, currentMove.cancelSpecial);
                                }
                                else
                                {
                                    if (currentMove.onCompleteDelay > 0.0f)
                                    {
                                        SetState(State.CompletingMove);
                                    }
                                    else
                                    {
                                        CompleteMove();
                                    }
                                }
                            }
                            else
                            {
                                // Set speed, move delta and rotation speed.
                                speed = 0.0f;
                                moveDelta = new Vector3(0.0f, moveDelta.y, 0.0f);
                                rotateSpeed = 0.0f;
                            }
                        }
                        break;
                    case State.CompletingMove:
                        {
                            // Possibly turn to position.
                            if (currentMove.turnToWhileCompleting.HasValue)
                            {
                                var turnToDirection = currentMove.turnToWhileCompleting.Value - transform.position;
                                if (turnToDirection.magnitude > distanceEpsilon)
                                {
                                    TurnToDirection(turnToDirection, 0.0f, currentMove.rotationSpeedMultiplier, currentMove.cancelSpecial);
                                }
                            }

                            if (waitedTime > currentMove.onCompleteDelay)
                            {
                                CompleteMove();
                            }
                        }
                        break;
                    case State.Turning:
                        {
                            if (waitedTime > currentTurnTarget.turnDelay)
                            {
                                var direction = currentTurnTarget.target - transform.position;

                                // Neutralize y component.
                                direction.y = 0.0f;

                                if (direction.magnitude > distanceEpsilon)
                                {
                                    TurnToDirection(direction, currentTurnTarget.minAngle, currentTurnTarget.rotationSpeedMultiplier, currentTurnTarget.cancelSpecial);
                                }

                                if (Vector3.Angle(transform.forward, direction) <= currentTurnTarget.minAngle + angleEpsilon)
                                {
                                    if (currentTurnTarget.onCompleteDelay > 0.0f)
                                    {
                                        SetState(State.CompletingTurn);
                                    }
                                    else
                                    {
                                        CompleteTurn();
                                    }
                                }
                            }
                            else
                            {
                                // Set speed, move delta and rotation speed.
                                speed = 0.0f;
                                moveDelta = new Vector3(0.0f, moveDelta.y, 0.0f);
                                rotateSpeed = 0.0f;
                            }
                            break;
                        }
                    case State.CompletingTurn:
                        {
                            if (waitedTime > currentTurnTarget.onCompleteDelay)
                            {
                                CompleteTurn();
                            }
                            break;
                        }
                    case State.Following:
                        {
                            if (waitedTime > currentFollowTarget.followDelay)
                            {
                                var direction = currentFollowTarget.target.position - transform.position;

                                // Neutralize y component.
                                direction.y = 0.0f;

                                if (direction.magnitude > currentFollowTarget.minDistance + distanceEpsilon)
                                {
                                    var shouldBreak = currentFollowTarget.onCompleteDelay > 0.0f || (moves.Count > 0 && moves[0].moveDelay > 0.0f);
                                    MoveInDirection(direction, currentFollowTarget.minDistance, shouldBreak, currentFollowTarget.speedMultiplier, 0.0f, currentFollowTarget.rotationSpeedMultiplier, currentFollowTarget.cancelSpecial);
                                }
                                else
                                {
                                    if (currentFollowTarget.onCompleteDelay > 0.0f)
                                    {
                                        SetState(State.CompletingFollow);
                                    }
                                    else
                                    {
                                        CompleteFollow();
                                    }
                                }
                            }
                            else
                            {
                                // Set speed, move delta and rotation speed.
                                speed = 0.0f;
                                moveDelta = new Vector3(0.0f, moveDelta.y, 0.0f);
                                rotateSpeed = 0.0f;
                            }
                            break;
                        }
                    case State.CompletingFollow:
                        {
                            var direction = currentFollowTarget.target.position - transform.position;

                            // Neutralize y component.
                            direction.y = 0.0f;

                            if (direction.magnitude > currentFollowTarget.minDistance + distanceEpsilon)
                            {
                                // Start following again, with no delay.
                                SetState(State.Following, currentFollowTarget.followDelay);
                            }
                            else
                            {
                                // Possibly turn to position.
                                if (currentFollowTarget.turnToWhileCompleting.HasValue)
                                {
                                    var turnToDirection = currentFollowTarget.turnToWhileCompleting.Value - transform.position;
                                    if (turnToDirection.magnitude > distanceEpsilon)
                                    {
                                        TurnToDirection(direction, 0.0f, currentFollowTarget.rotationSpeedMultiplier, currentFollowTarget.cancelSpecial);
                                    }
                                }

                                if (waitedTime > currentFollowTarget.onCompleteDelay)
                                {
                                    CompleteFollow();
                                }
                            }
                            break;
                        }
                }
            }

            // Handle external motion.
            externalMotion = Vector3.zero;
            externalRotation = 0.0f;

            var wasGrounded = controller.isGrounded;

            if (!controller.isGrounded)
            {
                // Apply gravity.
                moveDelta.y -= gravity * Time.deltaTime;

                groundedTransform = null;

                airborneTime += Time.deltaTime;
            }
            else
            {
                // Apply external motion and rotation.
                if (groundedTransform && Time.deltaTime > 0.0f)
                {
                    var newGroundedPosition = groundedTransform.TransformPoint(groundedLocalPosition);
                    externalMotion = (newGroundedPosition - oldGroundedPosition) / Time.deltaTime;
                    oldGroundedPosition = newGroundedPosition;

                    var newGroundedRotation = groundedTransform.rotation;
                    // FIXME Breaks down if rotating more than 180 degrees per frame.
                    var diffRotation = newGroundedRotation * Quaternion.Inverse(oldGroundedRotation);
                    var rotatedRight = diffRotation * Vector3.right;
                    rotatedRight.y = 0.0f;
                    if (rotatedRight.magnitude > 0.0f)
                    {
                        rotatedRight.Normalize();
                        externalRotation = Vector3.SignedAngle(Vector3.right, rotatedRight, Vector3.up) / Time.deltaTime;
                    }
                    oldGroundedRotation = newGroundedRotation;
                }
            }

            // Move minifig - check if game object was made inactive in some callback to avoid warnings from CharacterController.Move.
            if (gameObject.activeInHierarchy)
            {
                // Use a sticky move to make the minifig stay with moving platforms.
                var stickyMove = airborneTime < stickyTime ? Vector3.down * stickyForce * Time.deltaTime : Vector3.zero;
                controller.Move((moveDelta + externalMotion) * Time.deltaTime + stickyMove);
            }

            // If becoming grounded by this Move, reset y movement and airborne time.
            if (!wasGrounded && controller.isGrounded)
            {
                // Play landing sound if landing sufficiently hard.
                if (moveDelta.y < -5.0f)
                {
                    if (landAudioClips.Count > 0)
                    {
                        var landAudioClip = landAudioClips[UnityEngine.Random.Range(0, landAudioClips.Count)];
                        if (landAudioClip)
                        {
                            audioSource.PlayOneShot(landAudioClip);
                        }
                    }
                }

                moveDelta.y = 0.0f;
                airborneTime = 0.0f;
            }

            // Update airborne state.
            airborne = airborneTime >= coyoteDelay;

            // Rotate minifig.
            transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
            transform.RotateAround(oldGroundedPosition, Vector3.up, externalRotation * Time.deltaTime);

            // Stop special if requested.
            cancelSpecial |= stopSpecial;
            stopSpecial = false;

            // Update animation - delay airborne animation slightly to avoid flailing arms when falling a short distance.
            animator.SetBool(cancelSpecialHash, cancelSpecial);
            animator.SetFloat(speedHash, speed);
            animator.SetFloat(rotateSpeedHash, rotateSpeed);
            animator.SetBool(groundedHash, !airborne);

            // Update rigs.
            UpdateRig(leftArmRig, leftHandReaching, ref leftHandReachWeight, leftHandReachSpeed);
            UpdateRig(rightArmRig, rightHandReaching, ref rightHandReachWeight, rightHandReachSpeed);
            UpdateRig(leftLegRig, leftFootReaching, ref leftFootReachWeight, leftFootReachSpeed);
            UpdateRig(rightLegRig, rightFootReaching, ref rightFootReachWeight, rightFootReachSpeed);
            UpdateRig(headRig, headLooking, ref headLookAtWeight, headLookAtSpeed);
        }

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
        }

        public void SetVirtualJoystick(VirtualJoystick virtualJoystick)
        {
            this.virtualJoystick = virtualJoystick;
        }

        public void PlaySpecialAnimation(SpecialAnimation animation, AudioClip specialAudioClip = null, Action<bool> onSpecialComplete = null)
        {
            animator.SetBool(playSpecialHash, true);
            animator.SetInteger(specialIdHash, (int)animation);

            if (specialAudioClip)
            {
                audioSource.PlayOneShot(specialAudioClip);
            }

            this.onSpecialComplete = onSpecialComplete;
        }

        public void StopSpecialAnimation()
        {
            stopSpecial = true;
        }

        /// <summary>
        /// Do not call this directly.
        /// Called from SpecialAnimationBehaviour to signal that a special animation finished.
        /// </summary>
        public void SpecialAnimationFinished()
        {
            // Do callback.
            onSpecialComplete?.Invoke(animator.GetBool(cancelSpecialHash));
        }

        public void ReachHandsTo(Vector3 position, Quaternion? rotation, float time = 1.0f)
        {
            ReachLeftHandTo(position, rotation, time);
            ReachRightHandTo(position, rotation, time);
        }

        public void ReachLeftHandTo(Vector3 position, Quaternion? rotation, float time = 1.0f)
        {
            leftHandReaching = true;
            leftHandReachSpeed = 1.0f / time;
            SetRigTarget(leftArmRig, leftArmEffector, position, rotation, leftArmRoot.position, Quaternion.identity);
        }

        public void ReachRightHandTo(Vector3 position, Quaternion? rotation, float time = 1.0f)
        {
            rightHandReaching = true;
            rightHandReachSpeed = 1.0f / time;
            SetRigTarget(rightArmRig, rightArmEffector, position, rotation, rightArmRoot.position, Quaternion.identity);
        }

        public void StopReachingHands(float time = 1.0f)
        {
            StopReachingLeftHand(time);
            StopReachingRightHand(time);
        }

        public void StopReachingLeftHand(float time = 1.0f)
        {
            leftHandReaching = false;
            leftHandReachSpeed = 1.0f / time;
        }

        public void StopReachingRightHand(float time = 1.0f)
        {
            rightHandReaching = false;
            rightHandReachSpeed = 1.0f / time;
        }

        public void ReachLeftFootTo(Vector3 position, Quaternion? rotation, float time = 1.0f)
        {
            leftFootReaching = true;
            leftFootReachSpeed = 1.0f / time;
            SetRigTarget(leftLegRig, leftLegEffector, position, rotation, leftLegRoot.position, Quaternion.Euler(Vector3.left * 90.0f));
        }

        public void ReachRightFootTo(Vector3 position, Quaternion? rotation, float time = 1.0f)
        {
            rightFootReaching = true;
            rightFootReachSpeed = 1.0f / time;
            SetRigTarget(rightLegRig, rightLegEffector, position, rotation, rightLegRoot.position, Quaternion.Euler(Vector3.left * 90.0f));
        }

        public void StopReachingLeftFoot(float time = 1.0f)
        {
            leftFootReaching = false;
            leftFootReachSpeed = 1.0f / time;
        }

        public void StopReachingRightFoot(float time = 1.0f)
        {
            rightFootReaching = false;
            rightFootReachSpeed = 1.0f / time;
        }

        public void LookAt(Vector3 position, float time = 1.0f)
        {
            headLooking = true;
            headLookAtSpeed = 1.0f / time;
            SetRigTarget(headRig, headEffector, position, Quaternion.identity, Vector3.zero, Quaternion.identity);
        }

        public void StopLooking(float time = 1.0f)
        {
            headLooking = false;
            headLookAtSpeed = 1.0f / time;
        }

        public void TeleportTo(Vector3 position)
        {
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
        }

        public void Explode()
        {
            const float horizontalVelocityTransferRatio = 0.35f;
            const float verticalVelocityTransferRatio = 0.1f;
            const float angularVelocityTransferRatio = 1.0f;

            if (!exploded)
            {
                exploded = true;
                animator.enabled = false;
                controller.enabled = false;

                var transferredSpeed = Vector3.Scale(moveDelta + externalMotion, new Vector3(horizontalVelocityTransferRatio, verticalVelocityTransferRatio, horizontalVelocityTransferRatio));
                var transferredAngularSpeed = (rotateSpeed + externalRotation) * angularVelocityTransferRatio;

                if (explodeAudioClip)
                {
                    audioSource.PlayOneShot(explodeAudioClip);
                }

                MinifigExploder.Explode(minifig, leftArmRig, rightArmRig, leftLegRig, rightLegRig, headRig, transferredSpeed, transferredAngularSpeed);
            }
        }

        public void MoveTo(Vector3 destination, float minDistance = 0.0f, Action onComplete = null, float onCompleteDelay = 0.0f,
            float moveDelay = 0.0f, bool cancelSpecial = true, float speedMultiplier = 1.0f, float rotationSpeedMultiplier = 1.0f, Vector3? turnToWhileCompleting = null)
        {
            MoveTarget move = new MoveTarget()
            {
                destination = destination,
                minDistance = minDistance,
                onComplete = onComplete,
                onCompleteDelay = onCompleteDelay,
                moveDelay = moveDelay,
                cancelSpecial = cancelSpecial,
                speedMultiplier = speedMultiplier,
                rotationSpeedMultiplier = rotationSpeedMultiplier,
                turnToWhileCompleting = turnToWhileCompleting
            };

            moves.Add(move);

            UpdateState();
        }

        public void ClearMoves()
        {
            moves.Clear();

            UpdateState();
        }

        public void TurnTo(Vector3 target, float minAngle = 0.0f, Action onComplete = null, float onCompleteDelay = 0.0f,
            float turnDelay = 0.0f, bool cancelSpecial = false, float rotationSpeedMultiplier = 1.0f)
        {
            TurnTarget turnTarget = new TurnTarget()
            {
                target = target,
                minAngle = minAngle,
                onComplete = onComplete,
                onCompleteDelay = onCompleteDelay,
                turnDelay = turnDelay,
                cancelSpecial = cancelSpecial,
                rotationSpeedMultiplier = rotationSpeedMultiplier
            };

            currentTurnTarget = turnTarget;

            UpdateState();
        }

        public void StopTurning()
        {
            currentTurnTarget = null;
            UpdateState();
        }

        public void Follow(Transform target, float minDistance = 0.0f, Action onComplete = null, float onCompleteDelay = 0.0f,
            float followDelay = 0.0f, bool cancelSpecial = true, float speedMultiplier = 1.0f, float rotationSpeedMultiplier = 1.0f, Vector3? turnToWhileCompleting = null)
        {
            FollowTarget followTarget = new FollowTarget()
            {
                target = target,
                minDistance = minDistance,
                onComplete = onComplete,
                onCompleteDelay = onCompleteDelay,
                followDelay = followDelay,
                cancelSpecial = cancelSpecial,
                speedMultiplier = speedMultiplier,
                rotationSpeedMultiplier = rotationSpeedMultiplier,
                turnToWhileCompleting = turnToWhileCompleting
            };

            currentFollowTarget = followTarget;

            UpdateState();
        }

        public void StopFollowing()
        {
            currentFollowTarget = null;
            UpdateState();
        }

        // Animation event.
        public void StepFoot()
        {
            if (!stepped)
            {
                if (stepAudioClips.Count > 0)
                {
                    var stepAudioClip = stepAudioClips[UnityEngine.Random.Range(0, stepAudioClips.Count)];
                    if (stepAudioClip)
                    {
                        audioSource.PlayOneShot(stepAudioClip);
                    }
                }
            }
            stepped = true;
        }

        // Animation event.
        public void LiftFoot()
        {
            stepped = false;
        }

        void SetRigTarget(Rig rig, Transform effector, Vector3 position, Quaternion? rotation, Vector3 rootPosition, Quaternion forwardRotation)
        {
            effector.localPosition = rig.transform.InverseTransformPoint(position);
            if (rotation.HasValue)
            {
                effector.localRotation = Quaternion.Inverse(rig.transform.rotation) * rotation.Value;
            }
            else
            {
                // Compute forward vector from root to effector.
                effector.localRotation = forwardRotation * Quaternion.LookRotation(effector.localPosition - rig.transform.InverseTransformPoint(rootPosition));
            }
        }

        void SetupRig(Rig rig, ref Transform effector, ref Transform root)
        {
            var constraint = rig.transform.GetChild(0).GetComponent<TwoBoneIKConstraint>();
            effector = constraint.transform.GetChild(0);
            root = constraint.data.root;
        }

        void UpdateRig(Rig rig, bool targeting, ref float weight, float speed)
        {
            if (targeting)
            {
                weight = Mathf.Min(1.0f, weight + speed * Time.deltaTime);
            }
            else
            {
                weight = Mathf.Max(0.0f, weight - speed * Time.deltaTime);
            }
            rig.weight = rigEasing.Evaluate(weight);
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (controller.isGrounded)
            {
                RaycastHit raycastHit;
                if (Physics.SphereCast(transform.position + Vector3.up * 0.8f, 0.8f, Vector3.down, out raycastHit, 0.1f, -1, QueryTriggerInteraction.Ignore))
                {
                    groundedTransform = raycastHit.collider.transform;
                    oldGroundedPosition = raycastHit.point;
                    groundedLocalPosition = groundedTransform.InverseTransformPoint(oldGroundedPosition);
                    oldGroundedRotation = groundedTransform.rotation;
                }
            }
        }

        // Assumes that direction is not a zero vector.
        void MoveInDirection(Vector3 direction, float minDistance, bool breakBeforeTarget, float speedMultiplier, float minAngle, float rotationSpeedMultiplier, bool cancelSpecial)
        {
            // Distance required to stop current speed.
            var breakDistance = (speed * speed) / (2.0f * acceleration);

            // If breaking before target, only set target speed if still possible to break.
            var targetSpeed = 0.0f;
            if (!breakBeforeTarget || direction.magnitude - minDistance > breakDistance + distanceEpsilon)
            {
                targetSpeed = speedMultiplier * maxForwardSpeed;
            }

            // Adjust speed based on target speed.
            if (targetSpeed > speed)
            {
                speed = Mathf.Min(targetSpeed, speed + acceleration * Time.deltaTime);
            }
            else if (targetSpeed < speed)
            {
                speed = Mathf.Max(targetSpeed, speed - acceleration * Time.deltaTime);
            }

            // Calculate move delta - prevent overshoot by limiting speed.
            // Assumes that direction is not zero length as it will cause issues when Time.deltaTime is zero.
            var moveDirection = direction.normalized * Mathf.Min(speed, direction.magnitude / Time.deltaTime);
            moveDelta = new Vector3(moveDirection.x, moveDelta.y, moveDirection.z);

            // Angle required to stop current rotate speed.
            var breakAngle = (rotateSpeed * rotateSpeed) / (2.0f * rotateAcceleration);

            // Only set target rotate speed if still possible to break.
            var targetRotateSpeed = 0.0f;
            var angleDiff = Vector3.SignedAngle(transform.forward, direction.normalized, Vector3.up);

            if (Mathf.Abs(angleDiff) - minAngle > breakAngle + angleEpsilon)
            {
                targetRotateSpeed = Mathf.Clamp(angleDiff, -1.0f, 1.0f) * rotationSpeedMultiplier * maxRotateSpeed;
            }

            // Adjust rotate speed based on target rotate speed.
            if (targetRotateSpeed > rotateSpeed)
            {
                rotateSpeed = Mathf.Min(targetRotateSpeed, rotateSpeed + rotateAcceleration * Time.deltaTime);
            }
            else if (targetRotateSpeed < rotateSpeed)
            {
                rotateSpeed = Mathf.Max(targetRotateSpeed, rotateSpeed - rotateAcceleration * Time.deltaTime);
            }

            // Prevent overshoot by limiting rotateSpeed.
            if (angleDiff < 0.0f && rotateSpeed < 0.0f)
            {
                rotateSpeed = Mathf.Max(rotateSpeed, angleDiff / Time.deltaTime);
            }
            else if (angleDiff > 0.0f && rotateSpeed > 0.0f)
            {
                rotateSpeed = Mathf.Min(rotateSpeed, angleDiff / Time.deltaTime);
            }

            // Cancel special.
            this.cancelSpecial = cancelSpecial;
        }

        // Assumes the direction is not a zero vector.
        void TurnToDirection(Vector3 direction, float minAngle, float rotationSpeedMultiplier, bool cancelSpecial)
        {
            speed = Mathf.Max(0.0f, speed - acceleration * Time.deltaTime);

            // Calculate move delta - prevent overshoot by limiting speed.
            var moveDirection = transform.forward * speed;
            moveDelta = new Vector3(moveDirection.x, moveDelta.y, moveDirection.z);

            // Angle required to stop current rotate speed.
            var breakAngle = (rotateSpeed * rotateSpeed) / (2.0f * rotateAcceleration);

            // Only set target rotate speed if still possible to break.
            var targetRotateSpeed = 0.0f;
            var angleDiff = Vector3.SignedAngle(transform.forward, direction.normalized, Vector3.up);

            if (Mathf.Abs(angleDiff) - minAngle > breakAngle + angleEpsilon)
            {
                targetRotateSpeed = Mathf.Clamp(angleDiff, -1.0f, 1.0f) * rotationSpeedMultiplier * maxRotateSpeed;
            }

            // Adjust rotate speed based on target rotate speed.
            if (targetRotateSpeed > rotateSpeed)
            {
                rotateSpeed = Mathf.Min(targetRotateSpeed, rotateSpeed + rotateAcceleration * Time.deltaTime);
            }
            else if (targetRotateSpeed < rotateSpeed)
            {
                rotateSpeed = Mathf.Max(targetRotateSpeed, rotateSpeed - rotateAcceleration * Time.deltaTime);
            }

            // Prevent overshoot by limiting rotateSpeed.
            if (angleDiff < 0.0f && rotateSpeed < 0.0f)
            {
                rotateSpeed = Mathf.Max(rotateSpeed, angleDiff / Time.deltaTime);
            }
            else if (angleDiff > 0.0f && rotateSpeed > 0.0f)
            {
                rotateSpeed = Mathf.Min(rotateSpeed, angleDiff / Time.deltaTime);
            }

            // Cancel special.
            this.cancelSpecial = cancelSpecial;
        }

        void SetState(State newState, float initialWaitedTime = 0.0f)
        {
            waitedTime = initialWaitedTime;
            state = newState;

            // Stop cancelling special.
            cancelSpecial = false;

            // Disable input if not idle.
            if (state != State.Idle)
            {
                inputEnabled = false;
            }

            switch (state)
            {
                case State.Moving:
                    {
                        currentMove = moves[0];
                        break;
                    }
                case State.CompletingMove:
                    {
                        // Set speed and move delta.
                        speed = 0.0f;
                        moveDelta = new Vector3(0.0f, moveDelta.y, 0.0f);

                        break;
                    }
                case State.Idle:
                case State.CompletingTurn:
                case State.CompletingFollow:
                    {
                        // Set speed, move delta and rotation speed.
                        speed = 0.0f;
                        moveDelta = new Vector3(0.0f, moveDelta.y, 0.0f);
                        rotateSpeed = 0.0f;

                        break;
                    }
            }
        }

        void UpdateState()
        {
            if (currentTurnTarget != null)
            {
                SetState(State.Turning);
            }
            else if (currentFollowTarget != null)
            {
                SetState(State.Following);
            }
            else if (moves.Count > 0)
            {
                SetState(State.Moving);
            }
            else
            {
                SetState(State.Idle);
            }
        }

        void CompleteMove()
        {
            // Remove move from queue.
            if (moves.Count > 0)
            {
                moves.RemoveAt(0);
            }

            // Do callbacks.
            currentMove.onComplete?.Invoke();

            UpdateState();
        }

        void CompleteTurn()
        {
            var completeFunc = currentTurnTarget.onComplete;
            currentTurnTarget = null;

            completeFunc?.Invoke();

            UpdateState();
        }

        void CompleteFollow()
        {
            var completeFunc = currentFollowTarget.onComplete;
            currentFollowTarget = null;

            completeFunc?.Invoke();

            UpdateState();
        }

        float GetAxisInput(string axis)
        {
            return Mathf.Clamp(Input.GetAxisRaw(axis) + (virtualJoystick && axis == "Horizontal" ? virtualJoystick.Horizontal : (virtualJoystick && axis == "Vertical" ? virtualJoystick.Vertical : 0f)), -1f, 1f);
        }

        bool GetJumpInput()
        {
            return Input.GetButtonDown("Jump") || (virtualJoystick ? virtualJoystick.Jumped : false);
        }
    }

}
