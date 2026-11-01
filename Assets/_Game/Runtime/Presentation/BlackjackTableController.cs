using System;
using System.Collections.Generic;
using DoubleDown.Blackjack;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DoubleDown.Presentation
{
    public sealed class BlackjackTableController : MonoBehaviour
    {
        [SerializeField] private BlackjackRulesConfig rulesAsset;
        [SerializeField] private Sprite[] cardFaces;
        [SerializeField] private Sprite cardBack;
        [SerializeField] private Material cardInkMaterial;
        [SerializeField] private TMP_FontAsset fontAsset;
        [SerializeField] private AudioClip dealSound;
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private AudioClip flipSound;
        [SerializeField] private uint runSeed = 20261101u;

        private readonly Dictionary<string, Sprite> spriteByName = new Dictionary<string, Sprite>(52);
        private readonly Dictionary<PlayingCard, BlackjackCardVisual> visibleCards =
            new Dictionary<PlayingCard, BlackjackCardVisual>(52);
        private readonly List<BlackjackCardVisual> cardPool = new List<BlackjackCardVisual>(52);
        private BlackjackSession session;
        private AudioSource audioSource;
        private long selectedWager;
        private int usedCards;
        private float dealerTimer;
        private TextMeshProUGUI bankrollText;
        private TextMeshProUGUI wagerText;
        private TextMeshProUGUI roundText;
        private TextMeshProUGUI messageText;
        private TextMeshProUGUI dealerText;
        private TextMeshProUGUI playerText;
        private Button dealButton;
        private Button minusButton;
        private Button plusButton;
        private Button hitButton;
        private Button standButton;
        private Button doubleButton;
        private Button splitButton;
        private Button insuranceButton;
        private Button declineButton;

        private static readonly Color Panel = new Color(0.018f, 0.039f, 0.070f, 0.94f);
        private static readonly Color PanelSoft = new Color(0.025f, 0.075f, 0.105f, 0.90f);
        private static readonly Color Gold = new Color(0.98f, 0.73f, 0.38f, 1f);
        private static readonly Color Cyan = new Color(0.54f, 0.84f, 0.91f, 1f);

        private void Awake()
        {
            if (rulesAsset == null || cardBack == null || cardInkMaterial == null ||
                cardFaces == null || cardFaces.Length != 52)
                throw new InvalidOperationException("Blackjack table scene references are incomplete.");

            foreach (var sprite in cardFaces)
            {
                var name = sprite.texture.name;
                spriteByName[name] = sprite;
            }

            var rules = rulesAsset.ToRules();
            session = new BlackjackSession(rules, runSeed);
            selectedWager = rules.MinimumWager;
            session.EventRaised += OnGameEvent;
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            BuildHud();
            RefreshHud();
        }

        private void Start() => TryDeal();

        private void OnDestroy()
        {
            if (session != null) session.EventRaised -= OnGameEvent;
        }

        private void Update()
        {
            HandleKeyboard();
            if (session.Phase == BlackjackPhase.DealerTurn)
            {
                dealerTimer -= Time.unscaledDeltaTime;
                if (dealerTimer <= 0f)
                {
                    dealerTimer = 0.55f;
                    session.AdvanceDealerStep();
                }
            }
            for (var i = 0; i < usedCards; i++) cardPool[i].Tick(Time.unscaledDeltaTime);
        }

        private void HandleKeyboard()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.spaceKey.wasPressedThisFrame) TryDeal();
            if (keyboard.hKey.wasPressedThisFrame) session.Hit();
            if (keyboard.sKey.wasPressedThisFrame) session.Stand();
            if (keyboard.dKey.wasPressedThisFrame) session.DoubleDown();
            if (keyboard.pKey.wasPressedThisFrame) session.Split();
            if (keyboard.yKey.wasPressedThisFrame) session.ChooseInsurance(true);
            if (keyboard.nKey.wasPressedThisFrame) session.ChooseInsurance(false);
        }

        private void TryDeal()
        {
            if (session.Phase != BlackjackPhase.Ready && session.Phase != BlackjackPhase.Settled) return;
            if (selectedWager > session.Bankroll) selectedWager = rulesAsset.minimumWager;
            if (!session.StartRound(selectedWager)) RefreshHud();
        }

        private void ChangeWager(int direction)
        {
            if (session.Phase != BlackjackPhase.Ready && session.Phase != BlackjackPhase.Settled) return;
            var step = rulesAsset.minimumWager;
            selectedWager = Math.Max(step, Math.Min(session.Bankroll, selectedWager + direction * step));
            RefreshHud();
        }

        private void OnGameEvent(BlackjackEvent gameEvent)
        {
            if (gameEvent.Type == BlackjackEventType.RoundStarted) RecycleCards();
            if (gameEvent.Type == BlackjackEventType.CardDealt) PlaySound(dealSound, 0.91f, 1.08f);
            if (gameEvent.Type == BlackjackEventType.HoleRevealed) PlaySound(flipSound, 1f, 1f);
            if (gameEvent.Type == BlackjackEventType.RoundSettled) PlaySound(selectSound, 0.95f, 1.05f);
            if (gameEvent.Type == BlackjackEventType.PhaseChanged && session.Phase == BlackjackPhase.DealerTurn)
                dealerTimer = 0.65f;
            RefreshCards();
            RefreshHud();
        }

        private void PlaySound(AudioClip clip, float minPitch, float maxPitch)
        {
            if (clip == null) return;
            audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(clip, 0.62f);
        }

        private void RecycleCards()
        {
            foreach (var card in cardPool) card.gameObject.SetActive(false);
            usedCards = 0;
            visibleCards.Clear();
        }

        private BlackjackCardVisual GetVisual(PlayingCard card)
        {
            if (visibleCards.TryGetValue(card, out var existing)) return existing;
            if (!spriteByName.TryGetValue(card.SpriteName, out var sprite))
                throw new InvalidOperationException("Missing sprite for " + card.SpriteName);
            BlackjackCardVisual visual;
            if (usedCards == cardPool.Count)
            {
                var objectName = new GameObject("Playing Card");
                objectName.transform.SetParent(transform, false);
                visual = objectName.AddComponent<BlackjackCardVisual>();
                cardPool.Add(visual);
            }
            else visual = cardPool[usedCards];
            usedCards++;
            visual.name = card.SpriteName;
            visual.Initialize(card, sprite, cardBack, cardInkMaterial, usedCards);
            visibleCards.Add(card, visual);
            return visual;
        }

        private void RefreshCards()
        {
            for (var i = 0; i < session.DealerCards.Count; i++)
            {
                var card = session.DealerCards[i];
                var visual = GetVisual(card);
                var count = session.DealerCards.Count;
                var spacing = Mathf.Min(0.82f, 4.9f / Mathf.Max(1, count - 1));
                visual.SetTarget(new Vector3(-1.35f + (i - (count - 1) * 0.5f) * spacing,
                    1.00f + 0.055f * (1f - Mathf.Abs(i - (count - 1) * 0.5f)), 0f),
                    (i - (count - 1) * 0.5f) * -2.3f, 10 + i);
                visual.SetFaceUp(i != 1 || session.DealerHoleRevealed);
            }

            for (var handIndex = 0; handIndex < session.Hands.Count; handIndex++)
            {
                var hand = session.Hands[handIndex];
                var centre = session.Hands.Count == 1 ? -1.35f
                    : -3.05f + handIndex * (6.1f / (session.Hands.Count - 1));
                var spacing = session.Hands.Count == 1 ? 0.86f : 0.62f;
                spacing = Mathf.Min(spacing,
                    (session.Hands.Count == 1 ? 4.9f : 2.4f) / Mathf.Max(1, hand.Cards.Count - 1));
                for (var i = 0; i < hand.Cards.Count; i++)
                {
                    var visual = GetVisual(hand.Cards[i]);
                    visual.SetTarget(new Vector3(centre + (i - (hand.Cards.Count - 1) * 0.5f) * spacing,
                        -1.25f + 0.045f * (1f - Mathf.Abs(i - (hand.Cards.Count - 1) * 0.5f)), 0f),
                        (i - (hand.Cards.Count - 1) * 0.5f) * -2.5f,
                        30 + handIndex * 15 + i);
                    visual.SetFaceUp(true);
                }
            }
        }

        private void RefreshHud()
        {
            if (bankrollText == null) return;
            bankrollText.text = $"FICHAS  {session.Bankroll:N0}";
            wagerText.text = (session.Phase == BlackjackPhase.Ready || session.Phase == BlackjackPhase.Settled
                ? selectedWager : session.Wager).ToString("N0");
            roundText.text = $"RONDA {Math.Max(1, session.RoundIndex):00}";
            messageText.text = session.LastMessage;
            dealerText.text = session.DealerCards.Count == 0 ? "BANCA\n—"
                : $"BANCA\n{(session.DealerHoleRevealed ? session.DealerValue.Total.ToString() : session.DealerVisibleTotal + " + ?")}";
            playerText.text = session.Hands.Count == 0 ? "TUS MANOS\n—"
                : session.Hands.Count == 1
                    ? $"TU MANO  {session.Hands[0].Value.Total}\n{OutcomeLabel(session.Hands[0].Outcome)}"
                    : $"MANO 1  {session.Hands[0].Value.Total}  {OutcomeLabel(session.Hands[0].Outcome)}\n" +
                      $"MANO 2  {session.Hands[1].Value.Total}  {OutcomeLabel(session.Hands[1].Outcome)}";

            var betting = session.Phase == BlackjackPhase.Ready || session.Phase == BlackjackPhase.Settled;
            dealButton.gameObject.SetActive(betting);
            minusButton.gameObject.SetActive(betting);
            plusButton.gameObject.SetActive(betting);
            dealButton.interactable = session.Bankroll >= rulesAsset.minimumWager;
            minusButton.interactable = selectedWager > rulesAsset.minimumWager;
            plusButton.interactable = selectedWager + rulesAsset.minimumWager <= session.Bankroll;

            var playing = session.Phase == BlackjackPhase.PlayerTurn;
            hitButton.gameObject.SetActive(playing);
            standButton.gameObject.SetActive(playing);
            doubleButton.gameObject.SetActive(playing);
            splitButton.gameObject.SetActive(playing);
            hitButton.interactable = session.CanHit;
            standButton.interactable = session.CanStand;
            doubleButton.interactable = session.CanDouble;
            splitButton.interactable = session.CanSplit;

            var insurance = session.Phase == BlackjackPhase.Insurance;
            insuranceButton.gameObject.SetActive(insurance);
            declineButton.gameObject.SetActive(insurance);
            insuranceButton.interactable = session.CanTakeInsurance;
        }

        private static string OutcomeLabel(HandOutcome outcome) => outcome switch
        {
            HandOutcome.Win => "GANA",
            HandOutcome.Blackjack => "BLACKJACK",
            HandOutcome.Push => "EMPATE",
            HandOutcome.Loss => "PIERDE",
            _ => "EN JUEGO"
        };

        private void BuildHud()
        {
            if (EventSystem.current == null)
            {
                var events = new GameObject("UI Event System");
                events.AddComponent<EventSystem>();
                events.AddComponent<InputSystemUIInputModule>();
            }

            var canvasObject = new GameObject("Blackjack HUD", typeof(RectTransform),
                typeof(Canvas), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 2000;
            var canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(640f, 360f);
            canvasRect.pivot = new Vector2(0f, 1f);
            canvasRect.position = new Vector3(-5f, 2.8125f, -1f);
            canvasRect.localScale = Vector3.one / 64f;

            var top = MakePanel(canvas.transform, "Top Bar", 0, 0, 640, 42, Panel);
            MakePanel(top.transform, "Coral Accent", 0, 39, 640, 3,
                new Color(0.72f, 0.22f, 0.26f, 1f));
            MakeText(top.transform, "Title", "DOUBLE DOWN", 16, 5, 250, 30, 23, Gold, true);
            MakeText(top.transform, "Rule", "BLACKJACK  •  3:2  •  S17", 300, 13, 320, 20, 11, Cyan);

            var side = MakePanel(canvas.transform, "Action Panel", 443, 50, 187, 300, Panel);
            MakePanel(side.transform, "Panel Accent", 0, 0, 187, 3, Gold);
            roundText = MakeText(side.transform, "Round", "RONDA 01", 12, 12, 165, 24, 17, Gold, true);
            bankrollText = MakeText(side.transform, "Bankroll", "FICHAS", 12, 45, 165, 28, 15, Color.white, true);
            MakeText(side.transform, "Wager Label", "APUESTA", 12, 82, 165, 18, 11, Cyan, true);
            minusButton = MakeButton(side.transform, "Less Bet", "−", 12, 105, 34, 32,
                () => ChangeWager(-1));
            wagerText = MakeText(side.transform, "Wager", "10", 52, 108, 82, 26, 17, Gold, true);
            wagerText.alignment = TextAlignmentOptions.Center;
            plusButton = MakeButton(side.transform, "More Bet", "+", 141, 105, 34, 32,
                () => ChangeWager(1));

            dealButton = MakeButton(side.transform, "Deal", "REPARTIR  [ESPACIO]", 12, 149, 163, 38, TryDeal, true);
            hitButton = MakeButton(side.transform, "Hit", "PEDIR  [H]", 12, 149, 78, 35, () => session.Hit());
            standButton = MakeButton(side.transform, "Stand", "PLANTAR  [S]", 97, 149, 78, 35, () => session.Stand());
            doubleButton = MakeButton(side.transform, "Double", "DOBLAR  [D]", 12, 192, 78, 35, () => session.DoubleDown());
            splitButton = MakeButton(side.transform, "Split", "DIVIDIR  [P]", 97, 192, 78, 35, () => session.Split());
            insuranceButton = MakeButton(side.transform, "Insurance", "SEGURO  [Y]", 12, 149, 78, 35,
                () => session.ChooseInsurance(true));
            declineButton = MakeButton(side.transform, "No Insurance", "PASAR  [N]", 97, 149, 78, 35,
                () => session.ChooseInsurance(false));

            MakePanel(side.transform, "Message Background", 12, 240, 163, 48, PanelSoft);
            messageText = MakeText(side.transform, "Message", "", 20, 246, 148, 38, 11, Color.white);

            var dealerPanel = MakePanel(canvas.transform, "Dealer Label", 13, 64, 110, 52, PanelSoft);
            dealerText = MakeText(dealerPanel.transform, "Dealer Total", "BANCA", 8, 6, 100, 44, 13, Color.white, true);
            var playerPanel = MakePanel(canvas.transform, "Player Label", 13, 230, 110, 59, PanelSoft);
            playerText = MakeText(playerPanel.transform, "Player Total", "TU MANO", 8, 7, 100, 49, 12, Color.white, true);
        }

        private static Image MakePanel(Transform parent, string name, float x, float y,
            float width, float height, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            SetRect(panel.GetComponent<RectTransform>(), x, y, width, height);
            var image = panel.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private TextMeshProUGUI MakeText(Transform parent, string name, string value,
            float x, float y, float width, float height, float size, Color color, bool bold = false)
        {
            var label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            label.transform.SetParent(parent, false);
            SetRect(label.GetComponent<RectTransform>(), x, y, width, height);
            var text = label.GetComponent<TextMeshProUGUI>();
            text.font = fontAsset != null ? fontAsset : TMP_Settings.defaultFontAsset;
            text.fontSize = size;
            text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            text.color = color;
            text.text = value;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis;
            return text;
        }

        private Button MakeButton(Transform parent, string name, string label,
            float x, float y, float width, float height, UnityAction action, bool primary = false)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            SetRect(buttonObject.GetComponent<RectTransform>(), x, y, width, height);
            var image = buttonObject.GetComponent<Image>();
            image.color = primary ? new Color(0.58f, 0.18f, 0.22f, 1f) : PanelSoft;
            var button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f),
                pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f),
                selectedColor = Color.white,
                disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.5f),
                colorMultiplier = 1f,
                fadeDuration = 0.08f
            };
            button.onClick.AddListener(action);
            var text = MakeText(buttonObject.transform, "Label", label, 2, 4,
                width - 4, height - 7, primary ? 12 : 10, Color.white, true);
            text.alignment = TextAlignmentOptions.Center;
            return button;
        }

        private static void SetRect(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
