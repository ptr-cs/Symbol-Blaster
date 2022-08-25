using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace SymbolBlaster.Game
{
    public enum Direction
    {
        Top = 0,
        Left = 1,
        Bottom = 2,
        Right = 3
    }

    public enum GameControl
    {
        None = 0,
        Up = 1,
        Left = 2,
        Down = 3,
        Right = 4,
        Thrust = 5,
        Fire = 6,
        Warp = 7
    }

    public enum PlayerControlMode
    {
        Retro = 0,
        WASD = 1
    }

    public enum ColorScheme
    {
        Dark = 0,
        Light = 1,
        Console = 2,
        RetroGreen = 3,
        Mono = 4,
        Custom = 5
    };

    public enum SpriteGroup
    {
        Emoji = 0,
        Music = 1,
        Money = 2,
        Hieroglyphs = 3,
        Geometric = 4
    };

    public interface IFireProjectile
    {
        Projectile Fire(Projectile projectile, double angleDegrees);
    }

    public interface IDissipate
    {
        int DissipationCountdown { get; set; }
    }

    public interface IScoreValue
    {
        int ScoreValue { get; }
    }

    public interface IDebris { }

    public class GlyphGeometryBuilder
    {
        static readonly DpiScale dpiScale = new();
        static readonly Typeface typeFace = new("Segoe Ui");
        public GlyphGeometryBuilder() { }
        public static Geometry GetGlyphGeometry(string glyph, double fontSize = 24.0)
        {
            FormattedText formattedText = new(glyph,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeFace,
                fontSize,
                Brushes.Black, dpiScale.PixelsPerDip);

            Geometry geometry = formattedText.BuildGeometry(new Point(-formattedText.Width / 2, -formattedText.Height / 2));
            if (geometry.CanFreeze)
                geometry.Freeze();

            return geometry;
        }
    }

    public abstract class MovingShape : FrameworkElement
    {
        protected Geometry shapeGeometry = Geometry.Empty;
        protected VisualCollection children;

        static EdgeMode RenderQuality { get; set; }
        public SolidColorBrush Fill { get; set; } = new();
        public bool NeedsRefresh { get; set; } = false;
        public Point Location { get; set; }
        public Vector Movement { get; set; }

        protected MovingShape(Point location, Vector movement, SolidColorBrush brush)
        {
            Location = location;
            Movement = movement;
            Fill = brush;
            DataContext = null;
            FocusVisualStyle = null;
            Resources = null;
            LayoutTransform = null;
            BindingGroup = null;
            Style = null;
            children = new VisualCollection(this);
            RenderOptions.SetEdgeMode(this, RenderQuality);
        }

        public static void SetRenderQuality(EdgeMode edgeMode)
        {
            RenderQuality = edgeMode;
        }

        public virtual void DrawVisual()
        {
            children.Clear();
            DrawingVisual drawingVisual = new();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawGeometry(Fill, null, shapeGeometry);
            drawingContext.Close();

            children.Add(drawingVisual);
        }

        public virtual void DrawVisual(Transform transform)
        {
            children.Clear();
            DrawingVisual drawingVisual = new() { Transform = transform };
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawGeometry(Fill, null, shapeGeometry);
            drawingContext.Close();

            children.Add(drawingVisual);
        }

        public virtual void Refresh()
        {
            DrawVisual();
        }

        protected Geometry DefiningGeometry => shapeGeometry;

        protected Geometry GetDefiningGeometry() { return DefiningGeometry; }

        protected override bool IsEnabledCore => false;

        protected override int VisualChildrenCount
        {
            get { return children.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= children.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return children[index];
        }
    }

    public abstract class MovingGlyph : MovingShape
    {
        protected double FontSize { get; set; } = 34;
        public string GameSprite { get; set; } = ".";

        protected MovingGlyph(Point location, Vector movement, string sprite, SolidColorBrush brush) : base(location, movement, brush) 
        {
            GameSprite = sprite;
        }

        public virtual void RefreshGeometry()
        {
            shapeGeometry = GlyphGeometryBuilder.GetGlyphGeometry(GameSprite, FontSize);
        }

        public virtual void RefreshGeometry(Transform transform)
        {
            shapeGeometry = GlyphGeometryBuilder.GetGlyphGeometry(GameSprite, FontSize);
        }

        public override void Refresh()
        {
            RefreshGeometry();
            DrawVisual();
        }

        public virtual void Refresh(Transform transform)
        {
            RefreshGeometry();
            DrawVisual(transform);
        }
    }

    public class Projectile : MovingGlyph, IDissipate
    {
        public const int Velocity = 5;

        public RotateTransform Rotation = new();
        public int DissipationCountdown { get; set; } = 100;

        public Projectile(string sprite, SolidColorBrush brush, RotateTransform rotateTransform) : base(new(), new(), sprite, brush)
        {
            RenderTransform = Rotation;
            FontSize = 16;
            Refresh(rotateTransform);
        }
    }

    public class Player : MovingGlyph, IFireProjectile
    {
        public const int WarpInterval = 80;
        public const int AccelerationLimit = 2;
        public const double WASDAccelerationIncrement = .05;
        public const double AccelerationIncrement = .0002;
        public const double DeccelerationIncrement = .0001;
        public const int RotationIncrement = 2;

        public RotateTransform Rotation = new();

        public double Speed { get; set; } = 0;
        public double AccelerationTimeElapsed { get; set; }
        public double DeccelerationTimeElapsed { get; set; }
        public bool IsDead { get; set; } = false;
        public bool IsWarpingThroughSpaceTime { get; set; }
        public int WarpCountdown { get; set; } = WarpInterval;

        public Player(Point location, Vector movement, string sprite, SolidColorBrush brush, RotateTransform rotateTransform) : base(location, movement, sprite, brush)
        {
            this.Uid = Guid.NewGuid().ToString();
            Tag = this.Uid;
            RenderTransform = Rotation;
            Refresh(rotateTransform);
        }

        public Projectile Fire(Projectile projectile, double angleDegrees)
        {
            double angleRadians = angleDegrees * Math.PI / 180;

            Vector vector = new(
                (float)Math.Cos(angleRadians),
                (float)Math.Sin(angleRadians));

            Matrix rotationMatrix = new(vector.X * 2, vector.Y * 2, -vector.Y * 2, vector.X * 2, 0, 0);
            Point spawnLocation = Point.Add(this.Location, Vector.Multiply(new Vector(FontSize/3, -2), rotationMatrix));

            projectile.Location = spawnLocation;
            projectile.Movement = Vector.Multiply(Projectile.Velocity, vector);
            projectile.Rotation.Angle = angleDegrees - 180;

            projectile.Tag = this.Tag;

            return projectile;
        }
    }

    public abstract class Enemy : MovingGlyph, IFireProjectile, IScoreValue
    {
        public const int DirectionInterval = 256;
        public const int FireInterval = 96;

        public int ChangeDirectionCounter = DirectionInterval;
        public int FireProjectileCounter = FireInterval;

        public Rect GeometryBounds = new(new Size(0, 0));

        public abstract int ScoreValue { get; }

        public Enemy(Point location, Vector movement, string sprite, SolidColorBrush brush) : base(location, movement, sprite, brush) 
        {
            this.Uid = Guid.NewGuid().ToString();
            Tag = this.Uid;
        }

        public Projectile Fire(Projectile projectile, double angleDegrees)
        {
            double angleRadians = angleDegrees * Math.PI / 180;

            Vector vector = new((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians));

            Matrix rotationMatrix = new(vector.X * 2, vector.Y * 2, -vector.Y * 2, vector.X * 2, 0, 0);
            Point spawnLocation = Point.Add(this.Location, Vector.Multiply(new Vector(FontSize / 3, 0), rotationMatrix));

            projectile.Location = spawnLocation;
            projectile.Movement = Vector.Multiply(Projectile.Velocity, vector);
            projectile.Rotation.Angle = angleDegrees - 180;

            return projectile;
        }
    }

    public class LargeEnemy : Enemy
    {
        public override int ScoreValue { get => 256; }
        public LargeEnemy(Point location, Vector movement, string sprite, SolidColorBrush brush) : base(location, movement, sprite, brush)
        {
            FontSize = 64;
            Refresh();
        }
    }

    public class SmallEnemy : Enemy
    {
        public override int ScoreValue { get => 512; }
        public SmallEnemy(Point location, Vector movement, string sprite, SolidColorBrush brush) : base(location, movement, sprite, brush)
        {
            FontSize = 40;
            Refresh();
        }
    }

    public abstract class Obstacle : MovingGlyph, IScoreValue
    {
        public RotateTransform Rotation = new();
        public double RotationRate = 0;
        public Rect GeometryBounds = new(new Size(0, 0));
        public abstract int ScoreValue { get; } 

        public Obstacle(Point location, Vector movement, double rotationRate, string sprite, SolidColorBrush brush) : base(location, movement, sprite, brush)
        {
            Location = location;
            Movement = movement;
            RotationRate = rotationRate;
            RenderTransform = Rotation;
        }
    }

    public class LargeObstacle : Obstacle
    {
        public override int ScoreValue { get => 32; }
        public LargeObstacle(Point location, Vector movement, double rotationRate, string sprite, SolidColorBrush brush)
            : base(location, movement, rotationRate, sprite, brush)
        {
            FontSize = 72;
            Refresh();
        }
    }

    public class MediumObstacle : Obstacle
    {
        public override int ScoreValue { get => 64; }
        public MediumObstacle(Point location, Vector movement, double rotationRate, string sprite, SolidColorBrush brush)
            : base(location, movement, rotationRate, sprite, brush)
        {
            FontSize = 48;
            Refresh();
        }
    }

    public class SmallObstacle : Obstacle
    {
        public override int ScoreValue { get => 128; }
        public SmallObstacle(Point location, Vector movement, double rotationRate, string sprite, SolidColorBrush brush)
            : base(location, movement, rotationRate, sprite, brush)
        {
            FontSize = 28;
            Refresh();
        }
    }

    public class PlayerDebris : MovingGlyph, IDissipate, IDebris
    {
        public int DissipationCountdown { get; set; } = 500;

        public RotateTransform Rotation = new();
        public double RotationRate = 0;

        public PlayerDebris(Point location, Vector movement, string sprite, SolidColorBrush brush)
            : base(location, movement, sprite, brush)
        {
            FontSize = 14;
            RenderTransform = Rotation;
            Refresh();
        }
    }

    public class EllipseDebris : MovingShape, IDissipate, IDebris
    {
        public int DissipationCountdown { get; set; } = 500;

        public EllipseDebris(Point location, Vector movement, Rect definingRect, SolidColorBrush brush) : base(location, movement, brush)
        {
            shapeGeometry = new EllipseGeometry(definingRect);
            Refresh();
        }
    }

    public static class GameDefs
    {
        public const int MIN_LARGE_OBSTACLES = 2;
        public const int MAX_LARGE_OBSTACLES = 4;
        public const int PLAYER_DEBRIS_AMOUNT = 6;
        public const int ELLIPSE_DEBRIS_AMOUNT = 6;
        public const int PLAYER_PROJECTILE_LIMIT = 4;
        public const int PLAYER_STARTING_LIVES = 3;
        public const int MAX_NAME_LENGTH = 4;
        public const int MIN_ROTATION_RATE = 1;
        public const int MAX_ROTATION_RATE = 3;

        public const int SMALL_ENEMY_THRESHOLD = (MAX_LARGE_OBSTACLES / 2) * 7;
        public const int ENEMY_SPAWN_CUTOFF = (MAX_LARGE_OBSTACLES - 0) * 7;

        public const double DegreesToRadians = (Math.PI / 180);
        public const double RadiansToDegrees = (180 / Math.PI);

        public static Vector GetRandomVector(Random random, bool randomXPolarity = false, bool randomYPolarity = false)
        {
            int xPolarity = (randomXPolarity) ? GetRandomPolarity(random) : 1;
            int yPolarity = (randomYPolarity) ? GetRandomPolarity(random) : 1;
            return new Vector(random.NextSingle() * xPolarity, random.NextSingle() * yPolarity);
        }

        public static Vector GetRandomVector(Random random, Direction direction)
        {
            return direction switch
            {
                Direction.Left => new Vector(random.NextSingle(), random.NextSingle() * GetRandomPolarity(random)),
                Direction.Right => new Vector(-random.NextSingle(), random.NextSingle() * GetRandomPolarity(random)),
                Direction.Top => new Vector(random.NextSingle() * GetRandomPolarity(random), random.NextSingle()),
                Direction.Bottom => new Vector(random.NextSingle() * GetRandomPolarity(random), -random.NextSingle()),
                _ => new Vector(random.NextSingle() * GetRandomPolarity(random), random.NextSingle() * GetRandomPolarity(random)),
            };
        }
        public static Matrix GetRotationMatrix(double angleRadians)
        {
            return new Matrix(Math.Cos(angleRadians), Math.Sin(angleRadians), -Math.Sin(angleRadians), Math.Cos(angleRadians), 0, 0);
        }

        public static int GetRandomPolarity(Random random)
        {
            return (random.Next(0, 2) == 0) ? -1 : 1;
        }

        public static double GetRandomRotationRate(Random random)
        {
            return random.Next(MIN_ROTATION_RATE, MAX_ROTATION_RATE) * GetRandomPolarity(random);
        }

        static GameDefs()
        {
            if (MAX_LARGE_OBSTACLES < MIN_LARGE_OBSTACLES)
                throw new Exception(String.Format("{1} must be larger than {0}; increase the value of {1}.", MIN_LARGE_OBSTACLES, nameof(MAX_LARGE_OBSTACLES)));
        }
    }

    public class GameConfiguration : ICloneable
    {
        public string ConfigurationName { get; set; } = "";
        public PlayerControlMode PlayerControlMode { get; set; }
        public ColorScheme ColorScheme { get; set; }
        public SpriteGroup SpriteGroup { get; set; }
        public EdgeMode RenderQuality { get; set; }
        public Color GameForeground { get; set; }
        public Color GameBackground { get; set; }
        public Color PlayerColor { get; set; }
        public Color ProjectileColor { get; set; }
        public Color PlayerDebrisColor { get; set; }
        public Color EllipseDebrisColor { get; set; }
        public Color LargeObstacleColor { get; set; }
        public Color MediumObstacleColor { get; set; }
        public Color SmallObstacleColor { get; set; }
        public Color LargeEnemyColor { get; set; }
        public Color SmallEnemyColor { get; set; }
        public string PlayerGlyph { get; set; } = "";
        public string ProjectileGlyph { get; set; } = "";
        public string PlayerDebrisGlyph { get; set; } = "";
        public string LargeObstacleGlyph { get; set; } = "";
        public string MediumObstacleGlyph { get; set; } = "";
        public string SmallObstacleGlyph { get; set; } = "";
        public string LargeEnemyGlyph { get; set; } = "";
        public string SmallEnemyGlyph { get; set; } = "";
        public int PlayerRotation { get; set; }
        public int ProjectileRotation { get; set; }
        public GameConfiguration() { }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Geometry PlayerGlyphGeometry { get => GlyphGeometryBuilder.GetGlyphGeometry(PlayerGlyph); }
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Geometry ProjectileGlyphGeometry { get => GlyphGeometryBuilder.GetGlyphGeometry(ProjectileGlyph); }
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Geometry PlayerDebrisGlyphGeometry { get => GlyphGeometryBuilder.GetGlyphGeometry(PlayerDebrisGlyph); }
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Geometry LargeObstacleGlyphGeometry { get => GlyphGeometryBuilder.GetGlyphGeometry(LargeObstacleGlyph); }
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Geometry MediumObstacleGlyphGeometry { get => GlyphGeometryBuilder.GetGlyphGeometry(MediumObstacleGlyph); }
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Geometry SmallObstacleGlyphGeometry { get => GlyphGeometryBuilder.GetGlyphGeometry(SmallObstacleGlyph); }
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Geometry LargeEnemyGlyphGeometry { get => GlyphGeometryBuilder.GetGlyphGeometry(LargeEnemyGlyph); }
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Geometry SmallEnemyGlyphGeometry { get => GlyphGeometryBuilder.GetGlyphGeometry(SmallEnemyGlyph); }

        public object Clone()
        {
            return new GameConfiguration
            {
                ConfigurationName = this.ConfigurationName,
                PlayerControlMode = this.PlayerControlMode,
                ColorScheme = this.ColorScheme,
                SpriteGroup = this.SpriteGroup,
                RenderQuality = this.RenderQuality,
                GameForeground = this.GameForeground,
                GameBackground = this.GameBackground,
                PlayerColor = this.PlayerColor,
                ProjectileColor = this.ProjectileColor,
                PlayerDebrisColor = this.PlayerDebrisColor,
                EllipseDebrisColor = this.EllipseDebrisColor,
                LargeObstacleColor = this.LargeObstacleColor,
                MediumObstacleColor = this.MediumObstacleColor,
                SmallObstacleColor = this.SmallObstacleColor,
                LargeEnemyColor = this.LargeEnemyColor,
                SmallEnemyColor = this.SmallEnemyColor,
                PlayerGlyph = this.PlayerGlyph,
                ProjectileGlyph = this.ProjectileGlyph,
                PlayerDebrisGlyph = this.PlayerDebrisGlyph,
                LargeObstacleGlyph = this.LargeObstacleGlyph,
                MediumObstacleGlyph = this.MediumObstacleGlyph,
                SmallObstacleGlyph = this.SmallObstacleGlyph,
                LargeEnemyGlyph = this.LargeEnemyGlyph,
                SmallEnemyGlyph = this.SmallEnemyGlyph,
                PlayerRotation = this.PlayerRotation,
                ProjectileRotation = this.ProjectileRotation
            };
        }
    }
}
