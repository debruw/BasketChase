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

    public GameObject netCollider, Hoop;

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

    public void ActivateRagdoll()
    {
        m_animator.enabled = false;
        m_rigidBody.isKinematic = false;
        m_rigidBody.useGravity = true;
        GetComponent<BoxCollider>().enabled = false;
        foreach (Rigidbody item in ragdollRigidbodies)
        {
            if (item.gameObject.GetComponent<MeshCollider>() == null)
            {
                item.useGravity = true;
                item.isKinematic = false;
                item.velocity = new Vector3(0, 15, 15);
            }
        }
        foreach (Collider item in ragdollColliders)
        {
            if (item.gameObject.GetComponent<MeshCollider>() == null)
            {
                item.enabled = true;
            }
        }
        metarig.SetActive(true);
        m_rigidBody.velocity = new Vector3(0, 15, 15);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FinishLine"))
        {
            GameManager.Instance.isGameStarted = false;
            playerTr.GetComponent<Animator>().SetTrigger("Defeat");
            SmoothFollow.Instance.isOnFinish = true;
            GetComponent<Animator>().SetTrigger("Shuffle");
            if (playerTr.GetComponent<PlayerController>().currentBall != null)
            {
                playerTr.GetComponent<PlayerController>().currentBall.GetComponent<Collectable>().ThrowedProperties(GetComponent<Collider>());
            }
            GameManager.Instance.StartCoroutine(GameManager.Instance.WaitAndGameLose());
        }
    }
}
