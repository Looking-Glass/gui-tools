using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;

public class UIState : MonoBehaviour
{
    [System.Serializable]
    public enum State {
        Title,
        Game,
        PauseMenu,
        Browse,
        Tutorial,
        Credits
    }

    public State state = State.Game;
    public System.Action<State> OnStateChanged;

    [Header("Screens")]
    public GameObject groupTitle;
    public GameObject groupGame;
    public GameObject groupPause;
    public GameObject groupBrowse;
    public GameObject groupTutorial;
    public GameObject groupCredits;

    [Header("Buttons")]
    public Button TitleGameButton;
    public Button TitleBrowseButton;
    public Button TitleTutorialButton;
    public Button TitleCreditsButton;
    public Button BackButton;
    public Button GamePauseButton;
    public Button PauseContinueButton;
    public Button PauseExitCreationButton;

    [Header("Misc")]
    public MeshRenderer BlackOverlay;
    public ConfirmDialog confirmDialog;

    Listeners listeners;

    void OnEnable() {
        listeners = new Listeners();

        groupTitle.SetActive(false);
        groupPause.SetActive(false);
        groupGame.SetActive(false);
        groupBrowse.SetActive(false);
        groupCredits.SetActive(false);
        groupTutorial.SetActive(false);

        confirmDialog.Hide();

        RegisterButtonHandlers();
    }

    void RegisterButtonHandlers() {

        listeners.Add(TitleGameButton, () => ChangeState(State.Game));
        listeners.Add(TitleBrowseButton, () => ChangeState(State.Browse));
        listeners.Add(TitleTutorialButton, () => ChangeState(State.Tutorial));
        listeners.Add(TitleCreditsButton, () => ChangeState(State.Credits));
        
        listeners.Add(BackButton, () => ChangeState(State.Title));

        listeners.Add(GamePauseButton, () => ChangeState(State.PauseMenu));

        // Pause screen
        bool changesPresent = true; // TODO: implement your own
        listeners.Add(PauseContinueButton, () => ChangeState(State.Game));
        listeners.Add(PauseExitCreationButton, () => {
            if (changesPresent) {
                confirmDialog.Show(
                    "Are you sure you want to quit?", 
                    "Wait", () => { confirmDialog.Hide(); },
                    "Confirm", GoToTitleScreen
                );
            } else {
                GoToTitleScreen();
            }
        });
    }

    void Start() {
        ChangeState(State.Title); // Setup initial state.
    }

    static internal ConfirmDialog GetConfirmDialog() {
        var uiState = FindObjectOfType<UIState>();
        return uiState != null ? uiState.confirmDialog : null;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    void OnDisable() {
        listeners.RemoveAll();
        listeners = null;
    }

    internal void GoToTitleScreen() {
        TweenOverlayAlpha(0, 1, 0.1f).setOnComplete(() => {
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        });
    }

    public void ChangeState(State state, bool skipNewTimeline=false) {
        var prevState = state;
        this.state = state;

        groupTitle.SetActive(state == State.Title);
        groupPause.SetActive(state == State.PauseMenu);
        groupGame.SetActive(state == State.Game);
        groupBrowse.SetActive(state == State.Browse);
        groupCredits.SetActive(state == State.Credits);
        groupTutorial.SetActive(state == State.Tutorial);


        confirmDialog.Hide();

        OnStateChanged?.Invoke(state);
    }

    MaterialPropertyBlock mpr;

    void SetOverlayColorAndActivate(Color color) {
        if (mpr == null)
            mpr = new MaterialPropertyBlock();
        BlackOverlay.GetPropertyBlock(mpr);
        mpr.SetColor("_Color", color);
        BlackOverlay.SetPropertyBlock(mpr);
        BlackOverlay.gameObject.SetActive(true);
    }

    public LTDescr TweenOverlayAlpha(float fromAlpha, float toAlpha, float time = 0.4f) {
        if (mpr == null)
            mpr = new MaterialPropertyBlock();
        LeanTween.cancel(BlackOverlay.gameObject);
        var desc = LeanTween.value(BlackOverlay.gameObject, fromAlpha, toAlpha, time).setOnUpdate((f) => {
            mpr.SetColor("_Color", new Color(0, 0, 0, f));
            BlackOverlay.SetPropertyBlock(mpr);
        }).setOnComplete(() => {
            if (Mathf.Approximately(toAlpha, 0f))
                BlackOverlay.gameObject.SetActive(false);
        });

        if (toAlpha > 0f)
            BlackOverlay.gameObject.SetActive(true);

        return desc;
    }

    public void FadeToState(State state) {
        SetOverlayColorAndActivate(new Color(0,0,0,0));
        TweenOverlayAlpha(0, 1, 0.4f).setOnComplete(() => { ChangeState(state); TweenOverlayAlpha(1,0,0.25f); });
    }
}
