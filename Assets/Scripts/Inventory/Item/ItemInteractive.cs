using UnityEngine;
using System.Collections;

public class ItemInteractive : MonoBehaviour
{
    private bool isAnimating;

    private readonly WaitForSeconds pause = new(0.04f);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAnimating)
        {
            StartCoroutine(nameof(Rotate), !(other.transform.position.x < transform.position.x));
            
            EventHandler.CallPlaySoundEvent(SoundName.Rustle);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isAnimating)
        {
            StartCoroutine(nameof(Rotate), !(other.transform.position.x > transform.position.x));
            
            EventHandler.CallPlaySoundEvent(SoundName.Rustle);
        }
    }

    private IEnumerator Rotate(bool turnToLeft)
    {
        isAnimating = true;

        for (int i = 0; i < 4; i++)
        {
            transform.GetChild(0).Rotate(0, 0, turnToLeft ? 2 : -2);
            yield return pause;
        }

        for (int i = 0; i < 5; i++)
        {
            transform.GetChild(0).Rotate(0, 0, turnToLeft ? -2 : 2);
            yield return pause;
        }

        transform.GetChild(0).Rotate(0, 0, turnToLeft ? 2 : -2);

        isAnimating = false;
    }
}