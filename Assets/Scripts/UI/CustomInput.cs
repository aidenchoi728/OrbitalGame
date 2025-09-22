using UnityEngine;

public interface CustomInput
{
    public bool Correct { set; }

    public void SetColors(Color fieldNormalColor, Color fieldHighlightColor, Color fieldTextColor,
        Color itemNormalColor,
        Color itemHighlightColor, Color itemHoverColor, Color itemTextNormalColor, Color itemTextHighlightColor);

    public void ChangeSelected(int num);
}