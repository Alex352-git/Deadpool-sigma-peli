using UnityEngine;

public class MenuCameraMove : MonoBehaviour
{
    public Transform startPosition;
    public Transform targetPosition;

    [Header("Camera Movement")]
    public float moveDuration = 2f;

    [Header("Intro Sound")]
    public AudioSource introSound;
    public float soundDelay = 0f;

    private float timer;
    private bool soundPlayed = false;

    void Start()
    {
        transform.position = startPosition.position;
        transform.rotation = startPosition.rotation;
    }

    void Update()
    {
        // Kameran liike
        if (timer < moveDuration)
        {
            timer += Time.deltaTime;

            float t = timer / moveDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(
                startPosition.position,
                targetPosition.position,
                t
            );

            transform.rotation = Quaternion.Lerp(
                startPosition.rotation,
                targetPosition.rotation,
                t
            );
        }

        // Äänen ajoitus
        if (!soundPlayed && timer >= moveDuration + soundDelay)
        {
            soundPlayed = true;

            if (introSound != null)
            {
                introSound.Play();
            }
        }
    }
}