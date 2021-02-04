using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
//using TapticPlugin;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float m_moveSpeed = 5;

    [SerializeField] public Animator m_animator;
    [SerializeField] private Rigidbody m_rigidBody;

    private Vector3 translation;
    public float Xspeed = 25f;

    public GameObject currentBall;
    public Camera cam;
    public Transform ballTarget;
    public GameObject World;
    public Image ForceSlider;
    public GameObject PowerBar;

    public Rigidbody[] ragdollRigidbodies;
    public Collider[] ragdollColliders;
    public GameObject metarig;

    void Awake()
    {
        ragdollRigidbodies = metarig.GetComponentsInChildren<Rigidbody>();
        ragdollColliders = metarig.GetComponentsInChildren<Collider>();
        foreach (Rigidbody item in ragdollRigidbodies)
        {
            item.useGravity = false;
            item.isKinematic = true;
        }
    }

    Vector3 tempFingerPos;

    private Vector3 startPos; //mouse slide movement start pos
    private Vector3 endPos; //mouse slide movement end pos
    public float zDistance = 30.0f;//z distance
    private bool isThrown;
    public bool isClickedForBall = false, isMovementReleased, isClickedForDunk;
    Touch touch;
    Vector2 firstPressPos;
    public Animator IndicatorAnimator;
    public GameObject indicatorPlane;
    public GameObject EndEffect;
    void Update()
    {
        if (GameManager.Instance.isGameOver || !GameManager.Instance.isGameStarted)
        {
            return;
        }

        World.transform.position -= transform.forward * m_moveSpeed * Time.deltaTime;

        if (GameManager.Instance.currentBallcount == GameManager.Instance.maxBallCount && currentBall != null && GameManager.Instance.isReadyForDunk)
        {
            m_animator.SetTrigger("Dunk");
            IndicatorAnimator.gameObject.SetActive(true);
            PowerBar.SetActive(false);
            StartCoroutine(ScaleTime(1, .1f, .5f));

            //Wait for click on right time
            StartCoroutine(WaitForClick());

            cam.transform.parent.transform.DORotate(new Vector3(0, 45, 0), .5f);

            if (transform.position.x != 0)
            {
                transform.DOLocalMoveX(0, 1);
            }
        }

        if (waitingForClick)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isClickedForDunk = true;
            }
            else if (Input.GetMouseButtonUp(0) && isClickedForDunk)
            {
                IndicatorAnimator.enabled = false;
                if (indicatorPlane.transform.eulerAngles.z < 15 && indicatorPlane.transform.eulerAngles.z > 0 || indicatorPlane.transform.eulerAngles.z < 360 && indicatorPlane.transform.eulerAngles.z > 345)
                {
                    Debug.LogError("In range");
                    StartCoroutine(ScaleTime(.1f, 1f, .5f));
                    //ReleaseBall();
                    waitingForClick = false;
                    GameManager.Instance.basketPot.GetComponent<BasketPot>().ActivateRagdoll();
                    GameManager.Instance.isGameOver = true;
                    m_animator.SetTrigger("Shuffle");
                    Instantiate(EndEffect, GameManager.Instance.basketPot.transform.position + new Vector3(0, 2, 0), Quaternion.identity);
                    StartCoroutine(GameManager.Instance.WaitAndGameWin());
                }
                else
                {
                    Debug.LogError("Not in range");
                    StartCoroutine(ScaleTime(.1f, 1f, .5f));
                    Instantiate(EndEffect, transform.position + new Vector3(0, 2, 0), Quaternion.identity);
                    ActivateRagdoll();
                    GameManager.Instance.basketPot.GetComponent<Animator>().SetTrigger("Shuffle");
                    StartCoroutine(GameManager.Instance.WaitAndGameLose());
                    GameManager.Instance.isGameOver = true;
                }
            }
            return;
        }
