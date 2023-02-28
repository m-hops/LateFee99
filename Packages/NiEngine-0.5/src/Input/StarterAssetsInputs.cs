using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Nie
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
		bool m_LockControls = true;
        public void LockControls()
        {
			cursorLocked = true;
            cursorInputForLook = true;
			m_LockControls = true;
			move = Vector3.zero;
			look = Vector3.zero;
			jump = false;
			sprint = false;
			SetCursorState(cursorLocked);

        }
        public void UnlockControls()
        {
            cursorLocked = false;
            cursorInputForLook = false;
            m_LockControls = false;
            SetCursorState(cursorLocked);

        }
#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            if (!m_LockControls) return;
            MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
        {
            if (!m_LockControls) return;
            if (cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			if (!m_LockControls) return;
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
        {
            if (!m_LockControls) return;
            SprintInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
		
		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}