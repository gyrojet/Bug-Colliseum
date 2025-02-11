using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject UI;
    [SerializeField] TextMeshProUGUI code;
    bool done = true;


    private void Update(){
        if (code.text != "Code" && done){
            UI.SetActive(false);
            done = false;
        }
    }

}
