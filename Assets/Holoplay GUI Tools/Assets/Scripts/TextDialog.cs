using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Text.RegularExpressions;

public class TextDialog : MonoBehaviour
{
    public GameObject dialogParent;
    public TextMeshProUGUI dialogText;
    public TextAsset dialogJson;
    public bool resetSaveInEditor = true;

    DialogData dialog;

    // Stores if we've shown the tutorial on PlayerPrefs
    public static class TutorialShowedStatus {
        const string prefsPrefix = "mygame";

        public static void ClearPrefForState(UIState.State state) => PlayerPrefs.DeleteKey(GetKey(state));

        public static void ClearAllStatePrefs() {
            foreach (var state in System.Enum.GetValues(typeof(UIState.State)).Cast<UIState.State>())
                ClearPrefForState(state);
        }

        public static bool ShowTutorialIfNeeded(UIState.State state, System.Action action) {
            //  show once, depending on the State
            if (!Get(state) ||
                // Always show the tutorial one tho
                state == UIState.State.Tutorial) {
                Set(state, true);
                action?.Invoke();
                return true;
            }
            return false;
        }

        static int GetId(UIState.State state) { return (int)state; }
        static string GetKey(UIState.State state) { var id = GetId(state); return $"{prefsPrefix}.{id}"; }

        public static bool Get(UIState.State state) {
            var key = GetKey(state);
            return PlayerPrefs.HasKey(key) && PlayerPrefs.GetInt(key, 0) == 1;

        }
        public static void Set(UIState.State state, bool showed) {
            var key = GetKey(state);
            PlayerPrefs.SetInt(key, showed ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void Clear() {
            for (int i = 0; i < System.Enum.GetNames(typeof(UIState.State)).Length; i++) {
                var key = GetKey((UIState.State)i);
                if (PlayerPrefs.HasKey(key))
                    PlayerPrefs.DeleteKey(key);
            }
            PlayerPrefs.Save();
        }

    }

// Edit this structure to match your JSON dialog file
#pragma warning disable 0649
    [System.Serializable]
    struct TutorialData {
        public string[] Title;
        public string[] Game;
        public string[] Browse;
        public string[] Tutorial;

        public string[] GetTutorialForState(UIState.State state) {
            switch(state) {
                case UIState.State.Title:               return Title;
                case UIState.State.Browse:              return Browse;
                case UIState.State.Game:                return Game;
                case UIState.State.Tutorial:            return Tutorial;
            }
            return null;
        }
    }
    [System.Serializable]
    struct DialogData {
        public TutorialData tutorials;
    }
#pragma warning restore 0649

    UIState uiState;

    void OnEnable() {
        dialog = JsonUtility.FromJson<DialogData>(dialogJson.text);
        uiState = FindObjectOfType<UIState>();
        if (resetSaveInEditor)
            TutorialShowedStatus.ClearAllStatePrefs();

        HideAll();

        uiState.OnStateChanged += OnStateChanged;
    }

    void OnDisable() {
        uiState.OnStateChanged -= OnStateChanged;
    }

    void HideAll() {
        dialogParent.SetActive(false);
    }

    void OnStateChanged(UIState.State state) {
        // check if we should start a dialog
        TutorialShowedStatus.ShowTutorialIfNeeded(state, () => Show(state, dialog.tutorials.GetTutorialForState(state)));
    }

    public void Show(UIState.State state, string[] lines) { 
        if (lines != null && lines.Length > 0)
            Show(state, lines.ToList()); 
    }

    public void Show(UIState.State state, List<string> lines) {
        if (lines == null || lines.Count == 0)
            return;
        StartCoroutine(ShowLines(state, lines));
    }

    IEnumerator WaitForSecondsWithInterrupt(float seconds) {
        seconds = Mathf.Max(0f, seconds);
        var end = Time.time + seconds;
        while (Time.time < end) {
            yield return null;
            if (CancelDialogDown) break;
        }
    }

    bool DidCancel() {
        if (CancelDialogDown) {
            HideAll();
            return true;
        }
        return false;
    }

    IEnumerator ShowLines(UIState.State state, List<string> lines) {
        dialogParent.SetActive(true);
        foreach(var l in lines) {
            if (CancelDialogDown) break;
            var line = l;
            
            dialogText.text = line;

            yield return StartCoroutine(RevealCharacters(dialogText));
            var t = Time.time;
            while (!SkipTextDown && !CancelDialogDown && Time.time - t < 6)
                yield return null;
            if (CancelDialogDown) break;

            yield return null;
        }

        OnTutorialEnd();

    }

    void OnTutorialEnd() {
        HideAll();
        if (uiState.state == UIState.State.Tutorial) {
            uiState.FadeToState(UIState.State.Game);
        }
    }

    bool SkipTextDown { get => Input.GetMouseButtonDown(0); }
    bool CancelDialogDown { get => Input.GetKeyDown(KeyCode.Space); }

    [Header("Reveal")]
    public float delayPerChar = .1f;

    void Update() {
        // Cheat code to reset tutorial settings
        if (Application.isEditor && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T)) {
            Debug.Log("resetting tutorial state");
            TutorialShowedStatus.ClearAllStatePrefs();
        }
    }

    IEnumerator RevealCharacters(TMP_Text textComponent) {
        Debug.Log(textComponent);
        textComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = textComponent.textInfo;
        int totalVisibleCharacters = textInfo.characterCount; // Get # of Visible Character in text object
        int visibleCount = 0;
        var time = Time.time;
        while (visibleCount <= totalVisibleCharacters)
        {
            if (SkipTextDown || CancelDialogDown)
                visibleCount = totalVisibleCharacters;

            textComponent.maxVisibleCharacters = visibleCount; // How many characters should TextMeshPro display?

            var diff = Time.time - time;
            time = Time.time;
            int numToReveal = (int)(diff / delayPerChar);

            visibleCount += numToReveal;
            yield return WaitForSecondsWithInterrupt(delayPerChar);
        }

        textComponent.maxVisibleCharacters = totalVisibleCharacters;
    }
}
