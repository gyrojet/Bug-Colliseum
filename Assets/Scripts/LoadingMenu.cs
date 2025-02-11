using UnityEngine;

public class LoadingMenu : MonoBehaviour
{
    

    public void ClosePanel(){
        gameObject.SetActive(false);
    }
    public void OpenPanel(){
        gameObject.SetActive(true);
    }
}
