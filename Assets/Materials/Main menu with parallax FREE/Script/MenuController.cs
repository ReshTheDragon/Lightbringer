using Assets.Materials.Main_menu_with_parallax_FREE.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{

    public static MenuController instance;

    //Active option and bool to check if main menu is active
    private int option = 0;
    private bool mainMenu = true;

    //Check to move the menu using the keys or only the arrows
    [SerializeField, Tooltip("Check to move the menu using the keys or only the arrows")]
    public bool useKeys = true;

    //Check if using parallax or not
    public bool useParallax = true;

    //Scenes animation
    private bool isAnimating = false;
    private int activeScene = 1;
    [SerializeField, Tooltip("Animation speed in seconds")]
    public float animSpeed;

    //Option quantity
    [SerializeField, Tooltip("Introduce all the options in your menu")]
    public string[] options;

    //Backgrounds
    [SerializeField, Tooltip("Introduce all the backgrounds for the scenes in your menu")]
    public GameObject[] backgrounds;
    [SerializeField, Tooltip("Introduce all the backgrounds for the scenes in your menu")]
    public GameObject[] backgroundsParallax;
    [SerializeField, Tooltip("Introduce the main bck for your menu")]
    public GameObject mainBackgroundParallax;
    [SerializeField, Tooltip("Introduce the main bck for your menu")]
    public GameObject mainBackground;
    [SerializeField, HideInInspector]
    public Text menuText;
    [SerializeField]
    public GameObject[] activeBackground;

    //Arrow Animators
    [SerializeField, HideInInspector]
    public Animator ArrowR;
    [SerializeField, HideInInspector]
    public Animator ArrowL;

    //Menu bar gameobject
    [SerializeField, HideInInspector]
    public GameObject menuBar;

    //Backgrounds Controller
    [SerializeField, HideInInspector]
    public GameObject backgroundsController;

    //Sounds
    [Header("Sounds")]
    [Space(10)]
    public AudioClip Select;
    public AudioClip SceneSelect;
    private AudioSource Audio;

    //Events
    [SerializeField, HideInInspector]
    public UnityEvent[] Events;

    //Exit Menu
    [SerializeField, HideInInspector]
    public GameObject exitMenu;

    //Options menu
    [SerializeField, HideInInspector]
    public GameObject OptionsMenu;


    void Start()
    {
        Audio = gameObject.GetComponent<AudioSource>();
        instance = this;
        if (useParallax)
        {
            activeBackground = new GameObject[backgroundsParallax.Length];
        }
        else
        {
            activeBackground = new GameObject[backgrounds.Length];
        }

        if (!SaveSystem.HasSave())
        {
            // Disable nút Continue nếu không có file save
            Events[1] = new UnityEvent(); // Giả sử Continue là options[1]
        }

        if (SaveSystem.HasSave())
        {
            // Hiện một panel "Bạn có muốn tiếp tục không?"
            // Nếu người chơi chọn "Tiếp tục" thì gọi `continueGame()`
            // Nếu không thì gọi `newGame()` hoặc để họ chọn trên menu
        }   

        initiate();
    }


    void Update()
    {

        if (mainMenu)
        {
            menuText.text = options[option];
            if (option < 1)
            {
                ArrowL.SetBool("Deactivate", true);
            }
            else
            {
                ArrowL.SetBool("Deactivate", false);
            }

            if (option == options.Length - 1)
            {
                ArrowR.SetBool("Deactivate", true);
            }
            else
            {
                ArrowR.SetBool("Deactivate", false);
            }

            if (useKeys)
            {
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    moveRight();
                }

                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    moveLeft();
                }

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    pressEnter();
                }
            }
           
        }

        var anim = backgroundsController.GetComponent<Animation>();
        if (anim.isPlaying)
        {
            isAnimating = true;
        }
        else
        {
            isAnimating = false;
        }


    }

    private void initiate()
    {

        mainMenu = true;
        menuBar.SetActive(true);
        if (useParallax)
        {

            var Bck = Instantiate(mainBackgroundParallax) as GameObject;
            Bck.transform.SetParent(this.gameObject.transform);
            Bck.transform.localScale = new Vector3(1, 1, 1);
            Bck.transform.localPosition = new Vector3(0, 0, 0);
            Bck.transform.SetSiblingIndex(0);
            var rect = Bck.GetComponent<RectTransform>();
            rect.offsetMax = new Vector2(0, 0);
            rect.offsetMin = new Vector2(0, 0);
            activeBackground[0] = Bck;
        }
        else
        {
            var Bck = Instantiate(mainBackground) as GameObject;
            Bck.transform.SetParent(this.gameObject.transform);
            var rect = Bck.GetComponent<RectTransform>();
            Bck.transform.SetSiblingIndex(0);
            rect.transform.localScale = new Vector3(1, 1, 1);
            rect.transform.localPosition = new Vector3(0, 0, 0);
            rect.offsetMax = new Vector2(0, 0);
            rect.offsetMin = new Vector2(0, 0);
            activeBackground[0] = Bck;
        }
    }

    //Press enter or click on option 
    public void pressEnter()
    {
        Events[option].Invoke();
    }

    //Function to go foward in the menu
    public void moveRight()
    {
        if (option < options.Length - 1)
        {
            option = option + 1;
            ArrowR.SetBool("Click", true);
            Audio.clip = Select;
            Audio.Play();
        }
    }

    //Function to go back in the menu
    public void moveLeft()
    {
        if (option > 0)
        {
            option = option - 1;
            ArrowL.SetBool("Click", true);
            Audio.clip = Select;
            Audio.Play();
        }
    }

    //New Game event
    public void newGame()
    {

        SceneManager.LoadScene(1);
    }

    //Continue
    public void continueGame()
    {
        var data = SaveSystem.LoadGame();
        if (data != null)
        {
            SceneManager.LoadScene(data.currentSceneIndex);
        }
        else
        {
            Debug.LogWarning("No save found. Starting new game.");
            newGame(); // Hoặc disable nút continue
        }
    }

    public void selectScene()
    {
        // Nếu không có background nào đang hoạt động -> mặc định chọn scene 1
        if (activeBackground == null || activeBackground.Length == 0)
        {
            activeBackground = new GameObject[1];
        }

        // Nếu phần tử đầu null hoặc không tồn tại thì tạo lại background 1
        if (activeBackground[0] == null)
        {
            menuBar.SetActive(false);
            mainMenu = false;

            if (useParallax)
            {
                // Dự phòng nếu mảng chưa được khởi tạo
                if (backgroundsParallax == null || backgroundsParallax.Length == 0)
                    return;

                var bck = Instantiate(backgroundsParallax[0]) as GameObject;
                var rect = bck.GetComponent<RectTransform>();
                bck.transform.SetParent(backgroundsController.transform);
                bck.transform.localScale = Vector3.one;
                bck.transform.localPosition = Vector3.zero;
                bck.transform.SetSiblingIndex(0);

                var thisRect = gameObject.GetComponent<RectTransform>();
                rect.offsetMax = new Vector2(0, 0);
                rect.offsetMin = new Vector2(0, 0);

                activeBackground[0] = bck;
            }
            else
            {
                if (backgrounds == null || backgrounds.Length == 0)
                    return;

                var bck = Instantiate(backgrounds[0]) as GameObject;
                var rect = bck.GetComponent<RectTransform>();
                bck.transform.SetParent(backgroundsController.transform);
                bck.transform.localScale = Vector3.one;
                bck.transform.localPosition = Vector3.zero;
                bck.transform.SetSiblingIndex(0);

                var thisRect = gameObject.GetComponent<RectTransform>();
                rect.offsetMax = new Vector2(0, 0);
                rect.offsetMin = new Vector2(0, 0);

                activeBackground[0] = bck;
            }
        }
        else
        {
            // Nếu có sẵn background, tiếp tục xử lý như ban đầu
            Destroy(activeBackground[0]);

            if (useParallax)
            {
                for (int i = backgroundsParallax.Length - 1; i > -1; i--)
                {
                    var bck = Instantiate(backgroundsParallax[i]) as GameObject;
                    var rect = bck.GetComponent<RectTransform>();
                    bck.transform.SetParent(backgroundsController.transform);
                    bck.transform.localScale = Vector3.one;
                    bck.transform.localPosition = Vector3.zero;
                    bck.transform.SetSiblingIndex(0);
                    var thisRect = gameObject.GetComponent<RectTransform>();
                    rect.offsetMax = new Vector2((thisRect.rect.width * i), 0);
                    rect.offsetMin = new Vector2(thisRect.rect.width * i, 0);
                    activeBackground[i] = bck;
                    menuBar.SetActive(false);
                    mainMenu = false;
                }
            }
            else
            {
                for (int i = backgrounds.Length - 1; i > -1; i--)
                {
                    var bck = Instantiate(backgrounds[i]) as GameObject;
                    var rect = bck.GetComponent<RectTransform>();
                    bck.transform.SetParent(backgroundsController.transform);
                    bck.transform.localScale = Vector3.one;
                    bck.transform.localPosition = Vector3.zero;
                    bck.transform.SetSiblingIndex(0);
                    var thisRect = gameObject.GetComponent<RectTransform>();
                    rect.offsetMax = new Vector2((thisRect.rect.width * i), 0);
                    rect.offsetMin = new Vector2(thisRect.rect.width * i, 0);
                    activeBackground[i] = bck;
                    menuBar.SetActive(false);
                    mainMenu = false;
                }
            }
        }
    }

    //Advances throught the Scenes
    public void advanceScene()
    {
        //First check if we are animating and if we are not in the last scene
        if (!isAnimating && activeScene < activeBackground.Length)
        {
            Audio.clip = SceneSelect;
            Audio.Play();
            //Then create a new clip and a curve to animate the scenes moving
            var clip = new AnimationClip();
            var curve = new AnimationCurve();
            //Get the anim from the backgroundController to put the clip
            var anim = backgroundsController.GetComponent<Animation>();
            //If the clip already exist we remove it
            if (anim.GetClip("f") != null) { anim.RemoveClip("f"); }
            clip.legacy = true;
            //Now we check the distance between 2 scenes to move then
            float distance = Vector3.Distance(activeBackground[0].transform.localPosition, activeBackground[1].transform.localPosition);
            //Set the curve with the data
            curve = AnimationCurve.Linear(0, (backgroundsController.transform.localPosition.x), animSpeed, (distance * -1) * activeScene);
            Debug.Log(distance * activeScene);
            clip.SetCurve("", typeof(Transform), "localPosition.x", curve);
            //And play the animation
            anim.AddClip(clip, "f");
            anim.Play("f");
            //We also keep the count of the active scene in this variable
            activeScene++;
            //Now we put the active scene in the first sibling index to activate the parallax effect
            activeBackground[activeScene - 1].transform.SetAsFirstSibling();

        }
    }

    //Advances throught the Scenes
    public void goBackScene()
    {

        //First check if we are animating and if we are not in the first scene
        if (!isAnimating && activeScene > 1)
        {
            activeScene--;
            //Then create a new clip and a curve to animate the scenes moving
            var clip = new AnimationClip();
            var curve = new AnimationCurve();
            //Get the anim from the backgroundController to put the clip
            var anim = backgroundsController.GetComponent<Animation>();
            //If the clip already exist we remove it
            if (anim.GetClip("b") != null) { anim.RemoveClip("b"); }
            clip.legacy = true;
            //Now we check the distance between 2 scenes to move then
            float distance = Vector3.Distance(activeBackground[0].transform.localPosition, activeBackground[1].transform.localPosition);
            //Set the curve with the data
            curve = AnimationCurve.Linear(0, (backgroundsController.transform.localPosition.x), animSpeed, distance * (activeScene - 1) * -1);
            Debug.Log(distance * (activeScene - 1));
            clip.SetCurve("", typeof(Transform), "localPosition.x", curve);
            //And play the animation
            anim.AddClip(clip, "b");
            anim.Play("b");
            //We also keep the count of the active scene in this variable
            //Now we put the active scene in the first sibling index to activate the parallax effect
            activeBackground[activeScene].transform.SetAsLastSibling();
        }
    }

    //Closes the scenes menu
    public void closeScenes()
    {
        //Destroy all the active backgrounds and restart the menu
        for (int i = 0; i < activeBackground.Length; i++)
        {
            Destroy(activeBackground[i]);
        }
        initiate();
    }

    //Opens the exit menu
    public void exitMenuOpen()
    {
        var animEx = exitMenu.GetComponent<Animation>();
        exitMenu.transform.SetAsLastSibling();
        animEx.Play("Fade In");
        mainMenu = false;
    }

    //Closes the exit menu
    public void exitMenuClose()
    {
        var animEx = exitMenu.GetComponent<Animation>();
        animEx.Play("Fade out");
        mainMenu = true;
    }

    //Exit Game
    public void exitGame()
    {
        Application.Quit();
    }

    //Open Options
    public void openOptions()
    {
        OptionsMenu.gameObject.GetComponent<Animation>().Play("Fade In");
        mainMenu = false;
        OptionsMenu.transform.SetAsLastSibling();
    }

    //Close Options
    public void closeOptions()
    {
        OptionsMenu.gameObject.GetComponent<Animation>().Play("Fade out");
        mainMenu = true;
    }

}

