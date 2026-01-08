using MFPS.Internal.Scriptables;
using System;
using UnityEngine;

namespace MFPS.PlayerController
{
    [Serializable]
    public class MouseLook : MouseLookBase
    {
        public class FrameSmoothing
        {
            private readonly float[] frames;
            private int currentFrame = 0;
            private float average;
            private readonly bool init = false;
            private readonly int frameCount = 1;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public float GetValue() => average;

            public FrameSmoothing(int maxFrames)
            {
                frames = new float[maxFrames];
                frameCount = maxFrames;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="frameValue"></param>
            public void Set(float frameValue)
            {
                if (!init)
                {
                    for (int i = 0; i < frameCount; i++)
                    {
                        frames[i] = frameValue;
                    }
                }

                frames[currentFrame] = frameValue;
                currentFrame = (currentFrame + 1) % frameCount;
                average = 0;

                for (int i = 0; i < frameCount; i++)
                {
                    average += frames[i];
                }
                average /= frameCount;
            }
        }

        #region Public members
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        #endregion

        #region Private members
        private Quaternion m_CharacterTargetRot, m_CameraTargetRot;
        private bool InvertVertical, InvertHorizontal;
        private float verticalRotation, horizontalRotation;
        private float sensitivity, aimSensitivity = 3f;
        private Quaternion extraRotation = Quaternion.identity;
        private Transform m_CameraTransform, m_CharacterBody;
        private FrameSmoothing xSmoothing, ySmoothing;
        private bool ClampHorizontal = false;
        private Vector2 horizontalClamp = new Vector2(-360, 360);
        private bl_PlayerReferences playerReferences;
        private MouseLookSettings lookSettings;
        private float tiltValue, tiltTarget = 0;
        private float verticalOffset = 0;
        private bool isHorizontalClamp = false; // Added variable
        private float horizontalClampCenter = 0f;     // Added variable
        private float horizontalClampRange = 40f;     // Added variable
        #endregion

        public float CurrentSensitivity { get; set; } = 3;
        public bool OnlyCameraTransform { get; set; } = false;

        /// <summary>
        ///  Initialize the camera controller with the character initial rotation.
        /// </summary>
        public override void Init(Transform character, Transform camera)
        {
            m_CameraTransform = camera;
            m_CharacterBody = character;
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
            FetchSettings();
            CurrentSensitivity = sensitivity;
            playerReferences = character.GetComponent<bl_PlayerReferences>();
            lookSettings = bl_GameData.Instance.mouseLookSettings;
            xSmoothing = new FrameSmoothing(lookSettings.framesOfSmoothing);
            ySmoothing = new FrameSmoothing(lookSettings.framesOfSmoothing);
        }