#if UNITY_EDITOR
        if (currentBall != null && isMovementReleased)
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
                //get release mouse position
                Vector3 mousePos = Input.mousePosition * -1.0f;
                mousePos.z = zDistance; //add z distance

                // convert mouse position to world position
                endPos = Camera.main.ScreenToWorldPoint(mousePos);
                endPos.z = Camera.main.nearClipPlane; //removing this breaks stuff,no idea why though

                Vector3 throwDir = (startPos - endPos).normalized;//get throw direction based on start and end pos
                ForceSlider.fillAmount = 0;

                if ((startPos - endPos).y > GameManager.Instance.currentZDistance - 2 && (startPos - endPos).y < GameManager.Instance.currentZDistance + 2)
                {
                    if (GameManager.Instance.currentBallcount != GameManager.Instance.maxBallCount)
                    {
                        currentBall.GetComponent<Collectable>().ThrowedProperties(GetComponent<Collider>());
                        m_animator.SetTrigger("Throw");
                        ThrowBall(ballTarget.position);
                        currentBall = null;
                    }
                    //else if (GameManager.Instance.currentBallcount == GameManager.Instance.maxBallCount)
                    //{//Last shot. Make dunk
                    //    m_animator.SetTrigger("Dunk");
                    //    IndicatorAnimator.gameObject.SetActive(true);
                    //    PowerBar.SetActive(false);
                    //    StartCoroutine(ScaleTime(1, .1f, .5f));

                    //    //Wait for click on right time
                    //    StartCoroutine(WaitForClick());

                    //    cam.transform.parent.transform.DORotate(new Vector3(0, 45, 0), .5f);

                    //    if (transform.position.x != 0)
                    //    {
                    //        transform.DOLocalMoveX(0, 1);
                    //    }
                    //}
                }
                else if ((startPos - endPos).y < GameManager.Instance.currentZDistance - 2)
                {
                    currentBall.GetComponent<Collectable>().ThrowedProperties(GetComponent<Collider>());
                    m_animator.SetTrigger("Throw");
                    ThrowBall(ballTarget.position + new Vector3(0, 0, Random.Range(-5, 2)));
                    currentBall = null;
                }
                else if ((startPos - endPos).y > GameManager.Instance.currentZDistance + 2)
                {
                    currentBall.GetComponent<Collectable>().ThrowedProperties(GetComponent<Collider>());
                    m_animator.SetTrigger("Throw");
                    ThrowBall(ballTarget.position + new Vector3(0, 0, Random.Range(2, 5)));
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

                ForceSlider.fillAmount = (startPos - endPos).y / (GameManager.Instance.currentZDistance * 2);
            }
            return;
        }

        if (Input.GetMouseButton(0))
        {
            isMovementReleased = false;
            translation = new Vector3(Input.GetAxis("Mouse X"), 0, 0) * Time.deltaTime * Xspeed;

            transform.Translate(translation, Space.World);
            transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x, -3f, 3f), transform.localPosition.y, transform.localPosition.z);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isMovementReleased = true;
        }
#elif UNITY_IOS || UNITY_ANDROID
        if (currentBall != null && Input.touchCount > 0 && isMovementReleased)
        {
            touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                isClickedForBall = true;
                //get start mouse position
                Vector3 mousePos = Input.mousePosition * -1.0f;
                mousePos.z = zDistance; //add z distance

                startPos = Camera.main.ScreenToWorldPoint(mousePos);
            }
            else if (touch.phase == TouchPhase.Ended && isClickedForBall)
            {
                isClickedForBall = false;
                //get release mouse position
                Vector3 mousePos = Input.mousePosition * -1.0f;
                mousePos.z = zDistance; //add z distance

                // convert mouse position to world position
                endPos = Camera.main.ScreenToWorldPoint(mousePos);
                endPos.z = Camera.main.nearClipPlane; //removing this breaks stuff,no idea why though

                Vector3 throwDir = (startPos - endPos).normalized;//get throw direction based on start and end pos
                ForceSlider.fillAmount = 0;

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
                        if (transform.position.x != 0)
                        {
                            transform.DOLocalMoveX(0, 1);
                        }
                    }
                }
                else if ((startPos - endPos).y < GameManager.Instance.currentZDistance - 2)
                {
                    currentBall.GetComponent<Collectable>().ThrowedProperties(GetComponent<Collider>());
                    m_animator.SetTrigger("Throw");
                    ThrowBall(ballTarget.position + new Vector3(0, 0, Random.Range(-5, 2)));
                    currentBall = null;
                }
                else if ((startPos - endPos).y > GameManager.Instance.currentZDistance + 2)
                {
                    currentBall.GetComponent<Collectable>().ThrowedProperties(GetComponent<Collider>());
                    m_animator.SetTrigger("Throw");
                    ThrowBall(ballTarget.position + new Vector3(0, 0, Random.Range(2, 5)));
                    currentBall = null;
                }
                isThrown = true;
            }
            else if (touch.phase == TouchPhase.Moved && isClickedForBall)
            {
                //get release mouse position
                Vector3 mousePos = Input.mousePosition * -1.0f;
                mousePos.z = zDistance; //add z distance

                // convert mouse position to world position
                endPos = Camera.main.ScreenToWorldPoint(mousePos);
                endPos.z = Camera.main.nearClipPlane; //removing this breaks stuff,no idea why though

                ForceSlider.fillAmount = (startPos - endPos).y / (GameManager.Instance.currentZDistance * 2);
            }
            return;
        }

        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                isMovementReleased = false;
                transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x + touch.deltaPosition.x * 0.01f, -3, 3), transform.localPosition.y, transform.localPosition.z);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isMovementReleased = true;
            }
            else if (touch.phase == TouchPhase.Began)
            {
                //save began touch 2d point
                firstPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            }
        }
