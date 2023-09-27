using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterializeEffect : MonoBehaviour
{

    public IEnumerator MaterializeRoutine(Shader shader, Color color, float time, SpriteRenderer[] spriteRenderers, Material normalMaterial)
    {
        var materializeMaterial = new Material(shader);
        materializeMaterial.SetColor("_EmissionColor", color);

        foreach (var spriteRender in spriteRenderers)
        {
            spriteRender.material = materializeMaterial;
        }

        float dissolveAmount = 0f;

        while (dissolveAmount < 1f) 
        { 
            dissolveAmount += Time.deltaTime / time;

            materializeMaterial.SetFloat("_DissolveAmount", dissolveAmount);

            yield return null;
        }

        foreach (var spriteRender in spriteRenderers)
        {
            spriteRender.material = normalMaterial;
        }
    }
}
