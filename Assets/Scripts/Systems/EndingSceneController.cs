using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class EndingSceneController : MonoBehaviour
{
    public Light2D globalLight;
    public GameObject player;
    public Transform podiumPosition;
    public GameObject chest;
    public GameObject lightOrb;
    public GameObject loseDialog;
    public GameObject winDialog;
    public GameObject lightCoreDialog;
    public GameObject winFinalDialog;
    public GameObject firefliesEffect;

    public Button retryButton;
    public Button endGameButton;
    public Button openChestButton;
    public Button cancelButton;
    public Button exitButton;
    public Button newGameButton;

    public Animator lightOrbAnimator;
    private Animator playerAnimator; 

    private bool isWin = true;
    private bool isMovingToPodium = true;
    private float moveSpeed = 5f;

    public AudioClip winSound;
    public AudioClip loseSound;
    private AudioSource audioSource;

    void Start()
    {
        loseDialog.SetActive(false);
        winDialog.SetActive(false);
        lightCoreDialog.SetActive(false);
        lightOrb.SetActive(false);
        winFinalDialog.SetActive(false);
        if (firefliesEffect != null) firefliesEffect.SetActive(false);

        exitButton.onClick.AddListener(ExitGame);
        newGameButton.onClick.AddListener(NewGame);
        retryButton.onClick.AddListener(RetryGame);
        endGameButton.onClick.AddListener(EndGame);
        openChestButton.onClick.AddListener(OpenChest);
        cancelButton.onClick.AddListener(CancelOpenChest);

        isWin = PlayerPrefs.GetInt("IsWin", 0) == 1;
        audioSource = GetComponent<AudioSource>();

        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (isMovingToPodium)
        {
            Vector2 oldPos = player.transform.position;
            player.transform.position = Vector2.MoveTowards(
                player.transform.position,
                podiumPosition.position,
                moveSpeed * Time.deltaTime
            );

            if (playerAnimator != null)
            {
                bool isWalking = Vector2.Distance(player.transform.position, podiumPosition.position) >= 0.1f;
                playerAnimator.SetBool("isWalking", isWalking);
            }

            if (Vector2.Distance(player.transform.position, podiumPosition.position) < 0.1f)
            {
                isMovingToPodium = false;
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("isWalking", false); 
                }

                if (isWin)
                {
                    winDialog.SetActive(true);
                }
                else
                {
                    loseDialog.SetActive(true);
                    audioSource.PlayOneShot(loseSound, 0.5f);
                }
            }
        }
    }

    void OpenChest()
    {
        winDialog.SetActive(false);
        StartCoroutine(WaitForChestAnimation());
    }

    private IEnumerator WaitForChestAnimation()
    {
        ChestOpener chestOpener = chest.GetComponent<ChestOpener>();
        if (chestOpener != null && chestOpener.animator != null)
        {
            chestOpener.OpenChest();
            while (chestOpener.animator.GetCurrentAnimatorStateInfo(0).IsName("ChestOpen") &&
                   chestOpener.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }
        }

        lightOrb.SetActive(true);
        if (lightOrbAnimator != null)
        {
            lightOrbAnimator.SetTrigger("Appear");
            StartCoroutine(WaitForLightOrbAnimation());
        }
        else
        {
            StartCoroutine(WaitForLightCoreDialogDelay());
        }
    }

    private IEnumerator WaitForLightOrbAnimation()
    {
        while (lightOrbAnimator != null && lightOrbAnimator.GetCurrentAnimatorStateInfo(0).IsName("LightOrbAppear") &&
               lightOrbAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }
        StartCoroutine(WaitForLightCoreDialogDelay());
    }

    private IEnumerator WaitForLightCoreDialogDelay()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(WaitForLightCoreDialog());
    }

    private IEnumerator WaitForLightCoreDialog()
    {
        lightCoreDialog.SetActive(true);
        yield return new WaitForSeconds(2f);

        if (firefliesEffect != null)
        {
            audioSource.PlayOneShot(winSound, 0.5f);
            firefliesEffect.SetActive(true);
        }

        float duration = 5f;
        float targetIntensity = 2f;
        float startIntensity = globalLight.intensity;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, timeElapsed / duration);
            yield return null;
        }

        lightCoreDialog.SetActive(false);
        lightOrb.SetActive(false);
        winFinalDialog.SetActive(true);
        if (firefliesEffect != null)
        {
            firefliesEffect.SetActive(false);
        }
    }

    void CancelOpenChest()
    {
        winDialog.SetActive(false);
    }

    void RetryGame()
    {
        string lastLevel = PlayerPrefs.GetString("LastLevel", "Main Menu");
        Debug.Log("Retrying level: " + lastLevel);
        SceneManager.LoadScene(lastLevel);
    }

    void EndGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void NewGame()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Main Menu");
    }

    public void OnChestClicked()
    {
        if (!winDialog.activeSelf && !lightCoreDialog.activeSelf && !winFinalDialog.activeSelf)
        {
            winDialog.SetActive(true);
        }
    }
}
