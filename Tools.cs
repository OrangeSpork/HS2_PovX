using AIChara;
using System.Linq;
using UnityEngine;

namespace HS2_PovX
{
    public static class Tools
	{
		public static bool ShouldHideHead()
		{
			return Controller.povEnabled && HS2_PovX.HideHead.Value;
		}

		// Return the offset of the eyes in the head's object space.
		public static Vector3 GetEyesOffset(ChaControl chaCtrl)
		{
			Transform head = chaCtrl.GetComponentsInChildren<Transform>().Where(x => x.name.Equals(Controller.headBone)).FirstOrDefault();

			Transform[] eyes = new Transform[2];
			eyes[0] = chaCtrl.GetComponentsInChildren<Transform>().Where(x => x.name.Equals(Controller.leftEyePupil)).FirstOrDefault();
			eyes[1] = chaCtrl.GetComponentsInChildren<Transform>().Where(x => x.name.Equals(Controller.rightEyePupil)).FirstOrDefault();

			if (HS2_PovX.CameraPoVLocation.Value == HS2_PovX.CameraLocation.LeftEye)
				return GetEyesOffsetInternal(head, eyes[0]);
			else if (HS2_PovX.CameraPoVLocation.Value == HS2_PovX.CameraLocation.RightEye)
				return GetEyesOffsetInternal(head, eyes[1]);

			return Vector3.Lerp(
				GetEyesOffsetInternal(head, eyes[0]),
				GetEyesOffsetInternal(head, eyes[1]),
				0.5f);
		}
		
		private static Vector3 GetEyesOffsetInternal(Transform head, Transform eye)
		{
			Vector3 offset = Vector3.zero;

			for (int bone = 0; bone < 50; bone++)
			{
				if (eye == null || eye == head)
					break;

				offset += eye.localPosition;
				eye = eye.parent;
			}

			return offset;
		}
	}
}
