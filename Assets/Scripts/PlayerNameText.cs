// PlayerNameText.cs:
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameText : MonoBehaviour
{
    private Text nameText;

    private void Start()
    {
        nameText = GetComponent<Text>();

        if (AuthManager.Instance.User != null)
        {
            nameText.text = $"Hi! {AuthManager.Instance.User.Email}";
        } else
        {
            nameText.text = "ERROR : AuthManager.User is null";
        }
    }
}
