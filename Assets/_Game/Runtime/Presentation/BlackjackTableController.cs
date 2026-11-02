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
        private RelicRack relicRack;
        private RunShop shop;
        private int winStreak;
        private AudioClip scoreSound;
        private Material backdropMaterial;
        private long selectedWager;
        private int usedCards;
        private float dealerTimer;
        private Text bankrollText;
        private Text goldText;
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
            if ((presentation != null && presentation.IsOpen) || (relicRack != null && relicRack.IsOpen) || (shop != null && shop.IsOpen)) return;
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
            if (shop != null && shop.IsOpen) return;
            if (session.Phase != BlackjackPhase.Ready && session.Phase != BlackjackPhase.Settled) return;
            if (scoreRun.State == ScoreRunState.Won)
            {
                if (shop.DepartedRound < scoreRun.Round)
                {
                    shop.Open(scoreRun.Round, TryDeal, scoreRun.HandsRemaining);
                    RefreshHud();
                    return;
                }
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
                shop.ResetRun();
                relicRack.ResetLoadout();
                winStreak = 0;
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
            var activations = new List<int>();
            var hands = new ScoreBreakdown[session.Hands.Count];
            for (var i = 0; i < hands.Length; i++)
            {
                var hand = session.Hands[i];
                hands[i] = ScoreCalculator.Calculate(hand.Value, hand.Outcome,
                    hand.Cards.Count, hand.Stake, scoreRules);
                if (hand.Outcome == HandOutcome.Win || hand.Outcome == HandOutcome.Blackjack) winStreak++;
                else if (hand.Outcome == HandOutcome.Loss) winStreak = 0;
                hands[i] = RelicEngine.Apply(hands[i], hand, relicRack.Loadout, winStreak, activations);
            }
            relicRack.Activate(activations);
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
            formulaText.text = hands.Length > 1 ? "2 MANOS = " + scoreGain :
                first.Chips + " x " + first.Multiplier.ToString() + " = " + scoreGain;
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
            scoreFill.sizeDelta = new Vector2(Mathf.Round(100f * (float)ratio), 4f);
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
            for (var i=0;i<session.DealerCards.Count;i++)
            {
                var visual=GetVisual(session.DealerCards[i]);
                visual.transform.localScale=Vector3.one*1.35f;
                visual.SetTarget(TableLayout.CardPosition(i,session.DealerCards.Count,true,0,false),0f,10+i);
                visual.SetFaceUp(i!=1 || session.DealerHoleRevealed);
            }
            for(var h=0;h<session.Hands.Count;h++)
            {
                var hand=session.Hands[h];
                var split=session.Hands.Count>1;
                for(var i=0;i<hand.Cards.Count;i++)
                {
                    var visual=GetVisual(hand.Cards[i]);
                    visual.transform.localScale=Vector3.one*(split?1.15f:1.35f);
                    visual.SetTarget(TableLayout.CardPosition(i,hand.Cards.Count,false,h,split),0f,30+h*15+i);
                    visual.SetFaceUp(true);
                }
            }
        }

        private void RefreshHud()
        {
            if (bankrollText == null) return;
            bankrollText.text = BigNumber.From(session.Bankroll).ToString();
            goldText.text = shop.Economy.Gold.ToString();
            wagerText.text = (session.Phase == BlackjackPhase.Ready || session.Phase == BlackjackPhase.Settled
                ? selectedWager : session.Wager).ToString("N0");
            roundText.text = $"RONDA    {scoreRun.Round:00}";
            messageText.text = scoreRun.State == ScoreRunState.Won ? "META COMPLETA!\nSIGUIENTE RONDA"
                : scoreRun.State == ScoreRunState.Lost ? "SIN MANOS.\nREINTENTA LA RUN"
                : session.Phase == BlackjackPhase.PlayerTurn ? "PIDE O\nPLANTATE."
                : session.Phase == BlackjackPhase.DealerTurn ? "JUEGA LA BANCA."
                : PixelText(session.LastMessage);
            goalText.text = "META " + scoreRun.Goal;
            handsText.text = "MANOS " + scoreRun.HandsRemaining + "/" + scoreRules.HandsPerRound;
            DrawScore(scoreAnimation >= 1f ? scoreRun.Score : scoreBeforeAnimation);
            dealerText.text = session.DealerCards.Count == 0 ? "BANCA\n-"
                : $"BANCA\n{(session.DealerHoleRevealed ? session.DealerValue.Total.ToString() : session.DealerVisibleTotal + " + ?")}";
            playerText.text = session.Hands.Count == 0 ? "TUS MANOS\n-"
                : session.Hands.Count == 1
                    ? $"TU MANO\n{session.Hands[0].Value.Total}  {OutcomeLabel(session.Hands[0].Outcome)}"
                    : $"MANO 1  {session.Hands[0].Value.Total}\n{SplitHandStatus(0)}\n" +
                      $"MANO 2  {session.Hands[1].Value.Total}\n{SplitHandStatus(1)}";

            var betting = session.Phase == BlackjackPhase.Ready || session.Phase == BlackjackPhase.Settled;
            dealButton.gameObject.SetActive(betting);
            minusButton.gameObject.SetActive(betting && scoreRun.State != ScoreRunState.Lost);
            plusButton.gameObject.SetActive(betting && scoreRun.State != ScoreRunState.Lost);
            dealButton.interactable = session.Bankroll >= rulesAsset.minimumWager;
            if (scoreRun.State == ScoreRunState.Lost) dealButton.interactable = true;
            dealButton.GetComponentInChildren<Text>().text = scoreRun.State == ScoreRunState.Won
                ? "TIENDA [ESP]" : scoreRun.State == ScoreRunState.Lost
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

        private string SplitHandStatus(int index)
        {
            var hand = session.Hands[index];
            if (hand.Value.IsBust) return "PASADA";
            if (session.Phase != BlackjackPhase.PlayerTurn) return OutcomeLabel(hand.Outcome);
            if (index == session.ActiveHandIndex) return "TU TURNO";
            return hand.IsFinished ? "PLANTADA" : "ESPERA";
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

            MakePanel(canvas.transform, "Run Rail", 8, 8, 148, 344, CasinoUiSkin.Felt);
            var top = MakePanel(canvas.transform, "Top Bar", 16, 12, 132, 48, Color.clear);
            MakeIcon(top.transform, "Crown", "crown_b", 8, 12, 24, CasinoUiSkin.Paper);
            MakeText(top.transform, "Title", "DOUBLE DOWN", 44, 0, 84, 48, 24, CasinoUiSkin.Paper);
            var roundBadge = MakePanel(canvas.transform, "Round Badge", 20, 66, 124, 24, CasinoUiSkin.Red);
            CasinoUiSkin.Surface(roundBadge, "Banner", CasinoUiSkin.Red);
            roundText = MakeText(roundBadge.transform, "Round", "", 6, 0, 112, 24, 16, CasinoUiSkin.Paper);
            roundText.alignment = TextAnchor.MiddleCenter;

            var meter = MakePanel(canvas.transform, "Score Meter", 20, 98, 124, 106, CasinoUiSkin.Ink);
            scorePanel = meter.GetComponent<RectTransform>();
            goalText = MakeText(meter.transform, "Goal", "", 8, 6, 108, 18, 16, CasinoUiSkin.Muted);
            goalText.alignment = TextAnchor.MiddleCenter;
            scoreText = MakeText(meter.transform, "Score", "0", 8, 24, 108, 34, 32, CasinoUiSkin.Paper);
            scoreText.alignment = TextAnchor.MiddleCenter;
            var progressTrack = MakeStripe(meter.transform, "Progress Track", 12, 62, 100, 4, CasinoUiSkin.Felt);
            CasinoUiSkin.Surface(progressTrack, "Progress", CasinoUiSkin.Felt);
            scoreFill = MakeStripe(meter.transform, "Progress Fill", 12, 62, 0, 4, Gold).GetComponent<RectTransform>();
            CasinoUiSkin.Surface(scoreFill.GetComponent<Image>(), "Progress", Gold);
            MakeIcon(meter.transform, "Hand Icon", "cards_stack", 12, 73, 12, CasinoUiSkin.Muted);
            handsText = MakeText(meter.transform, "Hands", "", 30, 70, 86, 18, 16, CasinoUiSkin.Muted);
            formulaText = MakeText(meter.transform, "Formula", "FICHAS x MULT", 8, 90, 108, 14, 12, Gold);
            formulaText.alignment = TextAnchor.MiddleCenter;

            var chips = MakePanel(canvas.transform, "Chips Wallet", 20, 212, 60, 44, CasinoUiSkin.Ink);
            MakeIcon(chips.transform, "Icon", "tokens", 6, 5, 12, CasinoUiSkin.Muted);
            MakeText(chips.transform, "Caption", "FICHAS", 22, 3, 34, 14, 12, CasinoUiSkin.Muted);
            bankrollText = MakeText(chips.transform, "Bankroll", "", 4, 18, 52, 24, 24, CasinoUiSkin.Paper);
            bankrollText.alignment = TextAnchor.MiddleCenter;
            var wallet = MakePanel(canvas.transform, "Gold Wallet", 84, 212, 60, 44, CasinoUiSkin.Ink);
            MakeIcon(wallet.transform, "Icon", "token", 6, 5, 12, Gold);
            MakeText(wallet.transform, "Caption", "ORO", 22, 3, 34, 14, 12, Gold);
            goldText = MakeText(wallet.transform, "Gold", "", 4, 18, 52, 24, 24, Gold);
            goldText.alignment = TextAnchor.MiddleCenter;

            var betCaption = MakeText(canvas.transform, "Wager Label", "APUESTA", 52, 264, 60, 14, 12, CasinoUiSkin.Muted);
            betCaption.alignment = TextAnchor.MiddleCenter;
            minusButton = MakeButton(canvas.transform, "Less Bet", "-", 20, 274, 28, 26, ()=>ChangeWager(-1));
            wagerText = MakeText(canvas.transform, "Wager", "10", 52, 278, 60, 24, 24, CasinoUiSkin.Paper);
            wagerText.alignment = TextAnchor.MiddleCenter;
            plusButton = MakeButton(canvas.transform, "More Bet", "+", 116, 274, 28, 26, ()=>ChangeWager(1));
            messageText = MakeText(canvas.transform, "Message", "", 20, 310, 124, 24, 12, CasinoUiSkin.Muted);
            messageText.alignment = TextAnchor.MiddleCenter;

            var side = MakePanel(canvas.transform, "Action Panel", 176, 308, 448, 40, Color.clear);
            hitButton = MakeButton(side.transform, "Hit", "PEDIR [H]", 4, 0, 100, 34, ()=>session.Hit(), true);
            standButton = MakeButton(side.transform, "Stand", "PLANTAR [S]", 116, 0, 100, 34, ()=>session.Stand());
            doubleButton = MakeButton(side.transform, "Double", "DOBLAR [D]", 228, 0, 100, 34, ()=>session.DoubleDown());
            splitButton = MakeButton(side.transform, "Split", "DIVIDIR [P]", 340, 0, 100, 34, ()=>session.Split());
            dealButton = MakeButton(side.transform, "Deal", "REPARTIR [ESP]", 116, 0, 212, 34, TryDeal, true);
            insuranceButton = MakeButton(side.transform, "Insurance", "SEGURO [Y]", 4, 0, 212, 34, ()=>session.ChooseInsurance(true));
            declineButton = MakeButton(side.transform, "No Insurance", "PASAR [N]", 228, 0, 212, 34, ()=>session.ChooseInsurance(false));
            dealerText = MakeText(canvas.transform, "Dealer Total", "BANCA", 180, 128, 88, 54, 16, Cyan);
            playerText = MakeText(canvas.transform, "Player Total", "TU MANO", 180, 222, 88, 70, 16, Gold);
            MakeStripe(canvas.transform, "Dealer Divider", 180, 192, 436, 1, new Color(0.2f,0.4f,0.44f,0.4f));
            relicRack = gameObject.AddComponent<RelicRack>();
            relicRack.Initialize(canvas, fontAsset, () => session.Phase == BlackjackPhase.Ready || session.Phase == BlackjackPhase.Settled);
            shop = gameObject.AddComponent<RunShop>();
            shop.Initialize(canvas, fontAsset, unchecked((int)runSeed));
            shop.Changed += RefreshHud;
            relicRack.Owns = shop.Economy.Owns;
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
            if (color.a > 0f) CasinoUiSkin.Surface(image, "Frame", color);
            return image;
        }

        private static void MakeIcon(Transform parent, string name, string asset, float x, float y, float size, Color tint)
        {
            var icon = MakeStripe(parent, name, x, y, size, size, tint);
            icon.sprite = Resources.Load<Sprite>("TablePresentation/Icons/" + asset);
            icon.preserveAspect = true;
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
            text.material = Graphic.defaultGraphicMaterial;
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
            CasinoUiSkin.Surface(image, "Button", primary ? CasinoUiSkin.Red : CasinoUiSkin.Felt);
            CasinoUiSkin.Surface(shadow, "Button", CasinoUiSkin.Ink);
            var button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(action);
            var text = MakeText(buttonObject.transform, "Label", label, 2, 4,
                width - 4, height - 7, 16, Color.white, true);
            text.alignment = TextAnchor.MiddleCenter;
            buttonObject.GetComponent<ArcadeButtonFeel>().Initialize(image, shadow,
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
