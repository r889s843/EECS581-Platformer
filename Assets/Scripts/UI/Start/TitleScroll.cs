using UnityEngine;

public class ScrollingBanner : MonoBehaviour
{
    public float scrollSpeed = 2f; // Speed of the scrolling
    public RectTransform banner1;  // First banner image
    public RectTransform banner2;  // Second banner image

    private float bannerWidth;  // Width of the banner images

    void Start()
    {
        // Calculate the width of one banner and account for the scaling factor
        bannerWidth = banner1.rect.width * banner1.localScale.x; // Using localScale.x to factor in scaling

        // Ensure both banners are positioned side-by-side with no gap
        banner2.anchoredPosition = new Vector2(bannerWidth, 0);
        
        // Ensure both banners have the correct scale
        banner1.localScale = new Vector3(1f, 1f, 1);
        banner2.localScale = new Vector3(1f, 1f, 1);
    }

    void Update()
    {
        // Move both banners horizontally
        banner1.anchoredPosition += new Vector2(-scrollSpeed * Time.deltaTime, 0);
        banner2.anchoredPosition += new Vector2(-scrollSpeed * Time.deltaTime, 0);

        // Check if banner1 has moved off screen (left side)
        if (banner1.anchoredPosition.x <= -bannerWidth)
        {
            // Move banner1 to the right of banner2 to create a seamless loop
            banner1.anchoredPosition = new Vector2(banner2.anchoredPosition.x + bannerWidth, 0);
        }

        // Check if banner2 has moved off screen (left side)
        if (banner2.anchoredPosition.x <= -bannerWidth)
        {
            // Move banner2 to the right of banner1 to create a seamless loop
            banner2.anchoredPosition = new Vector2(banner1.anchoredPosition.x + bannerWidth, 0);
        }
    }
}
