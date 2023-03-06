using UnityEngine;

public class TriggerItemFader : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        ItemFader[] faders = other.GetComponentsInChildren<ItemFader>();

        if (faders.Length <= 0) return;
        foreach (ItemFader item in faders) item.FadeOut();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ItemFader[] faders = other.GetComponentsInChildren<ItemFader>();

        if (faders.Length <= 0) return;
        foreach (ItemFader item in faders) item.FadeIn();
    }
}