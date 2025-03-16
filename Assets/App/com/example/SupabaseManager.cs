using System;
using System.Collections.Generic;
using Supabase;
using Supabase.Gotrue;
using TMPro;
using UnityEngine;
using Client = Supabase.Gotrue.Client;

namespace com.example
{
	public class SupabaseManager : MonoBehaviour
	{

		// Public Unity references
		public SessionListener SessionListener = null!;

		public TMP_Text ErrorText = null!;

		// Public in case other components are interested in network status
		private readonly NetworkStatus _networkStatus = new();

		// Internals
		private Client _client;

		public Client Supabase() => _client;

        private const string supabaseUrl = "https://eocuqtmbedabexutmuyz.supabase.co";
        private const string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImVvY3VxdG1iZWRhYmV4dXRtdXl6Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDIwNjE2NTgsImV4cCI6MjA1NzYzNzY1OH0.ARlDLi01Pu6kghyFsfqXtuwZUb1KBjK2P1IaCqM14BY";

        private async void Start()
		{
			SupabaseOptions options = new();
			// We set an option to refresh the token automatically using a background thread.
			options.AutoRefreshToken = true;
            Client client = new Client(new ClientOptions
            {
                AutoRefreshToken = true,
                Url = supabaseUrl + "/auth/v1",
                Headers = new Dictionary<string, string>
				{
					{ "apikey", supabaseAnonKey }
				}
            });


			// The first thing we do is attach the debug listener
			client.AddDebugListener(DebugListener!);

			// Next we set up the network status listener and tell it to turn the client online/offline
			_networkStatus.Client = client;

			// Next we set up the session persistence - without this the client will forget the session
			// each time the app is restarted
            client.SetPersistence(new UnitySession());

			// This will be called whenever the session changes
			client.AddStateChangedListener(SessionListener.UnityAuthListener);

			// Fetch the session from the persistence layer
			// If there is a valid/unexpired session available this counts as a user log in
			// and will send an event to the UnityAuthListener above.
			client.LoadSession();

			// Allow unconfirmed user sessions. If you turn this on you will have to complete the
			// email verification flow before you can use the session.
			client.Options.AllowUnconfirmedUserSessions = true;

			// We check the network status to see if we are online or offline using a request to fetch
			// the server settings from our project. Here's how we build that URL.
			string url = $"{supabaseUrl}/auth/v1/settings?apikey={supabaseAnonKey}";
			try
			{
				// This will get the current network status
				client.Online = await _networkStatus.StartAsync(url);
			}
			catch (NotSupportedException)
			{
				// Some platforms don't support network status checks, so we just assume we are online
				client.Online = true;
			}
			catch (Exception e)
			{
				// Something else went wrong, so we assume we are offline
				ErrorText.text = e.Message;
				Debug.Log(e.Message, gameObject);
				Debug.LogException(e, gameObject);

				client.Online = false;
			}

			if (client.Online)
			{
                // Now we start up the client, which will in turn start up the background thread.
                // This will attempt to refresh the session token, which in turn may send a second
                // user login event to the UnityAuthListener.
                await client.RetrieveSessionAsync();	// TODO: this always expires...

				// Here we fetch the server settings and log them to the console
				Settings serverConfiguration = (await client.Settings())!;
				Debug.Log($"Auto-confirm emails on this server: {serverConfiguration.MailerAutoConfirm}");
			}
			_client = client;
		}

		private void DebugListener(string message, Exception e)
		{
			ErrorText.text = message;
			Debug.Log(message, gameObject);
			// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
			if (e != null)
				Debug.LogException(e, gameObject);
		}

		// This is called when Unity shuts down. You want to be sure to include this so that the
		// background thread is terminated cleanly. Keep in mind that if you are running the app
		// in the Unity Editor, if you don't call this method you will leak the background thread!
		private void OnApplicationQuit()
		{
			if (_client != null)
			{
				_client?.Shutdown();
				_client = null;
			}
		}
	}
}
