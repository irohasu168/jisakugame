using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    [System.Serializable]
    public class TutorialPage
    {
        public string title;

        [TextArea(3, 8)]
        public string body;

        [Header("Visual")]
        public Sprite image;

        public VideoClip videoClip;

        public bool useVideo;
    }

    [Header("References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject dungeonSelectPanel;
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private GameObject turnInputRoot;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject debugRoot;
    [SerializeField] private TurnUIVisibility turnUIVisibility;

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text stepText;

    [Header("Visuals")]
    [SerializeField] private Image tutorialImage;
    [SerializeField] private RawImage tutorialVideoImage;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button skipButton;

    [Header("Pages")]
    [SerializeField] private TutorialPage[] pages;

    [Header("Options")]
    [SerializeField] private bool showOnStart = true;
    [SerializeField] private bool rememberCompleted = false;

    private const string TutorialCompletedKey = "TutorialCompleted";
    private int currentIndex;

    private void Awake()
    {
        if (tutorialPanel == null)
            tutorialPanel = gameObject;

        if (turnUIVisibility == null)
            turnUIVisibility = FindFirstObjectByType<TurnUIVisibility>();

        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);

        if (backButton != null)
            backButton.onClick.AddListener(PrevPage);

        if (skipButton != null)
            skipButton.onClick.AddListener(FinishTutorial);
    }

    private void Start()
    {
        bool alreadyCompleted =
            rememberCompleted &&
            PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 1;

        if (showOnStart && !alreadyCompleted)
        {
            ShowTutorial();
        }
        else
        {
            HideTutorialAndShowSelect();
        }
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(NextPage);

        if (backButton != null)
            backButton.onClick.RemoveListener(PrevPage);

        if (skipButton != null)
            skipButton.onClick.RemoveListener(FinishTutorial);
    }

    public void ShowTutorial()
    {
        currentIndex = 0;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        if (dungeonSelectPanel != null)
            dungeonSelectPanel.SetActive(false);

        if (hudRoot != null)
            hudRoot.SetActive(false);

        if (turnInputRoot != null)
            turnInputRoot.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (debugRoot != null)
            debugRoot.SetActive(false);

        if (turnUIVisibility != null)
            turnUIVisibility.SetUILocked(true);

        Refresh();
    }

    private void NextPage()
    {
        if (pages == null || pages.Length == 0)
        {
            FinishTutorial();
            return;
        }

        if (currentIndex >= pages.Length - 1)
        {
            FinishTutorial();
            return;
        }

        currentIndex++;
        Refresh();
    }

    private void PrevPage()
    {
        if (pages == null || pages.Length == 0)
            return;

        currentIndex = Mathf.Max(0, currentIndex - 1);
        Refresh();
    }

    private void FinishTutorial()
    {
        StopVideo();

        if (rememberCompleted)
        {
            PlayerPrefs.SetInt(TutorialCompletedKey, 1);
            PlayerPrefs.Save();
        }

        HideTutorialAndShowSelect();
    }

    private void HideTutorialAndShowSelect()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        if (dungeonSelectPanel != null)
            dungeonSelectPanel.SetActive(true);

        if (hudRoot != null)
            hudRoot.SetActive(false);

        if (turnInputRoot != null)
            turnInputRoot.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (debugRoot != null)
            debugRoot.SetActive(false);

        if (turnUIVisibility != null)
            turnUIVisibility.SetUILocked(true);
    }

    private void Refresh()
    {
        StopVideo();

        if (pages == null || pages.Length == 0)
        {
            if (titleText != null)
                titleText.text = "Tutorial";

            if (bodyText != null)
                bodyText.text = "No tutorial pages.";

            if (stepText != null)
                stepText.text = "0 / 0";

            SetImageVisible(false);
            SetVideoVisible(false);

            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, pages.Length - 1);

        TutorialPage page = pages[currentIndex];

        if (titleText != null)
            titleText.text = page.title;

        if (bodyText != null)
            bodyText.text = page.body;

        if (stepText != null)
            stepText.text = $"{currentIndex + 1} / {pages.Length}";

        RefreshVisual(page);
        RefreshButtons();
    }

    private void RefreshVisual(TutorialPage page)
    {
        if (page == null)
        {
            SetImageVisible(false);
            SetVideoVisible(false);
            return;
        }

        if (page.useVideo && page.videoClip != null && videoPlayer != null)
        {
            SetImageVisible(false);
            SetVideoVisible(true);

            videoPlayer.clip = page.videoClip;
            videoPlayer.isLooping = true;
            videoPlayer.Play();
        }
        else if (page.image != null)
        {
            SetVideoVisible(false);
            SetImageVisible(true);

            if (tutorialImage != null)
                tutorialImage.sprite = page.image;
        }
        else
        {
            SetImageVisible(false);
            SetVideoVisible(false);
        }
    }

    private void RefreshButtons()
    {
        if (backButton != null)
            backButton.gameObject.SetActive(currentIndex > 0);

        if (nextButton != null)
        {
            TMP_Text buttonText = nextButton.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.text =
                    currentIndex >= pages.Length - 1
                        ? "Start"
                        : "Next";
            }
        }
    }

    private void StopVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();
    }

    private void SetImageVisible(bool visible)
    {
        if (tutorialImage != null)
            tutorialImage.gameObject.SetActive(visible);
    }

    private void SetVideoVisible(bool visible)
    {
        if (tutorialVideoImage != null)
            tutorialVideoImage.gameObject.SetActive(visible);
    }

#if UNITY_EDITOR
    [ContextMenu("Reset Tutorial Completed")]
    private void ResetTutorialCompleted()
    {
        PlayerPrefs.DeleteKey(TutorialCompletedKey);
        PlayerPrefs.Save();
        Debug.Log("[Tutorial] Completed flag reset.");
    }
#endif
}