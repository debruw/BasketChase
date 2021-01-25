using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketNetCollider : MonoBehaviour
{
    public List<GameObject> collecteds;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Collectable") && !collecteds.Contains(collision.gameObject))
        {
            Debug.Log("Collected");
            collecteds.Add(collision.gameObject);

            collision.gameObject.transform.parent = collision.gameObject.transform;
            collision.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            collision.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            collision.gameObject.transform.rotation = Quaternion.identity;

            GameManager.Instance.AddBall();
        }
    }
}
