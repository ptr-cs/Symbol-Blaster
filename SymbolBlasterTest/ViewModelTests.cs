using NUnit.Framework;
using SymbolBlaster.Game;
using SymbolBlaster.UI.Controls;
using SymbolBlaster.ViewModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace SymbolBlasterTest
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Used to retrieve private fields in order to test without breaking encapsulation
        /// (see https://stackoverflow.com/questions/95910/find-a-private-field-with-reflection)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T? GetFieldValue<T>(this object obj, string name)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T?)field?.GetValue(obj);
        }

        public static T? GetStaticFieldValue<T>(this object obj, string name)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T?)field?.GetValue(obj);
        }

        public static MethodInfo? GetMethod(this object obj, string methodName)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return method;
        }

        public static void InvokeMethod(this object obj, string methodName, object[] parameters)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(obj, parameters);
        }

        public static MethodInfo? GetStaticMethod(this object obj, string methodName)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            return method;
        }

        public static void InvokeStaticMethod(this object obj, string methodName, object[] parameters)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            method?.Invoke(obj, parameters);
        }
    }

    /// <summary>
    /// Fixture for testing assertions involving game control user input
    /// </summary>
    [TestFixture]
    public class GameControlTests
    {
        GameViewModel gvm;

        [SetUp]
        public void Setup()
        {
            gvm = new GameViewModel();
            gvm.SetGameContainer(new Canvas());
        }

        /// <summary>
        /// Assert that random keys are correctly accepted/rejected as part of the set of user input keys
        /// </summary>
        [Test]
        [Repeat(20)]
        [RequiresThread(ApartmentState.STA)]
        public void IsKeyInUserInputSet()
        {
            Key key = (Key)TestContext.CurrentContext.Random.Next(0, Enum.GetValues(typeof(Key)).Length - 1);
            if (GameViewModel.UserInputKeys.Contains(key))
                Assert.That(GameViewModel.IsKeyInUserInputSet(key), Is.True);
            else
                Assert.That(GameViewModel.IsKeyInUserInputSet(key), Is.False);
        }

        /// <summary>
        /// Assert that user input keys are correctly mapped to their corresponding game controls
        /// </summary>
        /// <param name="inputKeys"></param>
        /// <param name="control"></param>
        [Test, Sequential]
        [RequiresThread(ApartmentState.STA)]
        public void ProcessUserInput([Values(Key.Left, Key.A, Key.Right, Key.D, Key.Up, Key.W, Key.Down, Key.S, Key.Space, Key.H)] Key inputKeys,
            [Values(GameControl.Left, GameControl.Left, GameControl.Right, GameControl.Right, GameControl.Up, GameControl.Up, GameControl.Down, GameControl.Down, GameControl.Fire, GameControl.Warp)] GameControl control)
        {
            Assert.That(gvm.ProcessUserInput(new(Keyboard.PrimaryDevice, new HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero), 0, inputKeys)), Is.EqualTo(control));
        }

        /// <summary>
        /// Assert KeyDown events are processed correctly
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void HandleKeyDown()
        {
            gvm.PlayerControlMode = PlayerControlMode.Retro;
            KeyEventArgs kea = new(Keyboard.PrimaryDevice, new HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero), 0, Key.Left);
            gvm.HandleKeyDown(kea);
            Assert.That(gvm.IsLeftPressed, Is.True);
            gvm.SpawnPlayer();
            kea = new(Keyboard.PrimaryDevice, new HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero), 0, Key.Space);
            gvm.HandleKeyDown(kea);
            Assert.That(gvm.Projectiles, Has.Count.GreaterThan(0));
        }

        /// <summary>
        /// Assert KeyUp events are processed correctly
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void HandleKeyUp([Values] PlayerControlMode mode)
        {
            gvm.SetPlayerControlMode(mode);
            KeyEventArgs kea = new(Keyboard.PrimaryDevice, new HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero), 0, Key.Left);
            gvm.HandleKeyUp(kea);
            Assert.That(gvm.IsLeftPressed, Is.False);
        }
    }

    /// <summary>
    /// TestFixture for testing name entry, current score, and high score processing 
    /// </summary>
    [TestFixture]
    public class ScoreAndNameEntryTests
    {
        GameViewModel gvm;

        [SetUp]
        public void Setup()
        {
            gvm = new GameViewModel();
            gvm.SetGameContainer(new Canvas());
        }

        /// <summary>
        /// Assert that names entered for corresponding scores are formatted correctly
        /// </summary>
        [Test]
        [Repeat(10)]
        [RequiresThread(ApartmentState.STA)]
        public void FormatNameEntryString()
        {
            int stringLength = TestContext.CurrentContext.Random.Next(10);
            string randomString = TestContext.CurrentContext.Random.GetString(stringLength);
            Assert.That(GameViewModel.FormatNameEntryString(randomString), Has.Length.EqualTo(GameDefs.MAX_NAME_LENGTH));
        }

        /// <summary>
        /// Assert that adding a high score results in the correct name-score pairing
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void AddHighScore([Random(0, 1000, 5)] int score, [Values("one", "two", "three")] string name)
        {
            gvm.AddToScore(score);
            gvm.NameEntryString = name;
            gvm.AddHighScore();
            Assert.That(gvm.HighScores, Contains.Item(new Tuple<string, int>(GameViewModel.FormatNameEntryString(name), score)));
        }

        /// <summary>
        /// Assert that the new high score and game state are correctly set during name entry
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void EnterName([Random(0, 1000, 3)] int previousScore, [Random(0, 1000, 3)] int currentScore)
        {
            Tuple<string, int> score = new("LOW", previousScore);
            gvm.HighScores.Add(score);
            gvm.AddToScore(currentScore);
            gvm.EnterName();
            Assert.Multiple(() =>
            {
                if (previousScore < currentScore)
                    Assert.That(gvm.HighScore, Is.EqualTo(currentScore));
                else
                    Assert.That(gvm.HighScore, Is.EqualTo(previousScore));
                Assert.That(gvm.GameState, Is.EqualTo(GameState.ENTER_NAME));
            });
        }

        /// <summary>
        /// Assert that the correct game state is set when showing previous scores
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void ShowHighScores()
        {
            gvm.ShowHighScores();
            Assert.That(gvm.GameState, Is.EqualTo(GameState.SHOW_SCORES));
        }

        /// <summary>
        /// Assert the CurrentScore is able to be modified
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void AddToScore([Random(0, 1000, 5)] int score)
        {
            gvm.AddToScore(score);
            Assert.That(gvm.CurrentScore, Is.EqualTo(score));
        }
    }

    /// <summary>
    /// TestFixture for testing assertions involving player actions
    /// </summary>
    [TestFixture]
    public class PlayerActionTests
    {
        GameViewModel gvm;
        Player player;

        [SetUp]
        public void Setup()
        {
            gvm = new GameViewModel();
            player = new(new(), new(), ".", new(Colors.Gray), new()) { Tag = "player" };
            gvm.SetGameContainer(new Canvas());
        }

        /// <summary>
        /// Assert player movement is handled correctly in Retro control mode
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void HandlePlayerMovementRetro()
        {
            gvm.PlayerControlMode = PlayerControlMode.Retro;
            gvm.IsLeftPressed = true;
            gvm.IsUpPressed = true;
            gvm.SetGameContainer(new Canvas() { Width = 100, Height = 100 });
            gvm.HandlePlayerMovement(player);

            Assert.Multiple(() =>
            {
                Assert.That(player.Rotation.Angle, Is.EqualTo(-Player.RotationIncrement));
                Assert.That(player.AccelerationTimeElapsed, Is.EqualTo(1));
                Assert.That(player.Speed, Is.GreaterThan(0));
            });

            gvm.IsLeftPressed = false;
            gvm.IsUpPressed = false;
            gvm.IsRightPressed = true;
            gvm.HandlePlayerMovement(player);

            Assert.Multiple(() =>
            {
                Assert.That(player.Rotation.Angle, Is.EqualTo(0));
                Assert.That(player.DeccelerationTimeElapsed, Is.EqualTo(1));
            });
        }

        /// <summary>
        /// Assert player movement is handled correctly in WASD control mode
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void HandlePlayerMovementWASD()
        {
            gvm.PlayerControlMode = PlayerControlMode.WASD;
            gvm.IsLeftPressed = true;
            gvm.IsUpPressed = true;
            gvm.SetGameContainer(new Canvas() { Width = 100, Height = 100 });
            gvm.HandlePlayerMovement(player);

            Assert.Multiple(() =>
            {
                Assert.That(player.Rotation.Angle, Is.EqualTo(315));
                Assert.That(player.Movement.X, Is.GreaterThan(0));
                Assert.That(player.Movement.Y, Is.GreaterThan(0));
            });

            gvm.IsLeftPressed = false;
            gvm.HandlePlayerMovement(player);

            Assert.That(player.Rotation.Angle, Is.EqualTo(0));
        }

        /// <summary>
        /// Assert player movement is handled correctly when player location is beyond size of game container
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void HandlePlayerMovementOffScreenCoordinates()
        {
            gvm.PlayerControlMode = PlayerControlMode.WASD;
            gvm.SetGameContainer(new Canvas() { Width = 100, Height = 100 });
            player.Location = new Point(101, 101);
            gvm.HandlePlayerMovement(player);

            Assert.That(player.Location, Is.EqualTo(new Point(0, 0)));
        }

        /// <summary>
        /// Assert that the player object is warped correctly and removed from game object container
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void HandleWarp()
        {
            gvm.SetGameContainer(new Canvas() { Children = { player } });
            gvm.HandleWarp(player);
            Assert.Multiple(() =>
            {
                Assert.That(player.IsWarpingThroughSpaceTime, Is.True);
                Assert.That(player.WarpCountdown, Is.EqualTo(Player.WarpInterval - 1));
                Assert.That(gvm.GetGameContainer()?.Children.Contains(player), Is.False);
            });
        }

        /// <summary>
        /// Assert upon player's death that player flags are set and player-related game objects update
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void KillPlayer([Values(1, 2, 3)] int livesRemaining)
        {
            gvm.SpawnPlayer();
            Player newPlayer = gvm.Players.First();
            gvm.LivesRemaining = livesRemaining;
            gvm.KillPlayer(newPlayer);
            Assert.Multiple(() =>
            {
                Assert.That(newPlayer.IsDead, Is.True);
                Assert.That(gvm.Players, Is.Empty);
                Assert.That(gvm.PlayerDebrisCollection, Is.Not.Empty);
                Assert.That(gvm.LivesRemaining, Is.EqualTo(livesRemaining - 1));
                if (livesRemaining == 0)
                    Assert.That(gvm.GameState, Is.EqualTo(GameState.GAME_OVER));
            });
        }
    }

    /// <summary>
    /// TestFixture for testing assertions involving the creation of game objects
    /// </summary>
    [TestFixture]
    public class SpawnGameObjectTests
    {
        GameViewModel gvm;

        [SetUp]
        public void Setup()
        {
            gvm = new GameViewModel();
            gvm.SetGameContainer(new Canvas());
        }

        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void SpawnLargeEnemy([Values(0, 100)] int pointX, [Values(0, 100)] int pointY,
            [Values(-1, 0, 1)] int vectorX, [Values(-1, 0, 1)] int vectorY)
        {
            gvm.SpawnLargeEnemy(new(pointX, pointY), new(vectorX, vectorY));
            Assert.That(gvm.Enemies, Has.Count.GreaterThan(0));
        }

        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void SpawnSmallEnemy([Values(0, 100)] int pointX, [Values(0, 100)] int pointY,
            [Values(-1, 0, 1)] int vectorX, [Values(-1, 0, 1)] int vectorY)
        {
            gvm.SpawnSmallEnemy(new(pointX, pointY), new(vectorX, vectorY));
            Assert.That(gvm.Enemies, Has.Count.GreaterThan(0));
        }

        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void SpawnLargeObstacle([Values(0, 100)] int pointX, [Values(0, 100)] int pointY,
            [Values(-1, 0, 1)] int vectorX, [Values(-1, 0, 1)] int vectorY, [Random(0, 2, 2)] int rotation)
        {
            gvm.SpawnLargeObstacle(new(pointX, pointY), new(vectorX, vectorY), rotation);
            Assert.That(gvm.Obstacles, Has.Count.GreaterThan(0));
        }

        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void SpawnMediumObstacle([Values(0, 100)] int pointX, [Values(0, 100)] int pointY,
            [Values(-1, 0, 1)] int vectorX, [Values(-1, 0, 1)] int vectorY, [Random(0, 2, 2)] int rotation)
        {
            gvm.SpawnMediumObstacle(new(pointX, pointY), new(vectorX, vectorY), rotation);
            Assert.That(gvm.Obstacles, Has.Count.GreaterThan(0));
        }

        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void SpawnSmallObstacle([Values(0, 100)] int pointX, [Values(0, 100)] int pointY,
            [Values(-1, 0, 1)] int vectorX, [Values(-1, 0, 1)] int vectorY, [Random(0, 2, 2)] int rotation)
        {
            gvm.SpawnSmallObstacle(new(pointX, pointY), new(vectorX, vectorY), rotation);
            Assert.That(gvm.Obstacles, Has.Count.GreaterThan(0));
        }

        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void SpawnPlayer()
        {
            gvm.SpawnPlayer();
            Assert.That(gvm.Players, Has.Count.GreaterThan(0));
        }

        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void SpawnPlayerDebris([Values(0, 100)] int pointX, [Values(0, 100)] int pointY)
        {
            gvm.SpawnPlayerDebris(new(pointX, pointY));
            Assert.That(gvm.PlayerDebrisCollection, Has.Count.GreaterThan(0));
        }

        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void SpawnEllipseDebris([Values(0, 100)] int pointX, [Values(0, 100)] int pointY)
        {
            gvm.SpawnEllipseDebris(new(pointX, pointY));
            Assert.That(gvm.EllipseDebrisCollection, Has.Count.GreaterThan(0));
        }
    }

    /// <summary>
    /// Fixture for testing assertions related to game object and configuration color settings
    /// </summary>
    [TestFixture]
    public class ColorTests
    {
        GameViewModel gvm;
        Button control;
        ColorSelector selector;
        Popup popup;

        [SetUp]
        public void Setup()
        {
            gvm = new GameViewModel();
            gvm.SetGameContainer(new Canvas());

            control = new();
            selector = new();
            popup = new() { Child = selector };
        }

        /// <summary>
        /// Assert that a frozen RotateTransform is able to be created
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void CreateFrozenRotation()
        {
            RotateTransform rt = GameViewModel.CreateFrozenRotation(45);
            Assert.That(rt.IsFrozen, Is.True);
        }

        /// <summary>
        /// Assert that a frozen Brush is able to be created
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void CreateFrozenBrush()
        {
            SolidColorBrush brush = GameViewModel.CreateFrozenBrush(Colors.Black);
            Assert.That(brush.IsFrozen, Is.True);
        }

        /// <summary>
        /// Assert a random color is able to be generated correctly
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void GetRandomRgbColor()
        {
            Assert.That(() => gvm.GetRandomRgbColor(), Throws.Nothing);
        }

        /// <summary>
        /// Assert that Color Schemes are correctly set on the game configuration
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void SetColorScheme([Values] ColorScheme scheme)
        {
            gvm.SetColorScheme(scheme);
            Assert.That(gvm.Config.ColorScheme, Is.EqualTo(scheme));
        }

        /// <summary>
        /// Assert that the game world's foreground color is able to be applied to all other game objects
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void OnApplyForegroundBackgroundToAll()
        {
            gvm.SetGameForegroundColorBrush(gvm.GetRandomRgbColor());
            gvm.ApplyForegroundBackgroundToAll?.Execute(null);
            Assert.That(gvm.Config.ColorScheme, Is.EqualTo(ColorScheme.Custom));
            Assert.Multiple(() =>
            {
                Assert.That(gvm.Config.PlayerColor, Is.EqualTo(gvm.GameForegroundBrush.Color));
                Assert.That(gvm.Config.PlayerDebrisColor, Is.EqualTo(gvm.GameForegroundBrush.Color));
                Assert.That(gvm.Config.EllipseDebrisColor, Is.EqualTo(gvm.GameForegroundBrush.Color));
                Assert.That(gvm.Config.LargeEnemyColor, Is.EqualTo(gvm.GameForegroundBrush.Color));
                Assert.That(gvm.Config.SmallEnemyColor, Is.EqualTo(gvm.GameForegroundBrush.Color));
                Assert.That(gvm.Config.LargeObstacleColor, Is.EqualTo(gvm.GameForegroundBrush.Color));
                Assert.That(gvm.Config.MediumObstacleColor, Is.EqualTo(gvm.GameForegroundBrush.Color));
                Assert.That(gvm.Config.SmallObstacleColor, Is.EqualTo(gvm.GameForegroundBrush.Color));
                Assert.That(gvm.Config.ProjectileColor, Is.EqualTo(gvm.GameForegroundBrush.Color));
                Assert.That(gvm.Config.GameForeground, Is.EqualTo(gvm.GameForegroundBrush.Color));
            });
        }

        /// <summary>
        /// Assert that the game object color selection objects correctly handle UI coordination 
        /// and color selection
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void HandleSetGameObjectFill()
        {
            control.Tag = typeof(Player);

            gvm.HandleSetGameObjectFill(control, selector, popup);

            Assert.Multiple(() =>
            {
                Assert.That(selector.CurrentColor, Is.EqualTo(gvm.Config.PlayerColor));
                Assert.That(popup.PlacementTarget, Is.EqualTo(control));
                Assert.That(popup.IsOpen, Is.True);
            });
        }

        /// <summary>
        /// Assert that the game foreground color selection objects correctly handle UI coordination 
        /// and color selection
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void HandleSetGameForeground()
        {
            control.Tag = typeof(GameViewModel);

            gvm.HandleSetGameForeground(control, selector, popup);

            Assert.Multiple(() =>
            {
                Assert.That(selector.CurrentColor, Is.EqualTo(gvm.Config.PlayerColor));
                Assert.That(popup.PlacementTarget, Is.EqualTo(control));
                Assert.That(popup.IsOpen, Is.True);
            });
        }

        /// <summary>
        /// Assert that the game foreground color is able to be set correctly after selection 
        /// via color selector
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void HandleForegroundColorSelected()
        {
            selector.SelectedColor = gvm.GetRandomRgbColor();
            selector.Tag = typeof(GameViewModel);

            gvm.HandleForegroundColorSelected(selector, popup);

            Assert.Multiple(() =>
            {
                Assert.That(gvm.Config.GameForeground, Is.EqualTo(selector.SelectedColor.Value));
                Assert.That(gvm.Config.ColorScheme, Is.EqualTo(ColorScheme.Custom));
                Assert.That(popup.IsOpen, Is.False);
            });
        }

        /// <summary>
        /// Assert that game object colors are able to be set correctly after selection 
        /// via color selector
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void HandleFillColorSelected()
        {
            selector.SelectedColor = gvm.GetRandomRgbColor();
            selector.Tag = typeof(Player);

            gvm.HandleFillColorSelected(selector, popup);

            Assert.Multiple(() =>
            {
                Assert.That(gvm.Config.PlayerColor, Is.EqualTo(selector.SelectedColor.Value));
                Assert.That(gvm.Config.ColorScheme, Is.EqualTo(ColorScheme.Custom));
                Assert.That(popup.IsOpen, Is.False);
            });
        }

        /// <summary>
        /// Assert the state of color selection objects are reset upon closing a color selector 
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void ColorSelector_Closed()
        {
            ColorSelector? selector = gvm.GetFieldValue<ColorSelector?>("colorSelector");
            Popup? popup = gvm.GetFieldValue<Popup?>("colorSelectorPopup");

            if (selector is null || popup is null)
            {
                Assert.Fail("selector and popup are null");
                return;
            }

            gvm.InvokeMethod("ColorSelector_Closed", new object[] { gvm, new RoutedEventArgs() });

            Assert.Multiple(() =>
            {
                Assert.That(selector.SelectedColor, Is.Null);
                Assert.That(popup.IsOpen, Is.False);
            });
        }
    }

    /// <summary>
    /// Fixture to test functions within the GameViewModel class
    /// </summary>
    [TestFixture]
    public class GameViewModelTests
    {
        GameViewModel gvm;
        Player player;
        Projectile projectile;
        readonly string sprite = "🔳";

        /// <summary>
        /// Define objects used in tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            SolidColorBrush brush = new();
            RotateTransform transform = new();

            gvm = new GameViewModel();
            player = new(new(), new(), sprite, brush, transform) { Tag = "player" };
            projectile = new(sprite, brush, transform);
        }

        /// <summary>
        /// Assert the Projectile limit-checking logic, given an arbitrary limit and MovingShape to compare against
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void CheckProjectileLimitReached([Values(3, 4, 5)] int limit, [Random(0, 10, 5)] int timesFired)
        {
            for (int i = 0; i < timesFired; i++)
                gvm.Projectiles.Add(player.Fire(projectile, 0));

            if (timesFired < limit)
                Assert.That(gvm.CheckProjectileLimitReached(player, limit), Is.True);
            else
                Assert.That(gvm.CheckProjectileLimitReached(player, limit), Is.False);
        }

        /// <summary>
        /// Assert that the game-object container is able to be set
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void SetGameContainer()
        {
            Canvas c = new();
            gvm.SetGameContainer(c);
            Assert.That(gvm.GetGameContainer(), Is.Not.Null);
        }

        /// <summary>
        /// Assert the game-object container is able to be returned
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void GetGameContainer()
        {
            Assert.That(gvm.GetGameContainer(), Is.Not.Null.Or.Null);
        }


        /// <summary>
        /// Assert the PlayerControlMode is able to be set
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void SetPlayerControlMode([Values] PlayerControlMode mode)
        {
            gvm.SetPlayerControlMode(mode);
            Assert.That(gvm.PlayerControlMode, Is.EqualTo(mode));
        }

        /// <summary>
        /// Assert the RenderQuality is able to be set
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void SetRenderQuality([Values] EdgeMode mode)
        {
            gvm.SetRenderQuality(mode);
            Assert.That(gvm.RenderQuality, Is.EqualTo(mode));
        }

        /// <summary>
        /// Assert the player's remaining lives are able to be decremented
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void DecrementLives()
        {
            int lives = gvm.LivesRemaining;
            gvm.DecrementLives(".");
            Assert.That(gvm.LivesRemaining, Is.EqualTo(lives - 1));
        }

        /// <summary>
        /// Assert the string representation of the player's remaining lives is correctly generated
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void RebuildLivesString([Values("🐸", "🐹", "🐺")] string sprite)
        {
            string expected = "";
            gvm.RebuildLivesString(sprite);
            for (int i = 0; i < gvm.LivesRemaining; ++i)
                expected += sprite;
            Assert.That(gvm.LivesRemainingString, Is.EqualTo(expected));
        }

        /// <summary>
        /// Assert that reseting the game sets correct initial values and clears game object collections
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void ResetGame()
        {
            int levelCounter = gvm.LevelCounter;
            gvm.ResetGame(playerBeatPreviousLevel: false);
            Assert.Multiple(() =>
            {
                Assert.That(gvm.IsLeftPressed, Is.False);
                Assert.That(gvm.IsRightPressed, Is.False);
                Assert.That(gvm.IsUpPressed, Is.False);
                Assert.That(gvm.IsDownPressed, Is.False);
                Assert.That(gvm.IsFirePressed, Is.False);
                Assert.That(gvm.IsWarpPressed, Is.False);

                Assert.That(gvm.CurrentScore, Is.EqualTo(0));
                Assert.That(gvm.LivesRemaining, Is.EqualTo(GameDefs.PLAYER_STARTING_LIVES));

                Assert.That(gvm.Players, Is.Empty);
                Assert.That(gvm.Obstacles, Is.Empty);
                Assert.That(gvm.Projectiles, Is.Empty);
                Assert.That(gvm.Enemies, Is.Empty);
                Assert.That(gvm.EllipseDebrisCollection, Is.Empty);
                Assert.That(gvm.PlayerDebrisCollection, Is.Empty);
                Assert.That(gvm.ShapesToBeRemoved, Is.Empty);
                Assert.That(gvm.NameEntryString, Is.EqualTo(""));
            });

            gvm.ResetGame(playerBeatPreviousLevel: true);
            Assert.Multiple(() =>
            {
                Assert.That(gvm.LevelCounter, Is.EqualTo(levelCounter + 1));
                Assert.That(gvm.ShowLevelCounter, Is.True);
            });
        }

        /// <summary>
        /// Assert that the game object container is able to be cleared
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void ClearGameContainer()
        {
            gvm.SetGameContainer(new Canvas() { Children = { new Canvas() } });
            gvm.ClearGameContainer();
            Assert.That(gvm.GetGameContainer()?.Children, Is.Empty);
        }

        /// <summary>
        /// Assert that the ClearCollectionAndAddNewItems utility function clears and adds new items
        /// to a collection
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void ClearCollectionAndAddNewItems()
        {
            ObservableCollection<string> collection = new() { "four", "five", "six" };
            List<string> enumerable = new() { "one", "two", "three" };
            GameViewModel.ClearCollectionAndAddNewItems(collection, enumerable);
            Assert.Multiple(() =>
            {
                Assert.That(collection, Does.Not.Contain("four"));
                Assert.That(collection, Contains.Item("one"));
            });
        }

        /// <summary>
        /// Assert that Sprite Groups are able to be assigned to the game configuration
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void ApplySpriteGroup([Values] SpriteGroup group)
        {
            gvm.ApplySpriteGroup(group);
            Assert.That(gvm.Config.SpriteGroup, Is.EqualTo(group));
        }

        /// <summary>
        /// Assert that Configuration values are correctly applied to settings and game objects
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void OnLoadConfigurationPreset([Values] PresetType presetType)
        {
            GameConfiguration gc = new();

            if (presetType is PresetType.BuiltIn)
            {
                gvm.ConfigurationPresetBuiltInCollectionView.MoveCurrentToLast();
                gc = (GameConfiguration)gvm.ConfigurationPresetBuiltInCollectionView.CurrentItem;
            }
            else if (presetType is PresetType.Custom)
            {
                gvm.ConfigurationPresetsCustom.Add(gvm.ConfigurationPresetsBuiltIn.First());
                gvm.ConfigurationPresetCustomCollectionView.MoveCurrentToLast();
                gc = (GameConfiguration)gvm.ConfigurationPresetCustomCollectionView.CurrentItem;
            }

            gvm.LoadConfigurationPreset?.Execute(presetType);
            Assert.Multiple(() =>
            {
                Assert.That(gvm.PlayerControlMode, Is.EqualTo(gc.PlayerControlMode));
                Assert.That(gvm.RenderQuality, Is.EqualTo(gc.RenderQuality));
                Assert.That(gvm.ColorSchemeCollectionView.CurrentItem, Is.EqualTo(gc.ColorScheme));
                Assert.That(gvm.SpriteGroupCollectionView.CurrentItem, Is.EqualTo(gc.SpriteGroup));
                Assert.That(gvm.PlayerSpriteCollectionView.CurrentItem, Is.EqualTo(gc.PlayerGlyph));
                Assert.That(gvm.ProjectileSpriteCollectionView.CurrentItem, Is.EqualTo(gc.ProjectileGlyph));
                Assert.That(gvm.PlayerDebrisSpriteCollectionView.CurrentItem, Is.EqualTo(gc.PlayerDebrisGlyph));
                Assert.That(gvm.LargeObstacleSpriteCollectionView.CurrentItem, Is.EqualTo(gc.LargeObstacleGlyph));
                Assert.That(gvm.MediumObstacleSpriteCollectionView.CurrentItem, Is.EqualTo(gc.MediumObstacleGlyph));
                Assert.That(gvm.SmallObstacleSpriteCollectionView.CurrentItem, Is.EqualTo(gc.SmallObstacleGlyph));
                Assert.That(gvm.LargeEnemySpriteCollectionView.CurrentItem, Is.EqualTo(gc.LargeEnemyGlyph));
                Assert.That(gvm.SmallEnemySpriteCollectionView.CurrentItem, Is.EqualTo(gc.SmallEnemyGlyph));
            });
        }

        /// <summary>
        /// Assert that custom presets are able to be deleted
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void OnDeleteConfigurationPreset()
        {
            GameConfiguration gc = new();
            gvm.ConfigurationPresetsCustom.Add(gc);
            gvm.ConfigurationPresetCustomCollectionView.MoveCurrentTo(gc);
            gvm.DeleteConfigurationPreset?.Execute(null);
            Assert.That(gvm.ConfigurationPresetsCustom, Does.Not.Contain(gc));
        }

        /// <summary>
        /// Assert that custom presets are able to be saved
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void OnSaveConfigurationPreset()
        {
            gvm.SaveConfigurationPresetName = "test";
            gvm.SaveConfigurationPreset?.Execute(null);
            bool containsNewConfig = gvm.ConfigurationPresetsCustom.Where(config => config.ConfigurationName == "test").Any();
            Assert.That(containsNewConfig, Is.True);
        }

        /// <summary>
        /// Assert that game objects can be rotated counterclockwise to expected values
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void OnRotateGameSpriteCounterClockwise([Values (typeof(Player), typeof(Projectile))] Type type)
        {
            gvm.RotateGameSpriteCounterClockwise?.Execute(type);
            if (type == typeof(Player))
                Assert.That(gvm.Config.PlayerRotation, Is.EqualTo(-45));
            else if (type == typeof(Projectile))
                Assert.That(gvm.Config.ProjectileRotation, Is.EqualTo(-45));
        }

        /// <summary>
        /// Assert that game objects can be rotated clockwise to expected values
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void OnRotateGameSpriteClockwise([Values(typeof(Player), typeof(Projectile))] Type type)
        {
            gvm.RotateGameSpriteClockwise?.Execute(type);
            if (type == typeof(Player))
                Assert.That(gvm.Config.PlayerRotation, Is.EqualTo(45));
            else if (type == typeof(Projectile))
                Assert.That(gvm.Config.ProjectileRotation, Is.EqualTo(45));
        }

        /// <summary>
        /// Assert that a positive hit test result is generated when the player object's geometry 
        /// collides with another game object's geometry
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void ExecuteHitTestPlayer()
        {
            gvm.SetGameContainer(new Canvas() { Children = { player } });
            gvm.Obstacles.Add(new LargeObstacle(player.Location, new(), 0, "🔳", new(Colors.Gray)));
            gvm.ExecuteHitTest(player);
            List<DependencyObject>? hitTestResults = gvm.GetStaticFieldValue<List<DependencyObject>>("hitResultsList");

            if (hitTestResults is null)
            {
                Assert.Fail("hitTestResults is null");
                return;
            }

            Assert.That(hitTestResults, Has.Count.GreaterThan(0));
        }

        /// <summary>
        /// Assert that the game objects are able to be updated and that flags are reset after updates
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void UpdateItems()
        {
            gvm.SetGameContainer(new Canvas());

            bool pRefresh = true;
            bool lRefresh = true;
            bool eRefresh = true;

            gvm.SpawnPlayer();
            gvm.SpawnLargeObstacle(new(), new(), 0);
            gvm.SpawnEllipseDebris(new());

            gvm.SetPlayerColorBrush(Colors.Gray);
            gvm.SetLargeObstacleColorBrush(Colors.Gray);
            gvm.SetEllipseDebrisColorBrush(Colors.Gray);

            GameViewModel.UpdateItems(ref eRefresh, gvm.EllipseDebrisCollection, gvm.EllipseDebrisBrush);
            GameViewModel.UpdateItems(ref pRefresh, gvm.Players, gvm.PlayerBrush, ".", new RotateTransform());
            GameViewModel.UpdateItems(ref lRefresh, gvm.Obstacles.OfType<LargeObstacle>().ToList(), gvm.LargeObstacleBrush, ".");

            Assert.Multiple(() =>
            {
                Assert.That(pRefresh, Is.False);
                Assert.That(lRefresh, Is.False);
                Assert.That(eRefresh, Is.False);

                Assert.That(gvm.Players.First().Fill.Color, Is.EqualTo(Colors.Gray));
                Assert.That(gvm.Obstacles.OfType<LargeObstacle>().ToList().First().Fill.Color, Is.EqualTo(Colors.Gray));
                Assert.That(gvm.EllipseDebrisCollection.First().Fill.Color, Is.EqualTo(Colors.Gray));
            });
        }

        /// <summary>
        /// Assert that the ViewModel's update flags are properly updating when changes to game objects are made
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void ApplyVisualUpdates()
        {
            gvm.SpawnPlayer();
            gvm.SpawnLargeObstacle(new(), new(), 0);
            gvm.SpawnEllipseDebris(new());

            gvm.SetPlayerColorBrush(Colors.Gray);
            gvm.SetLargeObstacleColorBrush(Colors.Gray);
            gvm.SetEllipseDebrisColorBrush(Colors.Gray);

            bool? playerNeedsRefresh = gvm.GetFieldValue<bool?>(nameof(playerNeedsRefresh));
            bool? largeObstacleNeedsRefresh = gvm.GetFieldValue<bool?>(nameof(largeObstacleNeedsRefresh));
            bool? ellipseDebrisNeedsRefresh = gvm.GetFieldValue<bool?>(nameof(ellipseDebrisNeedsRefresh));

            if (playerNeedsRefresh is null || largeObstacleNeedsRefresh is null || ellipseDebrisNeedsRefresh is null)
            {
                Assert.Fail($"{nameof(playerNeedsRefresh)} or {nameof(largeObstacleNeedsRefresh)} or {nameof(ellipseDebrisNeedsRefresh)} is null");
                return;
            }

            Assert.Multiple(() =>
            {
                Assert.That(playerNeedsRefresh, Is.True);
                Assert.That(largeObstacleNeedsRefresh, Is.True);
                Assert.That(ellipseDebrisNeedsRefresh, Is.True);
            });

            gvm.ApplyVisualUpdates();

            playerNeedsRefresh = gvm.GetFieldValue<bool?>(nameof(playerNeedsRefresh));
            largeObstacleNeedsRefresh = gvm.GetFieldValue<bool?>(nameof(largeObstacleNeedsRefresh));
            ellipseDebrisNeedsRefresh = gvm.GetFieldValue<bool?>(nameof(ellipseDebrisNeedsRefresh));

            if (playerNeedsRefresh is null || largeObstacleNeedsRefresh is null || ellipseDebrisNeedsRefresh is null)
            {
                Assert.Fail($"{nameof(playerNeedsRefresh)} or {nameof(largeObstacleNeedsRefresh)} or {nameof(ellipseDebrisNeedsRefresh)} is null");
                return;
            }

            Assert.Multiple(() =>
            {
                Assert.That(playerNeedsRefresh, Is.False);
                Assert.That(largeObstacleNeedsRefresh, Is.False);
                Assert.That(ellipseDebrisNeedsRefresh, Is.False);

                Assert.That(gvm.Players.First().Fill.Color, Is.EqualTo(Colors.Gray));
                Assert.That(gvm.Obstacles.OfType<LargeObstacle>().ToList().First().Fill.Color, Is.EqualTo(Colors.Gray));
                Assert.That(gvm.EllipseDebrisCollection.First().Fill.Color, Is.EqualTo(Colors.Gray));
            });
        }
    }

    /// <summary>
    /// Fixture to test functions within the MainViewModel class
    /// </summary>
    [TestFixture]
    public class MainViewModelTests
    {
        MainViewModel mvm;
        GameViewModel gvm;

        [SetUp]
        public void Setup()
        {
            gvm = new GameViewModel();
            mvm = new MainViewModel() { GameViewModel = gvm };
        }

        /// <summary>
        /// Assert that the UI control mappings section is able to be toggled
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void OnToggleShowUiControlMappings()
        {
            bool currentState = mvm.ShowUiControlMappings;
            mvm.InvokeMethod(nameof(OnToggleShowUiControlMappings), new object[] { new object() });
            Assert.That(mvm.ShowUiControlMappings, Is.Not.EqualTo(currentState));
        }

        /// <summary>
        /// Assert that the main menu visibility is able to be toggled
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void OnToggleMenuVisibility()
        {
            bool currentState = mvm.IsMenuOpen;
            mvm.InvokeMethod(nameof(OnToggleMenuVisibility), new object[] { new object() });
            Assert.That(mvm.IsMenuOpen, Is.Not.EqualTo(currentState));
        }

        /// <summary>
        /// Assert that the game sprite mappings section is able to be toggled
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void OnToggleShowGameSpriteMappings()
        {
            bool currentState = mvm.ShowGameSpriteMappings;
            mvm.InvokeMethod(nameof(OnToggleShowGameSpriteMappings), new object[] { new object() });
            Assert.That(mvm.ShowGameSpriteMappings, Is.Not.EqualTo(currentState));
        }

        /// <summary>
        /// Assert that the help section expansion is able to be toggled
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void OnSetIsHelpPanelExpanded()
        {
            bool currentState = mvm.IsHelpPanelExpanded;
            mvm.InvokeMethod(nameof(OnSetIsHelpPanelExpanded), new object[] { new object() });
            Assert.That(mvm.IsHelpPanelExpanded, Is.Not.EqualTo(currentState));
        }

        /// <summary>
        /// Assert that the PresetsTabControlSelection is able to be set with enum values
        /// </summary>
        /// <param name="page"></param>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void OnSetPresetsTabControlSelection([Values] PresetsTabPage page)
        {
            mvm.InvokeMethod(nameof(OnSetPresetsTabControlSelection), new object[] { page });
            Assert.That(mvm.PresetsTabControlSelection, Is.EqualTo(page));
        }

        /// <summary>
        /// Assert that the MainTabControlSelection is able to be set with enum values
        /// </summary>
        /// <param name="page"></param>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void OnSetMainTabControlSelection([Values] MainTabPage page)
        {
            mvm.InvokeMethod(nameof(OnSetMainTabControlSelection), new object[] { page });
            Assert.That(mvm.MainTabControlSelection, Is.EqualTo(page));
        }

        /// <summary>
        /// Assert that the help section font size is able to be adjusted
        /// </summary>
        [Test]
        [RequiresThread(ApartmentState.STA)]
        public void OnChangeHelpDocsFontSize()
        {
            double currentState = mvm.HelpDocsFontSize;
            mvm.InvokeMethod(nameof(OnChangeHelpDocsFontSize), new object[] { 10.0 });
            Assert.That(mvm.HelpDocsFontSize, Is.EqualTo(currentState + 10.0));
        }
    }
}