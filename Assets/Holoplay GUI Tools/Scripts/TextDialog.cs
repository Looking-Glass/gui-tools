using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Text.RegularExpressions;

namespace Diorama {
public class TextDialog : MonoBehaviour
{
    public Button mouseBlocker;
    public GameObject parent;
    public GameObject characterParent;
    public GameObject dialogParent;
    public TextMeshProUGUI dialogText;
    public TextAsset dialogJson;

    public GameObject parentRight;
    public GameObject characterParentRight;
    public GameObject dialogParentRight;
    public TextMeshProUGUI dialogTextRight;

    DialogData dialog;

    public static class TutorialShowedStatus {

        const string prefsPrefix = "contentCreatorSimulator";

        public static void ClearPrefForState(UIState.State state) => PlayerPrefs.DeleteKey(GetKey(state));

        public static void ClearAllStatePrefs() {
            foreach (var state in System.Enum.GetValues(typeof(UIState.State)).Cast<UIState.State>())
                ClearPrefForState(state);
        }

        public static bool ShowTutorialIfNeeded(UIState.State state, System.Action action) {
            if (!Get(state)) {
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

#pragma warning disable 0649
    [System.Serializable]
    struct TutorialData {
        public string[] Title;
        public string[] NewSceneMenu;
        public string[] Editing;
        public string[] ObjectSearch;
        public string[] Browsing;
        public string[] ConfirmPublish;
        public string[] TutorialControls;
        public string[] TutorialIntro;

        public string[] GetTutorialForState(UIState.State state) {
            switch(state) {
                case UIState.State.Title:               return Title;
                case UIState.State.Game:             return Editing;
                case UIState.State.Tutorial:       return TutorialIntro;
                case UIState.State.Logo: return null;
            }
            return null;
        }
    }
    [System.Serializable]
    struct DialogData {
        public TutorialData tutorial;
        public string[] publish_reactions;
        public string[] prompts;
        public string[] phrase_endings;
    }
#pragma warning restore 0649

    UIState uiState;

    void OnEnable() {
        parent.SetActive(true);
        parentRight.SetActive(true);
        dialog = JsonUtility.FromJson<DialogData>(dialogJson.text);
        uiState = FindObjectOfType<UIState>();

        HideAll();
        mouseBlocker.onClick.AddListener(OnBlockerClick);

        uiState.OnStateChanged += OnStateChanged;
    }

    void OnDisable() {
        uiState.OnStateChanged -= OnStateChanged;
        mouseBlocker.onClick.RemoveListener(OnBlockerClick);
    }

    void HideAll() {
        dialogParent.SetActive(false);
        characterParent.SetActive(false);
        mouseBlocker.gameObject.SetActive(false);

        dialogParentRight.SetActive(false);
        characterParentRight.SetActive(false);
    }

    void OnBlockerClick() {
        
    }

    void OnStateChanged(UIState.State state) {
        // check if we should start a dialog
        TutorialShowedStatus.ShowTutorialIfNeeded(state, () => Show(state, dialog.tutorial.GetTutorialForState(state)));
    }

    public void Show(UIState.State state, string[] lines) { 
        if (lines != null && lines.Length > 0)
            Show(state, lines.ToList()); 
    }

    public void Show(UIState.State state, List<string> lines) {
        if (lines == null || lines.Count == 0)
            return;
        // parse emoji whatever
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

    GameObject GetParent(UIState.State state) {
        if (state == UIState.State.Tutorial)
            return characterParentRight;
        return characterParent;
    }
    GameObject GetDialogParent(UIState.State state) {
        if (state == UIState.State.Tutorial)
            return dialogParentRight;
        return dialogParent;
    }
    TextMeshProUGUI GetText(UIState.State state) {
        if (state == UIState.State.Tutorial)
            return dialogTextRight;
        return dialogText;
    }

    IEnumerator ShowLines(UIState.State state, List<string> lines) {
        yield return WaitForSecondsWithInterrupt(0.5f);
        if (DidCancel()) {
            OnTutorialEnd();
            yield break;
        }

        GetParent(state).SetActive(true);
        mouseBlocker.gameObject.SetActive(true);

        yield return WaitForSecondsWithInterrupt(0.5f);
        if (DidCancel()) {
            OnTutorialEnd();
            yield break;
        }

        GetDialogParent(state).SetActive(true);
        foreach(var l in lines) {
            if (CancelDialogDown) break;
            var line = l;
            if (Random.value > .2f)
                line += " " + dialog.phrase_endings.Pick();
            
            line = ParseLine(line);

            GetText(state).text = line;

            yield return StartCoroutine(RevealCharacters(GetText(state)));
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

    const string CLOUD_COLOR = "#095092";
    const string CONTENT_COLOR = "#e71ab9";
    const int EMOJI_AMOUNT = 109;

    string ParseLine(string line) {
        line = Regex.Replace(line, @"cloud", SetTextColor("#CLOUD", CLOUD_COLOR));
        line = Regex.Replace(line, @"#ContentCreator", SetTextColor("#ContentCreator", CONTENT_COLOR));
        line = Regex.Replace(line, @"#content", SetTextColor("#content ", CONTENT_COLOR));
        // emoji
        line = Regex.Replace(line, @"(\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff])", delegate(Match m) {
            return string.Format("<sprite=\"emoji\" index={0}\">",Random.Range(0, EMOJI_AMOUNT));
        });
        
        return line;
    }
    string SetTextColor(string text, string color) { 
        return string.Format("<color={0}>{1}</color>", color, text);
    }


    bool SkipTextDown { get => Input.GetMouseButtonDown(0); }
    bool CancelDialogDown { get => Input.GetKeyDown(KeyCode.Space); }

    [Header("Reveal")]
    public float delayPerChar = .1f;

    void Update() {
        if (Input.GetKeyDown(KeyCode.T)) {
            if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) &&
                (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {

                Debug.Log("resetting tutorial state");
                TutorialShowedStatus.ClearAllStatePrefs();

            }
        }
    }

    IEnumerator RevealCharacters(TMP_Text textComponent) {
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
}
