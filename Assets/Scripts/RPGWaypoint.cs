using UnityEngine;
using System.Collections;

public class RPGWaypoint : MonoBehaviour
{
    [Header("Floating Animation")]
    public float floatHeight = 0.5f;
    public float floatSpeed = 2f;
    
    [Header("Glow Effect")]
    public float glowSpeed = 1.5f;
    public float minGlowAlpha = 0.3f;
    public float maxGlowAlpha = 1f;
    
    [Header("Rotation")]
    public bool enableRotation = true;
    public float rotationSpeed = 30f;
    
    [Header("Scale Pulse")]
    public bool enableScalePulse = false;
    public float pulseScale = 0.1f;
    public float pulseSpeed = 1f;
    
    [Header("Components")]
    public SpriteRenderer spriteRenderer;
    
    private Vector3 startPosition;
    private Vector3 originalScale;
    private Color originalColor;
    
    void Start()
    {
        // Get components
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Store original values
        startPosition = transform.position;
        originalScale = transform.localScale;
        originalColor = spriteRenderer.color;
        
        // Start animations
        StartCoroutine(FloatAnimation());
        StartCoroutine(GlowAnimation());
        
        if (enableScalePulse)
            StartCoroutine(ScalePulseAnimation());
    }
    
    void Update()
    {
        // Rotation animation
        if (enableRotation)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }
    
    IEnumerator FloatAnimation()
    {
        while (true)
        {
            // Move up
            float timer = 0;
            Vector3 startPos = transform.position;
            Vector3 targetPos = startPosition + Vector3.up * floatHeight;
            
            while (timer < 1f)
            {
                timer += Time.deltaTime * floatSpeed;
                float yPos = Mathf.Lerp(startPos.y, targetPos.y, 
                    Mathf.Sin(timer * Mathf.PI * 0.5f));
                transform.position = new Vector3(startPosition.x, yPos, startPosition.z);
                yield return null;
            }
            
            // Move down
            timer = 0;
            startPos = transform.position;
            targetPos = startPosition;
            
            while (timer < 1f)
            {
                timer += Time.deltaTime * floatSpeed;
                float yPos = Mathf.Lerp(startPos.y, targetPos.y, 
                    Mathf.Sin(timer * Mathf.PI * 0.5f));
                transform.position = new Vector3(startPosition.x, yPos, startPosition.z);
                yield return null;
            }
        }
    }
    
    IEnumerator GlowAnimation()
    {
        while (true)
        {
            // Fade in
            float timer = 0;
            while (timer < 1f)
            {
                timer += Time.deltaTime * glowSpeed;
                float alpha = Mathf.Lerp(minGlowAlpha, maxGlowAlpha, 
                    Mathf.Sin(timer * Mathf.PI * 0.5f));
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, 
                    originalColor.b, alpha);
                yield return null;
            }
            
            // Fade out
            timer = 0;
            while (timer < 1f)
            {
                timer += Time.deltaTime * glowSpeed;
                float alpha = Mathf.Lerp(maxGlowAlpha, minGlowAlpha, 
                    Mathf.Sin(timer * Mathf.PI * 0.5f));
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, 
                    originalColor.b, alpha);
                yield return null;
            }
        }
    }
    
    IEnumerator ScalePulseAnimation()
    {
        while (true)
        {
            // Scale up
            float timer = 0;
            while (timer < 1f)
            {
                timer += Time.deltaTime * pulseSpeed;
                float scale = Mathf.Lerp(1f, 1f + pulseScale, 
                    Mathf.Sin(timer * Mathf.PI * 0.5f));
                transform.localScale = originalScale * scale;
                yield return null;
            }
            
            // Scale down
            timer = 0;
            while (timer < 1f)
            {
                timer += Time.deltaTime * pulseSpeed;
                float scale = Mathf.Lerp(1f + pulseScale, 1f, 
                    Mathf.Sin(timer * Mathf.PI * 0.5f));
                transform.localScale = originalScale * scale;
                yield return null;
            }
        }
    }
    
    // Method untuk hide waypoint
    public void HideWaypoint()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
    
    // Method untuk show waypoint
    public void ShowWaypoint()
    {
        gameObject.SetActive(true);
        Start(); // Restart animations
    }
}