using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DoubleDown.Presentation
{
    public sealed class FeelSandboxController : MonoBehaviour
    {
        private const int MaximumCards = 20;
        private const int SparkCount = 56;
        private const float PanelWidth = 326f;
        private const float CompactPanelHeight = 54f;
        private const float ExpandedPanelHeight = 590f;

        [Header("Configuration")]
        [SerializeField] private FeelProfile profileAsset;
        [SerializeField] private InputActionAsset inputActions;

        [Header("Sprites")]
        [SerializeField] private Sprite[] cardFaces;
        [SerializeField] private Sprite cardBack;
        [SerializeField] private Sprite cardShadow;
        [SerializeField] private Sprite sparkSprite;
        [SerializeField] private Material cardInkMaterial;

        [Header("Audio: enum FeelSound order")]
        [SerializeField] private AudioClip[] soundClips;

        private readonly List<FeelCardView> cards = new List<FeelCardView>(MaximumCards);
        private readonly FeelCardView[] cardPool = new FeelCardView[MaximumCards];
        private readonly Spark[] sparks = new Spark[SparkCount];
        private readonly AudioSource[] audioSources = new AudioSource[8];
        private readonly System.Random visualRandom = new System.Random(89127);
        private FeelProfile profile;
        private Camera tableCamera;
        private InputAction pointAction;
        private InputAction selectAction;
        private InputAction cancelAction;
        private InputAction confirmAction;
        private FeelCardView pressedCard;
        private FeelCardView hoveredCard;
        private Vector3 pressPoint;
        private Vector3 dragOffset;
        private int audioIndex;
        private int resetCount;
        private int nextSpark;
        private bool reduceMotion;
        private bool muteAudio;
        private bool showTuning;
        private Vector2 tuningScroll;
        private float averageFrameTime = 1f / 60f;

        private sealed class Spark
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Vector3 Velocity;
            public float Life;
        }

        private void Awake()
        {
            if (profileAsset == null || inputActions == null || cardBack == null || cardInkMaterial == null ||
                cardFaces == null || cardFaces.Length == 0)
            {
                throw new InvalidOperationException("Feel Sandbox scene references are incomplete.");
            }

            profile = Instantiate(profileAsset);
            profile.name = profileAsset.name + " (Play Mode)";
            tableCamera = Camera.main;
            if (tableCamera == null)
            {
                throw new InvalidOperationException("Feel Sandbox requires a camera tagged MainCamera.");
            }

            pointAction = inputActions.FindAction("Table/Point", true);
            selectAction = inputActions.FindAction("Table/Select", true);
            cancelAction = inputActions.FindAction("Table/Cancel", true);
            confirmAction = inputActions.FindAction("Table/Confirm", true);

            CreateCardPool();
            CreateSparkPool();
            CreateAudioPool();
        }

        private void OnEnable()
        {
            if (pointAction != null)
            {
                pointAction.actionMap.Enable();
            }
        }

        private void Start()
        {
            ResetHand(7);
        }

        private void OnDisable()
        {
            if (pointAction != null)
            {
                pointAction.actionMap.Disable();
            }
        }

        private void OnDestroy()
        {
            if (profile != null)
            {
                Destroy(profile);
            }
        }

        private void Update()
        {
            var dt = Time.unscaledDeltaTime;
            averageFrameTime = Mathf.Lerp(averageFrameTime, dt, 0.05f);
            var pointerScreen = pointAction.ReadValue<Vector2>();
            var pointerWorld = tableCamera.ScreenToWorldPoint(
                new Vector3(pointerScreen.x, pointerScreen.y, -tableCamera.transform.position.z));
            pointerWorld.z = 0f;

            HandleShortcuts();
            HandlePointer(pointerScreen, pointerWorld);
            UpdateLayout();

            var motionFactor = reduceMotion ? 0.15f : 1f;
            for (var i = 0; i < cards.Count; i++)
            {
                cards[i].Tick(dt, pointerWorld, motionFactor);
            }

            TickSparks(dt);
        }

        private void HandleShortcuts()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.dKey.wasPressedThisFrame) ResetHand(cards.Count == 0 ? 7 : cards.Count);
                if (keyboard.fKey.wasPressedThisFrame) FlipFocusedCard();
                if (keyboard.rKey.wasPressedThisFrame) ResetHand(7);
                if (keyboard.tKey.wasPressedThisFrame) ResetHand(20);
                if (keyboard.f1Key.wasPressedThisFrame) showTuning = !showTuning;
            }

            if (cancelAction.WasPressedThisFrame())
            {
                if (pressedCard != null)
                {
                    pressedCard.EndPress();
                    if (pressedCard.IsDragging) pressedCard.EndDrag();
                }
                pressedCard = null;
                for (var i = 0; i < cards.Count; i++) cards[i].SetSelected(false);
            }

            if (confirmAction.WasPressedThisFrame()) FlipFocusedCard();
        }

        private void HandlePointer(Vector2 screenPoint, Vector3 worldPoint)
        {
            var panelHeight = Mathf.Min(showTuning ? ExpandedPanelHeight : CompactPanelHeight, Screen.height - 20f);
            var overPanel = screenPoint.x < PanelWidth + 20f && screenPoint.y > Screen.height - panelHeight - 10f;
            if (pressedCard == null)
            {
                var hit = overPanel ? null : FindTopCard(worldPoint);
                if (hoveredCard != hit)
                {
                    if (hoveredCard != null) hoveredCard.SetHover(false);
                    hoveredCard = hit;
                    if (hoveredCard != null) hoveredCard.SetHover(true);
                }
            }

            if (selectAction.WasPressedThisFrame() && !overPanel)
            {
                pressedCard = FindTopCard(worldPoint);
                pressPoint = worldPoint;
                dragOffset = pressedCard != null ? pressedCard.Position - worldPoint : Vector3.zero;
                if (pressedCard != null) pressedCard.BeginPress();
            }

            if (pressedCard != null && selectAction.IsPressed())
            {
                if (!pressedCard.IsDragging && Vector3.Distance(worldPoint, pressPoint) > profile.dragThreshold)
                {
                    pressedCard.BeginDrag();
                }

                if (pressedCard.IsDragging)
                {
                    pressedCard.SetDragTarget(worldPoint + dragOffset);
                    TrySwap(pressedCard, worldPoint.x + dragOffset.x);
                }
            }

            if (selectAction.WasReleasedThisFrame() && pressedCard != null)
            {
                pressedCard.EndPress();
                if (pressedCard.IsDragging)
                {
                    pressedCard.EndDrag();
                }
                else
                {
                    pressedCard.SetSelected(!pressedCard.IsSelected);
                    PlaySound(FeelSound.Select);
                    EmitSparks(pressedCard.Position);
                }

                pressedCard = null;
            }
        }

        private FeelCardView FindTopCard(Vector3 point)
        {
            if (hoveredCard != null && hoveredCard.HitTest(point)) return hoveredCard;
            for (var i = cards.Count - 1; i >= 0; i--)
            {
                if (cards[i].HitTest(point)) return cards[i];
            }

            return null;
        }

        private void TrySwap(FeelCardView card, float x)
        {
            var index = cards.IndexOf(card);
            if (index < 0) return;

            var swapIndex = -1;
            if (index < cards.Count - 1 && x > LayoutPosition(index + 1, cards.Count).x + 0.08f)
                swapIndex = index + 1;
            else if (index > 0 && x < LayoutPosition(index - 1, cards.Count).x - 0.08f)
                swapIndex = index - 1;

            if (swapIndex < 0) return;
            var crossed = cards[swapIndex];
            cards.RemoveAt(index);
            cards.Insert(swapIndex, card);
            card.SwapPulse(Mathf.Sign(swapIndex - index));
            crossed.SwapPulse(Mathf.Sign(index - swapIndex));
            PlaySound(FeelSound.Swap);
        }

        private void FlipFocusedCard()
        {
            var target = hoveredCard;
            if (target == null)
            {
                for (var i = cards.Count - 1; i >= 0; i--)
                {
                    if (cards[i].IsSelected)
                    {
                        target = cards[i];
                        break;
                    }
                }
            }

            if (target != null) target.Flip();
        }

        private void ResetHand(int count)
        {
            count = Mathf.Clamp(count, 1, MaximumCards);
            pressedCard = null;
            hoveredCard = null;
            cards.Clear();
            resetCount++;
            var used = new bool[cardFaces.Length];
            var random = new System.Random(7301 + resetCount);

            for (var i = 0; i < MaximumCards; i++)
            {
                var card = cardPool[i];
                if (i >= count)
                {
                    card.gameObject.SetActive(false);
                    continue;
                }

                var faceIndex = random.Next(cardFaces.Length);
                if (count <= cardFaces.Length)
                {
                    while (used[faceIndex]) faceIndex = random.Next(cardFaces.Length);
                    used[faceIndex] = true;
                }

                cards.Add(card);
                var target = LayoutPosition(i, count);
                card.ResetForDeal(cardFaces[faceIndex], new Vector3(-4.3f, 1.55f, 0f), target,
                    LayoutAngle(i, count), i * 0.075f);
                card.SetLayout(target, LayoutAngle(i, count), i);
            }

            UpdateLayout();
        }

        private void UpdateLayout()
        {
            for (var i = 0; i < cards.Count; i++)
            {
                var position = LayoutPosition(i, cards.Count);
                var angle = LayoutAngle(i, cards.Count);
                cards[i].SetLayout(position, angle, i);
            }
        }

        private Vector3 LayoutPosition(int index, int count)
        {
            var spacing = Mathf.Min(profile.cardSpacing, 7.7f / Mathf.Max(1, count - 1));
            var normalized = count <= 1 ? 0f : index / (float)(count - 1) * 2f - 1f;
            // The reference hand rises at the centre and returns to the baseline
            // at the outer cards. The curve stays gentle at seven cards and scales
            // down for small hands.
            var influence = Mathf.Clamp01((count - 1f) / 6f);
            return new Vector3((index - (count - 1) * 0.5f) * spacing,
                -1.42f + profile.fanCurve.Evaluate((normalized + 1f) * 0.5f) * profile.fanHeight * influence, 0f);
        }

        private float LayoutAngle(int index, int count)
        {
            var normalized = count <= 1 ? 0f : index / (float)(count - 1) * 2f - 1f;
            var influence = Mathf.Clamp01((count - 1f) / 6f);
            return -normalized * profile.fanAngle * influence;
        }

        private void CreateCardPool()
        {
            var parent = new GameObject("Card Pool").transform;
            parent.SetParent(transform, false);
            for (var i = 0; i < MaximumCards; i++)
            {
                var gameObject = new GameObject("Card " + (i + 1));
                gameObject.transform.SetParent(parent, false);
                cardPool[i] = gameObject.AddComponent<FeelCardView>();
                cardPool[i].Initialize(profile, cardFaces[0], cardBack, cardShadow, cardInkMaterial, PlaySound, i);
            }
        }

        private void CreateSparkPool()
        {
            var parent = new GameObject("Spark Pool").transform;
            parent.SetParent(transform, false);
            for (var i = 0; i < SparkCount; i++)
            {
                var gameObject = new GameObject("Spark " + i);
                gameObject.transform.SetParent(parent, false);
                var renderer = gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = sparkSprite;
                renderer.sortingOrder = 1500;
                sparks[i] = new Spark { Transform = gameObject.transform, Renderer = renderer };
                gameObject.SetActive(false);
            }
        }

        private void CreateAudioPool()
        {
            for (var i = 0; i < audioSources.Length; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                audioSources[i] = source;
            }
        }

        private void PlaySound(FeelSound sound)
        {
            var index = (int)sound;
            if (muteAudio || soundClips == null || index >= soundClips.Length || soundClips[index] == null)
                return;

            var source = audioSources[audioIndex++ % audioSources.Length];
            source.Stop();
            source.clip = soundClips[index];
            source.pitch = 0.94f + (float)visualRandom.NextDouble() * 0.12f;
            source.volume = profile.sfxVolume;
            source.Play();
        }

        private void EmitSparks(Vector3 position)
        {
            for (var i = 0; i < 7; i++)
            {
                var spark = sparks[nextSpark++ % SparkCount];
                var angle = (float)visualRandom.NextDouble() * Mathf.PI * 2f;
                var speed = 1.1f + (float)visualRandom.NextDouble() * 1.8f;
                spark.Transform.position = position;
                spark.Transform.localScale = Vector3.one * (0.28f + (float)visualRandom.NextDouble() * 0.2f);
                spark.Velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * speed;
                spark.Life = 0.26f;
                spark.Renderer.color = Color.white;
                spark.Transform.gameObject.SetActive(true);
            }
        }

        private void TickSparks(float dt)
        {
            for (var i = 0; i < SparkCount; i++)
            {
                var spark = sparks[i];
                if (spark.Life <= 0f) continue;
                spark.Life -= dt;
                if (spark.Life <= 0f)
                {
                    spark.Transform.gameObject.SetActive(false);
                    continue;
                }

                spark.Transform.position += spark.Velocity * dt;
                spark.Velocity *= Mathf.Exp(-5f * dt);
                var color = spark.Renderer.color;
                color.a = spark.Life / 0.26f;
                spark.Renderer.color = color;
            }
        }

        private void OnGUI()
        {
            if (profile == null) return;
            var panelHeight = Mathf.Min(showTuning ? ExpandedPanelHeight : CompactPanelHeight, Screen.height - 20f);
            GUILayout.BeginArea(new Rect(10f, 10f, PanelWidth, panelHeight), GUI.skin.box);
            GUILayout.Label("DOUBLE DOWN  |  FEEL SANDBOX");
            if (GUILayout.Button(showTuning ? "Ocultar ajustes (F1)" : "Ajustes (F1)"))
                showTuning = !showTuning;
            if (!showTuning)
            {
                GUILayout.EndArea();
                return;
            }
            tuningScroll = GUILayout.BeginScrollView(tuningScroll);
            GUILayout.Label("Arrastra para reordenar. Clic: seleccionar. F: voltear.");
            GUILayout.Label("D: repartir  R: 7 cartas  T: 20 cartas  Esc: limpiar");
            GUILayout.Label("FPS aproximados: " + (1f / Mathf.Max(averageFrameTime, 0.001f)).ToString("F0"));
            GUILayout.Space(5f);
            if (GUILayout.Button("Repartir de nuevo (D)")) ResetHand(cards.Count == 0 ? 7 : cards.Count);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("7 cartas (R)")) ResetHand(7);
            if (GUILayout.Button("20 cartas (T)")) ResetHand(20);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Voltear carta enfocada (F / Enter)")) FlipFocusedCard();
            GUILayout.Space(4f);
            profile.springStrength = Slider("Muelle", profile.springStrength, 15f, 200f);
            profile.springDamping = Slider("Amortiguacion", profile.springDamping, 2f, 32f);
            profile.cardSpacing = Slider("Separacion", profile.cardSpacing, 0.35f, 1.2f);
            profile.fanHeight = Slider("Curva mano", profile.fanHeight, 0f, 0.8f);
            profile.fanAngle = Slider("Angulo abanico", profile.fanAngle, 0f, 30f);
            profile.hoverLift = Slider("Elevacion hover", profile.hoverLift, 0f, 0.8f);
            profile.hoverScale = Slider("Escala hover", profile.hoverScale, 1f, 1.5f);
            profile.cursorTilt = Slider("Giro cursor", profile.cursorTilt, 0f, 30f);
            profile.velocityTilt = Slider("Tilt velocidad", profile.velocityTilt, 0f, 20f);
            profile.swayAngle = Slider("Sway", profile.swayAngle, 0f, 5f);
            profile.dealDuration = Slider("Tiempo reparto", profile.dealDuration, 0.15f, 1.2f);
            profile.flipDuration = Slider("Tiempo volteo", profile.flipDuration, 0.1f, 0.8f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Abanico suave"))
                profile.fanCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));
            if (GUILayout.Button("Abanico marcado"))
                profile.fanCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.25f, 0.15f),
                    new Keyframe(0.5f, 1f), new Keyframe(0.75f, 0.15f), new Keyframe(1f, 0f));
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Curva de reparto rapida"))
                profile.dealCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 3.5f), new Keyframe(1f, 1f, 0f, 0f));
            reduceMotion = GUILayout.Toggle(reduceMotion, "Reducir movimiento");
            muteAudio = GUILayout.Toggle(muteAudio, "Silenciar SFX");
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private static float Slider(string label, float value, float min, float max)
        {
            GUILayout.Label(label + ": " + value.ToString("F2"));
            return GUILayout.HorizontalSlider(value, min, max);
        }
    }
}
