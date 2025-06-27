using TMPro;
using UnityEngine;

public class FontManager : MonoBehaviour
{
    public TMP_FontAsset mainFont;
    
    public void AddCharactersToFont(string newCharacters)
    {
        if(mainFont != null && !string.IsNullOrEmpty(newCharacters))
        {
            mainFont.TryAddCharacters(newCharacters);
        }
    }
}
