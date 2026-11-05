using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DoubleDown.Presentation
{
    public enum TableAction { Confirm, Hit, Stand, Double, Split, Insurance, DeclineInsurance, Relics, Restart, LessBet, MoreBet }

    public sealed class ControlBindings
    {
        public static readonly TableAction[] Actions = (TableAction[])Enum.GetValues(typeof(TableAction));
        private readonly Key[] keys = { Key.Space, Key.H, Key.S, Key.D, Key.P, Key.Y, Key.N, Key.Tab, Key.R, Key.LeftArrow, Key.RightArrow };
        public Key this[TableAction action] => keys[(int)action];
        public static bool IsAllowed(Key key) => key != Key.None && key != Key.Escape && Enum.IsDefined(typeof(Key), key);
        public bool Rebind(TableAction action, Key key)
        {
            if (!Enum.IsDefined(typeof(TableAction), action) || !IsAllowed(key)) return false;
            var index = (int)action;
            var other = Array.IndexOf(keys, key);
            if (other >= 0) keys[other] = keys[index];
            keys[index] = key;
            return true;
        }
        [Serializable] private sealed class Saved { public int version = 1; public int[] keys; }
        public string ToJson() => JsonUtility.ToJson(new Saved { keys = Array.ConvertAll(keys, key => (int)key) });
        public static ControlBindings FromJson(string json)
        {
            var result = new ControlBindings();
            if (string.IsNullOrWhiteSpace(json)) return result;
            try
            {
                var saved = JsonUtility.FromJson<Saved>(json);
                if (saved == null || saved.version != 1 || saved.keys == null || saved.keys.Length != Actions.Length) return result;
                var used = new HashSet<int>();
                foreach (var key in saved.keys) if (!IsAllowed((Key)key) || !used.Add(key)) return result;
                for (var i = 0; i < saved.keys.Length; i++) result.keys[i] = (Key)saved.keys[i];
            }
            catch (ArgumentException) { }
            return result;
        }
        public static string Name(TableAction action) => action switch
        {
            TableAction.Confirm => "REPARTIR / SEGUIR", TableAction.Hit => "PEDIR", TableAction.Stand => "PLANTARSE",
            TableAction.Double => "DOBLAR", TableAction.Split => "DIVIDIR", TableAction.Insurance => "ACEPTAR SEGURO",
            TableAction.DeclineInsurance => "RECHAZAR SEGURO", TableAction.Relics => "RELIQUIAS", TableAction.Restart => "REINTENTAR",
            TableAction.LessBet => "BAJAR APUESTA", TableAction.MoreBet => "SUBIR APUESTA", _ => ""
        };
    }

    [DefaultExecutionOrder(-100)]
    public sealed class TableControls : MonoBehaviour
    {
        public const string PreferenceKey = "controls.bindings.v1";
        public ControlBindings Bindings { get; private set; }
        public TableAction? Capturing { get; private set; }
        public event Action Changed;
        private int consumedFrame = -1;
        private void Awake() => Bindings = ControlBindings.FromJson(PlayerPrefs.GetString(PreferenceKey, ""));
        public bool Pressed(TableAction action) => !Capturing.HasValue && consumedFrame != Time.frameCount &&
            Keyboard.current != null && Keyboard.current[Bindings[action]].wasPressedThisFrame;
        public bool EscapePressed => consumedFrame != Time.frameCount && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        public void ConsumeFrame() => consumedFrame = Time.frameCount;
        public void BeginCapture(TableAction action) { Capturing = action; UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null); consumedFrame = Time.frameCount; Changed?.Invoke(); }
        public void CancelCapture() { Capturing = null; consumedFrame = Time.frameCount; Changed?.Invoke(); }
        public bool CaptureKey(Key key, bool persist = true)
        {
            if (!Capturing.HasValue || !Bindings.Rebind(Capturing.Value, key)) return false;
            Capturing = null;
            consumedFrame = Time.frameCount;
            if (persist) Save();
            Changed?.Invoke();
            return true;
        }
        public void RestoreDefaults(bool persist = true)
        {
            Bindings = new ControlBindings(); Capturing = null; consumedFrame = Time.frameCount;
            if (persist) Save();
            Changed?.Invoke();
        }
        private void Save() { PlayerPrefs.SetString(PreferenceKey, Bindings.ToJson()); PlayerPrefs.Save(); }
        private void Update()
        {
            if (!Capturing.HasValue || Keyboard.current == null || consumedFrame == Time.frameCount) return;
            if (Keyboard.current.escapeKey.wasPressedThisFrame) { CancelCapture(); return; }
            foreach (var key in Keyboard.current.allKeys)
                if (key.wasPressedThisFrame && CaptureKey(key.keyCode)) break;
        }
    }
}