        /// <summary>
        /// Updates the character and camera rotation based on the player input.
        /// </summary>
        public override void UpdateLook(Transform character, Transform camera)
        {
            // When use a mobile device or Unity Remote
            if (bl_UtilityHelper.isMobile)
            {
#if MFPSM
                Vector2 input = bl_TouchPad.Instance.GetInput(CurrentSensitivity);
                input.x = !InvertHorizontal ? input.x : (input.x * -1f);
                input.y = InvertVertical ? (input.y * -1f) : input.y;

                Move(input.x, input.y, character, camera);
#endif
            }
            else
            {
                if (!bl_GameInput.IsCursorLocked)
                    return;

                Move(bl_GameInput.MouseX, bl_GameInput.MouseY, character, camera);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Move(float inputX, float inputY, Transform character, Transform camera)
        {
            horizontalRotation = inputX * CurrentSensitivity;
            horizontalRotation = (InvertHorizontal) ? (horizontalRotation * -1f) : horizontalRotation;

            if (lookSettings.useSmoothing)
            {
                xSmoothing.Set(horizontalRotation);
                inputX = xSmoothing.GetValue();
            }
            else inputX = horizontalRotation;

            m_CharacterTargetRot *= Quaternion.Euler(0f, inputX, 0f);

            verticalRotation = inputY * CurrentSensitivity;
            verticalRotation = (InvertVertical) ? (verticalRotation * -1f) : verticalRotation;
            if (lookSettings.useSmoothing)
            {
                ySmoothing.Set(verticalRotation);
                inputY = ySmoothing.GetValue();
            }
            else inputY = verticalRotation;

            if (!OnlyCameraTransform)
                m_CameraTargetRot *= Quaternion.Euler(-inputY, 0f, 0);
            else
            {
                m_CameraTargetRot *= Quaternion.Euler(-inputY, inputX, 0);
            }

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            // Handle climbing clamp
            if (isHorizontalClamp)
            {
                var eulerY = m_CharacterTargetRot.eulerAngles.y;
                float minAngle = horizontalClampCenter - (horizontalClampRange / 2f);
                float maxAngle = horizontalClampCenter + (horizontalClampRange / 2f);

                eulerY = ClampAngle(eulerY, minAngle, maxAngle);
                m_CharacterTargetRot = Quaternion.Euler(0f, eulerY, 0f);
            }
            else if (ClampHorizontal)
            {
                var re = m_CharacterTargetRot.eulerAngles;
                re.y = bl_MathUtility.Clamp360Angle(re.y, horizontalClamp.x, horizontalClamp.y);
                m_CharacterTargetRot = Quaternion.Euler(re);
            }

            if (lookSettings.lerpMovement)
            {
                if (character != null && !OnlyCameraTransform) { character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, lookSettings.smoothTime * Time.deltaTime); }
                if (camera != null) { camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot * extraRotation, lookSettings.smoothTime * Time.deltaTime); }
            }
            else
            {
                if (!OnlyCameraTransform)
                {
                    character.localRotation = m_CharacterTargetRot;
                }
                else
                {
                    var fixEuler = m_CameraTargetRot.eulerAngles;
                    fixEuler.z = 0;
                    m_CameraTargetRot = Quaternion.Euler(fixEuler);
                }
                camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot * extraRotation, 1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            tiltValue = Mathf.Lerp(tiltValue, tiltTarget, Time.deltaTime * 4);
            extraRotation = Quaternion.Euler(verticalOffset, 0, tiltValue);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void SetVerticalOffset(float amount)
        {
            verticalOffset = amount;
            extraRotation = Quaternion.Euler(amount, 0, tiltValue);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void CombineVerticalOffset()
        {
            m_CameraTargetRot *= extraRotation;
            verticalOffset = 0;
            extraRotation = Quaternion.Euler(verticalOffset, 0, tiltValue);

            if (clampVerticalRotation)
            {
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
            }
        }

        /// <summary>
        /// Don't rotate the character body, only the Camera/Head
        /// </summary>
        public void UseOnlyCameraRotation()
        {
            OnlyCameraTransform = true;
        }

        /// <summary>
        /// Port the Current Camera Rotation to separate the vertical and horizontal rotation in the body and head
        /// horizontal rotation for the body and vertical for the camera/head
        /// That should only be called when OnlyCameraRotation was used before.
        /// </summary>
        public void PortBodyOrientationToCamera()
        {
            OnlyCameraTransform = false;
            Vector3 direction = Vector3.zero;
            direction.y = m_CameraTransform.eulerAngles.y;
            m_CharacterBody.eulerAngles = direction;

            direction = Vector3.zero;
            direction.x = m_CameraTransform.localEulerAngles.x;
            m_CameraTransform.localEulerAngles = direction;

            m_CharacterTargetRot = m_CharacterBody.localRotation;
            m_CameraTargetRot = m_CameraTransform.localRotation;

            m_CameraTransform.localRotation *= extraRotation;
        }

        /// <summary>
        /// Forces the character to look at a position in the world.
        /// </summary>
        public override void LookAt(Transform reference, bool extrapolate = true)
        {
            m_CharacterTargetRot = Quaternion.Euler(0f, reference.eulerAngles.y, 0f);
            Quaternion relative = Quaternion.Inverse(Quaternion.identity) * reference.rotation;
            m_CameraTargetRot = Quaternion.Euler(relative.eulerAngles.x, 0, 0);

            if (extrapolate)
            {
                m_CharacterBody.localRotation = m_CharacterTargetRot;
                m_CameraTransform.localRotation = m_CameraTargetRot;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void LookAt(Vector3 direction, bool extrapolate = true, float influence = 1)
        {
            Quaternion rotate = Quaternion.LookRotation(direction);
            rotate = Quaternion.Slerp(m_CameraTransform.rotation, rotate, influence);

            m_CharacterTargetRot = Quaternion.Euler(0f, rotate.eulerAngles.y, 0f);
            m_CameraTargetRot = Quaternion.Euler(rotate.eulerAngles.x, 0, 0);

            if (extrapolate)
            {
                m_CharacterBody.localRotation = m_CharacterTargetRot;
                m_CameraTransform.localRotation = m_CameraTargetRot;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle"></param>
        public override void SetTiltAngle(float angle)
        {
            tiltTarget = angle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="forward"></param>
        public override Vector3 SetCharacterForward(Vector3 forward)
        {
            // Calculate the rotation based on the forward vector
            Quaternion targetRotation = Quaternion.LookRotation(forward);

            // Update internal variables
            m_CharacterTargetRot = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);

            // Apply rotation
            if (!OnlyCameraTransform)
            {
                m_CharacterBody.localRotation = m_CharacterTargetRot;
            }
            else
            {
                // If only the camera is rotating, adjust accordingly
                var fixEuler = m_CameraTargetRot.eulerAngles;
                fixEuler.y = targetRotation.eulerAngles.y;
                m_CameraTargetRot = Quaternion.Euler(fixEuler);
                m_CameraTransform.localRotation = m_CameraTargetRot * extraRotation;
            }
            return m_CharacterTargetRot.eulerAngles;
        }

        /// <summary>
        /// 
        /// </summary>
        public void FetchSettings()
        {
            sensitivity = (float)bl_MFPS.Settings.GetSettingOf("Sensitivity");
            aimSensitivity = (float)bl_MFPS.Settings.GetSettingOf("Aim Sensitivity");
            InvertHorizontal = (bool)bl_MFPS.Settings.GetSettingOf("MouseH Invert");
            InvertVertical = (bool)bl_MFPS.Settings.GetSettingOf("MouseV Invert");
            CurrentSensitivity = sensitivity;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnAimChange(bool isAiming)
        {
            CurrentSensitivity = isAiming ? sensitivity * aimSensitivity : sensitivity;

            if (isAiming && lookSettings.aimSensitivityAdjust == MouseLookSettings.AimSensitivityAdjust.Relative)
                AdjustSensitivityBasedOnFOV();
        }

        /// <summary>
        /// Adjust the mouse sensitivity based on the camera field of view.
        /// </summary>
        public void AdjustSensitivityBasedOnFOV()
        {
            if (playerReferences == null) return;

            float fovPercentage = playerReferences.playerCamera.fieldOfView / playerReferences.DefaultCameraFOV;
            CurrentSensitivity *= fovPercentage;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void ClampHorizontalRotation(float min, float max)
        {
            horizontalClamp = new Vector2(min, max);
            ClampHorizontal = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void UnClampHorizontal() => ClampHorizontal = false;

        /// <summary>
        /// Enables the climbing clamp mode with a center angle and range.
        /// </summary>
        public override void SetActiveHorizontalClamp(bool active, float centerAngle = 0, float range = 45)
        {
            isHorizontalClamp = active;
            if (!active) return;

            horizontalClampCenter = centerAngle;
            horizontalClampRange = range;
        }

        /// <summary>
        /// Clamps an angle between a minimum and maximum value, handling angle wrapping.
        /// </summary>
        private float ClampAngle(float angle, float min, float max)
        {
            angle = NormalizeAngle(angle);
            min = NormalizeAngle(min);
            max = NormalizeAngle(max);

            // Check if the angle range crosses the -180/180 boundary
            bool angleRangeCrossesBoundary = min > max;

            if (!angleRangeCrossesBoundary)
            {
                // Standard clamping
                return Mathf.Clamp(angle, min, max);
            }
            else
            {
                // Angle range crosses the boundary
                // Check if the angle is outside the allowed range
                if (angle > max && angle < min)
                {
                    // Decide whether to clamp to min or max based on which is closer
                    float diffToMin = Mathf.Abs(Mathf.DeltaAngle(angle, min));
                    float diffToMax = Mathf.Abs(Mathf.DeltaAngle(angle, max));
                    return (diffToMin < diffToMax) ? min : max;
                }
                else
                {
                    // Angle is within the allowed range
                    return angle;
                }
            }
        }

        private float NormalizeAngle(float angle)
        {
            angle = angle % 360;
            if (angle > 180)
                angle -= 360;
            else if (angle < -180)
                angle += 360;
            return angle;
        }


        public float VerticalAngle => m_CameraTransform.localEulerAngles.x;

        Quaternion ClampRotationAroundXAxis(Quaternion q) => bl_MathUtility.ClampRotationAroundAxis(q, MinimumX, MaximumX, UnityEngine.Animations.Axis.X);

        public override Vector2 HorizontalLimits { get => horizontalClamp; set => horizontalClamp = value; }
        public override Vector2 VerticalLimits
        {
            get => new Vector2(MinimumX, MaximumX);
            set
            {
                MinimumX = value.x;
                MaximumX = value.y;
            }
        }
    }
}