using UnityEngine;
using TMPro;
using System.Collections;

public class Clicks : MonoBehaviour
{

    [SerializeField] TMP_InputField UserSU;
    [SerializeField] TMP_InputField PassSU;
    [SerializeField] TMP_InputField UserLI;
    [SerializeField] TMP_InputField PassLI;
    [SerializeField] TextMeshProUGUI warnings;
    private Auth auth;

    public void Awake(){
        auth = FindAnyObjectByType<Auth>().GetComponent<Auth>();
    }

    public void SU(){
        if(IsPassValid(PassSU.text)){
            auth.SUAsynch(UserSU.text, PassSU.text);
        }
        else{
        Debug.Log(PassSU.text);
            StartCoroutine(WarningText("Invalid Password, remember:\n1 Uppercase\n1 lowercase\n1 Digit\n 1 Symbol\n 8 to 30 characters"));
        }
    }
    public void LI(){
        auth.LIAsynch(UserLI.text, PassLI.text);
    }


    IEnumerator WarningText(string message){
        warnings.text = message;
        yield return new WaitForSeconds(6);
        warnings.text = "";
    }
    

    private bool IsPassValid(string pass){
        if (pass.Length<8 || pass.Length>30){
            return false;
        }

        bool hasUpercase = false;
        bool hasLowercase = false;
        bool hasDigit = false;
        bool hasSymbol = false;

        foreach (char c in pass)
        {
            if(char.IsUpper(c)){
                hasUpercase = true;
            }
            if(char.IsLower(c)){
                hasLowercase = true;
            }
            if (char.IsDigit(c)){
                hasDigit = true;
            }
            if (!char.IsLetterOrDigit(c)){
                hasSymbol=true;
            }
        }
        return hasUpercase && hasLowercase && hasDigit && hasSymbol;
    }
}
