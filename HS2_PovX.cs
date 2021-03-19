using UnityEngine;
using Manager;
using BepInEx;
using BepInEx.Configuration;

namespace HS2_PovX
{
	[BepInProcess("HoneySelect2")]
	[BepInProcess("StudioNEOV2")]
	[BepInPlugin(GUID, Name, Version)]
	public partial class HS2_PovX : BaseUnityPlugin
	{
		const string GUID = "com.2155x.fairbair.hs2_povx";
		const string Name = "HS2 PoV X";
		const string Version = "1.2.2";

		const string SECTION_GENERAL = "General";
		const string SECTION_CAMERA = "Camera";
		const string SECTION_ANIMATION = "Animation";
		const string SECTION_HOTKEYS = "Hotkeys";

		const string DESCRIPTION_H_SCENE_LOCK_CURSOR =
			"Should the cursor be locked during H scenes? " +
			"Use the 'Cursor Release Key' to reveal the cursor. " +
			"This also include situations where the focus is not the player.";

		const string DESCRIPTION_OFFSET_X =
			"Sideway offset from the character's eyes.";
		const string DESCRIPTION_OFFSET_Y =
			"Vertical offset from the character's eyes.";
		const string DESCRIPTION_OFFSET_Z =
			"Forward offset from the character's eyes.";
		const string DESCRIPTION_CAMERA_NORMALIZE =
			"Stops the camera from tilting. " +
			"Always enabled when locked-on.";

		const string DESCRIPTION_HEAD_MAX_PITCH =
			"Highest upward/downward angle the head can rotate.";
		const string DESCRIPTION_HEAD_MAX_YAW =
			"The farthest the head can rotate left/right";
		const string DESCRIPTION_EYE_MAX_PITCH =
			"Highest upward/downward angle the eyes can rotate.";
		const string DESCRIPTION_EYE_MAX_YAW =
			"The farthest the eyes can rotate left/right";

		const string DESCRIPTION_CHARA_CYCLE_KEY =
			"Switch between characters during PoV mode.";
		const string DESCRIPTION_LOCK_ON_KEY =
			"Lock-on to any of the other characters during PoV mode. " +
			"Press again to cycle between characters or exit lock-on mode.";
		const string DESCRIPTION_CAMERA_DRAG_KEY =
			"During PoV mode, holding down this key will move the camera if the mouse isn't locked.";
		const string DESCRIPTION_CURSOR_TOGGLE_KEY =
			"Pressing this key will force the cursor to be revealed in any scenes. " +
			"Press the key again to turn off.";
		const string DESCRIPTION_HIDE_HEAD =
			"Should the head be invisible when in PoV mode? " +
			"Head is always invisible during H scenes or " +
			"situations where the player can't move.";
		const string DESCRIPTION_HIDE_HEAD_SCALE_Z =
			"Amount to scale Z when hiding head.";
		const string DESCRIPTION_CAMERA_LOCK_HEAD_KEY =
			"During PoV mode in HScenes, pressing this key will lock/unlock the characters head to the default animation position.";

		internal static ConfigEntry<bool> HideHead { get; set; }
		internal static ConfigEntry<float> HideHeadScaleZ { get; set; }
		internal static ConfigEntry<bool> HSceneLockCursor { get; set; }

		internal static ConfigEntry<float> Sensitivity { get; set; }
		internal static ConfigEntry<float> NearClip { get; set; }
		internal static ConfigEntry<float> Fov { get; set; }
		internal static ConfigEntry<float> ZoomFov { get; set; }
		internal static ConfigEntry<float> OffsetX { get; set; }
		internal static ConfigEntry<float> OffsetY { get; set; }
		internal static ConfigEntry<float> OffsetZ { get; set; }
		internal static ConfigEntry<CameraLocation> CameraPoVLocation { get; set; }

		internal static ConfigEntry<float> HeadMaxPitch { get; set; }
		internal static ConfigEntry<float> HeadMaxYaw { get; set; }
		internal static ConfigEntry<float> EyeMaxPitch { get; set; }
		internal static ConfigEntry<float> EyeMaxYaw { get; set; }
		internal static ConfigEntry<bool> CameraNormalize { get; set; }
		internal static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> PovKey { get; set; }
		internal static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> CharaCycleKey { get; set; }
		internal static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> CameraDragKey { get; set; }
		internal static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> CursorToggleKey { get; set; }
		internal static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> ZoomKey { get; set; }
		internal static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> HeadLockKey { get; set; }
		internal static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> LockOnKey { get; set; }
		public enum CameraLocation
        {
			Center,
			LeftEye,
			RightEye
        }

