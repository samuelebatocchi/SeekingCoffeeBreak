using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;

public class PlayerAccountsAuthManager : MonoBehaviour
{
    [SerializeField] private Button loginButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private TextMeshProUGUI loggedInText;

    async void Start()
    {
        await UnityServices.InitializeAsync();

        PlayerAccountService.Instance.SignedIn += OnPlayerAccountsSignedIn;

        if (AuthenticationService.Instance.IsSignedIn)
        {
            await ShowLoggedInState();
            await SyncCloudDataIfAvailable();
        }
        else
        {
            ShowLoggedOutState();
        }
    }

    public void SignInWithGoogle()
    {
        PlayerAccountService.Instance.StartSignInAsync();
    }

    public void Logout()
    {
        // Esce sia da Unity Player Accounts che da Unity Authentication,
        // così al prossimo accesso viene chiesto di rifare il login da capo
        PlayerAccountService.Instance.SignOut();
        AuthenticationService.Instance.SignOut(true);

        if (CloudDataSync.Instance != null)
        {
            CloudDataSync.Instance.Reset();
        }

        ShowLoggedOutState();
        Debug.Log("Logout effettuato");
    }

    private async void OnPlayerAccountsSignedIn()
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUnityAsync(
                PlayerAccountService.Instance.AccessToken);

            Debug.Log($"Login riuscito! Player ID: {AuthenticationService.Instance.PlayerId}");

            await ShowLoggedInState();
            await SyncCloudDataIfAvailable();
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    private async System.Threading.Tasks.Task SyncCloudDataIfAvailable()
    {
        if (CloudDataSync.Instance != null)
        {
            await CloudDataSync.Instance.SyncOnLoginAsync();
        }
        else
        {
            Debug.LogWarning("CloudDataSync non inizializzato al momento del login: il sync è stato saltato.");
        }
    }

    private async System.Threading.Tasks.Task ShowLoggedInState()
    {
        string playerName = await AuthenticationService.Instance.GetPlayerNameAsync();

        loginButton.gameObject.SetActive(false);
        logoutButton.gameObject.SetActive(true);
        loggedInText.gameObject.SetActive(true);
        loggedInText.text = $"Logged as {playerName}";
    }

    private void ShowLoggedOutState()
    {
        loginButton.gameObject.SetActive(true);
        logoutButton.gameObject.SetActive(false);
        loggedInText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        PlayerAccountService.Instance.SignedIn -= OnPlayerAccountsSignedIn;
    }
}