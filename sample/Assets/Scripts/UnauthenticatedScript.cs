using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Immutable.Passport;
using Immutable.Passport.Model;

public class UnauthenticatedScript : MonoBehaviour
{
#pragma warning disable CS8618
    [SerializeField] private Text Output;

    [SerializeField] private Button ConnectButton;
    [SerializeField] private Button TryAgainButton;

    private Passport passport;
#pragma warning restore CS8618

    async void Start()
    {
        try
        {
            ShowOutput("Starting...");
            ConnectButton.gameObject.SetActive(false);
            TryAgainButton.gameObject.SetActive(false);

            string clientId = "ZJL7JvetcDFBNDlgRs5oJoxuAUUl6uQj";
            string environment = Immutable.Passport.Model.Environment.SANDBOX;
            string redirectUri = null;
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_STANDALONE_OSX
            redirectUri = "imxsample://callback";
#endif

            passport = await Passport.Init(
#if UNITY_STANDALONE_WIN
                clientId, environment, redirectUri, 10000
#else
                clientId, environment, redirectUri
#endif
                );

            // Check if user's logged in before
            bool hasCredsSaved = await passport.HasCredentialsSaved();
            if (hasCredsSaved)
            {
                // Use existing credentials to connect to Passport
                ShowOutput("Connecting to Passport using saved credentials...");

                bool connected = await passport.ConnectImxSilent();
                if (connected)
                {
                    // Successfully connected to Passport
                    NavigateToAuthenticatedScene();
                }
                else
                {
                    // Could not connect to Passport, enable connect button
                    ConnectButton.gameObject.SetActive(true);
                    ShowOutput("Failed to connect using saved credentials");
                }
            }
            else
            {
                // No existing credentials to use to connect
                ShowOutput("Ready");
                // Enable connect button
                ConnectButton.gameObject.SetActive(true);
            }
        }
        catch (Exception ex)
        {
            ShowOutput($"Start() error: {ex.Message}");
            TryAgainButton.gameObject.SetActive(true);
        }
    }

    public void OnTryAgain()
    {
        Start();
    }

    public async void Connect()
    {
        try
        {
            ShowOutput("Called Connect()...");
            ConnectButton.gameObject.SetActive(false);

            // macOS editor (play scene) does not support deeplinking
#if UNITY_ANDROID || UNITY_IPHONE || (UNITY_STANDALONE_OSX && !UNITY_EDITOR_OSX)
            await passport.ConnectImxPKCE();
#else
            await passport.ConnectImx();
#endif

            NavigateToAuthenticatedScene();
        }
        catch (Exception ex)
        {
            string error;
            if (ex is PassportException passportException && passportException.IsNetworkError())
            {
                error = $"Connect() error: Check your internet connection and try again";
            }
            else if (ex is OperationCanceledException)
            {
                error = "Connect() cancelled";
            }
            else
            {
                error = $"Connect() error: {ex.Message}";
                // Restart everything
                await passport.Logout();
            }

            Debug.Log(error);
            ShowOutput(error);
#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX
            ConnectButton.gameObject.SetActive(true);
#endif
        }
    }

    private void NavigateToAuthenticatedScene()
    {
        SceneManager.LoadScene(sceneName: "AuthenticatedScene");
    }

    private void ShowOutput(string message)
    {
        Debug.Log($"Output: {message}");
        if (Output != null)
        {
            Output.text = message;
        }
    }
}
