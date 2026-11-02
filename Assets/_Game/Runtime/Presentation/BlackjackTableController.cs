using System;
using System.Collections.Generic;
using System.Text;
using DoubleDown.Blackjack;
using DoubleDown.Scoring;
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
        [SerializeField] private ScoringBalanceConfig scoringAsset;
        [SerializeField] private Sprite[] cardFaces;
        [SerializeField] private Sprite cardBack;
        [SerializeField] private Material cardInkMaterial;
        [SerializeField] private Font fontAsset;
        [SerializeField] private AudioClip dealSound;
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private AudioClip flipSound;
        [SerializeField] private uint runSeed = 20261101u;

        private readonly Dictionary<string, Sprite> spriteByName = new Dictionary<string, Sprite>(52);
        private readonly Dictionary<PlayingCard, BlackjackCardVisual> visibleCards =
            new Dictionary<PlayingCard, BlackjackCardVisual>(52);
        private readonly List<BlackjackCardVisual> cardPool = new List<BlackjackCardVisual>(52);
        private BlackjackSession session;
        private ScoreRun scoreRun;
        private ScoreRules scoreRules;
        private AudioSource audioSource;
        private TablePresentation presentation;
        private AudioClip scoreSound;
        private Material backdropMaterial;
        private long selectedWager;
        private int usedCards;
        private float dealerTimer;
        private Text bankrollText;
        private Text wagerText;
        private Text roundText;
        private Text messageText;
        private Text dealerText;
        private Text playerText;
        private Text scoreText;
        private Text goalText;
        private Text handsText;
        private Text formulaText;
        private RectTransform scorePanel;
        private RectTransform scoreFill;
        private BigNumber scoreBeforeAnimation;
        private BigNumber scoreGain;
        private float scoreAnimation = 1f;
        private float scorePulse;
        private int scoreTick;
        private Button dealButton;
        private Button minusButton;
        private Button plusButton;
        private Button hitButton;
        private Button standButton;
        private Button doubleButton;
        private Button splitButton;
        private Button insuranceButton;
        private Button declineButton;

        private static readonly Color Panel = new Color(0.018f, 0.039f, 0.070f, 0.96f);
        private static readonly Color PanelSoft = new Color(0.025f, 0.075f, 0.105f, 0.94f);
        private static readonly Color Gold = new Color(0.98f, 0.73f, 0.38f, 1f);
        private static readonly Color Cyan = new Color(0.54f, 0.84f, 0.91f, 1f);

        private void Awake()
        {
            if (rulesAsset == null || scoringAsset == null || cardBack == null || cardInkMaterial == null ||
                cardFaces == null || cardFaces.Length != 52)
                throw new InvalidOperationException("Blackjack table scene references are incomplete.");

            foreach (var sprite in cardFaces)
            {
                var name = sprite.texture.name;
                spriteByName[name] = sprite;
            }

            var rules = rulesAsset.ToRules();
            session = new BlackjackSession(rules, runSeed);
            scoreRules = scoringAsset.ToRules();
            scoreRun = new ScoreRun(scoreRules);
            selectedWager = rules.MinimumWager;
            session.EventRaised += OnGameEvent;
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            scoreSound = Resources.Load<AudioClip>("TablePresentation/Audio/switch7");
            var backdrop = GameObject.Find("Casino Vortex (Shader)");
            if (backdrop != null) backdropMaterial = backdrop.GetComponent<MeshRenderer>().material;
            BuildHud();
            RefreshHud();
        }

        private void Start() => TryDeal();

        private void OnDestroy()
        {
            if (session != null) session.EventRaised -= OnGameEvent;
            if (backdropMaterial != null) Destroy(backdropMaterial);
        }

        private void Update()
        {
            if (presentation != null && presentation.IsOpen) return;
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
            UpdateScoreAnimation();
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
            if (keyboard.rKey.wasPressedThisFrame && scoreRun.State == ScoreRunState.Lost) TryDeal();
        }

        private void TryDeal()
        {
            if (session.Phase != BlackjackPhase.Ready && session.Phase != BlackjackPhase.Settled) return;
            if (scoreRun.State == ScoreRunState.Won)
            {
                scoreRun.NextRound();
                scoreBeforeAnimation = BigNumber.Zero;
                scoreGain = BigNumber.Zero;
                scoreAnimation = 1f;
            }
            else if (scoreRun.State == ScoreRunState.Lost)
            {
                session.EventRaised -= OnGameEvent;
                session = new BlackjackSession(rulesAsset.ToRules(), runSeed + (uint)session.RoundIndex);
                session.EventRaised += OnGameEvent;
                scoreRun.Restart();
                selectedWager = rulesAsset.minimumWager;
                scoreBeforeAnimation = BigNumber.Zero;
                scoreGain = BigNumber.Zero;
                scoreAnimation = 1f;
                RecycleCards();
            }
            if (selectedWager > session.Bankroll) selectedWager = rulesAsset.minimumWager;
            if (session.Bankroll < rulesAsset.minimumWager)
            {
                scoreRun.Forfeit();
                RefreshHud();
                return;
            }
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
            if (gameEvent.Type == BlackjackEventType.RoundStarted) { RecycleCards(); if (backdropMaterial != null) backdropMaterial.SetFloat("_State", 0f); }
            if (gameEvent.Type == BlackjackEventType.CardDealt) PlaySound(dealSound, 0.91f, 1.08f);
            if (gameEvent.Type == BlackjackEventType.HoleRevealed) PlaySound(flipSound, 1f, 1f);
            if (gameEvent.Type == BlackjackEventType.RoundSettled) PlaySound(selectSound, 0.95f, 1.05f);
            if (gameEvent.Type == BlackjackEventType.RoundSettled) RecordScore();
            if (gameEvent.Type == BlackjackEventType.PhaseChanged && session.Phase == BlackjackPhase.DealerTurn)
                dealerTimer = 0.65f;
            RefreshCards();
            RefreshHud();
        }

        private void RecordScore()
        {
            var hands = new ScoreBreakdown[session.Hands.Count];
            for (var i = 0; i < hands.Length; i++)
            {
                var hand = session.Hands[i];
                hands[i] = ScoreCalculator.Calculate(hand.Value, hand.Outcome,
                    hand.Cards.Count, hand.Stake, scoreRules);
            }
            scoreBeforeAnimation = scoreRun.Score;
            scoreGain = scoreRun.RecordDeal(hands);
            scoreAnimation = scoreGain.IsZero ? 1f : 0f;
            scoreTick = 0;
            scorePulse = scoreGain.IsZero ? 0.25f : 1f;
            foreach (var hand in session.Hands)
                foreach (var card in hand.Cards)
                    if (visibleCards.TryGetValue(card, out var visual))
                        visual.SetEdition(hand.Outcome == HandOutcome.Blackjack ? 2f :
                            hand.Outcome == HandOutcome.Win ? 1f : 0f);
            if (backdropMaterial != null) backdropMaterial.SetFloat("_State",
                scoreRun.State == ScoreRunState.Won ? 1f : scoreRun.State == ScoreRunState.Lost ? -1f : 0f);
            var first = hands[0];
            formulaText.text = first.Chips + " x " + first.Multiplier.ToString("0.#") +
                " = " + scoreGain;
        }

        private void UpdateScoreAnimation()
        {
            scorePulse = Mathf.MoveTowards(scorePulse, 0f, Time.unscaledDeltaTime * 1.6f);
            if (scoreAnimation < 1f)
            {
                scoreAnimation = Mathf.Min(1f, scoreAnimation + Time.unscaledDeltaTime / 0.72f);
                var eased = 1f - Mathf.Pow(1f - scoreAnimation, 3f);
                DrawScore(scoreBeforeAnimation + scoreGain * eased);
                var nextTick = Mathf.FloorToInt(scoreAnimation * 4f);
                if (nextTick > scoreTick)
                {
                    scoreTick = nextTick;
                    PlaySound(scoreSound, 0.90f + nextTick * 0.10f,
                        0.93f + nextTick * 0.10f);
                }
            }
            if (scorePanel != null)
                scorePanel.localScale = Vector3.one * (1f + (TablePresentation.ReducedMotion ? 0f : scorePulse * 0.035f * TablePresentation.EffectStrength));
            if (backdropMaterial != null)
                backdropMaterial.SetFloat("_Pulse", scorePulse * 0.8f * TablePresentation.EffectStrength);
        }

        private void DrawScore(BigNumber amount)
        {
            scoreText.text = amount.ToString();
            if (scoreFill == null) return;
            var ratio = amount.IsZero ? 0d : amount >= scoreRun.Goal ? 1d :
                amount.Mantissa / scoreRun.Goal.Mantissa *
                Math.Pow(10d, amount.Exponent - scoreRun.Goal.Exponent);
            scoreFill.sizeDelta = new Vector2(Mathf.Round(116f * (float)ratio), 4f);
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
            roundText.text = $"RONDA {scoreRun.Round:00}";
            messageText.text = scoreRun.State == ScoreRunState.Won ? "META COMPLETA!\nSIGUIENTE RONDA"
                : scoreRun.State == ScoreRunState.Lost ? "SIN MANOS.\nREINTENTA LA RUN"
                : PixelText(session.LastMessage);
            goalText.text = "META " + scoreRun.Goal;
            handsText.text = "MANOS " + scoreRun.HandsRemaining + "/" + scoreRules.HandsPerRound;
            DrawScore(scoreAnimation >= 1f ? scoreRun.Score : scoreBeforeAnimation);
            dealerText.text = session.DealerCards.Count == 0 ? "BANCA\n-"
                : $"BANCA\n{(session.DealerHoleRevealed ? session.DealerValue.Total.ToString() : session.DealerVisibleTotal + " + ?")}";
            playerText.text = session.Hands.Count == 0 ? "TUS MANOS\n-"
                : session.Hands.Count == 1
                    ? $"TU MANO  {session.Hands[0].Value.Total}\n{OutcomeLabel(session.Hands[0].Outcome)}"
                    : $"MANO 1  {session.Hands[0].Value.Total}  {OutcomeLabel(session.Hands[0].Outcome)}\n" +
                      $"MANO 2  {session.Hands[1].Value.Total}  {OutcomeLabel(session.Hands[1].Outcome)}";

            var betting = session.Phase == BlackjackPhase.Ready || session.Phase == BlackjackPhase.Settled;
            dealButton.gameObject.SetActive(betting);
            minusButton.gameObject.SetActive(betting && scoreRun.State != ScoreRunState.Lost);
            plusButton.gameObject.SetActive(betting && scoreRun.State != ScoreRunState.Lost);
            dealButton.interactable = session.Bankroll >= rulesAsset.minimumWager;
            if (scoreRun.State == ScoreRunState.Lost) dealButton.interactable = true;
            dealButton.GetComponentInChildren<Text>().text = scoreRun.State == ScoreRunState.Won
                ? "SIGUIENTE [ESP]" : scoreRun.State == ScoreRunState.Lost
                    ? "REINTENTAR [R]" : "REPARTIR [ESP]";
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

        private static string PixelText(string value)
        {
            var text = value.ToUpperInvariant().Normalize(NormalizationForm.FormD);
            var result = new StringBuilder(text.Length);
            foreach (var character in text)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character) ==
                    System.Globalization.UnicodeCategory.NonSpacingMark) continue;
                result.Append(character == '¿' ? '?' : character == '¡' ? '!' : character);
            }
            return result.ToString();
        }

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
            MakeText(top.transform, "Title", "DOUBLE DOWN", 16, 4, 250, 32, 32, Gold, true);
            MakeText(top.transform, "Rule", "BLACKJACK / 3:2 / S17", 295, 13, 325, 20, 16, Cyan);

            var side = MakePanel(canvas.transform, "Action Panel", 443, 50, 187, 300, Panel);
            MakePanel(side.transform, "Panel Accent", 0, 0, 187, 3, Gold);
            roundText = MakeText(side.transform, "Round", "RONDA 01", 12, 12, 165, 24, 16, Gold, true);
            bankrollText = MakeText(side.transform, "Bankroll", "FICHAS", 12, 45, 165, 28, 16, Color.white, true);
            MakeText(side.transform, "Wager Label", "APUESTA", 12, 82, 165, 18, 16, Cyan, true);
            minusButton = MakeButton(side.transform, "Less Bet", "-", 12, 105, 34, 32,
                () => ChangeWager(-1));
            wagerText = MakeText(side.transform, "Wager", "10", 52, 108, 82, 26, 16, Gold, true);
            wagerText.alignment = TextAnchor.MiddleCenter;
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
            messageText = MakeText(side.transform, "Message", "", 20, 246, 148, 38, 16, Color.white);

            var dealerPanel = MakePanel(canvas.transform, "Dealer Label", 13, 64, 110, 52, PanelSoft);
            dealerText = MakeText(dealerPanel.transform, "Dealer Total", "BANCA", 8, 6, 100, 44, 16, Color.white, true);
            var playerPanel = MakePanel(canvas.transform, "Player Label", 13, 230, 110, 59, PanelSoft);
            playerText = MakeText(playerPanel.transform, "Player Total", "TU MANO", 8, 7, 100, 49, 16, Color.white, true);

            var meter = MakePanel(canvas.transform, "Score Meter", 302, 127, 129, 106, Panel);
            scorePanel = meter.GetComponent<RectTransform>();
            MakeStripe(meter.transform, "Score Accent", 0, 0, 129, 3, Gold);
            goalText = MakeText(meter.transform, "Goal", "META 140", 7, 7, 116, 18, 16, Gold);
            scoreText = MakeText(meter.transform, "Score", "0", 7, 24, 116, 29, 24, Color.white);
            handsText = MakeText(meter.transform, "Hands", "MANOS 4/4", 7, 54, 116, 18, 16, Cyan);
            MakeStripe(meter.transform, "Progress Track", 7, 74, 116, 4,
                new Color(0.11f, 0.21f, 0.25f, 1f));
            scoreFill = MakeStripe(meter.transform, "Progress Fill", 7, 74, 0, 4, Gold)
                .GetComponent<RectTransform>();
            formulaText = MakeText(meter.transform, "Formula", "FICHAS x MULT", 7, 82, 116, 18, 16, Gold);
            var token = MakeStripe(side.transform, "Bankroll Icon", 147, 43, 24, 24, Gold);
            token.sprite = Resources.Load<Sprite>("TablePresentation/Icons/tokens");
            token.preserveAspect = true;
            var keyNames = new[] { "H", "S", "D", "P" };
            for (var i = 0; i < keyNames.Length; i++)
            {
                var key = MakeStripe(canvas.transform, "Key " + keyNames[i], 16 + i * 36, 320, 32, 32, Color.white);
                key.sprite = Resources.Load<Sprite>("TablePresentation/Keys/" + keyNames[i]);
                key.preserveAspect = true;
                key.gameObject.AddComponent<KeycapHint>().Initialize(key, key.sprite,
                    (Key)Enum.Parse(typeof(Key), keyNames[i]));
            }
            presentation = gameObject.AddComponent<TablePresentation>();
            presentation.Initialize(canvas, fontAsset, backdropMaterial);
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
            if (width > 8 && height > 8)
            {
                MakeStripe(panel.transform, "Top edge", 0, 0, width, 2,
                    new Color(0.28f, 0.52f, 0.58f, 0.65f));
                MakeStripe(panel.transform, "Left edge", 0, 0, 2, height,
                    new Color(0.19f, 0.38f, 0.44f, 0.65f));
                MakeStripe(panel.transform, "Bottom edge", 0, height - 3, width, 3,
                    new Color(0.006f, 0.016f, 0.03f, 0.95f));
                MakeStripe(panel.transform, "Right edge", width - 3, 0, 3, height,
                    new Color(0.006f, 0.016f, 0.03f, 0.95f));
            }
            return image;
        }

        private static Image MakeStripe(Transform parent, string name, float x, float y,
            float width, float height, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            SetRect(go.GetComponent<RectTransform>(), x, y, width, height);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private Text MakeText(Transform parent, string name, string value,
            float x, float y, float width, float height, float size, Color color, bool bold = false)
        {
            var label = new GameObject(name, typeof(RectTransform), typeof(Text));
            label.transform.SetParent(parent, false);
            SetRect(label.GetComponent<RectTransform>(), x, y, width, height);
            var text = label.GetComponent<Text>();
            text.font = fontAsset;
            text.fontSize = Mathf.RoundToInt(size);
            text.fontStyle = FontStyle.Normal;
            text.color = color;
            text.text = value;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private Button MakeButton(Transform parent, string name, string label,
            float x, float y, float width, float height, UnityAction action, bool primary = false)
        {
            var shadow = MakeStripe(parent, name + " Shadow", x + 3, y + 4, width, height,
                new Color(0.005f, 0.012f, 0.025f, 0.95f));
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image),
                typeof(Button), typeof(CanvasGroup), typeof(ArcadeButtonFeel));
            buttonObject.transform.SetParent(parent, false);
            SetRect(buttonObject.GetComponent<RectTransform>(), x, y, width, height);
            var image = buttonObject.GetComponent<Image>();
            image.color = primary ? new Color(0.65f, 0.20f, 0.24f, 1f)
                : new Color(0.075f, 0.20f, 0.24f, 1f);
            MakeStripe(buttonObject.transform, "Upper bevel", 0, 0, width, 3,
                primary ? new Color(1f, 0.60f, 0.40f, 1f) : new Color(0.38f, 0.70f, 0.70f, 1f));
            MakeStripe(buttonObject.transform, "Lower bevel", 0, height - 4, width, 4,
                new Color(0.008f, 0.025f, 0.045f, 0.85f));
            var shine = MakeStripe(buttonObject.transform, "Hover shine", 4, 4,
                width - 8, height - 10, Color.clear);
            var button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(action);
            var text = MakeText(buttonObject.transform, "Label", label, 2, 4,
                width - 4, height - 7, 16, Color.white, true);
            text.alignment = TextAnchor.MiddleCenter;
            buttonObject.GetComponent<ArcadeButtonFeel>().Initialize(image, shine, shadow,
                audioSource, Resources.Load<AudioClip>("TablePresentation/Audio/click3"));
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
