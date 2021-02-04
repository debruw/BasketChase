using System.Collections;
using System.Collections.Generic;
//using TapticPlugin;
using UnityEngine;

public class BasketNetCollider : MonoBehaviour
{
    public List<GameObject> collecteds;
    public ParticleSystem particle;
    public Transform World;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Collectable") && !collecteds.Contains(collision.gameObject))
        {
            Debug.Log("Collected");
            collecteds.Add(collision.gameObject);
            collision.gameObject.GetComponent<Collectable>().BallTrail.SetActive(false);
            collision.gameObject.transform.parent = transform;
            collision.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            collision.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            collision.gameObject.transform.rotation = Quaternion.identity;
            collision.gameObject.transform.localPosition = Vector3.zero;
            collision.gameObject.transform.GetChild(0).transform.localScale = new Vector3(.5f, .5f, .5f);
            collision.gameObject.GetComponent<SphereCollider>().radius = .15f;
            collision.gameObject.GetComponent<Collider>().material = null;            
            Instantiate(particle, transform.position, particle.transform.rotation, World);
            SoundManager.Instance.playSound(SoundManager.GameSounds.Ping);
            //if (PlayerPrefs.GetInt("VIBRATION") == 1)
            //    TapticManager.Impact(ImpactFeedback.Light);
            GameManager.Instance.AddBall();
        }
    }
}
