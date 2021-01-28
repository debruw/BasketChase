using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketPot : MonoBehaviour
{
    public Rigidbody[] ragdollRigidbodies;
    public Collider[] ragdollColliders;
    public GameObject metarig;
    public Transform playerTr;

    [SerializeField] private Animator m_animator;
    [SerializeField] private Rigidbody m_rigidBody;

    [SerializeField] private float m_moveSpeed = 12;

    public GameObject netCollider;

    void Awake()
    {
        ragdollRigidbodies = metarig.GetComponentsInChildren<Rigidbody>();
        ragdollColliders = metarig.GetComponentsInChildren<Collider>();
        foreach (Rigidbody item in ragdollRigidbodies)
        {
            item.useGravity = false;
            item.isKinematic = true;
        }
        foreach (Collider item in ragdollColliders)
        {
            item.enabled = false;
        }
        netCollider.GetComponent<Collider>().enabled = true;
    }

    private void Update()
    {
        if (!GameManager.Instance.isGameStarted)
        {
            return;
        }
        //if (transform.position.y < -0.121f)
        //{
        //    transform.position = new Vector3(transform.position.x, -0.121f, playerTr.position.z + GameManager.Instance.currentZDistance);
        //}
    }

    public void ActivateRagdoll()
    {
        m_animator.enabled = false;
        m_rigidBody.isKinematic = true;
        m_rigidBody.useGravity = false;
        GetComponent<BoxCollider>().enabled = false;
        foreach (Rigidbody item in ragdollRigidbodies)
        {
            item.useGravity = true;
            item.isKinematic = false;
        }
        foreach (Collider item in ragdollColliders)
        {
            item.enabled = true;
        }
        metarig.SetActive(true);
    }
}
