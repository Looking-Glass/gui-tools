using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;

namespace Diorama
{
public class UIState : MonoBehaviour
{
    [System.Serializable]
    public enum State {
        Title,
        Game,
        PauseMenu,
        Logo,
        Tutorial
    }

    public State state = State.Game;

    public System.Action<State> OnStateChanged;

    public ConfirmDialog confirmDialog;

    public PopupItemGroup groupEditing;

    [Header("Add menu")]
    public PopupItemGroup groupAdd;

    public Button AddBackButton;
    public Button AddPauseButton;

    [Header("Pause menu")]
    public PopupItemGroup groupPause;
    public Button PauseExitCreationButton;
    public Button PauseBackButton;

    [Header("Editing")]
    public Button EditingPauseButton;

    [Header("Object Selected")]
    public PopupItemGroup groupObjectSelected;
    public Button EditingTrashButton;
    public PopupItem trashBg;
    public Button LockButton;
    public Image LockButtonImg;
    public Sprite LockButtonLockedSprite;
    public Sprite LockButtonUnlockedSprite;
    public Button FlipButton;

    [Header("No object Selected")]
    public PopupItemGroup groupNoObjectSelected;
    public PopupItem undoButton;
    public PopupItem redoButton;
    public PopupItem doneButton;
    public Button PublishButton;
    public Button UnlockButton;
    public Button LightingSettingsButton;

    [Header("Title Screen")]
    public GameObject TitleScreen;
    public Button TitleScreenNewSceneButton;
    public Button TitleScreenBrowseButton;

    [Header("Misc")]
    public MeshRenderer BlackOverlay;

    Listeners listeners;

    void OnEnable() {
        listeners = new Listeners();

        confirmDialog.Hide();

        RegisterButtonHandlers();
    }

    void RegisterButtonHandlers() {
        // Editing
        listeners.Add(EditingPauseButton, () => ChangeState(State.PauseMenu));
        listeners.Add(AddBackButton, () => ChangeState(State.Game));
        listeners.Add(AddPauseButton, () => ChangeState(State.PauseMenu));

        // Pause screen
        bool changesPresent = true; // TODO: implement your own

        listeners.Add(PauseExitCreationButton, () => {
            if (changesPresent) {
                confirmDialog.Show(
                    "Your scene has unsaved changes. Are you sure?", 
                    "Wait", () => { confirmDialog.Hide(); },
                    "Confirm", GoToTitleScreen
                );
            } else {
                GoToTitleScreen();
            }
        });
        
        listeners.Add(PauseBackButton, () => ChangeState(State.Game));
    }

    void Start() {
        ChangeState(State.Logo); // Setup initial state.
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

        groupEditing.gameObject.SetActive(state == State.Game);
        groupPause.gameObject.SetActive(state == State.PauseMenu);

        TitleScreen.SetActive(state == State.Title);


        // Clear scene on certain screens
        switch (state) {
            case State.Logo:
                StartCoroutine(LookingGlassLogo());
            break;
        }

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

    IEnumerator LookingGlassLogo() {
        SetOverlayColorAndActivate(Color.black);

        TweenOverlayAlpha(1f, 0f);

        float t = 0, duration = 5;
        while(t < duration) {
            if (Input.GetMouseButtonDown(0))
                t = duration;
            t += Time.unscaledDeltaTime;
            yield return 0;
        }

        TweenOverlayAlpha(0f, 1f).setOnComplete(() => {
            ChangeState(State.Title);
            TweenOverlayAlpha(1f, 0f);
        });
    }

    public void FadeToState(State state) {
        SetOverlayColorAndActivate(new Color(0,0,0,0));
        TweenOverlayAlpha(0, 1, 0.4f).setOnComplete(() => { ChangeState(state); TweenOverlayAlpha(1,0,0.25f); });
    }
}
}
