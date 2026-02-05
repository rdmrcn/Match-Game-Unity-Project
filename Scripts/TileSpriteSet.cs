using UnityEngine;

[CreateAssetMenu(menuName = "MatchGame/TileSpriteSet")]
public class TileSpriteSet : ScriptableObject
{
    // [colorId, tier] -> sprite
    // tier: 0=Default, 1=Icon1, 2=Icon2, 3=Icon3
    public Sprite[] defaultSprites; // size = K
    public Sprite[] icon1Sprites;   // size = K
    public Sprite[] icon2Sprites;   // size = K
    public Sprite[] icon3Sprites;   // size = K

    public Sprite GetSprite(int colorId, int tier)
    {
        return tier switch
        {
            1 => icon1Sprites[colorId],
            2 => icon2Sprites[colorId],
            3 => icon3Sprites[colorId],
            _ => defaultSprites[colorId]
        };
    }
}