using AIChara;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HS2_PovX
{
	public static class Controller
	{
		public static bool povEnabled = false;
		public static bool showCursor = false;

		// total camera rotation relative to body forward
		public static float cameraLocalPitch = 0f;
		public static float cameraLocalYaw = 0f;
		// portion of camera rotation that is acheived through head/neck rotation
		public static float headLocalPitch = 0f;
		public static float headLocalYaw = 0f;
		// portion of camera rotation that is acheived through eye rotation
		public static float eyeLocalPitch = 0f;
		public static float eyeLocalYaw = 0f;

		// 0 = Player; 1 = 1st Partner; 2 = 2nd Partner; 3 = ...
		public static int povFocus = 0;
		public static int targetFocus = 0;
		public static ChaControl[] characters = new ChaControl[0];
		public static ChaControl povCharacter;
		public static ChaControl targetCharacter;
		public static Vector3 eyeOffset = Vector3.zero;
		public static Vector3 normalHeadScale;
		public static float backupFoV;

		public static bool inScene;
		public static bool lockHeadPosition;
		public static bool povSetThisFrame = false;

		public static HScene hScene;
		public static string currentHMotion;

		private static readonly List<string> firstMaleLockHeadAllHPositions = new List<string>() { "aia_f_10", "h2h_f_03", "ait_f_00", "ait_f_07" };
		private static readonly List<string> firstMaleLockHeadHPositions = new List<string>() { "aia_f_00", "aia_f_01", "aia_f_04", "aia_f_06", "aia_f_07", "aia_f_08", "aia_f_11", "aia_f_12", "aia_f_13", "aia_f_18", "aia_f_19", "aia_f_23", "aia_f_24", "aia_f_26", "ai3p", "h2a_f_00" };
		private static readonly List<string> secondMaleLockHeadAllHPositions = new List<string>() { "h2_m2f_f_00", "h2_m2f_f_02" };
		private static readonly List<string> secondMaleLockHeadHPositions = new List<string>() { };

		private static readonly List<string> firstFemaleLockHeadAllHPositions = new List<string>() { "ais_f_19", "aia_f_16", "ais_f_27" };
		private static readonly List<string> firstFemaleLockHeadHPositions = new List<string>() { "aia_f_00", "aia_f_01", "aia_f_07", "aia_f_11", "aia_f_12", "aih_f_00", "aih_f_04", "aih_f_05", "aih_f_09", "aih_f_10", "aih_f_12", "aih_f_13", "aih_f_14", "aih_f_16", "aih_f_17", "aih_f_19", "aih_f_21", "aih_f_23", "aih_f_25", "aih_f_26", "aih_f_27", "h2h_f_02", "h2h_f_03", "aih_f_06", "aih_f_07", "ail_f1_03", "ail_f1_04", "h2_mf2_f1_00", "h2_mf2_f2_03", "h2_m2f_f_01", "h2_m2f_f_04", "h2_m2f_f_05", "h2_m2f_f_06", "ait_f_07" };
		private static readonly List<string> secondFemaleLockHeadAllHPositions = new List<string>() { };
		private static readonly List<string> secondFemaleLockHeadHPositions = new List<string>() { "ail_f2_03", "ail_f2_04", "h2_mf2_f1_00", "h2_mf2_f2_03" };

		private static readonly List<List<string>> LockHeadAllHPositions = new List<List<string>>() { firstMaleLockHeadAllHPositions, secondMaleLockHeadAllHPositions, firstFemaleLockHeadAllHPositions, secondFemaleLockHeadAllHPositions };
		private static readonly List<List<string>> LockHeadHPositions = new List<List<string>>() { firstMaleLockHeadHPositions, secondMaleLockHeadHPositions, firstFemaleLockHeadHPositions, secondFemaleLockHeadHPositions };

		private static readonly List<string> lockHeadHMotionExceptions = new List<string>() { "Idle", "_A" };

		internal static readonly string lowerNeckBone = "cf_J_Neck";
		internal static readonly string upperNeckBone = "cf_J_Head";
		internal static readonly string headBone = "cf_J_Head_s";
		internal static readonly string lockBone = "N_Hitai";
		internal static readonly string leftEyeBone = "cf_J_eye_rs_L";
		internal static readonly string rightEyeBone = "cf_J_eye_rs_R";
		internal static readonly string leftEyePupil = "cf_J_pupil_s_L";
		internal static readonly string rightEyePupil = "cf_J_pupil_s_R";

		internal static Transform povUpperNeck;
		internal static Transform povLowerNeck;
		internal static Transform povHead;
		internal static Transform lockTarget;

		public static void Update()
		{
			povSetThisFrame = false;

			if (HS2_PovX.PovKey.Value.IsDown())
				EnablePoV(!povEnabled);

			if (!povEnabled)
				return;

			if (HS2_PovX.CharaCycleKey.Value.IsDown())
			{
				targetFocus = povFocus = GetValidFocus(povFocus + 1);
				SetPoVCharacter(GetValidCharacterFromFocus(ref povFocus));
				SetTargetCharacter(GetValidCharacterFromFocus(ref targetFocus));
			}

			if (HS2_PovX.HeadLockKey.Value.IsDown())
				LockPoVHead(!lockHeadPosition);

			if (HS2_PovX.LockOnKey.Value.IsDown())
			{
				targetFocus = GetValidFocus(targetFocus + 1);
				SetTargetCharacter(GetValidCharacterFromFocus(ref targetFocus));
			}

			if (HS2_PovX.CursorToggleKey.Value.IsDown())
			{
				Cursor.visible = !Cursor.visible;
				Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
			}
			else if (!HS2_PovX.ZoomKey.Value.IsPressed() && !Cursor.visible && Input.anyKeyDown)
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}

			if (povFocus == targetFocus)
				UpdateMouseLook();
		}

		public static void ResetPoVRotations()
        {
			ResetPoVPitch();
			ResetPoVYaw();
		}

		public static void ResetPoVPitch()
		{
			cameraLocalPitch = headLocalPitch = eyeLocalPitch = 0f;
		}

		public static void ResetPoVYaw()
		{
			cameraLocalYaw = headLocalYaw = eyeLocalYaw = 0f;
		}

		public static void EnablePoV(bool enable)
		{
			if (povEnabled == enable)
				return;

			povEnabled = enable;
			if (enable)
			{
				characters = GetSceneCharacters();

				if (!FocusCharacterValid(povFocus))
					targetFocus = povFocus = GetValidFocus(povFocus + 1);

				if (!FocusCharacterValid(targetFocus))
					targetFocus = GetValidFocus(targetFocus + 1);

				SetPoVCharacter(GetValidCharacterFromFocus(ref povFocus));
				SetTargetCharacter(GetValidCharacterFromFocus(ref targetFocus));
				ResetPoVRotations();
				backupFoV = Camera.main.fieldOfView;
			}
			else
			{
				if (HS2_PovX.HSceneLockCursor.Value)
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}

				Camera.main.fieldOfView = backupFoV;
				SetPoVCharacter(null);
				SetTargetCharacter(null);
			}
		}

		public static ChaControl[] GetSceneCharacters()
		{
			return UnityEngine.Object.FindObjectsOfType<ChaControl>();
		}

		public static void SetPoVCharacter(ChaControl character)
		{
			if (povCharacter == character)
				return;

			if (povCharacter != null)
			{
				povUpperNeck.localRotation = Quaternion.identity;
				povLowerNeck.localRotation = Quaternion.identity;
				povHead.localRotation = Quaternion.identity;
				eyeOffset = Vector3.zero;

				if (normalHeadScale != null)
					povCharacter.objHeadBone.transform.localScale = normalHeadScale;
			}

			povCharacter = character;
			if (povCharacter == null)
				return;

			povUpperNeck = povCharacter.GetComponentsInChildren<Transform>().Where(x => x.name.Equals(upperNeckBone)).FirstOrDefault();
			povLowerNeck = povCharacter.GetComponentsInChildren<Transform>().Where(x => x.name.Equals(lowerNeckBone)).FirstOrDefault();
			povHead = povCharacter.GetComponentsInChildren<Transform>().Where(x => x.name.Equals(headBone)).FirstOrDefault();
			normalHeadScale = povCharacter.objHeadBone.transform.localScale;

			CalculateEyesOffset();
			AdjustPoVHeadScale();
			CheckHSceneHeadLock();
		}

		public static void SetTargetCharacter(ChaControl character)
		{
			if (targetCharacter == character)
				return;
		
			targetCharacter = character;
			if (targetCharacter == null)
				return;

			lockTarget = targetCharacter.GetComponentsInChildren<Transform>().Where(x => x.name.Equals(lockBone)).FirstOrDefault();
		}

		public static int GetValidFocus(int focus)
		{
			if (focus >= characters.Length)
				focus %= characters.Length;

			for (int i = 0; i < characters.Length; i++)
			{
				if (FocusCharacterValid(focus))
					return focus;

				// Skip invisible or destroyed characters.
				focus = (focus + 1) % characters.Length;
			}

			return focus;
		}

		public static bool FocusCharacterValid(int focus)
		{
			if (focus >= characters.Length)
				return false;

			var focusCharacter = characters[focus];
			if (focusCharacter != null && focusCharacter.visibleAll)
				return true;

			return false;
		}

		public static ChaControl GetValidCharacterFromFocus(ref int focus)
		{
			if (characters.Length == 0)
				return null;

			focus = GetValidFocus(focus);
			return characters[focus];
		}

		public static void UpdateTargetLockedCamera(Transform head)
		{
			UpdateCamera(head);
			Camera.main.transform.LookAt(lockTarget.position, Vector3.up);
		}

		public static void UpdateCamera(Transform head, Vector3 offsetRotation)
		{
			UpdateCamera(head);

			if (HS2_PovX.CameraNormalize.Value)
				Camera.main.transform.rotation = Quaternion.Euler(head.eulerAngles.x, head.eulerAngles.y, 0);
			else
				Camera.main.transform.rotation = head.rotation;

			Camera.main.transform.Rotate(offsetRotation);		
		}

		public static void UpdateCamera(Transform head)
		{
			Camera.main.fieldOfView =
				HS2_PovX.ZoomKey.Value.IsPressed() ?
					HS2_PovX.ZoomFov.Value :
					HS2_PovX.Fov.Value;

			Camera.main.transform.position =
				head.position +
				(HS2_PovX.OffsetX.Value + eyeOffset.x) * head.right +
				(HS2_PovX.OffsetY.Value + eyeOffset.y) * head.up +
				(HS2_PovX.OffsetZ.Value + eyeOffset.z) * head.forward;

			Camera.main.nearClipPlane = HS2_PovX.NearClip.Value;
		}

		public static void UpdatePoVCamera()
		{
			UpdatePoVHScene();
			povSetThisFrame = true;
		}

		public static void UpdatePoVHScene()
		{
			if (povFocus != targetFocus)
			{
				UpdateTargetLockedCamera(povHead);
				return;
			}

			if (!lockHeadPosition)
				UpdateNeckRotations();

			UpdateCamera(povHead, new Vector3(eyeLocalPitch, eyeLocalYaw, 0f));
		}

		public static void CheckHSceneHeadLock(string hMotion = null)
		{
			if (!HS2_PovX.HSceneAutoHeadLock.Value ||hScene == null || povFocus >= LockHeadHPositions.Count)
				return;

			string currentHAnimation = hScene.ctrlFlag.nowAnimationInfo.fileFemale;

			if (hMotion != null)
				currentHMotion = hMotion;

			if (currentHAnimation == null || currentHMotion == null)
				return;

			if (LockHeadAllHPositions[povFocus].Contains(currentHAnimation) ||
				(LockHeadHPositions[povFocus].Contains(currentHAnimation) && !lockHeadHMotionExceptions.Contains(currentHMotion)))
				LockPoVHead(true);
			else
				LockPoVHead(false);
		}

		public static void LockPoVHead(bool locked)
        {
			lockHeadPosition = locked;

			if (locked)
				ResetPoVRotations();

		}

		public static void AdjustPoVHeadScale()
		{
			if (povCharacter == null)
				return;

			if (Tools.ShouldHideHead())
				povCharacter.objHeadBone.transform.localScale = new Vector3(povCharacter.objHeadBone.transform.localScale.x, povCharacter.objHeadBone.transform.localScale.y, HS2_PovX.HideHeadScaleZ.Value);
			else
				povCharacter.objHeadBone.transform.localScale = normalHeadScale;
		}

		public static void CalculateEyesOffset()
		{
			if (povCharacter == null)
				return;

			eyeOffset = Tools.GetEyesOffset(povCharacter);
		}

		private static void UpdateNeckRotations()
		{
			if (povUpperNeck == null || povLowerNeck == null)
				return;

			povLowerNeck.localRotation = Quaternion.Euler(headLocalPitch / 2, headLocalYaw / 2, 0);
			povUpperNeck.localRotation = Quaternion.Euler(headLocalPitch / 2, headLocalYaw / 2, 0);
		}

		private static void UpdateMouseLook()
		{
			if (Cursor.lockState == CursorLockMode.None && !HS2_PovX.CameraDragKey.Value.IsPressed())
				return;

			float sensitivity = HS2_PovX.Sensitivity.Value;

			if (HS2_PovX.ZoomKey.Value.IsPressed())
				sensitivity *= HS2_PovX.ZoomFov.Value / HS2_PovX.Fov.Value;

			float mouseY = Input.GetAxis("Mouse Y") * sensitivity;
			float mouseX = Input.GetAxis("Mouse X") * sensitivity;

			if (lockHeadPosition)
			{
				eyeLocalPitch = cameraLocalPitch = Mathf.Clamp(cameraLocalPitch - mouseY, -(HS2_PovX.EyeMaxPitch.Value), (HS2_PovX.EyeMaxPitch.Value));
				eyeLocalYaw = cameraLocalYaw = Mathf.Clamp(cameraLocalYaw + mouseX, -(HS2_PovX.EyeMaxYaw.Value), (HS2_PovX.EyeMaxYaw.Value));
				headLocalPitch = 0;
				headLocalYaw = 0;
			}
			else
			{
				cameraLocalPitch = Mathf.Clamp(cameraLocalPitch - mouseY, -(HS2_PovX.EyeMaxPitch.Value + HS2_PovX.HeadMaxPitch.Value), (HS2_PovX.EyeMaxPitch.Value + HS2_PovX.HeadMaxPitch.Value));
				cameraLocalYaw = Mathf.Clamp(cameraLocalYaw + mouseX, -(HS2_PovX.EyeMaxYaw.Value + HS2_PovX.HeadMaxYaw.Value), (HS2_PovX.EyeMaxYaw.Value + HS2_PovX.HeadMaxYaw.Value));
				headLocalPitch = cameraLocalPitch * HS2_PovX.HeadMaxPitch.Value / (HS2_PovX.EyeMaxPitch.Value + HS2_PovX.HeadMaxPitch.Value);
				headLocalYaw = cameraLocalYaw * HS2_PovX.HeadMaxYaw.Value / (HS2_PovX.EyeMaxYaw.Value + HS2_PovX.HeadMaxYaw.Value);
				eyeLocalPitch = cameraLocalPitch - headLocalPitch;
				eyeLocalYaw = cameraLocalYaw - headLocalYaw;
			}
		}
	}
}
