using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor( typeof( Card ) )]
public class CardIcon : Editor
{
    public override Texture2D RenderStaticPreview
    ( 
        string assetPath, 
        Object[] subAssets, 
        int width, 
        int height 
    )
    {
        var obj = target as Card;
        var icon = obj.image;

        if ( icon == null )
        {
            return base.RenderStaticPreview( assetPath, subAssets, width, height );
        }

        var preview = AssetPreview.GetAssetPreview( icon );
        var final = new Texture2D( width, height );

        EditorUtility.CopySerialized( preview, final );

        return final;
    }
}

#endif