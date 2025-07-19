using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    // Active option and bool to check if main menu is active
    private int option = 0;
    private bool mainMenu = true;

    // Check to move the menu using the keys or only the arrows
    [SerializeField, Tooltip("Check to move the menu using the keys or only the arrows")]
    public bool useKeys = true;

    // Check if using parallax or not
    public bool useParallax = true;

    // Option quantity - chỉ còn 3 options: Play, Settings, Exit
    [SerializeField, Tooltip("Menu options: Play, Settings, Exit")]
    public string[] options = { "Play", "Settings", "Exit" };

    // Backgrounds for parallax effect
    [Header("Backgrounds")]
    [SerializeField, Tooltip("Main background for parallax")]
    public GameObject mainBackgroundParallax;
    [SerializeField, Tooltip("Main background normal")]
    public GameObject mainBackground;
    [SerializeField]
    public GameObject[] activeBackground;

    // UI Components
    [SerializeField]
    public Text menuText;
    [SerializeField]
    public Animator ArrowR;
    [SerializeField]
    public Animator ArrowL;
    [SerializeField]
    public GameObject menuBar;

    // Sounds
    [Header("Sounds")]
    [Space(10)]
    public AudioClip Select;
    private AudioSource Audio;

    // Menu screens
    [SerializeField]
    public GameObject OptionsMenu;
    [SerializeField]
    public GameObject exitMenu;

    // Events for each option
    [SerializeField]
    public UnityEvent[] Events;

    // Game scene to load
    [Header("Game Settings")]
    public string gameSceneName = "Ground Level";
    public int gameSceneIndex = 1;

    public string howToPlayScene = "HowToPlay";
    public int howToPlaySceneIndex = 5;
    void Start()
    {
        Audio = gameObject.GetComponent<AudioSource>();
        instance = this;
        //Set the activeBackground array length
        if (useParallax && mainBackgroundParallax != null)
        {
            activeBackground = new GameObject[1];
        }
        else if (!useParallax && mainBackground != null)
        {
            activeBackground = new GameObject[1];
        }
        initiate();
    }

    void Update()
    {
        if (mainMenu)
        {
            // Changes the text corresponding option
            menuText.text = options[option];

            // Deactivate arrows
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

            // Input handling
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

                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    pressEnter();
                }
            }
        }
    }

    // Initiate menu
    private void initiate()
    {
        mainMenu = true;
        menuBar.SetActive(true);
        option = 0; // Reset to first option

        // Initialize background based on parallax setting
        if (useParallax && mainBackgroundParallax != null)
        {
            // Instantiate parallax background
            var Bck = Instantiate(mainBackgroundParallax) as GameObject;
            Bck.transform.SetParent(this.gameObject.transform);
            Bck.transform.localScale = new Vector3(1, 1, 1);
            Bck.transform.localPosition = new Vector3(0, 0, 0);
            Bck.transform.SetSiblingIndex(0);
            var rect = Bck.GetComponent<RectTransform>();
            rect.offsetMax = new Vector2(0, 0);
            rect.offsetMin = new Vector2(0, 0);
            if (activeBackground != null && activeBackground.Length > 0)
                activeBackground[0] = Bck;
        }
        else if (!useParallax && mainBackground != null)
        {
            // Instantiate normal background
            var Bck = Instantiate(mainBackground) as GameObject;
            Bck.transform.SetParent(this.gameObject.transform);
            var rect = Bck.GetComponent<RectTransform>();
            Bck.transform.SetSiblingIndex(0);
            rect.transform.localScale = new Vector3(1, 1, 1);
            rect.transform.localPosition = new Vector3(0, 0, 0);
            rect.offsetMax = new Vector2(0, 0);
            rect.offsetMin = new Vector2(0, 0);
            if (activeBackground != null && activeBackground.Length > 0)
                activeBackground[0] = Bck;
        }
    }

    // Press enter or click on option 
    public void pressEnter()
    {
        switch (option)
        {
            case 0: // Play
                startGame();
                break;
            case 1: // How To Play
                openHowToPlay();
                break;
            case 2: // Settings
                openOptions();
                break;
            case 3: // Exit
                exitMenuOpen();
                break;
        }
    }

    // Function to go forward in the menu
    public void moveRight()
    {
        if (option < options.Length - 1)
        {
            option = option + 1;
            ArrowR.SetBool("Click", true);
            playSelectSound();
        }
    }

    // Function to go back in the menu
    public void moveLeft()
    {
        if (option > 0)
        {
            option = option - 1;
            ArrowL.SetBool("Click", true);
            playSelectSound();
        }
    }

    // Play select sound
    private void playSelectSound()
    {
        if (Audio != null && Select != null)
        {
            Audio.clip = Select;
            Audio.Play();
        }
    }

    // Start Game - Load game scene
    public void resetInventoryJSON()
    {
        // Đường dẫn tới file JSON
        string filePath = Path.Combine(Application.persistentDataPath, "inventory.json");

        // Tạo dữ liệu rỗng
        string emptyInventoryJSON = @"{
        ""hotbarSlots"": [
            {
                ""itemName"": """",
                ""quantity"": 0
            },
            {
                ""itemName"": """",
                ""quantity"": 0
            },
            {
                ""itemName"": """",
                ""quantity"": 0
            }
        ],
        ""inventorySlots"": [
            {
                ""itemName"": """",
                ""quantity"": 0
            },
            {
                ""itemName"": """",
                ""quantity"": 0
            },
            {
                ""itemName"": """",
                ""quantity"": 0
            },
            {
                ""itemName"": """",
                ""quantity"": 0
            },
            {
                ""itemName"": """",
                ""quantity"": 0
            },
            {
                ""itemName"": """",
                ""quantity"": 0
            },
            {
                ""itemName"": """",
                ""quantity"": 0
            },
            {
                ""itemName"": """",
                ""quantity"": 0
            },
            {
                ""itemName"": """",
                ""quantity"": 0
            }
        ]
    }";

        // Ghi file
        File.WriteAllText(filePath, emptyInventoryJSON);
    }

    // Sửa lại function startGame
    public void startGame()
    {
        // Reset inventory JSON trước khi load scene
        resetInventoryJSON();

        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    public void openHowToPlay()
    {
        resetInventoryJSON();
        if (!string.IsNullOrEmpty(howToPlayScene))
        {
            SceneManager.LoadScene(howToPlayScene);
        }
        else
        {
            SceneManager.LoadScene(5);
        }
    }

    // Open Options Menu
    public void openOptions()
    {
        if (OptionsMenu != null)
        {
            OptionsMenu.gameObject.GetComponent<Animation>().Play("Fade In");
            mainMenu = false;
            OptionsMenu.transform.SetAsLastSibling();
        }
    }

    // Close Options Menu
    public void closeOptions()
    {
        if (OptionsMenu != null)
        {
            OptionsMenu.gameObject.GetComponent<Animation>().Play("Fade out");
            mainMenu = true;
        }
    }

    // Open Exit Menu
    public void exitMenuOpen()
    {
        if (exitMenu != null)
        {
            var animEx = exitMenu.GetComponent<Animation>();
            exitMenu.transform.SetAsLastSibling();
            animEx.Play("Fade In");
            mainMenu = false;
        }
    }

    // Close Exit Menu
    public void exitMenuClose()
    {
        if (exitMenu != null)
        {
            var animEx = exitMenu.GetComponent<Animation>();
            animEx.Play("Fade out");
            mainMenu = true;
        }
    }

    // Exit Game
    public void exitGame()
    {
        Application.Quit();

        // For testing in editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}