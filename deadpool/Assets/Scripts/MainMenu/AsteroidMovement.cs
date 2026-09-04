using UnityEngine;

public class AsteroidMovement : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;

    void Update()
    {
        if (target == null)
            return;

        // Laskee suunnan pallosta kohti Empty GameObjectia
        Vector3 direction = (target.position - transform.position).normalized;

        // Liikuttaa palloa kohti kohdetta
        transform.position += direction * speed * Time.deltaTime;

        // K‰‰nt‰‰ pallon kohti kohdetta
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == target)
        {
            Destroy(gameObject);
        }
    }
}