		internal void Awake()
		{
			HideHead = Config.Bind(SECTION_GENERAL, "Hide Head", false, DESCRIPTION_HIDE_HEAD);
			HideHeadScaleZ = Config.Bind(SECTION_GENERAL, "Hide Head Scale Z", 0.5f, new ConfigDescription(DESCRIPTION_HIDE_HEAD_SCALE_Z, new AcceptableValueRange<float>(0f, 1f)));
			HSceneLockCursor = Config.Bind(SECTION_GENERAL, "Lock Cursor During H Scenes", false, DESCRIPTION_H_SCENE_LOCK_CURSOR);

			Sensitivity = Config.Bind(SECTION_CAMERA, "Camera Sensitivity", 2f);
			NearClip = Config.Bind(SECTION_CAMERA, "Camera Near Clip Plane", 0.1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 2f)));
			Fov = Config.Bind(SECTION_CAMERA, "Field of View", 60f);
			ZoomFov = Config.Bind(SECTION_CAMERA, "Zoom Field of View", 15f);
			OffsetX = Config.Bind(SECTION_CAMERA, "Offset X", 0f, DESCRIPTION_OFFSET_X);
			OffsetY = Config.Bind(SECTION_CAMERA, "Offset Y", 0f, DESCRIPTION_OFFSET_Y);
			OffsetZ = Config.Bind(SECTION_CAMERA, "Offset Z", 0f, DESCRIPTION_OFFSET_Z);
			CameraPoVLocation = Config.Bind(SECTION_CAMERA, "Camera Location", CameraLocation.Center);
			CameraNormalize = Config.Bind(SECTION_CAMERA, "Normalize Camera Z-Axis", false, DESCRIPTION_CAMERA_NORMALIZE);

			HeadMaxPitch = Config.Bind(SECTION_ANIMATION, "Max Head Pitch (Up/Down)", 50f, DESCRIPTION_HEAD_MAX_PITCH);
			HeadMaxYaw = Config.Bind(SECTION_ANIMATION, "Max Head Yaw (Left/Right)", 60f, DESCRIPTION_HEAD_MAX_YAW);
			EyeMaxPitch = Config.Bind(SECTION_ANIMATION, "Max Eye Pitch (Up/Down)", 30f, DESCRIPTION_EYE_MAX_PITCH);
			EyeMaxYaw = Config.Bind(SECTION_ANIMATION, "Max Eye Yaw (Left/Right)", 35f, DESCRIPTION_EYE_MAX_YAW);

			PovKey = Config.Bind(SECTION_HOTKEYS, "PoV Toggle Key", new BepInEx.Configuration.KeyboardShortcut(KeyCode.Comma));
			CharaCycleKey = Config.Bind(SECTION_HOTKEYS, "Character Cycle Key", new BepInEx.Configuration.KeyboardShortcut(KeyCode.Period), DESCRIPTION_CHARA_CYCLE_KEY);
			CameraDragKey = Config.Bind(SECTION_HOTKEYS, "Camera Drag Key", new BepInEx.Configuration.KeyboardShortcut(KeyCode.Mouse0), DESCRIPTION_CAMERA_DRAG_KEY);
			CursorToggleKey = Config.Bind(SECTION_HOTKEYS, "Cursor Toggle Key", new BepInEx.Configuration.KeyboardShortcut(KeyCode.LeftControl), DESCRIPTION_CURSOR_TOGGLE_KEY);
			ZoomKey = Config.Bind(SECTION_HOTKEYS, "Zoom Key", new BepInEx.Configuration.KeyboardShortcut(KeyCode.Z));
			HeadLockKey = Config.Bind(SECTION_HOTKEYS, "Lock Head Key", new BepInEx.Configuration.KeyboardShortcut(KeyCode.X), DESCRIPTION_CAMERA_LOCK_HEAD_KEY);
			LockOnKey = Config.Bind(SECTION_HOTKEYS, "Lock-On Key", new BepInEx.Configuration.KeyboardShortcut(KeyCode.Semicolon), DESCRIPTION_LOCK_ON_KEY);

			CameraPoVLocation.SettingChanged += (sender, args) => { Controller.CalculateEyesOffset(); };
			HideHead.SettingChanged += (sender, args) => { Controller.AdjustPoVHeadScale(); };
			HideHeadScaleZ.SettingChanged += (sender, args) => { Controller.AdjustPoVHeadScale(); };

			HSceneLockCursor.SettingChanged += (sender, args) =>
			{
				if (Controller.povEnabled && !HSceneLockCursor.Value)
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}
			};

			HarmonyLib.Harmony.CreateAndPatchAll(typeof(HS2_PovX));
		}

		public void Update()
		{
			Controller.Update();
		}
	}
}
