using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace OfflinePhoton
{
    /// <summary>
    /// In-game overlay (F8) for editing OfflinePhoton's Mode/AppId/Region/Nickname config
    /// without hand-editing BepInEx/config/tabung.offline.cfg, plus a Verify button that
    /// test-connects an AppId using the same AppSettings the real game connects with.
    /// </summary>
    public class ConfigMenu : MonoBehaviour
    {
        private bool _showGui = false;
        private Rect _windowRect = new Rect(20f, 20f, 420f, 480f);

        private string _inputMode = "Offline";
        private string _inputAppIdRealtime = string.Empty;
        private string _inputAppIdVoice = string.Empty;
        private string _inputRegion = string.Empty;
        private string _inputNickname = "Player";

        private string _realtimeVerifyStatus = string.Empty;
        private string _voiceVerifyStatus = string.Empty;

        private string _toastMessage = string.Empty;
        private float _toastEndTime = 0f;

        // --- styling ---
        private bool _stylesInitialized = false;
        private GUIStyle _windowStyle;
        private GUIStyle _sectionLabelStyle;
        private GUIStyle _fieldLabelStyle;
        private GUIStyle _textFieldStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _primaryButtonStyle;
        private GUIStyle _statusOkStyle;
        private GUIStyle _statusWarnStyle;
        private GUIStyle _statusNeutralStyle;
        private GUIStyle _toastStyle;
        private GUIStyle _hintLabelStyle;
        private Texture2D _panelBgTex, _fieldBgTex, _buttonBgTex, _buttonBgHoverTex, _primaryBgTex, _primaryBgHoverTex, _toastBgTex;

        private void Awake()
        {
            // Seed the UI fields from the config values Plugin.Awake() already bound.
            _inputMode = Plugin.Mode?.Value ?? "Offline";
            _inputAppIdRealtime = Plugin.AppIdRealtime?.Value ?? string.Empty;
            _inputAppIdVoice = Plugin.AppIdVoice?.Value ?? string.Empty;
            _inputRegion = Plugin.Region?.Value ?? string.Empty;
            _inputNickname = Plugin.Nickname?.Value ?? "Player";
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8)) _showGui = !_showGui;
        }

        private static Texture2D MakeTex(Color color)
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;

            Color bg = new Color(0.114f, 0.125f, 0.153f, 0.97f);
            Color fieldBg = new Color(0.176f, 0.192f, 0.235f, 1f);
            Color buttonBg = new Color(0.208f, 0.227f, 0.278f, 1f);
            Color buttonHover = new Color(0.263f, 0.286f, 0.345f, 1f);
            Color primaryBg = new Color(0.176f, 0.478f, 0.322f, 1f);
            Color primaryHover = new Color(0.216f, 0.573f, 0.388f, 1f);
            Color toastBg = new Color(0.09f, 0.10f, 0.12f, 0.95f);

            _panelBgTex = MakeTex(bg);
            _fieldBgTex = MakeTex(fieldBg);
            _buttonBgTex = MakeTex(buttonBg);
            _buttonBgHoverTex = MakeTex(buttonHover);
            _primaryBgTex = MakeTex(primaryBg);
            _primaryBgHoverTex = MakeTex(primaryHover);
            _toastBgTex = MakeTex(toastBg);

            _windowStyle = new GUIStyle(GUI.skin.window);
            _windowStyle.normal.background = _panelBgTex;
            _windowStyle.onNormal.background = _panelBgTex;
            _windowStyle.border = new RectOffset(8, 8, 24, 8);
            _windowStyle.padding = new RectOffset(10, 10, 26, 10);
            _windowStyle.normal.textColor = Color.white;
            _windowStyle.fontStyle = FontStyle.Bold;
            _windowStyle.fontSize = 13;
            _windowStyle.alignment = TextAnchor.UpperCenter;

            _sectionLabelStyle = new GUIStyle(GUI.skin.label);
            _sectionLabelStyle.fontStyle = FontStyle.Bold;
            _sectionLabelStyle.fontSize = 12;
            _sectionLabelStyle.normal.textColor = new Color(0.55f, 0.75f, 1f);
            _sectionLabelStyle.margin = new RectOffset(0, 0, 10, 2);

            _fieldLabelStyle = new GUIStyle(GUI.skin.label);
            _fieldLabelStyle.fontSize = 11;
            _fieldLabelStyle.normal.textColor = new Color(0.82f, 0.84f, 0.88f);
            _fieldLabelStyle.margin = new RectOffset(0, 0, 4, 1);

            _hintLabelStyle = new GUIStyle(GUI.skin.label);
            _hintLabelStyle.fontSize = 10;
            _hintLabelStyle.fontStyle = FontStyle.Italic;
            _hintLabelStyle.normal.textColor = new Color(0.55f, 0.57f, 0.62f);

            _textFieldStyle = new GUIStyle(GUI.skin.textField);
            _textFieldStyle.normal.background = _fieldBgTex;
            _textFieldStyle.focused.background = _fieldBgTex;
            _textFieldStyle.hover.background = _fieldBgTex;
            _textFieldStyle.normal.textColor = Color.white;
            _textFieldStyle.focused.textColor = Color.white;
            _textFieldStyle.padding = new RectOffset(6, 6, 4, 4);
            _textFieldStyle.margin = new RectOffset(0, 0, 0, 4);

            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.normal.background = _buttonBgTex;
            _buttonStyle.hover.background = _buttonBgHoverTex;
            _buttonStyle.active.background = _buttonBgHoverTex;
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.hover.textColor = Color.white;
            _buttonStyle.padding = new RectOffset(8, 8, 5, 5);
            _buttonStyle.fontSize = 11;

            _primaryButtonStyle = new GUIStyle(_buttonStyle);
            _primaryButtonStyle.normal.background = _primaryBgTex;
            _primaryButtonStyle.hover.background = _primaryBgHoverTex;
            _primaryButtonStyle.active.background = _primaryBgHoverTex;
            _primaryButtonStyle.fontStyle = FontStyle.Bold;

            _statusOkStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Bold };
            _statusOkStyle.normal.textColor = new Color(0.42f, 0.82f, 0.52f);

            _statusWarnStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Bold };
            _statusWarnStyle.normal.textColor = new Color(0.92f, 0.7f, 0.35f);

            _statusNeutralStyle = new GUIStyle(GUI.skin.label) { fontSize = 10 };
            _statusNeutralStyle.normal.textColor = new Color(0.6f, 0.62f, 0.68f);

            _toastStyle = new GUIStyle(GUI.skin.box);
            _toastStyle.normal.background = _toastBgTex;
            _toastStyle.normal.textColor = Color.white;
            _toastStyle.fontSize = 11;
            _toastStyle.alignment = TextAnchor.MiddleLeft;
            _toastStyle.padding = new RectOffset(10, 10, 6, 6);
            _toastStyle.wordWrap = true;

            _stylesInitialized = true;
        }

        private GUIStyle StatusStyleFor(string status)
        {
            if (string.IsNullOrEmpty(status)) return _statusNeutralStyle;
            if (status == "OK") return _statusOkStyle;
            if (status.StartsWith("Verifying")) return _statusNeutralStyle;
            return _statusWarnStyle;
        }

        private void ShowToast(string message, bool isError)
        {
            _toastMessage = (isError ? "⚠ " : "✓ ") + message;
            _toastEndTime = Time.time + 4f;
            if (isError) Plugin.Log?.LogWarning(message);
            else Plugin.Log?.LogInfo(message);
        }

        private void OnGUI()
        {
            InitStyles();

            if (!_showGui)
            {
                GUI.Label(new Rect(10f, 10f, 320f, 20f), "Press F8 to open TabungCommunityPatch menu", _hintLabelStyle);
            }
            else
            {
                _windowRect = GUI.Window(654321, _windowRect, DrawWindow, "TabungCommunityPatch", _windowStyle);
            }

            if (!string.IsNullOrEmpty(_toastMessage) && Time.time < _toastEndTime)
            {
                Rect toastRect = new Rect(Screen.width - 370f, 10f, 360f, 32f);
                GUI.Box(toastRect, _toastMessage, _toastStyle);
            }
        }

        private void DrawWindow(int id)
        {
            GUILayout.Label("MODE", _sectionLabelStyle);
            GUILayout.BeginHorizontal();
            bool isOnline = string.Equals(_inputMode, "Online", StringComparison.OrdinalIgnoreCase);
            if (GUILayout.Toggle(isOnline, " Online", _buttonStyle) != isOnline)
            {
                _inputMode = isOnline ? "Offline" : "Online";
            }
            GUILayout.Label(_inputMode, _fieldLabelStyle);
            GUILayout.EndHorizontal();

            GUILayout.Label("Nickname", _fieldLabelStyle);
            _inputNickname = GUILayout.TextField(_inputNickname, _textFieldStyle);

            GUILayout.Label("PHOTON APP IDS (used when Mode = Online)", _sectionLabelStyle);

            GUILayout.Label("Realtime", _fieldLabelStyle);
            GUILayout.BeginHorizontal();
            _inputAppIdRealtime = GUILayout.TextField(_inputAppIdRealtime, _textFieldStyle, GUILayout.MinWidth(220f));
            if (GUILayout.Button("Verify", _buttonStyle, GUILayout.Width(65)))
            {
                ShowToast("Realtime verify button clicked.", false);
                StartCoroutine(VerifyAppIdCoroutine(_inputAppIdRealtime, "Realtime"));
            }
            GUILayout.Label(_realtimeVerifyStatus, StatusStyleFor(_realtimeVerifyStatus), GUILayout.Width(70));
            GUILayout.EndHorizontal();

            GUILayout.Label("Voice (blank = reuse Realtime AppId)", _fieldLabelStyle);
            GUILayout.BeginHorizontal();
            _inputAppIdVoice = GUILayout.TextField(_inputAppIdVoice, _textFieldStyle, GUILayout.MinWidth(220f));
            if (GUILayout.Button("Verify", _buttonStyle, GUILayout.Width(65)))
            {
                string idToVerify = string.IsNullOrWhiteSpace(_inputAppIdVoice) ? _inputAppIdRealtime : _inputAppIdVoice;
                StartCoroutine(VerifyAppIdCoroutine(idToVerify, "Voice"));
            }
            GUILayout.Label(_voiceVerifyStatus, StatusStyleFor(_voiceVerifyStatus), GUILayout.Width(70));
            GUILayout.EndHorizontal();

            GUILayout.Label("Fixed Region (blank = auto)", _fieldLabelStyle);
            _inputRegion = GUILayout.TextField(_inputRegion, _textFieldStyle);

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply & Save", _primaryButtonStyle, GUILayout.Width(150)))
            {
                ApplyAndSave();
            }
            if (GUILayout.Button("Close", _buttonStyle, GUILayout.Width(100))) _showGui = false;
            GUILayout.EndHorizontal();

            GUILayout.Label("Changes take effect after Apply. A restart may be needed\nif the game already opened a Photon connection this session.", _hintLabelStyle);

            GUI.DragWindow(new Rect(0, 0, 10000, 24));

            // Bottom-right resize handle
            Rect resizeHandle = new Rect(_windowRect.width - 18f, _windowRect.height - 18f, 16f, 16f);
            GUI.Box(resizeHandle, "");
            if (Event.current.type == EventType.MouseDrag && resizeHandle.Contains(Event.current.mousePosition))
            {
                _windowRect.width = Mathf.Max(360f, _windowRect.width + Event.current.delta.x);
                _windowRect.height = Mathf.Max(300f, _windowRect.height + Event.current.delta.y);
            }
        }

        private void ApplyAndSave()
        {
            try
            {
                Plugin.Mode.Value = _inputMode;
                Plugin.AppIdRealtime.Value = _inputAppIdRealtime.Trim();
                Plugin.AppIdVoice.Value = _inputAppIdVoice.Trim();
                Plugin.Region.Value = _inputRegion.Trim();
                Plugin.Nickname.Value = _inputNickname.Trim();

                Plugin.ApplyPhotonConfigPublic();

                ShowToast("Settings applied.", false);
            }
            catch (Exception ex)
            {
                ShowToast("Failed to apply settings: " + ex.Message, true);
            }
        }

        private IEnumerator VerifyAppIdCoroutine(string appId, string label)
        {
            string id = appId?.Trim() ?? string.Empty;
            if (!Guid.TryParse(id, out _))
            {
                ShowToast(label + " AppId is not a valid GUID.", true);
                if (label == "Realtime") _realtimeVerifyStatus = "Invalid GUID"; else _voiceVerifyStatus = "Invalid GUID";
                yield break;
            }

            if (label == "Realtime") _realtimeVerifyStatus = "Verifying..."; else _voiceVerifyStatus = "Verifying...";

            var client = new LoadBalancingClient();
            client.UserId = "Verify_" + Guid.NewGuid().ToString().Substring(0, 8);

            // Mirror the real AppSettings the game connects with (protocol, auth mode, etc.)
            // rather than only setting AppId/AppVersion, so this behaves like the real connection.
            AppSettings verifySettings = new AppSettings();
            ServerSettings baseSettings = PhotonNetwork.PhotonServerSettings;
            if (baseSettings != null && baseSettings.AppSettings != null)
            {
                AppSettings real = baseSettings.AppSettings;
                verifySettings.AppIdRealtime = id;
                verifySettings.AppVersion = !string.IsNullOrEmpty(real.AppVersion) ? real.AppVersion : (!string.IsNullOrEmpty(PhotonNetwork.AppVersion) ? PhotonNetwork.AppVersion : "1.0");
                verifySettings.Protocol = real.Protocol;
                verifySettings.AuthMode = real.AuthMode;
                verifySettings.EnableProtocolFallback = real.EnableProtocolFallback;
                verifySettings.UseNameServer = true;
                verifySettings.FixedRegion = !string.IsNullOrWhiteSpace(_inputRegion) ? _inputRegion.Trim() : real.FixedRegion;
            }
            else
            {
                verifySettings.AppIdRealtime = id;
                verifySettings.AppVersion = !string.IsNullOrEmpty(PhotonNetwork.AppVersion) ? PhotonNetwork.AppVersion : "1.0";
                verifySettings.UseNameServer = true;
                verifySettings.FixedRegion = _inputRegion?.Trim() ?? string.Empty;
            }

            bool started = false;
            Plugin.Log?.LogInfo($"Verify[{label}] starting ConnectUsingSettings with AppVersion={verifySettings.AppVersion}, Protocol={verifySettings.Protocol}, AuthMode={verifySettings.AuthMode}");
            try { started = client.ConnectUsingSettings(verifySettings); }
            catch (Exception ex)
            {
                ShowToast(label + " verify start failed: " + ex.Message, true);
                if (label == "Realtime") _realtimeVerifyStatus = "Start failed"; else _voiceVerifyStatus = "Start failed";
                yield break;
            }

            if (!started)
            {
                ShowToast(label + " verify failed to start.", true);
                if (label == "Realtime") _realtimeVerifyStatus = "Start failed"; else _voiceVerifyStatus = "Start failed";
                yield break;
            }

            float timeout = 20f;
            float start = Time.realtimeSinceStartup;
            bool ok = false;
            var lastState = client.State;
            Plugin.Log?.LogInfo($"Verify[{label}] start. AppId={id}, initial State={lastState}");
            while (Time.realtimeSinceStartup - start < timeout)
            {
                client.Service();
                if (client.State != lastState)
                {
                    Plugin.Log?.LogInfo($"Verify[{label}] state change: {lastState} -> {client.State}");
                    lastState = client.State;
                }
                if (client.State == ClientState.Authenticated || client.State == ClientState.ConnectedToMasterServer || client.State == ClientState.Joined)
                {
                    ok = true;
                    break;
                }
                if (client.State == ClientState.Disconnected || client.State == ClientState.Disconnecting)
                {
                    Plugin.Log?.LogInfo($"Verify[{label}] ended early with state {client.State}");
                    break;
                }
                yield return null;
            }

            bool timedOut = Time.realtimeSinceStartup - start >= timeout;
            if (timedOut)
            {
                Plugin.Log?.LogInfo($"Verify[{label}] timed out after {timeout}s at state {client.State}. This can happen if the name server is slow to hand off to a regional master server — it does not necessarily mean the AppId is invalid.");
            }

            try { client.Disconnect(); } catch { }

            if (ok)
            {
                ShowToast(label + " AppId verified successfully.", false);
                if (label == "Realtime") _realtimeVerifyStatus = "OK"; else _voiceVerifyStatus = "OK";
            }
            else
            {
                string stateDesc = lastState.ToString();
                string verb = timedOut ? "timed out" : "failed";
                ShowToast(label + " AppId verification " + verb + " (state=" + stateDesc + "). This does not always mean the AppId is wrong — check if the game itself connects.", true);
                string status = (timedOut ? "Timeout(" : "Failed(") + stateDesc + ")";
                if (label == "Realtime") _realtimeVerifyStatus = status; else _voiceVerifyStatus = status;
            }
        }
    }
}
