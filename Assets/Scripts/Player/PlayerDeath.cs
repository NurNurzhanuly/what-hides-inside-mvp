using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using Cinemachine; 

public class PlayerDeath : MonoBehaviour, IDamageable
{
    public float delayBeforeFade = 1.0f;
    public float fadeDuration = 1.5f;
    private bool _isDead = false;

    public void TakeDamage(float amount)
    {
        if (_isDead) return;
        _isDead = true;
        
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        PlayerInteraction interaction = GetComponent<PlayerInteraction>();
        if (interaction != null) interaction.enabled = false;

        // ОБНУЛЯЕМ СКОРОСТЬ (чтобы он не скользил по инерции вперед, но мог упасть вниз)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // ОТПУСКАЕМ ЯЩИК (если держали его в момент смерти)
        FixedJoint2D joint = GetComponent<FixedJoint2D>();
        if (joint != null) Destroy(joint);

        CinemachineVirtualCamera cam = Object.FindFirstObjectByType<CinemachineVirtualCamera>();
        if (cam != null) cam.Follow = null;

        yield return new WaitForSeconds(delayBeforeFade);

        GameObject canvasObj = new GameObject("FadeCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        GameObject imageObj = new GameObject("BlackScreen");
        imageObj.transform.SetParent(canvasObj.transform, false);
        Image fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        
        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}