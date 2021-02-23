using AIChara;
using HarmonyLib;

namespace HS2_PovX
{
	public partial class HS2_PovX
	{
		[HarmonyPrefix, HarmonyPatch(typeof(NeckLookControllerVer2), "LateUpdate")]
		public static bool Prefix_NeckLookControllerVer2_LateUpdate(NeckLookControllerVer2 __instance)
		{
			if (!Controller.povEnabled ||
				Controller.povCharacter == null ||
				Controller.povSetThisFrame ||
				Controller.povCharacter.neckLookCtrl.enabled && __instance != Controller.povCharacter.neckLookCtrl)
				return true;

			Controller.UpdatePoVCamera();
			return false;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(HScene), "SetStartVoice")]
		public static void HScene_Post_SetStartVoice(HScene __instance)
        {
			Controller.hScene = __instance;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(HScene), "ChangeAnimation")]
		public static void HScene_Post_ChangeAnimation()
		{
			Controller.CheckHSceneHeadLock();
		}

		[HarmonyPostfix, HarmonyPatch(typeof(HScene), "SetMovePositionPoint")]
		public static void HScene_Post_SetMovePositionPoint()
        {
			Controller.CheckHSceneHeadLock();
		}

		[HarmonyPostfix, HarmonyPatch(typeof(ChaControl), "setPlay")]
		public static void ChaControl_Post_SetPlay(string _strAnmName)
		{
			if (_strAnmName.IsNullOrEmpty())
				return;

			Controller.CheckHSceneHeadLock(_strAnmName);
		}

		[HarmonyPostfix, HarmonyPatch(typeof(ChaControl), "AnimPlay")]
		public static void ChaControl_Post_AnimPlay(string stateName)
		{
			if (stateName.IsNullOrEmpty())
				return;

			Controller.CheckHSceneHeadLock(stateName);
		}

		[HarmonyPostfix, HarmonyPatch(typeof(ChaControl), "syncPlay", typeof(string), typeof (int), typeof (float))]
		public static void ChaControl_Post_SyncPlay(string _strameHash)
		{
			if (_strameHash.IsNullOrEmpty())
				return;

			Controller.CheckHSceneHeadLock(_strameHash);
		}

		[HarmonyPrefix, HarmonyPatch(typeof(CameraControl_Ver2), "LateUpdate")]
		public static bool Prefix_CameraControl_Ver2_LateUpdate()
		{
			return !Controller.povEnabled;
		}
	}
}
