using UnityEngine;
using System.Collections;

public class CauldronCookingEffect : MonoBehaviour
{
    [Header("Cooking Settings")]
    public GameObject cauldronObject; // Котел
    public Transform cookingEffectPosition; // Позиция для пара
    public AudioClip cookingSound;
    public float cookingDuration = 3f;

    [Header("Particle Effects")]
    public GameObject smokeParticlePrefab;
    private ParticleSystem smokeVFX;
    private Light cookingLight;
    private AudioSource audioSource;

    public void PlayCookingEffect()
    {
        if (cauldronObject == null)
        {
            Debug.LogWarning("Cauldron object not assigned!");
            return;
        }

        StartCoroutine(CookingSequence());
    }

    IEnumerator CookingSequence()
    {
        Debug.Log("🍲 Nauryz Kozhe is cooking...");

        // Создаём визуальные эффекты
        CreateSmokeEffect();
        CreateCookingLight();
        PlayCookingSound();
        AnimateCauldron();

        yield return new WaitForSeconds(cookingDuration);

        Debug.Log("✅ Nauryz Kozhe is ready!");
        StopCookingEffects();
    }

    void CreateSmokeEffect()
    {
        // Если есть готовый префаб — используем его
        if (smokeParticlePrefab != null)
        {
            GameObject smoke = Instantiate(smokeParticlePrefab, cookingEffectPosition != null ? cookingEffectPosition : cauldronObject.transform);
            smokeVFX = smoke.GetComponent<ParticleSystem>();
            return;
        }

        // Создаём дым вручную (встроенное в Unity)
        GameObject smokeObj = new GameObject("CookingSmoke");
        smokeObj.transform.SetParent(cookingEffectPosition != null ? cookingEffectPosition : cauldronObject.transform);
        smokeObj.transform.localPosition = Vector3.up * 0.5f;

        smokeVFX = smokeObj.AddComponent<ParticleSystem>();

        // Настройка Particle System для дыма
        var main = smokeVFX.main;
        main.duration = cookingDuration;
        main.loop = true;
        main.startLifetime = 2f;
        main.startSize = 0.5f;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.8f, 0.8f, 0.8f, 0.5f), Color.white);

        var emission = smokeVFX.emission;
        emission.rateOverTime = 50f;

        var shape = smokeVFX.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var velocity = smokeVFX.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = 2f;

        Debug.Log("✅ Smoke effect created!");
    }

    void CreateCookingLight()
    {
        GameObject lightObj = new GameObject("CookingLight");
        lightObj.transform.SetParent(cauldronObject.transform);
        lightObj.transform.localPosition = Vector3.up * 1f;

        cookingLight = lightObj.AddComponent<Light>();
        cookingLight.type = LightType.Point;
        cookingLight.range = 5f;
        cookingLight.intensity = 0f;
        cookingLight.color = new Color(1f, 0.6f, 0.2f); // Жаркий оранжевый

        StartCoroutine(PulseLight());
        Debug.Log("✅ Light effect created!");
    }

    IEnumerator PulseLight()
    {
        float elapsed = 0f;
        while (elapsed < cookingDuration)
        {
            if (cookingLight != null)
            {
                float pulse = Mathf.Sin(elapsed * 4f) * 0.5f + 0.5f;
                cookingLight.intensity = pulse * 2f;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void AnimateCauldron()
    {
        StartCoroutine(RotateCauldronSlightly());
    }

    IEnumerator RotateCauldronSlightly()
    {
        float elapsed = 0f;
        Vector3 originalRotation = cauldronObject.transform.eulerAngles;

        while (elapsed < cookingDuration)
        {
            float rotation = Mathf.Sin(elapsed * 3f) * 5f; // Качание ±5 градусов
            cauldronObject.transform.eulerAngles = originalRotation + Vector3.up * rotation;
            elapsed += Time.deltaTime;
            yield return null;
        }

        cauldronObject.transform.eulerAngles = originalRotation;
    }

    void PlayCookingSound()
    {
        if (cookingSound == null)
        {
            Debug.LogWarning("Cooking sound not assigned. Create one with sound effects.");
            return;
        }

        if (audioSource == null)
        {
            audioSource = cauldronObject.AddComponent<AudioSource>();
        }

        audioSource.clip = cookingSound;
        audioSource.Play();
        Debug.Log("🔊 Playing cooking sound...");
    }

    void StopCookingEffects()
    {
        if (smokeVFX != null)
        {
            smokeVFX.Stop();
            Destroy(smokeVFX.gameObject, 2f);
        }

        if (cookingLight != null)
        {
            StartCoroutine(FadeOutLight());
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        Debug.Log("✅ Cooking effects finished!");
    }

    IEnumerator FadeOutLight()
    {
        float elapsed = 0f;
        float fadeDuration = 0.5f;

        while (elapsed < fadeDuration && cookingLight != null)
        {
            cookingLight.intensity = Mathf.Lerp(cookingLight.intensity, 0f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (cookingLight != null)
            Destroy(cookingLight.gameObject);
    }
}
