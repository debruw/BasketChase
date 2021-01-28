using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TapticPlugin;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float m_moveSpeed = 5;

    [SerializeField] private Animator m_animator;
    [SerializeField] private Rigidbody m_rigidBody;

    private Vector3 translation;
    public float Xspeed = 25f;

    public GameObject currentBall;
    public Camera cam;
    public Transform ballTarget;
    public GameObject World;
    public Slider ForceSlider;

    void Awake()
    {
        m_rigidBody.useGravity = true;
        m_rigidBody.isKinematic = false;
    }

    Vector3 tempFingerPos;

    private Vector3 startPos; //mouse slide movement start pos
    private Vector3 endPos; //mouse slide movement end pos
    public float zDistance = 30.0f;//z distance
    private bool isThrown;
    public bool isClickedForBall = false;
    void Update()
    {
        if (!GameManager.Instance.isGameStarted)
        {
            return;
        }
        World.transform.position -= transform.forward * m_moveSpeed * Time.deltaTime;

#if UNITY_EDITOR
        if (currentBall != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isClickedForBall = true;
                //get start mouse position
                Vector3 mousePos = Input.mousePosition * -1.0f;
                mousePos.z = zDistance; //add z distance

                startPos = Camera.main.ScreenToWorldPoint(mousePos);
            }
            else if (Input.GetMouseButtonUp(0) && isClickedForBall)
            {
                isClickedForBall = false;
                Debug.Log(isClickedForBall);
                //get release mouse position
                Vector3 mousePos = Input.mousePosition * -1.0f;
                mousePos.z = zDistance; //add z distance

                // convert mouse position to world position
                endPos = Camera.main.ScreenToWorldPoint(mousePos);
                endPos.z = Camera.main.nearClipPlane; //removing this breaks stuff,no idea why though

                Vector3 throwDir = (startPos - endPos).normalized;//get throw direction based on start and end pos
                ForceSlider.value = 0;

                if ((startPos - endPos).y > GameManager.Instance.currentZDistance - 2 && (startPos - endPos).y < GameManager.Instance.currentZDistance + 2)
                {
                    if (GameManager.Instance.currentBallcount != GameManager.Instance.maxBallCount)
                    {
                        currentBall.GetComponent<Collectable>().ThrowedProperties(GetComponent<Collider>());
                        m_animator.SetTrigger("Throw");
                        ThrowBall(ballTarget.position);
                        currentBall = null;
                    }
                    else if (GameManager.Instance.currentBallcount == GameManager.Instance.maxBallCount)
                    {//Last shot. Make dunk
                        m_animator.SetTrigger("Dunk");
                    }
                }
                else if ((startPos - endPos).y < GameManager.Instance.currentZDistance - 2)
                {
                    currentBall.GetComponent<Collectable>().ThrowedProperties(GetComponent<Collider>());
                    m_animator.SetTrigger("Throw");
                    ThrowBall(ballTarget.position + new Vector3(0, 0, Random.Range(-2f, 0f)));
                    currentBall = null;
                }
                else if ((startPos - endPos).y > GameManager.Instance.currentZDistance + 2)
                {
                    currentBall.GetComponent<Collectable>().ThrowedProperties(GetComponent<Collider>());
                    m_animator.SetTrigger("Throw");
                    ThrowBall(ballTarget.position + new Vector3(0, 0, Random.Range(0f, 2f)));
                    currentBall = null;
                }

                isThrown = true;
            }
            else if (Input.GetMouseButton(0) && isClickedForBall)
            {
                //get release mouse position
                Vector3 mousePos = Input.mousePosition * -1.0f;
                mousePos.z = zDistance; //add z distance

                // convert mouse position to world position
                endPos = Camera.main.ScreenToWorldPoint(mousePos);
                endPos.z = Camera.main.nearClipPlane; //removing this breaks stuff,no idea why though

                ForceSlider.value = (startPos - endPos).y / (GameManager.Instance.currentZDistance * 2);
            }
            return;
        }

        if (Input.GetMouseButton(0))
        {
            translation = new Vector3(Input.GetAxis("Mouse X"), 0, 0) * Time.deltaTime * Xspeed;

            transform.Translate(translation, Space.World);
            transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x, -3f, 3f), transform.localPosition.y, transform.localPosition.z);
        }
#elif UNITY_IOS || UNITY_ANDROID
        if (currentBall != null && Input.touchCount > 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //get start mouse position
                Vector3 mousePos = Input.mousePosition * -1.0f;
                mousePos.z = zDistance; //add z distance

                startPos = Camera.main.ScreenToWorldPoint(mousePos);

                //Print start Pos for debugging
                Debug.Log(startPos);
            }

            if (Input.GetMouseButtonUp(0))
            {
                //get release mouse position
                Vector3 mousePos = Input.mousePosition * -1.0f;
                mousePos.z = zDistance; //add z distance

                // convert mouse position to world position
                endPos = Camera.main.ScreenToWorldPoint(mousePos);
                endPos.z = Camera.main.nearClipPlane; //removing this breaks stuff,no idea why though

                //Print start Pos for debugging
                Debug.Log(endPos);

                Vector3 throwDir = (startPos - endPos).normalized;//get throw direction based on start and end pos
                currentBall.GetComponent<Collectable>().ActivateThrowProperties(GetComponent<Collider>());
                currentBall.GetComponent<Rigidbody>().velocity = (throwDir * (startPos - endPos).sqrMagnitude) *.1f;//add force to throw direction*magnitude

                isThrown = true;
            }

            return;
        }

        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                transform.position = new Vector3(Mathf.Clamp(transform.position.x + touch.deltaPosition.x * 0.01f, -4, 4), transform.position.y, transform.position.z);
            }
            else if (touch.phase == TouchPhase.Began)
            {
                //save began touch 2d point
                firstPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                GameManager.Instance.Tutorial1Canvas.SetActive(false);
            }
        }
#endif

    }

    public void ThrowBall(Vector3 target)
    {
        Physics.IgnoreCollision(currentBall.GetComponent<Collider>(), GetComponent<Collider>(), true);
        currentBall.GetComponent<Collectable>().ThrowedProperties(GetComponent<Collider>());
        currentBall.GetComponent<Launcher>().Launch(target, m_moveSpeed);
    }

    public Transform CarryObject;
    public GameObject PickUpParticle;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FinishLine"))
        {
            m_moveSpeed = 0;
            GameManager.Instance.isGameStarted = false;
            m_animator.SetTrigger("Idle");
            SmoothFollow.Instance.isOnFinish = true;
            GameManager.Instance.TapToLoadButton.SetActive(true);
        }
        else if (other.CompareTag("Collectable"))
        {
            Collect(other.gameObject);
        }
    }

    public void Collect(GameObject collectable)
    {
        if (currentBall == null)
        {
            currentBall = collectable;
            if (PlayerPrefs.GetInt("VIBRATION") == 1)
                TapticManager.Impact(ImpactFeedback.Light);
            SoundManager.Instance.playSound(SoundManager.GameSounds.Collect);
            Destroy(Instantiate(PickUpParticle, collectable.transform.position, Quaternion.identity), 2f);
            collectable.GetComponent<Collectable>().CollectedProperties();
            collectable.transform.parent = CarryObject;

            collectable.transform.localPosition = Vector3.zero;
            collectable.transform.localPosition = new Vector3(0, collectable.transform.localPosition.y, 0);
            collectable.transform.localRotation = Quaternion.identity;
        }
    }

    public void ReleaseBall()
    {
        currentBall.GetComponent<Collectable>().ThrowedProperties(GetComponent<Collider>());
        currentBall = null;
    }
}