#endif
    }

    public void ThrowBall(Vector3 target)
    {
        Physics.IgnoreCollision(currentBall.GetComponent<Collider>(), GetComponent<Collider>(), true);
        currentBall.GetComponent<Collectable>().ThrowedProperties(GetComponent<Collider>());
        currentBall.GetComponent<Collectable>().AddPhysicMaterial();
        currentBall.GetComponent<Launcher>().Launch(target, m_moveSpeed);
    }

    public Transform CarryObject;
    public GameObject PickUpParticle;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectable"))
        {
            Collect(other.gameObject);
            //if (GameManager.Instance.currentBallcount == GameManager.Instance.maxBallCount)
            //{//Last shot. Make dunk
            //    m_animator.SetTrigger("Dunk"); 
            //    IndicatorAnimator.gameObject.SetActive(true);
            //    PowerBar.SetActive(false);
            //    StartCoroutine(ScaleTime(1, .1f, .5f));

            //    //Wait for click on right time
            //    StartCoroutine(WaitForClick());

            //    cam.transform.parent.transform.DORotate(new Vector3(0, 45, 0), .5f);

            //    if (transform.position.x != 0)
            //    {
            //        transform.DOLocalMoveX(0, 1);
            //    }
            //}
        }
    }

    bool waitingForClick;
    public IEnumerator WaitForClick()
    {
        waitingForClick = true;
        yield return new WaitForSeconds(.75f);
        waitingForClick = false;
        if (!GameManager.Instance.isGameOver)
        {
            Debug.LogError("Not in range");
            Time.timeScale = 1f;
            Instantiate(EndEffect, transform.position + new Vector3(0, 2, 0), Quaternion.identity);
            ActivateRagdoll();
            GameManager.Instance.basketPot.GetComponent<Animator>().SetTrigger("Shuffle");
            StartCoroutine(GameManager.Instance.WaitAndGameLose());
            GameManager.Instance.isGameOver = true;
        }
    }

    public void Collect(GameObject collectable)
    {
        if (currentBall == null)
        {
            currentBall = collectable;
            //if (PlayerPrefs.GetInt("VIBRATION") == 1)
            //    TapticManager.Impact(ImpactFeedback.Light);
            SoundManager.Instance.playSound(SoundManager.GameSounds.Collect);
            Destroy(Instantiate(PickUpParticle, collectable.transform.position, Quaternion.identity), 2f);
            collectable.GetComponent<Collectable>().CollectedProperties();
            collectable.transform.parent = CarryObject;
            collectable.GetComponent<Collectable>().BallTrail.SetActive(true);
            collectable.transform.GetChild(0).transform.localScale = new Vector3(1f, 1f, 1f);
            collectable.GetComponent<SphereCollider>().radius = .3f;

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
            }
        }
    }

    IEnumerator ScaleTime(float start, float end, float time)
    {
        float lastTime = Time.realtimeSinceStartup;
        float timer = 0.0f;

        while (timer < time)
        {
            Time.timeScale = Mathf.Lerp(start, end, timer / time);
            timer += (Time.realtimeSinceStartup - lastTime);
            lastTime = Time.realtimeSinceStartup;
            yield return null;
        }
        Time.timeScale = end;
    }
}
