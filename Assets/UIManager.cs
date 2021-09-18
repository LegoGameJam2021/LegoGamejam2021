using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image DeathMessagePanel;

    public Text DeathMessageText;

    public Image StartPanel;

    public Text StartText1;

    public Text StartText2;

    public Text StartText3;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(this.ShowStartScene());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowDeathMessage()
    {
        //Workaround, I do not know why it is needed
        this.SetUIElementForCrossFade(this.DeathMessagePanel);
        this.SetUIElementForCrossFade(this.DeathMessageText);

        this.DeathMessagePanel.CrossFadeAlpha(1, 2f, false);
        this.DeathMessageText.CrossFadeAlpha(1, 2f, false);
    }

    public IEnumerator ShowStartScene()
    {
        this.SetUIElementForCrossFade(this.StartPanel);
        this.SetUIElementForCrossFade(this.StartText1);
        this.SetUIElementForCrossFade(this.StartText2);
        this.SetUIElementForCrossFade(this.StartText3);

        this.StartPanel.CrossFadeAlpha(1, 1f, false);
        this.StartText1.CrossFadeAlpha(1, 3f, false);
        yield return new WaitForSeconds(3);
        this.StartText2.CrossFadeAlpha(1, 3f, false);
        yield return new WaitForSeconds(3);
        this.StartText3.CrossFadeAlpha(1, 3f, false);
        yield return new WaitForSeconds(7);

        this.StartPanel.CrossFadeAlpha(0, 3f, false);
        this.StartText1.CrossFadeAlpha(0, 1f, false);
        this.StartText2.CrossFadeAlpha(0, 1f, false);
        this.StartText3.CrossFadeAlpha(0, 1f, false);

    }

    private void SetUIElementForCrossFade(Graphic el)
    {
        Color fixedColor = el.color;
        fixedColor.a = 1;
        el.color = fixedColor;
        el.CrossFadeAlpha(0f, 0f, true);
    }
}
