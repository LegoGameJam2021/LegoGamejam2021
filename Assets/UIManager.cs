using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image DeathMessagePanel;

    public Text DeathMessageText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowDeathMessage()
    {
        //Workaround, I do not know why it is needed
        Color fixedColor = this.DeathMessagePanel.color;
        fixedColor.a = 1;
        this.DeathMessagePanel.color = fixedColor;
        this.DeathMessagePanel.CrossFadeAlpha(0f, 0f, true);
        Color fixedColorText = this.DeathMessageText.color;
        fixedColorText.a = 1;
        this.DeathMessageText.color = fixedColorText;
        this.DeathMessageText.CrossFadeAlpha(0f, 0f, true);

        this.DeathMessagePanel.CrossFadeAlpha(1, 2f, false);
        this.DeathMessageText.CrossFadeAlpha(1, 2f, false);
    }
}
