using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    Rigidbody rb;
    public GameObject destroyEffect;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if(!GameManager.Instance.isGameStarted)
        {
            return;
        }
        if (transform.position.y < -3)
        {
            Destroy(gameObject);
        }
    }

    public void ThrowedProperties(Collider playerCollider)
    {
        transform.parent = null;
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Collider>().isTrigger = false;
        StartCoroutine(WaitAndActivateCollision(playerCollider));
    }

    IEnumerator WaitAndActivateCollision(Collider pC)
    {
        yield return new WaitForSeconds(.5f);
        Physics.IgnoreCollision(GetComponent<Collider>(), pC, false);
    }

    public GameObject shadow;
    public void CollectedProperties()
    {
        rb.useGravity = false;
        GetComponent<Rigidbody>().isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
        shadow.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(Instantiate(destroyEffect, transform.position,Quaternion.identity), 1f);
            Destroy(gameObject);
        }    
    }
}
