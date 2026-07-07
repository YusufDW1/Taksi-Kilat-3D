using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("UI Image Reference")]
    public Image tutorialImage;

    [Header("Sprites")]
    public Sprite[] tutorialSprites;

    [Header("Navigation Buttons")]
    public Button leftArrowButton;
    public Button rightArrowButton;

    [Header("Objects to Show on Last Slide")]
    public GameObject[] finishObjects; // Mulai_btn & Mengerti

    private int currentSlide = 0;

    void Start()
    {
        if (leftArrowButton != null) leftArrowButton.onClick.AddListener(PrevSlide);
        if (rightArrowButton != null) rightArrowButton.onClick.AddListener(NextSlide);
        UpdateUI();
    }

    public void NextSlide()
    {
        if (currentSlide < tutorialSprites.Length - 1)
        {
            currentSlide++;
            UpdateUI();
        }
    }

    public void PrevSlide()
    {
        if (currentSlide > 0)
        {
            currentSlide--;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (tutorialImage != null && tutorialSprites != null && tutorialSprites.Length > 0 && currentSlide < tutorialSprites.Length)
        {
            tutorialImage.sprite = tutorialSprites[currentSlide];
        }

        if (leftArrowButton != null)
        {
            leftArrowButton.gameObject.SetActive(currentSlide > 0);
        }

        if (rightArrowButton != null)
        {
            rightArrowButton.gameObject.SetActive(currentSlide < tutorialSprites.Length - 1);
        }

        bool isLastSlide = (tutorialSprites == null) || (currentSlide == tutorialSprites.Length - 1);
        foreach (var obj in finishObjects)
        {
            if (obj != null)
            {
                obj.SetActive(isLastSlide);
            }
        }
    }
}
