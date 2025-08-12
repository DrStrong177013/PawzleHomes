using UnityEngine;

[RequireComponent(typeof(ParticleSystem), typeof(ParticleSystemRenderer))]
public class PS_SpriteRandomizer : MonoBehaviour
{
    public Material particleMaterial;
    public Sprite[] sprites;

    void Awake()
    {
        var ps = GetComponent<ParticleSystem>();
        var rend = GetComponent<ParticleSystemRenderer>();
        rend.material = particleMaterial;

        var tsa = ps.textureSheetAnimation;
        tsa.enabled = true;
        tsa.mode = ParticleSystemAnimationMode.Sprites;


        for (int i = tsa.spriteCount - 1; i >= 0; i--) tsa.RemoveSprite(i);


        if (sprites != null)
        {
            foreach (var s in sprites)
                if (s != null) tsa.AddSprite(s);
        }


        tsa.frameOverTime = new ParticleSystem.MinMaxCurve(0f);

        int count = Mathf.Max(1, tsa.spriteCount);
        tsa.startFrame = new ParticleSystem.MinMaxCurve(0f, count - 1);
    }
}
