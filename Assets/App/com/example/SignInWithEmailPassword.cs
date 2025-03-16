using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using TMPro;
using UnityEngine;
namespace com.example
{
	public class SignInWithEmailPassword : MonoBehaviour
	{
		// Public Unity References
		public TMP_InputField EmailInput = null!;
		public TMP_InputField PasswordInput = null!;
		public TMP_Text ErrorText = null!;
		public SupabaseManager SupabaseManager = null!;

		// Private implementation
		private bool _doSignUp;
		private bool _doSignOut;
		private bool _doSignIn;

		// Unity does not allow async UI events, so we set a flag and use Update() to do the async work
		public void SignUp()
		{
			_doSignUp = true;
		}

		// Unity does not allow async UI events, so we set a flag and use Update() to do the async work
		public void SignOut()
		{
			_doSignOut = true;
		}

		public void SignIn()
		{
			_doSignIn = true;
		}

		[SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
		private async void Update()
		{
			// Unity does not allow async UI events, so we set a flag and use Update() to do the async work
			if (_doSignOut)
			{
				_doSignOut = false;
				await SupabaseManager.Supabase()!.SignOut();
				_doSignOut = false;
			}

			// Unity does not allow async UI events, so we set a flag and use Update() to do the async work
			if (_doSignUp)
			{
				_doSignUp = false;
				await PerformSignUp();
				_doSignUp = false;
			}

			if (_doSignIn)
            {
                _doSignIn = false;
                await PerformSignIn();
                _doSignIn = false;
            }
        }

		// This is where we do the async work and handle exceptions
		[SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
		private async Task PerformSignUp()
		{
			try
			{
				Session session = (await SupabaseManager.Supabase()!.SignUp(EmailInput.text, PasswordInput.text))!;
				ErrorText.text = $"Success! Signed Up as {session.User?.Email}";
            }
			catch (GotrueException goTrueException)
			{
				ErrorText.text = $"{goTrueException.Reason} {goTrueException.Message}";
				Debug.Log(goTrueException.Message, gameObject);
				Debug.LogException(goTrueException, gameObject);
			}
			catch (Exception e)
			{
				Debug.Log(e.Message, gameObject);
				Debug.Log(e, gameObject);
			}
		}

        // This is where we do the async work and handle exceptions
        [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
        private async Task PerformSignIn()
        {
            try
            {
                Session session = (await SupabaseManager.Supabase()!.SignIn(EmailInput.text, PasswordInput.text))!;
                ErrorText.text = $"Success! Signed In as {session.User?.Email}";
            }
            catch (GotrueException goTrueException)
            {
                ErrorText.text = $"{goTrueException.Reason} {goTrueException.Message}";
                Debug.Log(goTrueException.Message, gameObject);
                Debug.LogException(goTrueException, gameObject);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message, gameObject);
                Debug.Log(e, gameObject);
            }
        }
    }
}
