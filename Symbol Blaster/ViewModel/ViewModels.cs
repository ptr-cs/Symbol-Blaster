using SymbolBlaster.Game;
using SymbolBlaster.UI.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SymbolBlaster.ViewModel
{
    /// <summary>
    /// Enums representative of major game states.
    /// </summary>
    public enum GameState
    {
        READY_TO_PLAY = 0,
        GAME_ACTIVE = 1,
        GAME_OVER = 2,
        ENTER_NAME = 3,
        SHOW_SCORES = 4
    };

    public enum MainTabPage
    {
        Configure = 0,
        Presets = 1,
        Help = 2
    };

    public enum PresetsTabPage
    {
        BuiltIn = 0,
        Custom = 1
    };

    public enum PresetType
    {
        BuiltIn = 0,
        Custom = 1
    }

    /// <summary>
    /// GameViewModel contains Properties, Commands, and Methods that facilitate gameplay.
    /// 
    /// </summary>
    public class GameViewModel : BaseViewModel
    {
        private string screenOfFameString = "";
        public string ScreenOfFameString
        {
            get => screenOfFameString;
            set => SetProperty(ref screenOfFameString, value);
        }

        private string nameEntryString = "";
        public string NameEntryString
        {
            get => nameEntryString;
            set => SetProperty(ref nameEntryString, value);
        }

        private GameState gameState = GameState.GAME_ACTIVE;
        public GameState GameState
        {
            get => gameState;
            set => SetProperty(ref gameState, value);
        }

        private PlayerControlMode playerControlMode = PlayerControlMode.Retro;
        public PlayerControlMode PlayerControlMode
        {
            get => playerControlMode;
            set
            {
                SetProperty(ref playerControlMode, value);
                Config.PlayerControlMode = PlayerControlMode;
            }
        }

        private EdgeMode renderQuality = EdgeMode.Unspecified;
        public EdgeMode RenderQuality
        {
            get => renderQuality;
            set
            {
                if (renderQuality != value)
                    renderQualityChanged = true;
                SetProperty(ref renderQuality, value);
                Config.RenderQuality = RenderQuality;
            }
        }

        private int currentScore = 0;
        public int CurrentScore
        {
            get => currentScore;
            set => SetProperty(ref currentScore, value);
        }

        private int highScore = 0;
        public int HighScore
        {
            get => highScore;
            set => SetProperty(ref highScore, value);
        }

        private int livesRemaining = GameDefs.PLAYER_STARTING_LIVES;
        public int LivesRemaining
        {
            get => livesRemaining;
            set => SetProperty(ref livesRemaining, value);
        }

        private string livesRemainingString = "";
        public string LivesRemainingString
        {
            get => livesRemainingString;
            set => SetProperty(ref livesRemainingString, value);
        }

        private bool isLeftPressed = false;
        public bool IsLeftPressed
        {
            get => isLeftPressed;
            set => SetProperty(ref isLeftPressed, value);
        }

        private bool isRightPressed = false;
        public bool IsRightPressed
        {
            get => isRightPressed;
            set => SetProperty(ref isRightPressed, value);
        }

        private bool isUpPressed = false;
        public bool IsUpPressed
        {
            get => isUpPressed;
            set => SetProperty(ref isUpPressed, value);
        }

        private bool isDownPressed = false;
        public bool IsDownPressed
        {
            get => isDownPressed;
            set => SetProperty(ref isDownPressed, value);
        }

        private bool isAKeyPressed = false;
        public bool IsAKeyPressed
        {
            get => isAKeyPressed;
            set => SetProperty(ref isAKeyPressed, value);
        }

        private bool isDKeyPressed = false;
        public bool IsDKeyPressed
        {
            get => isDKeyPressed;
            set => SetProperty(ref isDKeyPressed, value);
        }

        private bool isWKeyPressed = false;
        public bool IsWKeyPressed
        {
            get => isWKeyPressed;
            set => SetProperty(ref isWKeyPressed, value);
        }

        private bool isSKeyPressed = false;
        public bool IsSKeyPressed
        {
            get => isSKeyPressed;
            set => SetProperty(ref isSKeyPressed, value);
        }

        private bool isLeftArrowPressed = false;
        public bool IsLeftArrowPressed
        {
            get => isLeftArrowPressed;
            set => SetProperty(ref isLeftArrowPressed, value);
        }

        private bool isRightArrowPressed = false;
        public bool IsRightArrowPressed
        {
            get => isRightArrowPressed;
            set => SetProperty(ref isRightArrowPressed, value);
        }

        private bool isUpArrowPressed = false;
        public bool IsUpArrowPressed
        {
            get => isUpArrowPressed;
            set => SetProperty(ref isUpArrowPressed, value);
        }

        private bool isDownArrowPressed = false;
        public bool IsDownArrowPressed
        {
            get => isDownArrowPressed;
            set => SetProperty(ref isDownArrowPressed, value);
        }

        private bool isFirePressed = false;
        public bool IsFirePressed
        {
            get => isFirePressed;
            set => SetProperty(ref isFirePressed, value);
        }

        private bool isWarpPressed = false;
        public bool IsWarpPressed
        {
            get => isWarpPressed;
            set => SetProperty(ref isWarpPressed, value);
        }

        private int levelCounter = 1;
        public int LevelCounter
        {
            get => levelCounter;
            set => SetProperty(ref levelCounter, value);
        }

        private bool showLevelCounter = false;
        public bool ShowLevelCounter
        {
            get => showLevelCounter;
            set => SetProperty(ref showLevelCounter, value);
        }

        private string saveConfigurationPresetName = "";
        public string SaveConfigurationPresetName
        {
            get => saveConfigurationPresetName;
            set => SetProperty(ref saveConfigurationPresetName, value);
        }

        private bool presetImportFailed = false;
        public bool PresetImportFailed
        {
            get => presetImportFailed;
            set => SetProperty(ref presetImportFailed, value);
        }

        public ICommand? RotateGameSpriteClockwise { get; set; }
        public ICommand? RotateGameSpriteCounterClockwise { get; set; }
        public ICommand? SetGameObjectStroke { get; set; }
        public ICommand? SetGameObjectFill { get; set; }
        public ICommand? SetGameForeground { get; set; }
        public ICommand? ApplyForegroundBackgroundToAll { get; set; }
        public ICommand? ResetAllForegroundBackground { get; set; }
        public ICommand? SaveConfigurationPreset { get; set; }
        public ICommand? DeleteConfigurationPreset { get; set; }
        public ICommand? LoadConfigurationPreset { get; set; }
        public ICommand? RandomizeCurrentSprites { get; set; }
        public ICommand? RandomizeGameColors { get; set; }
        public ICommand? ImportPresetFromFile { get; set; }
        public ICommand? ExportPresetToFile { get; set; }
        public ICommand? ResetPresetImportFailedFlag { get; set; }
        public ICommand? ToggleRenderQuality { get; set; }
        public ICommand? ToggleGameControlMode { get; set; }

        public ObservableCollection<GameConfiguration> ConfigurationPresetsCustom = new();
        public CollectionViewSource ConfigurationPresetCustomCollectionViewSource { get; set; } = new();
        public ICollectionView ConfigurationPresetCustomCollectionView { get; set; }

        public List<GameConfiguration> ConfigurationPresetsBuiltIn = gameConfigurations;
        public CollectionViewSource ConfigurationPresetBuiltInCollectionViewSource { get; set; } = new();
        public ICollectionView ConfigurationPresetBuiltInCollectionView { get; set; }
        public CollectionViewSource ColorSchemeCollectionViewSource { get; set; } = new();
        public ICollectionView ColorSchemeCollectionView { get; set; }
        public CollectionViewSource SpriteGroupCollectionViewSource { get; set; } = new();
        public ICollectionView SpriteGroupCollectionView { get; set; }
        public List<string> CurrentSpriteCollection { get; set; } = new(emojiSprites);
        public CollectionViewSource CurrentSpriteCollectionViewSource { get; set; } = new();
        public ICollectionView PlayerSpriteCollectionView { get; set; }
        public ICollectionView LargeObstacleSpriteCollectionView { get; set; }
        public ICollectionView MediumObstacleSpriteCollectionView { get; set; }
        public ICollectionView SmallObstacleSpriteCollectionView { get; set; }
        public ICollectionView LargeEnemySpriteCollectionView { get; set; }
        public ICollectionView SmallEnemySpriteCollectionView { get; set; }
        public ICollectionView ProjectileSpriteCollectionView { get; set; }
        public ICollectionView PlayerDebrisSpriteCollectionView { get; set; }

        private static readonly List<GameConfiguration> gameConfigurations = new()
        {
            new GameConfiguration()
            {
                ConfigurationName = "Ocean",
                PlayerControlMode = PlayerControlMode.Retro,
                ColorScheme = ColorScheme.Custom,
                SpriteGroup = SpriteGroup.Emoji,
                RenderQuality = EdgeMode.Unspecified,
                GameForeground = (Color)ColorConverter.ConvertFromString("#F7F9F8"),
                GameBackground = (Color)ColorConverter.ConvertFromString("#03297E"),
                PlayerColor = (Color)ColorConverter.ConvertFromString("#00C0FF"),
                ProjectileColor = Colors.SkyBlue,
                PlayerDebrisColor = (Color)ColorConverter.ConvertFromString("#F7F9F8"),
                LargeObstacleColor = (Color)ColorConverter.ConvertFromString("#FED440"),
                MediumObstacleColor = (Color)ColorConverter.ConvertFromString("#FC7C00"),
                SmallObstacleColor = (Color)ColorConverter.ConvertFromString("#FB0000"),
                LargeEnemyColor = (Color)ColorConverter.ConvertFromString("#C5EC41"),
                SmallEnemyColor = (Color)ColorConverter.ConvertFromString("#B119E0"),
                EllipseDebrisColor= Colors.SkyBlue,
                PlayerGlyph = "🐠",
                ProjectileGlyph = "💧",
                PlayerDebrisGlyph = "🦴",
                LargeObstacleGlyph = "🐳",
                MediumObstacleGlyph = "🦈",
                SmallObstacleGlyph = "🦞",
                LargeEnemyGlyph = "🦑",
                SmallEnemyGlyph = "🐙",
                PlayerRotation = 90,
                ProjectileRotation = 90,
            },
            new GameConfiguration()
            {
                ConfigurationName = "Money",
                PlayerControlMode = PlayerControlMode.Retro,
                ColorScheme = ColorScheme.Custom,
                SpriteGroup = SpriteGroup.Money,
                RenderQuality = EdgeMode.Unspecified,
                GameForeground = (Color)ColorConverter.ConvertFromString("#C5EC41"),
                GameBackground = (Color)ColorConverter.ConvertFromString("#002000"),
                PlayerColor = (Color)ColorConverter.ConvertFromString("#00FF00"),
                ProjectileColor = (Color)ColorConverter.ConvertFromString("#00EE00"),
                PlayerDebrisColor = (Color)ColorConverter.ConvertFromString("#00DD00"),
                LargeObstacleColor = (Color)ColorConverter.ConvertFromString("#00CC00"),
                MediumObstacleColor = (Color)ColorConverter.ConvertFromString("#00BB00"),
                SmallObstacleColor = (Color)ColorConverter.ConvertFromString("#00AA00"),
                LargeEnemyColor = (Color)ColorConverter.ConvertFromString("#009900"),
                SmallEnemyColor = (Color)ColorConverter.ConvertFromString("#008800"),
                PlayerGlyph = "$",
                ProjectileGlyph = "₵",
                PlayerDebrisGlyph = "₽",
                LargeObstacleGlyph = "₤",
                MediumObstacleGlyph = "₿",
                SmallObstacleGlyph = "₭",
                LargeEnemyGlyph = "€",
                SmallEnemyGlyph = "₦",
                PlayerRotation = 0,
                ProjectileRotation = 45,
            },
            new GameConfiguration()
            {
                ConfigurationName = "Shapes",
                PlayerControlMode = PlayerControlMode.Retro,
                ColorScheme = ColorScheme.Mono,
                SpriteGroup = SpriteGroup.Geometric,
                RenderQuality = EdgeMode.Unspecified,
                GameForeground = Colors.White,
                GameBackground = Colors.Black,
                PlayerColor = Colors.White,
                ProjectileColor = Colors.White,
                PlayerDebrisColor = Colors.White,
                LargeObstacleColor = Colors.White,
                MediumObstacleColor = Colors.White,
                SmallObstacleColor = Colors.White,
                LargeEnemyColor = Colors.White,
                SmallEnemyColor = Colors.White,
                PlayerGlyph = "◆",
                ProjectileGlyph = "◎",
                PlayerDebrisGlyph = "◇",
                LargeObstacleGlyph = "◕",
                MediumObstacleGlyph = "◑",
                SmallObstacleGlyph = "◔",
                LargeEnemyGlyph = "▯",
                SmallEnemyGlyph = "◻",
                PlayerRotation = 0,
                ProjectileRotation = 0,
            },
            new GameConfiguration()
            {
                ConfigurationName = "Vehicles",
                PlayerControlMode = PlayerControlMode.Retro,
                ColorScheme = ColorScheme.Custom,
                SpriteGroup = SpriteGroup.Emoji,
                RenderQuality = EdgeMode.Unspecified,
                GameForeground = (Color)ColorConverter.ConvertFromString("#F7F9F8"),
                GameBackground = (Color)ColorConverter.ConvertFromString("#283040"),
                PlayerColor = (Color)ColorConverter.ConvertFromString("#FB0000"),
                ProjectileColor = (Color)ColorConverter.ConvertFromString("#F7F9F8"),
                PlayerDebrisColor = (Color)ColorConverter.ConvertFromString("#FED440"),
                LargeObstacleColor = (Color)ColorConverter.ConvertFromString("#F2AE30"),
                MediumObstacleColor = (Color)ColorConverter.ConvertFromString("#aaa"),
                SmallObstacleColor = (Color)ColorConverter.ConvertFromString("#eee"),
                LargeEnemyColor = (Color)ColorConverter.ConvertFromString("#D9C39A"),
                SmallEnemyColor = (Color)ColorConverter.ConvertFromString("#95A8B5"),
                PlayerGlyph = "🏎",
                ProjectileGlyph = "🏁",
                PlayerDebrisGlyph = "💥",
                LargeObstacleGlyph = "🚌",
                MediumObstacleGlyph = "🚚",
                SmallObstacleGlyph = "🚗",
                LargeEnemyGlyph = "🛻",
                SmallEnemyGlyph = "🚓",
                PlayerRotation = 90,
                ProjectileRotation = 0,
            },
            new GameConfiguration()
            {
                ConfigurationName = "Jungle",
                PlayerControlMode = PlayerControlMode.Retro,
                ColorScheme = ColorScheme.Custom,
                SpriteGroup = SpriteGroup.Emoji,
                RenderQuality = EdgeMode.Unspecified,
                GameForeground = (Color)ColorConverter.ConvertFromString("#A8F25E"),
                GameBackground = (Color)ColorConverter.ConvertFromString("#022601"),
                PlayerColor = (Color)ColorConverter.ConvertFromString("#F2790F"),
                ProjectileColor = Colors.SaddleBrown,
                PlayerDebrisColor = (Color)ColorConverter.ConvertFromString("#FED440"),
                LargeObstacleColor = (Color)ColorConverter.ConvertFromString("#AF9588"),
                MediumObstacleColor = (Color)ColorConverter.ConvertFromString("#BEC2C1"),
                SmallObstacleColor = (Color)ColorConverter.ConvertFromString("#9EAFA7"),
                LargeEnemyColor = (Color)ColorConverter.ConvertFromString("#8C8490"),
                SmallEnemyColor = (Color)ColorConverter.ConvertFromString("#C1828A"),
                PlayerGlyph = "🦧",
                ProjectileGlyph = "💩",
                PlayerDebrisGlyph = "🍃",
                LargeObstacleGlyph = "🐘",
                MediumObstacleGlyph = "🐂",
                SmallObstacleGlyph = "🐊",
                LargeEnemyGlyph = "🦍",
                SmallEnemyGlyph = "🐒",
                PlayerRotation = 0,
                ProjectileRotation = 270,
            },
            new GameConfiguration()
            {
                ConfigurationName = "Sports",
                PlayerControlMode = PlayerControlMode.Retro,
                ColorScheme = ColorScheme.Custom,
                SpriteGroup = SpriteGroup.Emoji,
                RenderQuality = EdgeMode.Unspecified,
                GameForeground = (Color)ColorConverter.ConvertFromString("#eee"),
                GameBackground = (Color)ColorConverter.ConvertFromString("#83A603"),
                PlayerColor = (Color)ColorConverter.ConvertFromString("#D9043D"),
                ProjectileColor = Colors.SaddleBrown,
                PlayerDebrisColor = (Color)ColorConverter.ConvertFromString("#FFC000"),
                LargeObstacleColor = (Color)ColorConverter.ConvertFromString("#056CF2"),
                MediumObstacleColor = (Color)ColorConverter.ConvertFromString("#0583F2"),
                SmallObstacleColor = (Color)ColorConverter.ConvertFromString("#0460D9"),
                LargeEnemyColor = (Color)ColorConverter.ConvertFromString("#0034CB"),
                SmallEnemyColor = (Color)ColorConverter.ConvertFromString("#011140"),
                PlayerGlyph = "🚶",
                ProjectileGlyph = "🏈",
                PlayerDebrisGlyph = "📣",
                LargeObstacleGlyph = "🏄",
                MediumObstacleGlyph = "🤸",
                SmallObstacleGlyph = "🏃",
                LargeEnemyGlyph = "🏌",
                SmallEnemyGlyph = "🤾",
                PlayerRotation = 0,
                ProjectileRotation = 45,
            }
        };

        static readonly List<string> geometricShapeSprites = new() { "■","□","▢","▣","▤","▥","▦","▧","▨","▩","▪","▫","▬","▭","▮","▯","▰","▱","▲","△","▴","▵","▶","▷","▸","▹","►","▻","▼","▽","▾","▿","◀","◁","◂","◃","◄","◅","◆","◇","◈","◉","◊","○","◌","◍","◎","●","◐","◑","◒","◓","◔","◕","◖","◗","◘","◙","◚","◛","◜","◝","◞","◟","◠","◡","◢","◣","◤","◥","◦","◧","◨","◩","◪","◫","◬","◭","◮","◯","◰","◱","◲","◳","◴","◵","◶","◷","◸","◹","◺","◻","◼","◽","◾","◿" };

        static readonly List<string> egyptianHieroglyphSprites = new() { "𓀀","𓀁","𓀂","𓀃","𓀄","𓀅","𓀆","𓀇","𓀈","𓀉","𓀊","𓀋","𓀌","𓀍","𓀎","𓀏","𓀐","𓀑","𓀒","𓀓","𓀔","𓀕","𓀖","𓀗","𓀘","𓀙","𓀚","𓀛","𓀜","𓀝","𓀞","𓀟","𓀠","𓀡","𓀢","𓀣","𓀤","𓀥","𓀦","𓀧","𓀨","𓀩","𓀪","𓀫","𓀬","𓀭","𓀮","𓀯","𓀰","𓀱","𓀲","𓀳","𓀴","𓀵","𓀶","𓀷","𓀸","𓀹","𓀺","𓀻","𓀼","𓀽","𓀾","𓀿","𓁀","𓁁","𓁂","𓁃","𓁄","𓁅","𓁆","𓁇","𓁈","𓁉","𓁊","𓁋","𓁌","𓁍","𓁎","𓁏","𓁐","𓁑","𓁒","𓁓","𓁔","𓁕","𓁖","𓁗","𓁘","𓁙","𓁚","𓁛","𓁜","𓁝","𓁞","𓁟","𓁠","𓁡","𓁢","𓁣","𓁤","𓁥","𓁦","𓁧","𓁨","𓁩","𓁪","𓁫","𓁬","𓁭","𓁮","𓁯","𓁰","𓁱","𓁲","𓁳","𓁴","𓁵","𓁶","𓁷","𓁸","𓁹","𓁺","𓁻","𓁼","𓁽","𓁾","𓁿","𓂀","𓂁","𓂂","𓂃","𓂄","𓂅","𓂆","𓂇","𓂈","𓂉","𓂊","𓂋","𓂌","𓂍","𓂎","𓂏","𓂐","𓂑","𓂒","𓂓","𓂔","𓂕","𓂖","𓂗","𓂘","𓂙","𓂚","𓂛","𓂜","𓂝","𓂞","𓂟","𓂠","𓂡","𓂢","𓂣","𓂤","𓂥","𓂦","𓂧","𓂨","𓂩","𓂪","𓂫","𓂬","𓂭","𓂮","𓂯","𓂰","𓂱","𓂲","𓂳","𓂴","𓂵","𓂶","𓂷","𓂻","𓂼","𓂽","𓂾","𓂿","𓃀","𓃁","𓃂","𓃃","𓃄","𓃅","𓃆","𓃇","𓃈","𓃉","𓃊","𓃋","𓃌","𓃍","𓃎","𓃏","𓃐","𓃑","𓃒","𓃓","𓃔","𓃕","𓃖","𓃗","𓃘","𓃙","𓃚","𓃛","𓃜","𓃝","𓃞","𓃟","𓃠","𓃡","𓃢","𓃣","𓃤","𓃥","𓃦","𓃧","𓃨","𓃩","𓃪","𓃫","𓃬","𓃭","𓃮","𓃯","𓃰","𓃱","𓃲","𓃳","𓃴","𓃵","𓃶","𓃷","𓃸","𓃹","𓃺","𓃻","𓃼","𓃽","𓃾","𓃿","𓄀","𓄁","𓄂","𓄃","𓄄","𓄅","𓄆","𓄇","𓄈","𓄉","𓄊","𓄋","𓄌","𓄍","𓄎","𓄏","𓄐","𓄑","𓄒","𓄓","𓄔","𓄕","𓄖","𓄗","𓄘","𓄙","𓄚","𓄛","𓄜","𓄝","𓄞","𓄟","𓄠","𓄡","𓄢","𓄣","𓄤","𓄥","𓄦","𓄧","𓄨","𓄩","𓄪","𓄫","𓄬","𓄭","𓄮","𓄯","𓄰","𓄱","𓄲","𓄳","𓄴","𓄵","𓄶","𓄷","𓄸","𓄹","𓄺","𓄻","𓄼","𓄽","𓄾","𓄿","𓅀","𓅁","𓅂","𓅃","𓅄","𓅅","𓅆","𓅇","𓅈","𓅉","𓅊","𓅋","𓅌","𓅍","𓅎","𓅏","𓅐","𓅑","𓅒","𓅓","𓅔","𓅕","𓅖","𓅗","𓅘","𓅙","𓅚","𓅛","𓅜","𓅝","𓅞" };

        static readonly List<string> moneySprites = new() { "$","₠","₡","₢","₣","₤","₥","₦","₧","₨","₩","₪","₫","€","₭","₮","₯","₰","₱","₲","₳","₴","₵","₶","₷","₸","₹","₺","₻","₼","₽","₾","₿" };

        static readonly List<string> musicSprites = new() { "𝄆", "𝄇", "𝄉", "𝄊", "𝄋", "𝄞", "𝄟", "𝄠", "𝄡", "𝄢", "𝄣", "𝄤", "𝄫", "𝄬", "𝄭", "𝄮", "𝄯", "𝄰", "𝄱", "𝄲", "𝄳", "𝄴", "𝄵", "𝄶", "𝄷", "𝄸", "𝄹", "𝄺", "𝄻", "𝄼", "𝄽", "𝄾", "𝄿", "𝅀", "𝅁", "𝅂", "𝅝", "𝅗𝅥", "𝅘𝅥", "𝅘𝅥𝅮", "𝅘𝅥𝅯", "𝅘𝅥𝅰", "𝅘𝅥𝅱", "𝅘𝅥𝅲", "𝅮", "𝅯", "𝅰", "𝅱", "𝅲", "𝆌", "𝆍", "𝆎", "𝆏", "𝆐", "𝆑", "𝆒", "𝆓", "𝆔", "𝆕", "𝆖", "𝆗", "𝆘", "𝆙", "𝆚", "𝆛", "𝆜", "𝆝" };

        static readonly List<string> emojiSprites = new() { "⌚", "⌛", "⌨", "⏏", "⏰", "⏱", "⏲", "⏳", "Ⓜ", "▪", "▫", "◻", "◼", "◽", "◾", "☀", "☁", "☂", "☃", "☄", "☎", "☑", "☔", "☕", "☘", "☝", "☢", "☣", "☦", "☪", "☮", "☯", "☸", "☹", "☺", "♀", "♂", "♈", "♉", "♊", "♋", "♌", "♍", "♎", "♏", "♐", "♑", "♒", "♓", "♟", "♨", "♻", "♾", "♿", "⚒", "⚓", "⚔", "⚕", "⚖", "⚗", "⚙", "⚛", "⚜", "⚠", "⚡", "⚧", "⚪", "⚰", "⚱", "⚽", "⚾", "⛄", "⛅", "⛈", "⛎", "⛏", "⛑", "⛓", "⛔", "⛩", "⛪", "⛰", "⛱", "⛲", "⛳", "⛴", "⛵", "⛷", "⛸", "⛹", "⛺", "⛽", "✂", "✅", "✈", "✉", "✊", "✋", "✌", "✍", "✏", "✒", "✔", "✝", "✡", "✨", "✳", "✴", "❄", "❇", "❌", "❎", "❓", "❔", "❕", "❗", "❣", "❤", "➕", "➖", "➗", "➡", "⤴", "⤵", "⬅", "⬆", "⬛", "⬜", "⭐", "〰", "〽", "い", "秘", "🀄", "🃏", "🅰", "🅱", "🅾", "🅿", "🆎", "🆒", "🆓", "🆔", "🆗", "🆘", "🈁", "🈂", "🈚", "禁", "空", "合", "満", "有", "月", "申", "割", "営", "得", "可", "🌀", "🌁", "🌂", "🌃", "🌄", "🌅", "🌆", "🌇", "🌈", "🌉", "🌊", "🌋", "🌌", "🌍", "🌎", "🌏", "🌐", "🌑", "🌒", "🌓", "🌔", "🌕", "🌖", "🌗", "🌘", "🌙", "🌚", "🌛", "🌜", "🌝", "🌞", "🌟", "🌠", "🌡", "🌤", "🌥", "🌦", "🌧", "🌨", "🌩", "🌪", "🌫", "🌬", "🌭", "🌮", "🌯", "🌰", "🌱", "🌲", "🌳", "🌴", "🌵", "🌶", "🌷", "🌸", "🌹", "🌺", "🌻", "🌼", "🌽", "🌾", "🌿", "🍀", "🍁", "🍂", "🍃", "🍄", "🍅", "🍆", "🍇", "🍈", "🍉", "🍊", "🍋", "🍌", "🍍", "🍎", "🍏", "🍐", "🍑", "🍒", "🍓", "🍔", "🍕", "🍖", "🍗", "🍘", "🍙", "🍚", "🍛", "🍜", "🍝", "🍞", "🍟", "🍠", "🍡", "🍢", "🍣", "🍤", "🍥", "🍦", "🍧", "🍨", "🍩", "🍪", "🍫", "🍬", "🍭", "🍮", "🍯", "🍰", "🍱", "🍲", "🍳", "🍴", "🍵", "🍶", "🍷", "🍸", "🍹", "🍺", "🍻", "🍼", "🍽", "🍾", "🍿", "🎀", "🎁", "🎂", "🎃", "🎄", "🎅", "🎆", "🎇", "🎈", "🎉", "🎊", "🎋", "🎌", "🎍", "🎎", "🎏", "🎐", "🎑", "🎒", "🎓", "🎖", "🎗", "🎙", "🎚", "🎛", "🎞", "🎟", "🎠", "🎡", "🎢", "🎣", "🎤", "🎥", "🎦", "🎧", "🎨", "🎩", "🎪", "🎫", "🎬", "🎭", "🎮", "🎯", "🎰", "🎱", "🎲", "🎳", "🎴", "🎵", "🎶", "🎷", "🎸", "🎹", "🎺", "🎻", "🎼", "🎽", "🎾", "🎿", "🏀", "🏁", "🏂", "🏃", "🏄", "🏅", "🏆", "🏇", "🏈", "🏉", "🏊", "🏌", "🏍", "🏎", "🏏", "🏐", "🏑", "🏒", "🏓", "🏔", "🏕", "🏖", "🏗", "🏘", "🏙", "🏚", "🏛", "🏜", "🏝", "🏞", "🏟", "🏠", "🏡", "🏢", "〒", "🏤", "🏥", "🏦", "🏧", "🏨", "🏩", "🏪", "🏫", "🏬", "🏭", "🏮", "🏯", "🏰", "🏳", "🏴", "🏵", "🏷", "🏸", "🏹", "🏺", "🐀", "🐁", "🐂", "🐃", "🐄", "🐅", "🐆", "🐇", "🐈", "🐉", "🐊", "🐋", "🐌", "🐍", "🐎", "🐏", "🐐", "🐑", "🐒", "🐓", "🐔", "🐕", "🐖", "🐗", "🐘", "🐙", "🐚", "🐜", "🐝", "🐞", "🐟", "🐠", "🐡", "🐢", "🐣", "🐤", "🐥", "🐦", "🐧", "🐨", "🐩", "🐪", "🐫", "🐬", "🐭", "🐮", "🐯", "🐰", "🐱", "🐲", "🐳", "🐴", "🐵", "🐶", "🐷", "🐸", "🐹", "🐺", "🐻", "🐼", "🐽", "🐾", "🐿", "👀", "👁", "👂", "👃", "👄", "👅", "👆", "👇", "👈", "👉", "👋", "👌", "👍", "👎", "👏", "👐", "👑", "👒", "👓", "👔", "👕", "👖", "👗", "👘", "👙", "👚", "👛", "👜", "👞", "👟", "👠", "👡", "👢", "👣", "👤", "👥", "👦", "👧", "👨", "👩", "👪", "👫", "👬", "👭", "👮", "👯", "👰", "👱", "👲", "👳", "👴", "👵", "👶", "👷", "👸", "👹", "👺", "👻", "👼", "👽", "👾", "👿", "💀", "💂", "💃", "💄", "💅", "💆", "💇", "💈", "💉", "💊", "💋", "💌", "💍", "💎", "💏", "💐", "💑", "💒", "💓", "💔", "💕", "💖", "💗", "💘", "💙", "💚", "💛", "💜", "💝", "💞", "💟", "💠", "💡", "💢", "💣", "💤", "💥", "💦", "💧", "💨", "💩", "💪", "💫", "💬", "💭", "💮", "💯", "💰", "💱", "💲", "💳", "💴", "💵", "💶", "💷", "💸", "💹", "💺", "💻", "💼", "💽", "💾", "💿", "📀", "📁", "📂", "📃", "📄", "📅", "📆", "📇", "📈", "📉", "📊", "📋", "📌", "📍", "📎", "📏", "📐", "📑", "📒", "📓", "📔", "📕", "📖", "📗", "📘", "📙", "📚", "📛", "📜", "📝", "📞", "📟", "📠", "📡", "📢", "📣", "📤", "📥", "📦", "📧", "📨", "📩", "📪", "📫", "📬", "📭", "📮", "📯", "📰", "📱", "📲", "📳", "📴", "📵", "📶", "📷", "📸", "📹", "📺", "📻", "📼", "📽", "📿", "🔀", "🔁", "🔂", "🔃", "🔄", "🔅", "🔆", "🔈", "🔉", "🔊", "🔋", "🔌", "🔍", "🔎", "🔏", "🔐", "🔑", "🔒", "🔓", "🔕", "🔖", "🔗", "🔘", "🔙", "🔚", "🔛", "🔜", "🔝", "🔟", "🔠", "🔡", "🔢", "🔣", "🔤", "🔥", "🔦", "🔧", "🔨", "🔩", "🔪", "🔫", "🔬", "🔭", "🔮", "🔯", "🔰", "🔱", "🔲", "🔳", "🔵", "🔶", "🔷", "🔸", "🔹", "🔺", "🔻", "🔼", "🔽", "🕉", "🕊", "🕋", "🕌", "🕍", "🕎", "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙", "🕚", "🕛", "🕜", "🕝", "🕞", "🕟", "🕠", "🕡", "🕢", "🕣", "🕤", "🕥", "🕦", "🕧", "🕯", "🕰", "🕴", "🕵", "🕶", "🕷", "🕸", "🕹", "🕺", "🖇", "🖊", "🖋", "🖌", "🖍", "🖐", "🖕", "🖖", "🖤", "🖥", "🖨", "🖱", "🖲", "🖼", "🗂", "🗃", "🗄", "🗑", "🗒", "🗓", "🗜", "🗝", "🗞", "🗡", "🗣", "🗨", "🗯", "🗳", "🗺", "🗻", "🗼", "🗽", "🗾", "🗿", "😀", "😁", "😂", "😃", "😄", "😅", "😆", "😇", "😈", "😉", "😊", "😋", "😌", "😍", "😎", "😏", "😐", "😑", "😒", "😓", "😔", "😕", "😖", "😗", "😘", "😙", "😚", "😛", "😜", "😝", "😞", "😟", "😠", "😡", "😢", "😣", "😤", "😥", "😦", "😧", "😨", "😩", "😪", "😫", "😬", "😭", "😮", "😯", "😰", "😱", "😲", "😳", "😴", "😵", "😶", "😷", "😸", "😹", "😺", "😻", "😼", "😽", "😾", "😿", "🙀", "🙁", "🙂", "🙃", "🙄", "🙅", "🙆", "🙇", "🙈", "🙉", "🙊", "🙋", "🙌", "🙍", "🙎", "🙏", "🚀", "🚁", "🚂", "🚃", "🚄", "🚅", "🚆", "🚇", "🚈", "🚉", "🚊", "🚋", "🚌", "🚍", "🚎", "🚏", "🚐", "🚑", "🚒", "🚓", "🚔", "🚕", "🚖", "🚗", "🚘", "🚙", "🚚", "🚛", "🚜", "🚝", "🚞", "🚟", "🚠", "🚡", "🚢", "🚣", "🚤", "🚥", "🚦", "🚧", "🚨", "🚩", "🚪", "🚫", "🚬", "🚭", "🚮", "🚯", "🚰", "🚱", "🚲", "🚳", "🚴", "🚵", "🚶", "🚷", "🚸", "🚹", "🚺", "🚻", "🚼", "🚽", "🚾", "🚿", "🛀", "🛁", "🛂", "🛃", "🛄", "🛅", "🛋", "🛌", "🛍", "🛎", "🛏", "🛐", "🛑", "🛒", "🛕", "🛖", "🛗", "🛠", "🛡", "🛢", "🛣", "🛤", "🛥", "🛩", "🛫", "🛬", "🛰", "🛳", "🛴", "🛵", "🛶", "🛷", "🛸", "🛹", "🛺", "🛻", "🛼", "🤌", "🤍", "🤎", "🤏", "🤐", "🤑", "🤒", "🤓", "🤔", "🤕", "🤖", "🤗", "🤘", "🤙", "🤚", "🤛", "🤜", "🤝", "🤞", "🤟", "🤠", "🤡", "🤢", "🤣", "🤤", "🤥", "🤦", "🤧", "🤨", "🤩", "🤪", "🤫", "🤬", "🤭", "🤮", "🤯", "🤰", "🤱", "🤲", "🤳", "🤴", "🤵", "🤶", "🤷", "🤸", "🤹", "🤺", "🤼", "🤽", "🤾", "🤿", "🥀", "🥁", "🥂", "🥃", "🥄", "🥅", "🥇", "🥈", "🥉", "🥊", "🥋", "🥌", "🥍", "🥎", "🥏", "🥐", "🥑", "🥒", "🥓", "🥔", "🥕", "🥖", "🥗", "🥘", "🥙", "🥚", "🥛", "🥜", "🥝", "🥞", "🥟", "🥠", "🥡", "🥢", "🥣", "🥤", "🥥", "🥦", "🥧", "🥨", "🥩", "🥪", "🥫", "🥬", "🥭", "🥮", "🥯", "🥰", "🥱", "🥲", "🥳", "🥴", "🥵", "🥶", "🥷", "🥸", "🥺", "🥻", "🥼", "🥽", "🥾", "🥿", "🦀", "🦁", "🦂", "🦃", "🦄", "🦅", "🦆", "🦇", "🦈", "🦉", "🦊", "🦋", "🦌", "🦍", "🦎", "🦏", "🦐", "🦑", "🦒", "🦓", "🦔", "🦕", "🦖", "🦗", "🦘", "🦙", "🦚", "🦛", "🦜", "🦝", "🦞", "🦟", "🦠", "🦡", "🦢", "🦣", "🦤", "🦥", "🦦", "🦧", "🦨", "🦩", "🦪", "🦫", "🦬", "🦭", "🦮", "🦯", "🦰", "🦱", "🦲", "🦳", "🦴", "🦵", "🦶", "🦷", "🦸", "🦹", "🦺", "🦻", "🦼", "🦽", "🦾", "🦿", "🧀", "🧁", "🧂", "🧃", "🧄", "🧅", "🧆", "🧇", "🧈", "🧉", "🧊", "🧋", "🧍", "🧎", "🧏", "🧐", "🧑", "🧒", "🧓", "🧔", "🧕", "🧖", "🧗", "🧘", "🧙", "🧚", "🧛", "🧝", "🧞", "🧟", "🧠", "🧡", "🧢", "🧣", "🧤", "🧥", "🧦", "🧧", "🧨", "🧩", "🧪", "🧫", "🧬", "🧭", "🧮", "🧯", "🧰", "🧱", "🧲", "🧳", "🧴", "🧵", "🧶", "🧷", "🧸", "🧹", "🧺", "🧻", "🧼", "🧽", "🧾", "🧿" };


        public List<SpriteGroup> spriteGroups = new() { SpriteGroup.Emoji, SpriteGroup.Music, SpriteGroup.Money, SpriteGroup.Hieroglyphs, SpriteGroup.Geometric };

        public List<ColorScheme> colorSchemes = new() { ColorScheme.Dark, ColorScheme.Light, ColorScheme.Console, ColorScheme.RetroGreen, ColorScheme.Mono, ColorScheme.Custom };

        protected Panel? gameContainer;
        public List<Tuple<string, int>> HighScores = new();
        public Timer GameOverTimer = new(2000) { AutoReset = false };
        public Timer NewLevelTimer = new(2000) { AutoReset = false };
        public GameConfiguration Config;

        private Player? player1;

        readonly Random random;

        public ObservableCollection<Obstacle> Obstacles = new();
        public ObservableCollection<Projectile> Projectiles = new();
        public ObservableCollection<Enemy> Enemies = new();
        public ObservableCollection<EllipseDebris> EllipseDebrisCollection = new();
        public ObservableCollection<PlayerDebris> PlayerDebrisCollection = new();
        public ObservableCollection<Player> Players = new();

        public List<MovingShape> ShapesToBeRemoved = new();

        private readonly static List<DependencyObject> hitResultsList = new();

        public int startingObstaclesSpawned = 0;
        public int obstaclesDestroyed = 0;
        public bool renderQualityChanged = false;

        private readonly List<Color> colorSelectorPresets = new() { (Color)ColorConverter.ConvertFromString("#FB0000"), (Color)ColorConverter.ConvertFromString("#FFA019"), (Color)ColorConverter.ConvertFromString("#FED440"), (Color)ColorConverter.ConvertFromString("#C5EC41"), (Color)ColorConverter.ConvertFromString("#00C0FF"), (Color)ColorConverter.ConvertFromString("#BC0100"), (Color)ColorConverter.ConvertFromString("#FC7C00"), (Color)ColorConverter.ConvertFromString("#FFC000"), (Color)ColorConverter.ConvertFromString("#6DC400"), (Color)ColorConverter.ConvertFromString("#0597F2"), (Color)ColorConverter.ConvertFromString("#0044A4"), (Color)ColorConverter.ConvertFromString("#B119E0"), (Color)ColorConverter.ConvertFromString("#F7F9F8"), (Color)ColorConverter.ConvertFromString("#A3AABA"), (Color)ColorConverter.ConvertFromString("#222222"), (Color)ColorConverter.ConvertFromString("#03297E"), (Color)ColorConverter.ConvertFromString("#7000BC"), (Color)ColorConverter.ConvertFromString("#CDD2D6"), (Color)ColorConverter.ConvertFromString("#888DA0"), (Color)ColorConverter.ConvertFromString("#131313") };

        protected ColorSelector? colorSelector;
        protected Popup? colorSelectorPopup;

        public void SetGameContainer(Panel p) { gameContainer = p; }
        public Panel? GetGameContainer() { return gameContainer; }

        public void SetPlayerControlMode(PlayerControlMode mode)
        {
            Config.PlayerControlMode = mode;
            PlayerControlMode = mode;
        }

        public void SetRenderQuality(EdgeMode renderQuality)
        {
            Config.RenderQuality = renderQuality;
            RenderQuality = renderQuality;
        }

        public void AddToScore(int number)
        {
            CurrentScore += number;
        }

        public void DecrementLives(string livesString)
        {
            LivesRemaining--;
            RebuildLivesString(livesString);
        }

        public void RebuildLivesString(string livesString)
        {
            LivesRemainingString = "";
            for (int i = 0; i < LivesRemaining; ++i)
                LivesRemainingString += livesString;
        }

        public void ReadyToPlay()
        {
            GameState = GameState.READY_TO_PLAY;
        }

        public void RestartGame(bool playerBeatPreviousLevel = false)
        {
            ResetGame(playerBeatPreviousLevel);
            StartGame(playerBeatPreviousLevel);
        }

        public static string FormatNameEntryString(string name)
        {
            if (name.Length < GameDefs.MAX_NAME_LENGTH)
                return name.PadRight(GameDefs.MAX_NAME_LENGTH, ' ');
            else
                return name[..GameDefs.MAX_NAME_LENGTH];
        }

        public void AddHighScore()
        {
            NameEntryString = FormatNameEntryString(NameEntryString);
            HighScores.Add(new Tuple<string, int>(NameEntryString, CurrentScore));
        }

        public void ResetGame(bool playerBeatPreviousLevel = false)
        {
            if (!playerBeatPreviousLevel)
            {
                IsLeftPressed = false;
                IsRightPressed = false;
                IsUpPressed = false;
                IsDownPressed = false;
                IsFirePressed = false;
                IsWarpPressed = false;

                CurrentScore = 0;
                LivesRemaining = GameDefs.PLAYER_STARTING_LIVES;

                Players.Clear();
            }
            else
            {
                LevelCounter++;
                ShowLevelCounter = true;
                NewLevelTimer.Start();
            }

            Obstacles.Clear();
            Projectiles.Clear();
            Enemies.Clear();
            EllipseDebrisCollection.Clear();
            PlayerDebrisCollection.Clear();
            ShapesToBeRemoved.Clear();

            hitResultsList.Clear();

            startingObstaclesSpawned = obstaclesDestroyed = 0;

            NameEntryString = "";
        }

        public void ClearGameContainer()
        {
            gameContainer?.Children.Clear();
        }

        public void ClearScreen()
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                ClearGameContainer();
            });
        }

        public void ClearScreen(Player? keepPlayer)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                ClearGameContainer();
                gameContainer?.Children.Add(keepPlayer);
            });
        }

        public void StartGame(bool playerBeatPreviousLevel = false)
        {
            GameState = GameState.GAME_ACTIVE;
            SetCompositionTargetRendering();
            if (!playerBeatPreviousLevel)
            {
                SpawnPlayer();
            }
            RebuildLivesString(Config.PlayerGlyph);
        }

        public void GameOver()
        {
            GameState = GameState.GAME_OVER;
            GameOverTimer.Start();
        }

        private void GameOverTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            ClearScreen();
            EnterName();
        }

        private void NewRoundTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            ShowLevelCounter = false;
        }

        public void EnterName()
        {
            GameState = GameState.ENTER_NAME;
            if (HighScores.Count == 0)
            {
                HighScore = CurrentScore;
            }
            else
            {
                var sortedScores = HighScores.OrderByDescending(t => t.Item2).ToList();
                int highestScore = sortedScores[0].Item2;
                HighScore = (highestScore < CurrentScore) ? CurrentScore : highestScore;
            }
        }

        public void ShowHighScores()
        {
            ScreenOfFameString = "SCREEN OF FAME\n";
            var sortedScores = HighScores.OrderByDescending(t => t.Item2).ToList();
            for (int i = 0; i < sortedScores.Count; ++i)
            {
                ScreenOfFameString += String.Format("{0}. {1} {2}\n", (i + 1), sortedScores[i].Item1, sortedScores[i].Item2);
            }
            GameState = GameState.SHOW_SCORES;
        }

        public static RotateTransform CreateFrozenRotation(int angleDegrees)
        {
            RotateTransform rotation = new(angleDegrees);
            if (rotation.CanFreeze)
                rotation.Freeze();

            return rotation;
        }

        public static SolidColorBrush CreateFrozenBrush(Color color)
        {
            SolidColorBrush brush = new(color);
            if (brush.CanFreeze)
                brush.Freeze();

            return brush;
        }

        protected bool playerNeedsRefresh = false;
        private SolidColorBrush playerBrush = new();
        public SolidColorBrush PlayerBrush
        {
            get => playerBrush;
            set
            {
                if (playerBrush != value)
                    playerNeedsRefresh = true;
                SetProperty(ref playerBrush, value);
            }
        }
        public void SetPlayerColorBrush(Color color)
        {
            Config.PlayerColor = color;
            PlayerBrush = CreateFrozenBrush(color);
        }

        protected bool playerDebrisNeedsRefresh = false;
        private SolidColorBrush playerDebrisBrush = new();
        public SolidColorBrush PlayerDebrisBrush
        {
            get => playerDebrisBrush;
            set
            {
                if (playerDebrisBrush != value)
                    playerDebrisNeedsRefresh = true;
                SetProperty(ref playerDebrisBrush, value);
            }
        }
        public void SetPlayerDebrisColorBrush(Color color)
        {
            Config.PlayerDebrisColor = color;
            PlayerDebrisBrush = CreateFrozenBrush(color);
        }

        protected bool ellipseDebrisNeedsRefresh = false;
        private SolidColorBrush ellipseDebrisBrush = new();
        public SolidColorBrush EllipseDebrisBrush
        {
            get => ellipseDebrisBrush;
            set
            {
                if (ellipseDebrisBrush != value)
                    ellipseDebrisNeedsRefresh = true;
                SetProperty(ref ellipseDebrisBrush, value);
            }
        }
        public void SetEllipseDebrisColorBrush(Color color)
        {
            Config.EllipseDebrisColor = color;
            EllipseDebrisBrush = CreateFrozenBrush(color);
        }

        protected bool projectileNeedsRefresh = false;
        private SolidColorBrush projectileBrush = new();
        public SolidColorBrush ProjectileBrush
        {
            get => projectileBrush;
            set
            {
                if (projectileBrush != value)
                    projectileNeedsRefresh = true;
                SetProperty(ref projectileBrush, value);
            }
        }
        public void SetProjectileColorBrush(Color color)
        {
            Config.ProjectileColor = color;
            ProjectileBrush = CreateFrozenBrush(color);
        }

        protected bool largeObstacleNeedsRefresh = false;
        private SolidColorBrush largeObstacleBrush = new();
        public SolidColorBrush LargeObstacleBrush
        {
            get => largeObstacleBrush;
            set
            {
                if (largeObstacleBrush != value)
                    largeObstacleNeedsRefresh = true;
                SetProperty(ref largeObstacleBrush, value);
            }
        }
        public void SetLargeObstacleColorBrush(Color color)
        {
            Config.LargeObstacleColor = color;
            LargeObstacleBrush = CreateFrozenBrush(color);
        }

        protected bool mediumObstacleNeedsRefresh = false;
        private SolidColorBrush mediumObstacleBrush = new();
        public SolidColorBrush MediumObstacleBrush
        {
            get => mediumObstacleBrush;
            set
            {
                if (mediumObstacleBrush != value)
                    mediumObstacleNeedsRefresh = true;
                SetProperty(ref mediumObstacleBrush, value);
            }
        }
        public void SetMediumObstacleColorBrush(Color color)
        {
            Config.MediumObstacleColor = color;
            MediumObstacleBrush = CreateFrozenBrush(color);
        }

        protected bool smallObstacleNeedsRefresh = false;
        private SolidColorBrush smallObstacleBrush = new();
        public SolidColorBrush SmallObstacleBrush
        {
            get => smallObstacleBrush;
            set
            {
                if (smallObstacleBrush != value)
                    smallObstacleNeedsRefresh = true;
                SetProperty(ref smallObstacleBrush, value);
            }
        }
        public void SetSmallObstacleColorBrush(Color color)
        {
            Config.SmallObstacleColor = color;
            SmallObstacleBrush = CreateFrozenBrush(color);
        }

        protected bool largeEnemyNeedsRefresh = false;
        private SolidColorBrush largeEnemyBrush = new();
        public SolidColorBrush LargeEnemyBrush
        {
            get => largeEnemyBrush;
            set
            {
                if (largeEnemyBrush != value)
                    largeEnemyNeedsRefresh = true;
                SetProperty(ref largeEnemyBrush, value);
            }
        }
        public void SetLargeEnemyColorBrush(Color color)
        {
            Config.LargeEnemyColor = color;
            LargeEnemyBrush = CreateFrozenBrush(color);
        }

        protected bool smallEnemyNeedsRefresh = false;
        private SolidColorBrush smallEnemyBrush = new();
        public SolidColorBrush SmallEnemyBrush
        {
            get => smallEnemyBrush;
            set
            {
                if (smallEnemyBrush != value)
                    smallEnemyNeedsRefresh = true;
                SetProperty(ref smallEnemyBrush, value);
            }
        }
        public void SetSmallEnemyColorBrush(Color color)
        {
            Config.SmallEnemyColor = color;
            SmallEnemyBrush = CreateFrozenBrush(color);
        }

        private SolidColorBrush gameForegroundBrush = new();
        public SolidColorBrush GameForegroundBrush
        {
            get => gameForegroundBrush;
            set => SetProperty(ref gameForegroundBrush, value);
        }

        private SolidColorBrush gameBackgroundBrush = new();
        public SolidColorBrush GameBackgroundBrush
        {
            get => gameBackgroundBrush;
            set => SetProperty(ref gameBackgroundBrush, value);
        }

        public void SetGameForegroundColorBrush(Color color)
        {
            Config.GameForeground = color;
            GameForegroundBrush = CreateFrozenBrush(color);
        }

        public void SetGameBackgroundColorBrush(Color color)
        {
            Config.GameBackground = color;
            GameBackgroundBrush = CreateFrozenBrush(color);
        }

        private RotateTransform playerRotateTransform = new();
        public RotateTransform PlayerRotateTransform
        {
            get => playerRotateTransform;
            set
            {
                if (playerRotateTransform != value)
                    playerNeedsRefresh = true;
                SetProperty(ref playerRotateTransform, value);
            }
        }
        public void SetPlayerSpriteRotation(int angleDegrees)
        {
            Config.PlayerRotation = angleDegrees;
            PlayerRotateTransform = CreateFrozenRotation(Config.PlayerRotation);
        }

        private RotateTransform projectileRotateTransform = new();
        public RotateTransform ProjectileRotateTransform
        {
            get => projectileRotateTransform;
            set
            {
                if (projectileRotateTransform != value)
                    projectileNeedsRefresh = true;
                SetProperty(ref projectileRotateTransform, value);
            }
        }
        public void SetProjectileSpriteRotation(int angleDegrees)
        {
            Config.ProjectileRotation = angleDegrees;
            ProjectileRotateTransform = CreateFrozenRotation(Config.ProjectileRotation);
        }

        public GameViewModel()
        {
            random = new Random();
            Config = new GameConfiguration();
            colorSelector = new ColorSelector() { PresetColors = new ObservableCollection<Color>(colorSelectorPresets) };
            colorSelector.Closed += ColorSelector_Closed;
            colorSelectorPopup = new Popup() { Width = 160, Child = colorSelector };

            RotateGameSpriteClockwise = new DelegateCommand(OnRotateGameSpriteClockwise, null);
            RotateGameSpriteCounterClockwise = new DelegateCommand(OnRotateGameSpriteCounterClockwise, null);
            SetGameObjectFill = new DelegateCommand(OnSetGameObjectFill, null);
            SetGameForeground = new DelegateCommand(OnSetGameForeground, null);
            ApplyForegroundBackgroundToAll = new DelegateCommand(OnApplyForegroundBackgroundToAll, null);
            ResetAllForegroundBackground = new DelegateCommand(OnResetAllForegroundBackground, null);
            SaveConfigurationPreset = new DelegateCommand(OnSaveConfigurationPreset, null);
            DeleteConfigurationPreset = new DelegateCommand(OnDeleteConfigurationPreset, null);
            LoadConfigurationPreset = new DelegateCommand(OnLoadConfigurationPreset, null);
            RandomizeCurrentSprites = new DelegateCommand(OnRandomizeCurrentSprites, null);
            RandomizeGameColors = new DelegateCommand(OnRandomizeGameColors, null);
            ImportPresetFromFile = new DelegateCommand(OnImportPresetFromFile, null);
            ExportPresetToFile = new DelegateCommand(OnExportPresetToFile, null);
            ResetPresetImportFailedFlag = new DelegateCommand(OnResetPresetImportFailedFlag, null);
            ToggleGameControlMode = new DelegateCommand(OnGameControlModeToggled, null);
            ToggleRenderQuality = new DelegateCommand(OnToggleRenderQuality, null);

            // WPF-ism that seems to improve CompositionTarget.Rendering performance based on A-B test comparison
            // using the Visual Studio Performance Profiler:
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

            ColorSchemeCollectionViewSource.Source = colorSchemes;
            ColorSchemeCollectionView = new CollectionView(ColorSchemeCollectionViewSource.View);
            ColorSchemeCollectionView.CurrentChanged += ColorSchemesCollectionView_CurrentChanged;

            SpriteGroupCollectionViewSource.Source = spriteGroups;
            SpriteGroupCollectionView = new CollectionView(SpriteGroupCollectionViewSource.View);
            SpriteGroupCollectionView.CurrentChanged += SpriteGroupCollectionView_CurrentChanged;

            CurrentSpriteCollectionViewSource.Source = CurrentSpriteCollection;
            PlayerSpriteCollectionView = new CollectionView(CurrentSpriteCollectionViewSource.View);
            PlayerSpriteCollectionView.CurrentChanged += PlayerSpriteCollectionView_CurrentChanged;

            LargeObstacleSpriteCollectionView = new CollectionView(CurrentSpriteCollectionViewSource.View);
            LargeObstacleSpriteCollectionView.CurrentChanged += LargeObstacleSpriteCollectionView_CurrentChanged;

            MediumObstacleSpriteCollectionView = new CollectionView(CurrentSpriteCollectionViewSource.View);
            MediumObstacleSpriteCollectionView.CurrentChanged += MediumObstacleSpriteCollectionView_CurrentChanged;

            SmallObstacleSpriteCollectionView = new CollectionView(CurrentSpriteCollectionViewSource.View);
            SmallObstacleSpriteCollectionView.CurrentChanged += SmallObstacleSpriteCollectionView_CurrentChanged;

            LargeEnemySpriteCollectionView = new CollectionView(CurrentSpriteCollectionViewSource.View);
            LargeEnemySpriteCollectionView.CurrentChanged += LargeEnemySpriteCollectionView_CurrentChanged;

            SmallEnemySpriteCollectionView = new CollectionView(CurrentSpriteCollectionViewSource.View);
            SmallEnemySpriteCollectionView.CurrentChanged += SmallEnemySpriteCollectionView_CurrentChanged;

            ProjectileSpriteCollectionView = new CollectionView(CurrentSpriteCollectionViewSource.View);
            ProjectileSpriteCollectionView.CurrentChanged += ProjectileSpriteCollectionView_CurrentChanged;

            PlayerDebrisSpriteCollectionView = new CollectionView(CurrentSpriteCollectionViewSource.View);
            PlayerDebrisSpriteCollectionView.CurrentChanged += PlayerDebrisSpriteCollectionView_CurrentChanged;

            ConfigurationPresetCustomCollectionViewSource.Source = ConfigurationPresetsCustom;
            ConfigurationPresetCustomCollectionView = new CollectionView(ConfigurationPresetCustomCollectionViewSource.View);

            ConfigurationPresetBuiltInCollectionViewSource.Source = ConfigurationPresetsBuiltIn;
            ConfigurationPresetBuiltInCollectionView = new CollectionView(ConfigurationPresetBuiltInCollectionViewSource.View);

            SetPlayerControlMode(PlayerControlMode);
            SetRenderQuality(RenderQuality);

            GameOverTimer.Elapsed += GameOverTimer_Elapsed;
            NewLevelTimer.Elapsed += NewRoundTimer_Elapsed;

            Obstacles.CollectionChanged += Obstacles_CollectionChanged;
            Projectiles.CollectionChanged += Projectiles_CollectionChanged;
            Enemies.CollectionChanged += Enemies_CollectionChanged;
            EllipseDebrisCollection.CollectionChanged += EllipseDebris_CollectionChanged;
            PlayerDebrisCollection.CollectionChanged += PlayerDebrisCollection_CollectionChanged;
            Players.CollectionChanged += Players_CollectionChanged;
        }

        private void OnToggleRenderQuality(object? renderQuality)
        {
            if (renderQuality is EdgeMode mode)
                SetRenderQuality(mode);
        }

        private void OnGameControlModeToggled(object? mode)
        {
            if (mode is PlayerControlMode controlMode)
                SetPlayerControlMode(controlMode);
        }

        private void OnResetPresetImportFailedFlag(object? obj)
        {
            PresetImportFailed = false;
        }

        private void OnExportPresetToFile(object? obj)
        {
            if (ConfigurationPresetCustomCollectionView.CurrentItem == null)
                return;

            GameConfiguration gameConfiguration = (GameConfiguration)ConfigurationPresetCustomCollectionView.CurrentItem;
            string fileName = $"{gameConfiguration.ConfigurationName}.json";
            string jsonString = JsonSerializer.Serialize(gameConfiguration, new JsonSerializerOptions() { WriteIndented = true });

            var filePath = "";
            var myDocsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var initialDirectory = (myDocsDirectory != "") ? myDocsDirectory : "c:\\";

            using (System.Windows.Forms.SaveFileDialog saveFileDialog = new()
            {
                Filter = "JSON files (*.json)|*.json",
                InitialDirectory = initialDirectory,
                RestoreDirectory = true,
                FileName = fileName
            })
            {
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    filePath = saveFileDialog.FileName;
                }
            }

            if (filePath != "")
                File.WriteAllText(filePath, jsonString);
        }

        private void OnImportPresetFromFile(object? obj)
        {
            PresetImportFailed = false;

            var filePath = "";
            var myDocsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var initialDirectory = (myDocsDirectory != "") ? myDocsDirectory : "c:\\";

            using (System.Windows.Forms.OpenFileDialog openFileDialog = new()
            {
                Filter = "JSON files (*.json)|*.json",
                InitialDirectory = initialDirectory,
                RestoreDirectory = true,
                CheckFileExists = true
            })
            {
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                }
            }

            if (filePath == "")
                return;

            string jsonString = File.ReadAllText(filePath);

            GameConfiguration gameConfiguration;
            try
            {
                gameConfiguration = JsonSerializer.Deserialize<GameConfiguration>(jsonString)!;
            }
            catch (Exception) 
            {
                PresetImportFailed = true;
                return; 
            }

            if (gameConfiguration.ConfigurationName == "" ||
                gameConfiguration.PlayerGlyph == "" ||
                gameConfiguration.ProjectileGlyph == "" ||
                gameConfiguration.PlayerDebrisGlyph == "" ||
                gameConfiguration.LargeObstacleGlyph == "" ||
                gameConfiguration.MediumObstacleGlyph == "" ||
                gameConfiguration.SmallObstacleGlyph == "" ||
                gameConfiguration.LargeEnemyGlyph == "" ||
                gameConfiguration.SmallEnemyGlyph == "")
            {
                PresetImportFailed = true;
                return;
            }

            ConfigurationPresetsCustom.Add(gameConfiguration);
            ConfigurationPresetCustomCollectionView.MoveCurrentToLast();
        }

        public Color GetRandomRgbColor()
        {
            return Color.FromRgb(
                Convert.ToByte(random.Next(Byte.MinValue, Byte.MaxValue + 1)), 
                Convert.ToByte(random.Next(Byte.MinValue, Byte.MaxValue + 1)), 
                Convert.ToByte(random.Next(Byte.MinValue, Byte.MaxValue + 1)));
        }

        private void ApplyColorSchemeToGameObjects(Color playerColor, Color playerDebrisColor, Color ellipseDebrisColor, Color largeEnemyColor, Color smallEnemyColor, Color largeObstacleColor, Color mediumObstacleColor, Color smallObstacleColor, Color projectileColor, Color gameForegroundColor, Color gameBackgroundColor)
        {
            SetPlayerColorBrush(playerColor);
            SetPlayerDebrisColorBrush(playerDebrisColor);
            SetEllipseDebrisColorBrush(ellipseDebrisColor);
            SetLargeEnemyColorBrush(largeEnemyColor);
            SetSmallEnemyColorBrush(smallEnemyColor);
            SetLargeObstacleColorBrush(largeObstacleColor);
            SetMediumObstacleColorBrush(mediumObstacleColor);
            SetSmallObstacleColorBrush(smallObstacleColor);
            SetProjectileColorBrush(projectileColor);
            SetGameForegroundColorBrush(gameForegroundColor);
            SetGameBackgroundColorBrush(gameBackgroundColor);
        }

        private void OnRandomizeGameColors(object? obj)
        {
            ApplyColorSchemeToGameObjects(
                playerColor: GetRandomRgbColor(),
                playerDebrisColor: GetRandomRgbColor(),
                ellipseDebrisColor: GetRandomRgbColor(),
                largeEnemyColor: GetRandomRgbColor(),
                smallEnemyColor: GetRandomRgbColor(),
                largeObstacleColor: GetRandomRgbColor(),
                mediumObstacleColor: GetRandomRgbColor(),
                smallObstacleColor: GetRandomRgbColor(),
                projectileColor: GetRandomRgbColor(),
                gameForegroundColor: GetRandomRgbColor(),
                gameBackgroundColor: GetRandomRgbColor());

            if ((ColorScheme)ColorSchemeCollectionView.CurrentItem != ColorScheme.Custom)
                ColorSchemeCollectionView.MoveCurrentTo(ColorScheme.Custom);
        }

        public void SetColorScheme(ColorScheme colorScheme)
        {
            if (colorScheme == ColorScheme.RetroGreen)
            {
                Color Color1 = (Color)ColorConverter.ConvertFromString("#0F380F");
                Color Color2 = (Color)ColorConverter.ConvertFromString("#306230");
                Color Color3 = (Color)ColorConverter.ConvertFromString("#8BAC0F");
                Color Color4 = (Color)ColorConverter.ConvertFromString("#9BBC0F");

                ApplyColorSchemeToGameObjects(
                    playerColor: Color4,
                    playerDebrisColor: Color2,
                    ellipseDebrisColor: Color2,
                    largeEnemyColor: Color2,
                    smallEnemyColor: Color4,
                    largeObstacleColor: Color3,
                    mediumObstacleColor: Color3,
                    smallObstacleColor: Color3,
                    projectileColor: Color3,
                    gameForegroundColor: Color4,
                    gameBackgroundColor: Color1);

            }
            else if (colorScheme == ColorScheme.Mono)
            {
                ApplyColorSchemeToGameObjects(
                    playerColor: Colors.White,
                    playerDebrisColor: Colors.White,
                    ellipseDebrisColor: Colors.White,
                    largeEnemyColor: Colors.White,
                    smallEnemyColor: Colors.White,
                    largeObstacleColor: Colors.White,
                    mediumObstacleColor: Colors.White,
                    smallObstacleColor: Colors.White,
                    projectileColor: Colors.White,
                    gameForegroundColor: Colors.White,
                    gameBackgroundColor: Colors.Black);
            }
            else if (colorScheme == ColorScheme.Console)
            {
                Color Gray = (Color)ColorConverter.ConvertFromString("#B8B8B8");
                Color Yellow = (Color)ColorConverter.ConvertFromString("#FFFD33");
                Color Red = (Color)ColorConverter.ConvertFromString("#FF3016");
                Color RedDark = (Color)ColorConverter.ConvertFromString("#9B1707");
                Color Magenta = (Color)ColorConverter.ConvertFromString("#FF3FFC");
                Color Blue = (Color)ColorConverter.ConvertFromString("#0027FB");
                Color Cyan = (Color)ColorConverter.ConvertFromString("#00FCFE");
                Color Green = (Color)ColorConverter.ConvertFromString("#00F92C");

                ApplyColorSchemeToGameObjects(
                    playerColor: Colors.White,
                    playerDebrisColor: Gray,
                    ellipseDebrisColor: RedDark,
                    largeEnemyColor: Yellow,
                    smallEnemyColor: Magenta,
                    largeObstacleColor: Blue,
                    mediumObstacleColor: Green,
                    smallObstacleColor: Cyan,
                    projectileColor: Red,
                    gameForegroundColor: Colors.White,
                    gameBackgroundColor: Colors.Black);
            }
            else if (colorScheme == ColorScheme.Dark)
            {
                ApplyColorSchemeToGameObjects(
                    playerColor: (Color)ColorConverter.ConvertFromString("#eeeeee"),
                    playerDebrisColor: (Color)ColorConverter.ConvertFromString("#cccccc"),
                    ellipseDebrisColor: (Color)ColorConverter.ConvertFromString("#ECCE50"),
                    largeEnemyColor: (Color)ColorConverter.ConvertFromString("#ED9E50"),
                    smallEnemyColor: (Color)ColorConverter.ConvertFromString("#CFABD0"),
                    largeObstacleColor: (Color)ColorConverter.ConvertFromString("#C5D150"),
                    mediumObstacleColor: (Color)ColorConverter.ConvertFromString("#80CAB0"),
                    smallObstacleColor: (Color)ColorConverter.ConvertFromString("#8CB6E0"),
                    projectileColor: (Color)ColorConverter.ConvertFromString("#DF6560"),
                    gameForegroundColor: Colors.White,
                    gameBackgroundColor: Colors.Black);
            }
            else if (colorScheme == ColorScheme.Light)
            {
                ApplyColorSchemeToGameObjects(
                    playerColor: (Color)ColorConverter.ConvertFromString("#333333"),
                    playerDebrisColor: (Color)ColorConverter.ConvertFromString("#555555"),
                    ellipseDebrisColor: (Color)ColorConverter.ConvertFromString("#EFC210"),
                    largeEnemyColor: (Color)ColorConverter.ConvertFromString("#F99920"),
                    smallEnemyColor: (Color)ColorConverter.ConvertFromString("#9C71B0"),
                    largeObstacleColor: (Color)ColorConverter.ConvertFromString("#839B10"),
                    mediumObstacleColor: (Color)ColorConverter.ConvertFromString("#4BA8A0"),
                    smallObstacleColor: (Color)ColorConverter.ConvertFromString("#5286B0"),
                    projectileColor: (Color)ColorConverter.ConvertFromString("#D43E30"),
                    gameForegroundColor: Colors.Black,
                    gameBackgroundColor: Colors.White);
            }

            Config.ColorScheme = colorScheme;
        }

        private void ColorSchemesCollectionView_CurrentChanged(object? sender, EventArgs e)
        {
            if (ColorSchemeCollectionView.CurrentItem == null)
                return;

            ColorScheme colorScheme = (ColorScheme)ColorSchemeCollectionView.CurrentItem;
            SetColorScheme(colorScheme);
        }

        private void OnRandomizeCurrentSprites(object? obj)
        {
            RandomizeCurrentSpritesOfGameObjects();
        }

        public static void ClearCollectionAndAddNewItems(ICollection<string> collection, IEnumerable<string> itemsToAdd)
        {
            collection.Clear();
            foreach (string s in itemsToAdd)
                collection.Add(s);
        }

        public void ApplySpriteGroup(SpriteGroup spriteGroup)
        {
            switch (spriteGroup)
            {
                case SpriteGroup.Emoji:
                    {
                        ClearCollectionAndAddNewItems(CurrentSpriteCollection, emojiSprites);
                    }
                    break;
                case SpriteGroup.Music:
                    {
                        ClearCollectionAndAddNewItems(CurrentSpriteCollection, musicSprites);
                    }
                    break;
                case SpriteGroup.Money:
                    {
                        ClearCollectionAndAddNewItems(CurrentSpriteCollection, moneySprites);
                    }
                    break;
                case SpriteGroup.Hieroglyphs:
                    {
                        ClearCollectionAndAddNewItems(CurrentSpriteCollection, egyptianHieroglyphSprites);
                    }
                    break;
                case SpriteGroup.Geometric:
                    {
                        ClearCollectionAndAddNewItems(CurrentSpriteCollection, geometricShapeSprites);
                    }
                    break;
                default:
                    break;
            }

            PlayerSpriteCollectionView.Refresh();
            LargeObstacleSpriteCollectionView.Refresh();
            MediumObstacleSpriteCollectionView.Refresh();
            SmallObstacleSpriteCollectionView.Refresh();
            LargeEnemySpriteCollectionView.Refresh();
            SmallEnemySpriteCollectionView.Refresh();
            ProjectileSpriteCollectionView.Refresh();
            PlayerDebrisSpriteCollectionView.Refresh();

            Config.SpriteGroup = spriteGroup;
        }

        private void SpriteGroupCollectionView_CurrentChanged(object? sender, EventArgs e)
        {
            if (SpriteGroupCollectionView.CurrentItem == null)
                return;

            SpriteGroup group = (SpriteGroup)SpriteGroupCollectionView.CurrentItem;
            ApplySpriteGroup(group);
            RandomizeCurrentSpritesOfGameObjects();
        }

        public void RandomizeCurrentSpritesOfGameObjects()
        {
            PlayerSpriteCollectionView.MoveCurrentToPosition(random.Next(0, CurrentSpriteCollection.Count));
            LargeObstacleSpriteCollectionView.MoveCurrentToPosition(random.Next(0, CurrentSpriteCollection.Count));
            MediumObstacleSpriteCollectionView.MoveCurrentToPosition(random.Next(0, CurrentSpriteCollection.Count));
            SmallObstacleSpriteCollectionView.MoveCurrentToPosition(random.Next(0, CurrentSpriteCollection.Count));
            LargeEnemySpriteCollectionView.MoveCurrentToPosition(random.Next(0, CurrentSpriteCollection.Count));
            SmallEnemySpriteCollectionView.MoveCurrentToPosition(random.Next(0, CurrentSpriteCollection.Count));
            ProjectileSpriteCollectionView.MoveCurrentToPosition(random.Next(0, CurrentSpriteCollection.Count));
            PlayerDebrisSpriteCollectionView.MoveCurrentToPosition(random.Next(0, CurrentSpriteCollection.Count));
        }

        public void SetCompositionTargetRendering()
        {
            // Add event handler to run the game's update loop:
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            if (GameState == GameState.GAME_ACTIVE || GameState == GameState.GAME_OVER)
                UpdateLoop();
            else
                // Detach the game's update loop if the current game state does not involve rendering game object geometries:
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
        }

        private void OnLoadConfigurationPreset(object? configurationPresetType)
        {
            if (configurationPresetType == null)
                return;

            PresetType? presetType = (PresetType)configurationPresetType;

            GameConfiguration gc = new();

            if (presetType.Value == PresetType.BuiltIn)
                gc = (GameConfiguration)ConfigurationPresetBuiltInCollectionView.CurrentItem;
            else if (presetType.Value == PresetType.Custom)
                gc = (GameConfiguration)ConfigurationPresetCustomCollectionView.CurrentItem;

            if (Config == gc)
                return;

            PlayerControlMode = gc.PlayerControlMode;
            RenderQuality = gc.RenderQuality;

            if (gc.ColorScheme == ColorScheme.Custom)
            {
                ApplyColorSchemeToGameObjects(
                    playerColor: gc.PlayerColor,
                    playerDebrisColor: gc.PlayerDebrisColor,
                    ellipseDebrisColor: gc.EllipseDebrisColor,
                    largeEnemyColor: gc.LargeEnemyColor,
                    smallEnemyColor: gc.SmallEnemyColor,
                    largeObstacleColor: gc.LargeObstacleColor,
                    mediumObstacleColor: gc.MediumObstacleColor,
                    smallObstacleColor: gc.SmallObstacleColor,
                    projectileColor: gc.ProjectileColor,
                    gameForegroundColor: gc.GameForeground,
                    gameBackgroundColor: gc.GameBackground);
            }

            if ((ColorScheme)ColorSchemeCollectionView.CurrentItem != gc.ColorScheme)
            {
                ColorSchemeCollectionView.MoveCurrentTo(gc.ColorScheme);
            }
            else
            {
                SetColorScheme((ColorScheme)ColorSchemeCollectionView.CurrentItem);
            }

            if ((SpriteGroup)SpriteGroupCollectionView.CurrentItem != gc.SpriteGroup)
            {
                SpriteGroupCollectionView.MoveCurrentTo(gc.SpriteGroup);
            }
            else
            {
                ApplySpriteGroup((SpriteGroup)SpriteGroupCollectionView.CurrentItem);
            }

            PlayerSpriteCollectionView.MoveCurrentTo(gc.PlayerGlyph);
            ProjectileSpriteCollectionView.MoveCurrentTo(gc.ProjectileGlyph);
            PlayerDebrisSpriteCollectionView.MoveCurrentTo(gc.PlayerDebrisGlyph);
            LargeObstacleSpriteCollectionView.MoveCurrentTo(gc.LargeObstacleGlyph);
            MediumObstacleSpriteCollectionView.MoveCurrentTo(gc.MediumObstacleGlyph);
            SmallObstacleSpriteCollectionView.MoveCurrentTo(gc.SmallObstacleGlyph);
            LargeEnemySpriteCollectionView.MoveCurrentTo(gc.LargeEnemyGlyph);
            SmallEnemySpriteCollectionView.MoveCurrentTo(gc.SmallEnemyGlyph);

            SetPlayerSpriteRotation(gc.PlayerRotation);
            SetProjectileSpriteRotation(gc.ProjectileRotation);

            ApplyVisualUpdates();
        }

        private void OnDeleteConfigurationPreset(object? obj)
        {
            ConfigurationPresetsCustom.Remove((GameConfiguration)ConfigurationPresetCustomCollectionView.CurrentItem);
            ConfigurationPresetCustomCollectionView.MoveCurrentToNext();
        }

        private void OnSaveConfigurationPreset(object? obj)
        {
            Config.ConfigurationName = SaveConfigurationPresetName;
            ConfigurationPresetsCustom.Add((GameConfiguration)Config.Clone());
        }

        private void OnApplyForegroundBackgroundToAll(object? obj)
        {
            ApplyColorSchemeToGameObjects(
                playerColor: Config.GameForeground,
                playerDebrisColor: Config.GameForeground,
                ellipseDebrisColor: Config.GameForeground,
                largeEnemyColor: Config.GameForeground,
                smallEnemyColor: Config.GameForeground,
                largeObstacleColor: Config.GameForeground,
                mediumObstacleColor: Config.GameForeground,
                smallObstacleColor: Config.GameForeground,
                projectileColor: Config.GameForeground,
                gameForegroundColor: Config.GameForeground,
                gameBackgroundColor: Config.GameBackground);

            ColorSchemeCollectionView.MoveCurrentTo(ColorScheme.Custom);
        }

        private void OnResetAllForegroundBackground(object? obj)
        {
            ColorSchemeCollectionView.MoveCurrentTo(ColorScheme.Mono);
        }

        private void ResetColorSelectedHandler()
        {
            if (colorSelector is null)
                return;

            colorSelector.ColorSelected -= ColorSelector_FillColorSelected;
            colorSelector.ColorSelected -= ColorSelector_ForegroundColorSelected;
        }

        private void OnSetGameForeground(object? commandParameter)
        {
            if (commandParameter is null || colorSelector is null || colorSelectorPopup is null)
                return;

            Control parameter = (Control)commandParameter;

            if (parameter != null)
            {
                HandleSetGameForeground(parameter, colorSelector, colorSelectorPopup);
            }
        }

        public void HandleSetGameForeground(Control parameter, ColorSelector selector, Popup popup)
        {
            ResetColorSelectedHandler();
            selector.ColorSelected += ColorSelector_ForegroundColorSelected;
            selector.SelectedColor = null;
            selector.Tag = parameter.Tag;
            popup.PlacementTarget = parameter;

            Type targetType = (Type)selector.Tag;
            if (targetType == typeof(GameViewModel))
            {
                selector.CurrentColor = Config.GameForeground;
            }

            popup.IsOpen = true;
        }

        private void ColorSelector_ForegroundColorSelected(object sender, RoutedEventArgs e)
        {
            if (colorSelector is null || colorSelectorPopup is null)
                return;

            HandleForegroundColorSelected(colorSelector, colorSelectorPopup);
        }

        public void HandleForegroundColorSelected(ColorSelector selector, Popup popup)
        {
            if (selector.SelectedColor != null)
            {
                Color color = selector.SelectedColor.Value;
                Type targetType = (Type)selector.Tag;
                if (targetType == typeof(GameViewModel))
                {
                    SetGameForegroundColorBrush(color);
                }
                selector.Tag = null;
                ColorSchemeCollectionView.MoveCurrentTo(ColorScheme.Custom);
            }
            popup.IsOpen = false;
        }

        private void OnSetGameObjectFill(object? commandParameter)
        {
            if (commandParameter is null || colorSelector is null || colorSelectorPopup is null)
                return;

            Control parameter = (Control)commandParameter;

            if (parameter != null)
            {
                HandleSetGameObjectFill(parameter, colorSelector, colorSelectorPopup);
            }
        }

        public void HandleSetGameObjectFill(Control parameter, ColorSelector selector, Popup popup)
        {
            ResetColorSelectedHandler();
            selector.ColorSelected += ColorSelector_FillColorSelected;
            selector.SelectedColor = null;
            selector.Tag = parameter.Tag;
            popup.PlacementTarget = parameter;

            Type targetType = (Type)selector.Tag;
            if (targetType == typeof(Player))
                selector.CurrentColor = Config.PlayerColor;
            else if (targetType == typeof(PlayerDebris))
                selector.CurrentColor = Config.PlayerDebrisColor;
            else if (targetType == typeof(EllipseDebris))
                selector.CurrentColor = Config.EllipseDebrisColor;
            else if (targetType == typeof(LargeEnemy))
                selector.CurrentColor = Config.LargeEnemyColor;
            else if (targetType == typeof(SmallEnemy))
                selector.CurrentColor = Config.SmallEnemyColor;
            else if (targetType == typeof(LargeObstacle))
                selector.CurrentColor = Config.LargeObstacleColor;
            else if (targetType == typeof(MediumObstacle))
                selector.CurrentColor = Config.MediumObstacleColor;
            else if (targetType == typeof(SmallObstacle))
                selector.CurrentColor = Config.SmallObstacleColor;
            else if (targetType == typeof(Projectile))
                selector.CurrentColor = Config.ProjectileColor;
            else if (targetType == typeof(GameViewModel))
                selector.CurrentColor = Config.GameBackground;

            popup.IsOpen = true;
        }

        private void ColorSelector_FillColorSelected(object sender, RoutedEventArgs e)
        {
            if (colorSelector is null || colorSelectorPopup is null)
                return;

            HandleFillColorSelected(colorSelector, colorSelectorPopup);
        }

        public void HandleFillColorSelected(ColorSelector selector, Popup popup)
        {
            if (selector.SelectedColor != null)
            {
                Color color = selector.SelectedColor.Value;
                Type targetType = (Type)selector.Tag;

                if (targetType == typeof(Player))
                    SetPlayerColorBrush(color);
                else if (targetType == typeof(PlayerDebris))
                    SetPlayerDebrisColorBrush(color);
                else if (targetType == typeof(LargeEnemy))
                    SetLargeEnemyColorBrush(color);
                else if (targetType == typeof(SmallEnemy))
                    SetSmallEnemyColorBrush(color);
                else if (targetType == typeof(LargeObstacle))
                    SetLargeObstacleColorBrush(color);
                else if (targetType == typeof(MediumObstacle))
                    SetMediumObstacleColorBrush(color);
                else if (targetType == typeof(SmallObstacle))
                    SetSmallObstacleColorBrush(color);
                else if (targetType == typeof(Projectile))
                    SetProjectileColorBrush(color);
                else if (targetType == typeof(GameViewModel))
                    SetGameBackgroundColorBrush(color);

                selector.Tag = null;
                ColorSchemeCollectionView.MoveCurrentTo(ColorScheme.Custom);
            }
            popup.IsOpen = false;
        }

        private void ColorSelector_Closed(object sender, RoutedEventArgs e)
        {
            if (colorSelector is null || colorSelectorPopup is null)
                return;

            colorSelector.SelectedColor = null;
            colorSelectorPopup.IsOpen = false;
        }

        private void SmallEnemySpriteCollectionView_CurrentChanged(object? sender, EventArgs e)
        {
            Config.SmallEnemyGlyph = (string)SmallEnemySpriteCollectionView.CurrentItem;
            smallEnemyNeedsRefresh = true;
        }

        private void LargeEnemySpriteCollectionView_CurrentChanged(object? sender, EventArgs e)
        {
            Config.LargeEnemyGlyph = (string)LargeEnemySpriteCollectionView.CurrentItem;
            largeEnemyNeedsRefresh = true;
        }

        private void SmallObstacleSpriteCollectionView_CurrentChanged(object? sender, EventArgs e)
        {
            Config.SmallObstacleGlyph = (string)SmallObstacleSpriteCollectionView.CurrentItem;
            smallObstacleNeedsRefresh = true;
        }

        private void MediumObstacleSpriteCollectionView_CurrentChanged(object? sender, EventArgs e)
        {
            Config.MediumObstacleGlyph = (string)MediumObstacleSpriteCollectionView.CurrentItem;
            mediumObstacleNeedsRefresh = true;
        }

        private void LargeObstacleSpriteCollectionView_CurrentChanged(object? sender, EventArgs e)
        {
            Config.LargeObstacleGlyph = (string)LargeObstacleSpriteCollectionView.CurrentItem;
            largeObstacleNeedsRefresh = true;
        }

        private void ProjectileSpriteCollectionView_CurrentChanged(object? sender, EventArgs e)
        {
            Config.ProjectileGlyph = (string)ProjectileSpriteCollectionView.CurrentItem;
            projectileNeedsRefresh = true;
        }

        private void PlayerSpriteCollectionView_CurrentChanged(object? sender, EventArgs e)
        {
            Config.PlayerGlyph = (string)PlayerSpriteCollectionView.CurrentItem;
            playerNeedsRefresh = true;
            RebuildLivesString(Config.PlayerGlyph);
        }

        private void PlayerDebrisSpriteCollectionView_CurrentChanged(object? sender, EventArgs e)
        {
            Config.PlayerDebrisGlyph = (string)PlayerDebrisSpriteCollectionView.CurrentItem;
            playerDebrisNeedsRefresh = true;
        }

        private void OnRotateGameSpriteCounterClockwise(object? gameClass)
        {
            if (gameClass is null)
                return;

            Type classType = (Type)gameClass;

            if (classType == typeof(Player))
            {
                SetPlayerSpriteRotation(Config.PlayerRotation - 45);
            }
            else if (classType == typeof(Projectile))
            {
                SetProjectileSpriteRotation(Config.ProjectileRotation - 45);
            }
        }

        private void OnRotateGameSpriteClockwise(object? gameClass)
        {
            if (gameClass is null)
                return;

            Type classType = (Type)gameClass;

            if (classType == typeof(Player))
            {
                SetPlayerSpriteRotation(Config.PlayerRotation + 45);
            }
            else if (classType == typeof(Projectile))
            {
                SetProjectileSpriteRotation(Config.ProjectileRotation + 45);
            }
        }

        private void Players_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    var addObject = e?.NewItems?[0] as Player;
                    gameContainer?.Children.Add(addObject);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    var removeObject = e?.OldItems?[0] as Player;
                    gameContainer?.Children.Remove(removeObject);
                    break;
                default:
                    break;
            }
        }

        private void PlayerDebrisCollection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    var addObject = e?.NewItems?[0] as PlayerDebris;
                    gameContainer?.Children.Add(addObject);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    var removeObject = e?.OldItems?[0] as PlayerDebris;
                    gameContainer?.Children.Remove(removeObject);
                    break;
                default:
                    break;
            }
        }

        private void EllipseDebris_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    var addObject = e?.NewItems?[0] as EllipseDebris;
                    gameContainer?.Children.Add(addObject);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    var removeObject = e?.OldItems?[0] as EllipseDebris;
                    gameContainer?.Children.Remove(removeObject);
                    break;
                default:
                    break;
            }
        }

        private void Projectiles_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    var addObject = e?.NewItems?[0] as Projectile;
                    gameContainer?.Children.Add(addObject);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    var removeObject = e?.OldItems?[0] as Projectile;
                    gameContainer?.Children.Remove(removeObject);
                    break;
                default:
                    break;
            }
        }

        private void Enemies_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    var addObject = e?.NewItems?[0] as Enemy;
                    gameContainer?.Children.Add(addObject);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    var removeObject = e?.OldItems?[0] as Enemy;
                    gameContainer?.Children.Remove(removeObject);
                    break;
                default:
                    break;
            }
        }

        private void Obstacles_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    var addObject = e?.NewItems?[0] as Obstacle;
                    gameContainer?.Children.Add(addObject);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    var removeObject = e?.OldItems?[0] as Obstacle;
                    gameContainer?.Children.Remove(removeObject);
                    obstaclesDestroyed++;
                    break;
                default:
                    break;
            }
        }

        public void ResizeGameArea(FrameworkElement parentContainer)
        {
            if (gameContainer is null)
                return;

            gameContainer.Width = parentContainer.ActualWidth;
            gameContainer.Height = parentContainer.ActualHeight;
        }

        public bool CheckProjectileLimitReached(MovingShape limitedObject, int limit = GameDefs.PLAYER_PROJECTILE_LIMIT)
        {
            return Projectiles.Where(projectile => projectile.Tag == limitedObject.Tag).ToList().Count < limit;
        }

        public void ExecuteHitTest(Player player)
        {
            if (!player.IsDead)
            {
                hitResultsList.Clear();

                System.Windows.Media.VisualTreeHelper.HitTest(gameContainer,
                    PlayerHitTestFilterCallback,
                    HitTestResultCallback,
                    new System.Windows.Media.PointHitTestParameters(player.Location));

                if (hitResultsList.Count > 0)
                {
                    KillPlayer(player);
                }
            }
        }

        public void ExecuteHitTest(Projectile projectile)
        {
            hitResultsList.Clear();

            System.Windows.Media.VisualTreeHelper.HitTest(gameContainer,
                ProjectileHitTestFilterCallback,
                HitTestResultCallback,
                new System.Windows.Media.PointHitTestParameters(projectile.Location));

            if (hitResultsList.Count > 0)
            {
                DependencyObject depObj = ((DrawingVisual)hitResultsList[0]).Parent;
                if (depObj is Obstacle obstacle)
                {
                    if (projectile.Tag == player1?.Tag)
                    {
                        AddToScore(obstacle.ScoreValue);
                    }
                    ShapesToBeRemoved.Add(projectile);
                    ShapesToBeRemoved.Add(obstacle);
                    SpawnEllipseDebris(obstacle.Location);
                    if (obstacle is LargeObstacle)
                    {
                        double newAngleRadians = random.Next(-45, 46) * GameDefs.DegreesToRadians;

                        SpawnMediumObstacle(obstacle.Location,
                            Vector.Multiply(obstacle.Movement, GameDefs.GetRotationMatrix(newAngleRadians)) * (1 + random.NextSingle()), GameDefs.GetRandomRotationRate(random));

                        newAngleRadians = random.Next(-45, 46) * GameDefs.DegreesToRadians;

                        SpawnMediumObstacle(obstacle.Location,
                            Vector.Multiply(obstacle.Movement, GameDefs.GetRotationMatrix(newAngleRadians)) * (1 + random.NextSingle()), GameDefs.GetRandomRotationRate(random));
                    }
                    else if (obstacle is MediumObstacle)
                    {
                        double newAngleRadians = random.Next(-45, 46) * GameDefs.DegreesToRadians;

                        SpawnSmallObstacle(obstacle.Location,
                            Vector.Multiply(obstacle.Movement, GameDefs.GetRotationMatrix(newAngleRadians)) * (1.5 + random.NextSingle()), GameDefs.GetRandomRotationRate(random));

                        newAngleRadians = random.Next(-45, 46) * GameDefs.DegreesToRadians;

                        SpawnSmallObstacle(obstacle.Location,
                            Vector.Multiply(obstacle.Movement, GameDefs.GetRotationMatrix(newAngleRadians)) * (1.5 + random.NextSingle()), GameDefs.GetRandomRotationRate(random));
                    }
                    else if (obstacle is SmallObstacle)
                    {

                    }
                    return;
                }
                if (depObj is Enemy enemy)
                {
                    if (projectile.Tag == player1?.Tag)
                    {
                        AddToScore(enemy.ScoreValue);
                    }
                    ShapesToBeRemoved.Add(projectile);
                    ShapesToBeRemoved.Add(enemy);
                    SpawnEllipseDebris(enemy.Location);
                    return;
                }
                if (depObj is Player player)
                {
                    ShapesToBeRemoved.Add(projectile);
                    KillPlayer(player);
                    return;
                }
            }
        }

        readonly HitTestResultCallback HitTestResultCallback = new(HitTestResultHandler);
        readonly HitTestFilterCallback PlayerHitTestFilterCallback = new(PlayerHitTestFilter);
        readonly HitTestFilterCallback ProjectileHitTestFilterCallback = new(ProjectileHitTestFilter);

        public static HitTestResultBehavior HitTestResultHandler(System.Windows.Media.HitTestResult result)
        {
            // Add the hit test result to the list that will be processed after the enumeration.
            hitResultsList.Add(result.VisualHit);

            // Set the behavior to return visuals at all z-order levels.
            return System.Windows.Media.HitTestResultBehavior.Stop;
        }

        public static HitTestFilterBehavior PlayerHitTestFilter(DependencyObject o)
        {
            if (o is Player || o is IDebris)
                return HitTestFilterBehavior.ContinueSkipSelfAndChildren;

            return HitTestFilterBehavior.Continue;
        }

        public static HitTestFilterBehavior ProjectileHitTestFilter(DependencyObject o)
        {
            if (o is IDissipate)
                return HitTestFilterBehavior.ContinueSkipSelfAndChildren;

            return HitTestFilterBehavior.Continue;
        }

        public void SpawnLargeEnemy(Point spawnLocation, Vector movementDirection)
        {
            Enemies.Add(new LargeEnemy(spawnLocation, movementDirection, Config.LargeEnemyGlyph, LargeEnemyBrush));
        }

        public void SpawnSmallEnemy(Point spawnLocation, Vector movementDirection)
        {
            Enemies.Add(new SmallEnemy(spawnLocation, movementDirection, Config.SmallEnemyGlyph, SmallEnemyBrush));
        }

        public void SpawnLargeObstacle(Point spawnLocation, Vector movementDirection, double rotationRate)
        {
            Obstacles.Add(new LargeObstacle(spawnLocation, movementDirection, rotationRate, Config.LargeObstacleGlyph, LargeObstacleBrush));
        }

        public void SpawnMediumObstacle(Point spawnLocation, Vector movementDirection, double rotationRate)
        {
            Obstacles.Add(new MediumObstacle(spawnLocation, movementDirection, rotationRate, Config.MediumObstacleGlyph, MediumObstacleBrush));
        }

        public void SpawnSmallObstacle(Point spawnLocation, Vector movementDirection, double rotationRate)
        {
            Obstacles.Add(new SmallObstacle(spawnLocation, movementDirection, rotationRate, Config.SmallObstacleGlyph, SmallObstacleBrush));
        }

        public void ApplyRenderQualityChange()
        {
            if (gameContainer is null)
                return;

            foreach (MovingShape thing in gameContainer.Children)
                RenderOptions.SetEdgeMode(thing, RenderQuality);
            MovingShape.SetRenderQuality(RenderQuality);
            renderQualityChanged = false;
        }

        public void HandlePlayerMovement(Player player)
        {
            double moveX = player.Movement.X;
            double moveY = player.Movement.Y;
            double locX = player.Location.X;
            double locY = player.Location.Y;

            double gameContainerWidth = (gameContainer is null) ? 0 : gameContainer.ActualWidth;
            double gameContainerHeight = (gameContainer is null) ? 0 : gameContainer.ActualHeight;

            switch (PlayerControlMode)
            {
                case PlayerControlMode.Retro:
                    {
                        if (IsLeftPressed)
                        {
                            player.Rotation.Angle -= Player.RotationIncrement;
                        }
                        else if (IsRightPressed)
                        {
                            player.Rotation.Angle += Player.RotationIncrement;
                        }

                        if (IsUpPressed)
                        {
                            double angleRadians = (player.Rotation.Angle - 90) * GameDefs.DegreesToRadians;
                            moveX = (float)Math.Cos(angleRadians);
                            moveY = (float)Math.Sin(angleRadians);

                            player.DeccelerationTimeElapsed = 0;

                            player.AccelerationTimeElapsed++;
                            player.Speed += Player.AccelerationIncrement * player.AccelerationTimeElapsed;
                            player.Speed = Math.Clamp(player.Speed, 0, Player.AccelerationLimit);

                            locX = player.Location.X + player.Movement.X * player.Speed;
                            locY = player.Location.Y + player.Movement.Y * player.Speed;
                        }
                        else if (player.Speed > 0)
                        {
                            player.AccelerationTimeElapsed = 0;

                            player.DeccelerationTimeElapsed++;
                            player.Speed -= Player.DeccelerationIncrement * player.DeccelerationTimeElapsed;
                            player.Speed = Math.Clamp(player.Speed, 0, Player.AccelerationLimit);

                            locX = player.Location.X + player.Movement.X * player.Speed;
                            locY = player.Location.Y + player.Movement.Y * player.Speed;
                        }
                        break;
                    }
                case PlayerControlMode.WASD:
                    {
                        if (!IsLeftPressed && !IsRightPressed)
                        {
                            if (Math.Abs(player.Movement.X) > 0)
                            {
                                moveX = (player.Movement.X > 0) ?
                                    Math.Round(player.Movement.X - Player.WASDAccelerationIncrement, 1, MidpointRounding.ToZero) :
                                    Math.Round(player.Movement.X + Player.WASDAccelerationIncrement, 1, MidpointRounding.ToZero);

                                locX = player.Location.X - player.Movement.X;
                            }
                        }
                        else if (IsLeftPressed)
                        {
                            if (player.Movement.X < Player.AccelerationLimit)
                                moveX = player.Movement.X + Player.WASDAccelerationIncrement;

                            player.Rotation.Angle = 270;
                            locX = player.Location.X - player.Movement.X;
                        }
                        else if (IsRightPressed)
                        {
                            if (player.Movement.X > -Player.AccelerationLimit)
                                moveX = player.Movement.X - Player.WASDAccelerationIncrement;

                            player.Rotation.Angle = 90;
                            locX = player.Location.X - player.Movement.X;
                        }

                        if (!IsUpPressed && !IsDownPressed)
                        {
                            if (Math.Abs(player.Movement.Y) > 0)
                            {
                                moveY = (player.Movement.Y > 0) ?
                                    Math.Round(player.Movement.Y - Player.WASDAccelerationIncrement, 1, MidpointRounding.ToZero) :
                                    Math.Round(player.Movement.Y + Player.WASDAccelerationIncrement, 1, MidpointRounding.ToZero);

                                locY = player.Location.Y - player.Movement.Y;
                            }
                        }
                        else if (IsUpPressed)
                        {
                            if (player.Movement.Y < Player.AccelerationLimit)
                                moveY = player.Movement.Y + Player.WASDAccelerationIncrement;

                            player.Rotation.Angle = 0;
                            locY = player.Location.Y - player.Movement.Y;
                        }
                        else if (IsDownPressed)
                        {
                            if (player.Movement.Y > -Player.AccelerationLimit)
                                moveY = player.Movement.Y - Player.WASDAccelerationIncrement;

                            player.Rotation.Angle = 180;
                            locY = player.Location.Y - player.Movement.Y;
                        }

                        if (IsUpPressed && IsLeftPressed)
                        {
                            player.Rotation.Angle = 315;
                        }
                        else if (IsUpPressed && IsRightPressed)
                        {
                            player.Rotation.Angle = 45;
                        }
                        else if (IsDownPressed && IsLeftPressed)
                        {
                            player.Rotation.Angle = 225;
                        }
                        else if (IsDownPressed && IsRightPressed)
                        {
                            player.Rotation.Angle = 135;
                        }
                        break;
                    }
                default:
                    break;
            }

            if (locX > gameContainer?.ActualWidth)
                locX = 0;
            else if (locX < 0)
                locX = gameContainerWidth;

            if (locY > gameContainer?.ActualHeight)
                locY = 0;
            else if (locY < 0)
                locY = gameContainerHeight;

            player.Location = new Point(locX, locY);
            player.Movement = new Vector(moveX, moveY);

            Canvas.SetLeft(player, player.Location.X);
            Canvas.SetTop(player, player.Location.Y);

            ExecuteHitTest(player);
        }

        public void HandleWarp(Player player)
        {
            double gameContainerWidth = (gameContainer is null) ? 0 : gameContainer.ActualWidth;
            double gameContainerHeight = (gameContainer is null) ? 0 : gameContainer.ActualHeight;

            if (player.WarpCountdown == Player.WarpInterval)
            {
                player.IsWarpingThroughSpaceTime = true;
                gameContainer?.Children.Remove(player);
            }
            else if (player.WarpCountdown == 0)
            {
                player.IsWarpingThroughSpaceTime = false;
                player.WarpCountdown = Player.WarpInterval;
                player.Location = new Point(random.NextSingle() * gameContainerWidth, random.NextSingle() * gameContainerHeight);

                gameContainer?.Children.Add(player);

                Canvas.SetLeft(player, player.Location.X);
                Canvas.SetTop(player, player.Location.Y);

                ExecuteHitTest(player);

                return;
            }
            player.WarpCountdown--;
        }

        /// <summary>
        /// The main update loop for the game. Contains calculations used to 
        /// move game objects around, respond to user input and perform hit-test decisions.
        /// </summary>
        public void UpdateLoop()
        {
            if (startingObstaclesSpawned == GameDefs.MAX_LARGE_OBSTACLES && Obstacles.Count == 0 && Enemies.Count == 0)
            {
                ClearScreen(player1);
                ResetGame(true);
                return;
            }

            double gameContainerWidth = (gameContainer is null) ? 0 : gameContainer.ActualWidth;
            double gameContainerHeight = (gameContainer is null) ? 0 : gameContainer.ActualHeight;

            if (player1 is null)
                return;

            // Spawn new obstacle if:
            // 1) The per-level obstacle spawn limit has not been reached 
            // 2) The chance of spawning a new obstacle for this update loop cycle has met or exceeded
            // the threshold for obstacle creation:
            if ((startingObstaclesSpawned < GameDefs.MAX_LARGE_OBSTACLES) && random.Next(0, 100) == 99)
            {
                int spawnPerimeterSelection = random.Next(0, 4);

                double spawnX = 0;
                double spawnY = 0;
                Vector spawnVector;

                // New obstacle spawns from top edge game area of perimeter:
                if (spawnPerimeterSelection == 0)
                {
                    spawnX = random.NextSingle() * gameContainerWidth;
                    spawnVector = GameDefs.GetRandomVector(random, Direction.Top);
                }
                // New obstacle spawns from left edge game area of perimeter:
                else if (spawnPerimeterSelection == 1)
                {
                    spawnY = random.NextSingle() * gameContainerHeight;
                    spawnVector = GameDefs.GetRandomVector(random, Direction.Left);
                }
                // New obstacle spawns from bottom edge game area of perimeter:
                else if (spawnPerimeterSelection == 2)
                {
                    spawnX = random.NextSingle() * gameContainerWidth;
                    spawnY = gameContainerHeight;
                    spawnVector = GameDefs.GetRandomVector(random, Direction.Bottom);
                }
                // New obstacle spawns from right edge game area of perimeter:
                else
                {
                    spawnY = random.NextSingle() * gameContainerHeight;
                    spawnX = gameContainerWidth;
                    spawnVector = GameDefs.GetRandomVector(random, Direction.Right);
                }

                SpawnLargeObstacle(new Point(spawnX, spawnY), spawnVector, GameDefs.GetRandomRotationRate(random));
                startingObstaclesSpawned++;
            }

            foreach (Obstacle o in Obstacles)
            {
                double locX = o.Location.X;
                double locY = o.Location.Y;

                if (o.Location.X + o.GeometryBounds.Width < 0)
                    locX = gameContainerWidth + o.GeometryBounds.Width;
                else if (o.Location.X - o.GeometryBounds.Width > gameContainerWidth)
                    locX = 0 - o.GeometryBounds.Width;

                if (o.Location.Y + o.GeometryBounds.Height < 0)
                    locY = gameContainerHeight + o.GeometryBounds.Height;
                else if (o.Location.Y - o.GeometryBounds.Height > gameContainerHeight)
                    locY = 0 - o.GeometryBounds.Height;

                locX += o.Movement.X;
                locY += o.Movement.Y;

                o.Location = new Point(locX, locY);

                o.Rotation.Angle += o.RotationRate;

                Canvas.SetLeft(o, o.Location.X);
                Canvas.SetTop(o, o.Location.Y);
            }

            foreach (Enemy enemy in Enemies)
            {
                if (enemy.ChangeDirectionCounter == 0)
                {
                    enemy.Movement = GameDefs.GetRandomVector(random, true, true);
                    enemy.ChangeDirectionCounter = Enemy.DirectionInterval;
                }

                if (enemy.FireProjectileCounter == 0)
                {
                    if (enemy is LargeEnemy largeEnemy)
                    {
                        Projectile projectile = new(Config.ProjectileGlyph, ProjectileBrush, ProjectileRotateTransform);
                        Projectiles.Add(largeEnemy.Fire(projectile, random.Next(0, 360)));
                    }
                    else if (enemy is SmallEnemy smallEnemy)
                    {
                        if (!player1.IsDead) 
                        {
                            var radian = Math.Atan2((player1.Location.Y - enemy.Location.Y), (player1.Location.X - enemy.Location.X));
                            var angleToTargetDegrees = (radian * GameDefs.RadiansToDegrees + 360) % 360;
                            Projectile projectile = new(Config.ProjectileGlyph, ProjectileBrush, ProjectileRotateTransform);
                            Projectiles.Add(smallEnemy.Fire(projectile, angleToTargetDegrees));
                        }
                    }
                    enemy.FireProjectileCounter = Enemy.FireInterval;
                }

                double locX = enemy.Location.X;
                double locY = enemy.Location.Y;

                if (enemy.Location.X > gameContainerWidth)
                    locX = 0;
                else if (enemy.Location.X < 0)
                    locX = gameContainerWidth;

                if (enemy.Location.Y > gameContainerHeight)
                    locY = 0;
                else if (enemy.Location.Y < 0)
                    locY = gameContainerHeight;

                locX += enemy.Movement.X;
                locY += enemy.Movement.Y;

                enemy.Location = new Point(locX, locY);

                enemy.ChangeDirectionCounter--;
                enemy.FireProjectileCounter--;

                Canvas.SetLeft(enemy, enemy.Location.X);
                Canvas.SetTop(enemy, enemy.Location.Y);
            }

            // Spawn an enemy:
            if (startingObstaclesSpawned == GameDefs.MAX_LARGE_OBSTACLES && 
                obstaclesDestroyed < GameDefs.ENEMY_SPAWN_CUTOFF &&
                Enemies.Count < 3 && 
                random.Next(0, 256) == 255)
            {
                int spawnPerimeterSelection = random.Next(0, 4);

                double spawnX = 0;
                double spawnY = 0;
                Vector spawnVector;

                // New enemy spawns from top edge game area of perimeter:
                if (spawnPerimeterSelection == 0)
                {
                    spawnX = random.NextSingle() * gameContainerWidth;
                    spawnVector = GameDefs.GetRandomVector(random, Direction.Top);
                }
                // New enemy spawns from left edge game area of perimeter:
                else if (spawnPerimeterSelection == 1)
                {
                    spawnY = random.NextSingle() * gameContainerHeight;
                    spawnVector = GameDefs.GetRandomVector(random, Direction.Left);
                }
                // New enemy spawns from bottom edge game area of perimeter:
                else if (spawnPerimeterSelection == 2)
                {
                    spawnX = random.NextSingle() * gameContainerWidth;
                    spawnY = gameContainerHeight;
                    spawnVector = GameDefs.GetRandomVector(random, Direction.Bottom);
                }
                // New enemy spawns from right edge game area of perimeter:
                else
                {
                    spawnY = random.NextSingle() * gameContainerHeight;
                    spawnX = gameContainerWidth;
                    spawnVector = GameDefs.GetRandomVector(random, Direction.Right);
                }

                if (obstaclesDestroyed > GameDefs.SMALL_ENEMY_THRESHOLD && random.Next(0, 2) == 0)
                     SpawnSmallEnemy(new Point(spawnX, spawnY), spawnVector);
                else
                    SpawnLargeEnemy(new Point(spawnX, spawnY), spawnVector);

            }

            if (!player1.IsDead)
            {
                if (player1.IsWarpingThroughSpaceTime)
                {
                    HandleWarp(player1);
                }
                else
                {
                    HandlePlayerMovement(player1);
                }
            }
            else
            {
                foreach (PlayerDebris debris in PlayerDebrisCollection)
                {
                    if (debris.DissipationCountdown == 0)
                    {
                        ShapesToBeRemoved.Add(debris);
                    }
                    else
                    {
                        debris.Location = new Point(debris.Location.X + debris.Movement.X, debris.Location.Y + debris.Movement.Y);

                        debris.Rotation.Angle += debris.RotationRate;

                        debris.DissipationCountdown--;

                        Canvas.SetLeft(debris, debris.Location.X);
                        Canvas.SetTop(debris, debris.Location.Y);
                    }
                }

                if (PlayerDebrisCollection.Count == 0 && GameState != GameState.GAME_OVER)
                {
                    SpawnPlayer();
                }
            }

            foreach (EllipseDebris debris in EllipseDebrisCollection)
            {
                if (debris.DissipationCountdown == 0)
                {
                    ShapesToBeRemoved.Add(debris);
                }
                else
                {
                    debris.Location = new Point(debris.Location.X + debris.Movement.X, debris.Location.Y + debris.Movement.Y);

                    debris.DissipationCountdown--;

                    Canvas.SetLeft(debris, debris.Location.X);
                    Canvas.SetTop(debris, debris.Location.Y);
                }
            }

            // Handle projectile movement:
            foreach (Projectile p in Projectiles)
            {
                if (p.DissipationCountdown == 0)
                {
                    ShapesToBeRemoved.Add(p);
                }
                else
                {
                    double locX = p.Location.X;
                    double locY = p.Location.Y;

                    if (p.Location.X > gameContainerWidth)
                        locX = 0;
                    else if (p.Location.X < 0)
                        locX = gameContainerWidth;

                    if (p.Location.Y > gameContainerHeight)
                        locY = 0;
                    else if (p.Location.Y < 0)
                        locY = gameContainerHeight;

                    locX += p.Movement.X;
                    locY += p.Movement.Y;

                    p.Location = new Point(locX, locY);

                    p.DissipationCountdown--;

                    Canvas.SetLeft(p, p.Location.X);
                    Canvas.SetTop(p, p.Location.Y);

                    ExecuteHitTest(p);
                }
            }

            foreach (MovingShape? shape in ShapesToBeRemoved)
            {
                if (shape is Projectile projectile)
                    Projectiles.Remove(projectile);
                else if (shape is Obstacle obstacle)
                    Obstacles.Remove(obstacle);
                else if (shape is EllipseDebris debris1)
                    EllipseDebrisCollection.Remove(debris1);
                else if (shape is PlayerDebris debris)
                    PlayerDebrisCollection.Remove(debris);
                else if (shape is Player player)
                    Players.Remove(player);
                else if (shape is Enemy enemy)
                    Enemies.Remove(enemy);
            }

            ShapesToBeRemoved.Clear();

            ApplyVisualUpdates();
        }

        public static void UpdateItems(ref bool updateFlag, IEnumerable collection, SolidColorBrush brush)
        {
            if (updateFlag)
            {
                foreach (MovingShape item in collection)
                {
                    item.Fill = brush;
                    item.Refresh();
                }
                updateFlag = false;
            }
        }

        public static void UpdateItems(ref bool updateFlag, IEnumerable collection, SolidColorBrush brush, string glyph)
        {
            if (updateFlag)
            {
                foreach (MovingGlyph item in collection)
                {
                    item.Fill = brush;
                    item.GameSprite = glyph;
                    item.Refresh();
                }
                updateFlag = false;
            }
        }

        public static void UpdateItems(ref bool updateFlag, IEnumerable collection, SolidColorBrush brush, string glyph, Transform transform)
        {
            if (updateFlag)
            {
                foreach (MovingGlyph item in collection)
                {
                    item.Fill = brush;
                    item.GameSprite = glyph;
                    item.Refresh(transform);
                }
                updateFlag = false;
            }
        }

        public void ApplyVisualUpdates()
        {
            if (playerNeedsRefresh)
                UpdateItems(ref playerNeedsRefresh, Players, PlayerBrush, Config.PlayerGlyph, PlayerRotateTransform);
            if (playerDebrisNeedsRefresh)
                UpdateItems(ref playerDebrisNeedsRefresh, PlayerDebrisCollection, PlayerDebrisBrush, Config.PlayerDebrisGlyph);
            if (projectileNeedsRefresh)
                UpdateItems(ref projectileNeedsRefresh, Projectiles, ProjectileBrush, Config.ProjectileGlyph, ProjectileRotateTransform);
            if (largeObstacleNeedsRefresh)
                UpdateItems(ref largeObstacleNeedsRefresh, Obstacles.OfType<LargeObstacle>().ToList(), LargeObstacleBrush, Config.LargeObstacleGlyph);
            if (mediumObstacleNeedsRefresh)
                UpdateItems(ref mediumObstacleNeedsRefresh, Obstacles.OfType<MediumObstacle>().ToList(), MediumObstacleBrush, Config.MediumObstacleGlyph);
            if (smallObstacleNeedsRefresh)
                UpdateItems(ref smallObstacleNeedsRefresh, Obstacles.OfType<SmallObstacle>().ToList(), SmallObstacleBrush, Config.SmallObstacleGlyph);
            if (largeEnemyNeedsRefresh)
                UpdateItems(ref largeEnemyNeedsRefresh, Enemies.OfType<LargeEnemy>().ToList(), LargeEnemyBrush, Config.LargeEnemyGlyph);
            if (smallEnemyNeedsRefresh)
                UpdateItems(ref smallEnemyNeedsRefresh, Enemies.OfType<SmallEnemy>().ToList(), SmallEnemyBrush, Config.SmallEnemyGlyph);
            if (ellipseDebrisNeedsRefresh)
                UpdateItems(ref ellipseDebrisNeedsRefresh, EllipseDebrisCollection, EllipseDebrisBrush);

            if (renderQualityChanged)
            ApplyRenderQualityChange();
        }

        /// <summary>
        /// Called when a Player has been destroyed by collision with obstacles or projectiles.
        /// </summary>
        /// <seealso cref="SpawnPlayer"/>
        public void KillPlayer(Player player)
        {
            player.IsDead = true;
            Point shipDeathLocation = player.Location;
            Players.Remove(player);
            DecrementLives(Config.PlayerGlyph);
            SpawnPlayerDebris(shipDeathLocation);

            if (LivesRemaining == 0)
                GameOver();
        }

        /// <summary>
        /// Spawns Player at center of game area.
        /// </summary>
        public void SpawnPlayer()
        {
            double gameContainerWidth = (gameContainer is null) ? 0 : gameContainer.ActualWidth;
            double gameContainerHeight = (gameContainer is null) ? 0 : gameContainer.ActualHeight;

            Point spawnLocation = new(gameContainerWidth / 2, gameContainerHeight / 2);
            player1 = new Player(spawnLocation, new Vector(0, 0), Config.PlayerGlyph, PlayerBrush, PlayerRotateTransform);
            Players.Add(player1);
        }

        /// <summary>
        /// Generates, randomizes, and spawns Player debris whenever a Player is destroyed.
        /// </summary>
        /// <returns>A new collection of PlayerDebris.</returns>
        public void SpawnPlayerDebris(Point spawnLocation)
        {
            for (int i = 0; i < GameDefs.PLAYER_DEBRIS_AMOUNT; ++i)
            {
                int rotationPolarity = GameDefs.GetRandomPolarity(random);
                Vector movement = new(random.NextSingle() - .5, random.NextSingle() - .5);
                PlayerDebris debris = new(spawnLocation, movement, Config.PlayerDebrisGlyph, PlayerDebrisBrush)
                {
                    DissipationCountdown = i * 32,
                    RenderTransformOrigin = new Point(.5, .5),
                    RotationRate = random.NextSingle() * rotationPolarity
                };
                debris.Rotation.Angle = random.Next(0, 360);
                PlayerDebrisCollection.Add(debris);
            }
        }

        /// <summary>
        /// Generates, randomizes, and spawns EllipseDebris whenever an obstacle or enemy is destroyed.
        /// </summary>
        /// <returns>A new collection of EllipseDebris.</returns>
        public void SpawnEllipseDebris(Point spawnLocation)
        {
            for (int i = 0; i < GameDefs.ELLIPSE_DEBRIS_AMOUNT; ++i)
            {
                EllipseDebris debris = new(
                    spawnLocation,
                    new Vector(random.NextSingle() - .5, random.NextSingle() - .5),
                    new Rect(new Size(4,4)), EllipseDebrisBrush)
                {
                    DissipationCountdown = 100
                };
                EllipseDebrisCollection.Add(debris);
            }
        }

        public static readonly List<Key> UserInputKeys = new() { Key.A, Key.D, Key.W, Key.S, Key.H, Key.Left, Key.Right, Key.Up, Key.Down, Key.Space };

        public static bool IsKeyInUserInputSet(Key key)
        {
            return UserInputKeys.Contains(key);
        }

        public static bool IsKeyInUserInputSet(KeyEventArgs e)
        {
            return IsKeyInUserInputSet(e.Key);
        }

        public GameControl ProcessUserInput(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    IsLeftArrowPressed = (e.IsDown);
                    return GameControl.Left;
                case Key.A:
                    IsAKeyPressed = (e.IsDown);
                    return GameControl.Left;
                case Key.Right:
                    IsRightArrowPressed = (e.IsDown);
                    return GameControl.Right;
                case Key.D:
                    IsDKeyPressed = (e.IsDown);
                    return GameControl.Right;
                case Key.Up:
                    IsUpArrowPressed = (e.IsDown);
                    return GameControl.Up;
                case Key.W:
                    IsWKeyPressed = (e.IsDown);
                    return GameControl.Up;
                case Key.Down:
                    IsDownArrowPressed = (e.IsDown);
                    return GameControl.Down;
                case Key.S:
                    IsSKeyPressed = (e.IsDown);
                    return GameControl.Down;
                case Key.Space:
                    return GameControl.Fire;
                case Key.H:
                    return GameControl.Warp;
                default:
                    return GameControl.None;
            }
        }

        public void HandleKeyDown(KeyEventArgs e)
        {
            switch (PlayerControlMode)
            {
                case PlayerControlMode.Retro:
                    {
                        switch (ProcessUserInput(e))
                        {
                            case GameControl.Left:
                                IsLeftPressed = true;
                                break;
                            case GameControl.Right:
                                IsRightPressed = true;
                                break;
                            case GameControl.Up:
                                IsUpPressed = true;
                                break;
                            case GameControl.Fire:
                                if (player1 is null)
                                    break;

                                if (!IsFirePressed && !player1.IsDead &&
                                    CheckProjectileLimitReached(player1))
                                {
                                    Projectiles.Add(player1.Fire(new Projectile(
                                        Config.ProjectileGlyph, ProjectileBrush, ProjectileRotateTransform), 
                                        player1.Rotation.Angle - 90));
                                }
                                IsFirePressed = true;
                                break;
                            case GameControl.Warp:
                                IsWarpPressed = true;
                                if (player1 is null)
                                    return;

                                player1.IsWarpingThroughSpaceTime = true;
                                break;
                            default:
                                break;
                        }
                        break;
                    }
                case PlayerControlMode.WASD:
                    {
                        switch (ProcessUserInput(e))
                        {
                            case GameControl.Left:
                                IsLeftPressed = true;
                                IsRightPressed = false; // Enforce single X-Axis control input
                                break;
                            case GameControl.Right:
                                IsRightPressed = true;
                                IsLeftPressed = false; // Enforce single X-Axis control input
                                break;
                            case GameControl.Up:
                                IsUpPressed = true;
                                IsDownPressed = false; // Enforce single Y-Axis control input
                                break;
                            case GameControl.Down:
                                IsDownPressed = true;
                                IsUpPressed = false; // Enforce single Y-Axis control input
                                break;
                            case GameControl.Fire:
                                if (player1 is null)
                                    return;

                                if (!IsFirePressed && !player1.IsDead &&
                                    CheckProjectileLimitReached(player1))
                                {
                                    Projectiles.Add(player1.Fire(new Projectile(
                                        Config.ProjectileGlyph, ProjectileBrush, ProjectileRotateTransform),
                                        player1.Rotation.Angle - 90));
                                }
                                IsFirePressed = true;
                                break;
                            case GameControl.Warp:
                                IsWarpPressed = true;
                                if (player1 is null)
                                    return;

                                player1.IsWarpingThroughSpaceTime = true;
                                break;
                            default:
                                break;
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        public void HandleKeyUp(KeyEventArgs e)
        {
            switch (PlayerControlMode)
            {
                case PlayerControlMode.Retro:
                    {
                        switch (ProcessUserInput(e))
                        {
                            case GameControl.Left:
                                IsLeftPressed = false;
                                break;
                            case GameControl.Right:
                                IsRightPressed = false;
                                break;
                            case GameControl.Up:
                                IsUpPressed = false;
                                break;
                            case GameControl.Fire:
                                IsFirePressed = false;
                                break;
                            case GameControl.Warp:
                                IsWarpPressed = false;
                                break;
                            default:
                                break;
                        }
                        break;
                    }
                case PlayerControlMode.WASD:
                    {
                        switch (ProcessUserInput(e))
                        {
                            case GameControl.Left:
                                IsLeftPressed = false;
                                break;
                            case GameControl.Right:
                                IsRightPressed = false;
                                break;
                            case GameControl.Up:
                                IsUpPressed = false;
                                break;
                            case GameControl.Down:
                                IsDownPressed = false;
                                break;
                            case GameControl.Fire:
                                IsFirePressed = false;
                                break;
                            case GameControl.Warp:
                                IsWarpPressed = false;
                                break;
                            default:
                                break;
                        }
                        break;
                    }
                default:
                    break;
            }
        }
    }

    public class MainViewModel : BaseViewModel
    {
        private GameViewModel gameViewModel = new();
        public GameViewModel GameViewModel
        {
            get => gameViewModel;
            set => SetProperty(ref gameViewModel, value);
        }

        private bool isMenuOpen = true;
        public bool IsMenuOpen
        {
            get => isMenuOpen;
            set => SetProperty(ref isMenuOpen, value);
        }

        private bool showUiControlMappings = false;
        public bool ShowUiControlMappings
        {
            get => showUiControlMappings;
            set => SetProperty(ref showUiControlMappings, value);
        }

        private bool showGameSpriteMappings = true;
        public bool ShowGameSpriteMappings
        {
            get => showGameSpriteMappings;
            set => SetProperty(ref showGameSpriteMappings, value);
        }

        private double helpDocsFontSize = 14;
        public double HelpDocsFontSize
        {
            get => helpDocsFontSize;
            set => SetProperty(ref helpDocsFontSize, value);
        }

        private bool isHelpPanelExpanded= false;
        public bool IsHelpPanelExpanded
        {
            get => isHelpPanelExpanded;
            set => SetProperty(ref isHelpPanelExpanded, value);
        }

        private MainTabPage mainTabControlSelection = MainTabPage.Configure;
        public MainTabPage MainTabControlSelection
        {
            get => mainTabControlSelection;
            set => SetProperty(ref mainTabControlSelection, value);
        }

        private PresetsTabPage presetsTabControlSelection = PresetsTabPage.BuiltIn;
        public PresetsTabPage PresetsTabControlSelection
        {
            get => presetsTabControlSelection;
            set => SetProperty(ref presetsTabControlSelection, value);
        }

        public ICommand? ChangeHelpDocsFontSize { get; set; }
        public ICommand? ToggleShowUiControlMappings { get; set; }
        public ICommand? ToggleShowGameSpriteMappings { get; set; }
        public ICommand? ToggleMenuVisibility { get; set; }
        public ICommand? SetMainTabControlSelection { get; set; }
        public ICommand? SetPresetsTabControlSelection { get; set; }
        public ICommand? ToggleIsHelpPanelExpanded { get; set; }
        public ICommand? BringElementIntoView { get; set; }

        public MainViewModel()
        {
            ToggleShowUiControlMappings = new DelegateCommand(OnToggleShowUiControlMappings, null);
            ToggleShowGameSpriteMappings = new DelegateCommand(OnToggleShowGameSpriteMappings, null);
            ToggleMenuVisibility = new DelegateCommand(OnToggleMenuVisibility, null);
            ChangeHelpDocsFontSize = new DelegateCommand(OnChangeHelpDocsFontSize, null);
            ToggleIsHelpPanelExpanded = new DelegateCommand(OnSetIsHelpPanelExpanded, null);
            BringElementIntoView = new DelegateCommand(OnBringElementIntoView, null);

            SetMainTabControlSelection = new DelegateCommand(OnSetMainTabControlSelection, null);
            SetPresetsTabControlSelection = new DelegateCommand(OnSetPresetsTabControlSelection, null);
        }

        private void OnBringElementIntoView(object? frameworkElement)
        {
            if (frameworkElement == null)
                return;

            FrameworkElement? element = (FrameworkElement)frameworkElement;
            if (element == null)
                return;

            element.BringIntoView();
        }

        private void OnSetIsHelpPanelExpanded(object? isHelpPanelExpanded)
        {
            IsHelpPanelExpanded = !IsHelpPanelExpanded;
        }

        private void OnChangeHelpDocsFontSize(object? fontSizeIncrement)
        {
            if (fontSizeIncrement == null)
                return;

            double? addToCurrentFontSize = (double)fontSizeIncrement;

            // Enforce minimum and maximum font size to avoid oddities with extreme font sizes:
            if ((HelpDocsFontSize <= 8.0 && addToCurrentFontSize <= 0) || 
                (HelpDocsFontSize >= 96.0 && addToCurrentFontSize >= 0))
                return;

            HelpDocsFontSize += addToCurrentFontSize.Value;
        }

        private void OnSetPresetsTabControlSelection(object? presetsTabControlSelection)
        {
            if (presetsTabControlSelection is PresetsTabPage page)
                PresetsTabControlSelection = page;
        }

        private void OnSetMainTabControlSelection(object? mainTabControlSelection)
        {
            if (mainTabControlSelection is MainTabPage page)
            {
                MainTabControlSelection = page;
                if (MainTabControlSelection != MainTabPage.Help && IsHelpPanelExpanded)
                    IsHelpPanelExpanded = false;
            }
        }

        private void OnToggleShowGameSpriteMappings(object? obj)
        {
            ShowGameSpriteMappings = !ShowGameSpriteMappings;
        }

        private void OnToggleMenuVisibility(object? obj)
        {
            IsMenuOpen = !IsMenuOpen;
        }

        private void OnToggleShowUiControlMappings(object? obj)
        {
            ShowUiControlMappings = !ShowUiControlMappings;
        }
    }
}
