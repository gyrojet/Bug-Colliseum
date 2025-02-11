using UnityEngine;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System.Collections;


public class Auth : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI warnings;
    //goomoonryong
    //Aa111111$
    // private LoadingMenu lm;

    #region  Singleton Shit

    private bool initialized = false;
    private bool eventsinitialize = false;

    private static Auth singleton =null;


    public static Auth Singleton{
        get{
            if(singleton == null){
                singleton= FindFirstObjectByType<Auth>();
                singleton.Initialize();
            }
            return singleton;
        }
    }

    private void Initialize(){
        if (initialized){return;}
        initialized = true;
    }

    private void Destroy(){
        if (singleton==this){
            singleton=null;
        }
    }

    #endregion

    private async void Awake(){
        Application.runInBackground = true;
        // lm = FindAnyObjectByType<LoadingMenu>();

        
        StartClientService();
    }

    public async void StartClientService(){
        // lm.ClosePanel();
        // lm.OpenPanel();
        try
        {
            if(UnityServices.State != ServicesInitializationState.Initialized){
                InitializationOptions options = new InitializationOptions();
                options.SetProfile("DefaultProfile");
                await UnityServices.InitializeAsync();
            }
            if(!eventsinitialize){
                SetUpEvents();
            }
            // if(AuthenticationService.Instance.SessionTokenExists){
            //     SignInAnonymouslyAsync();
            //     lm.ClosePanel();
            // }
            // else{
            //     Debug.Log("Token doesn't exist");
            //     lm.ClosePanel();
            // }
        }
        catch (Exception ex)
        {
            // Debug.LogException(ex);
            StartCoroutine(WarningText(ex.Message));
        }
    }

    public async void SignInAnonymouslyAsync(){
        // lm.OpenPanel();
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Token exists");
            Debug.Log(AuthenticationService.Instance.PlayerId);
        }
        catch (AuthenticationException ex)
        {
            // Debug.LogException(ex);
            StartCoroutine(WarningText(ex.Message));
        }
        catch(RequestFailedException ex){
            // Debug.LogException(ex);
            StartCoroutine(WarningText(ex.Message));
        }

    }

    private void SetUpEvents(){
        eventsinitialize=true;
        AuthenticationService.Instance.SignedIn += ()=>{
            
        };

        AuthenticationService.Instance.SignedOut += ()=>{

        };

        AuthenticationService.Instance.Expired += ()=>{

        };
    }

    public async void SUAsynch(string u, string p){
        await SignUpWithUsernamePasswordAsync(u, p);
    }
    async Task SignUpWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log("SignUp is successful.");
            SceneManager.LoadScene(1);
        }
        catch (AuthenticationException ex)
        {
            Debug.Log("cat");

            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            // Debug.LogException(ex);
            StartCoroutine(WarningText(ex.Message));

        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            // Debug.LogException(ex);
            StartCoroutine(WarningText(ex.Message));
        }
    }

    public async void LIAsynch(string u, string p){
        await SignInWithUsernamePasswordAsync(u, p);
    }
    async Task SignInWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            Debug.Log("SignIn is successful.");
            SceneManager.LoadScene(1);
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            // Debug.LogException(ex);
            StartCoroutine(WarningText(ex.Message));
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            // Debug.LogException(ex);
            StartCoroutine(WarningText(ex.Message));
        }
    }

    IEnumerator WarningText(string message){
        warnings.text = message;
        yield return new WaitForSeconds(6);
        warnings.text = "";
    }
   
}
