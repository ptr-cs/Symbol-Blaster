using NUnit.Framework;
using SymbolBlaster.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SymbolBlasterTest
{
    /// <summary>
    /// Tests classes within the SymbolBlaster.Game namespace
    /// </summary>
    [TestFixture]
    [RequiresThread(ApartmentState.STA)]
    public class GameObjectTests
    {
        Random random;
        Player player;
        Projectile projectile;
        LargeEnemy largeEnemy;

        /// <summary>
        /// Define objects used in tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            SolidColorBrush brush = new();
            RotateTransform transform = new();
            random = new Random();

            player = new(new(), new(), ".", brush, transform) { Tag = "player" };
            projectile = new(".", brush, transform);
            largeEnemy = new(new(), new(), ".", brush);
        }

        /// <summary>
        /// Assert the polarity of resulting Vectors from calls to GetRandomVector()
        /// </summary>
        [Test]
        public void GetRandomVectorTest()
        {
            Vector vector = GameDefs.GetRandomVector(random);
            Assert.Multiple(() =>
            {
                Assert.That(vector.X, Is.Positive);
                Assert.That(vector.Y, Is.Positive);
            });

            vector = GameDefs.GetRandomVector(random, randomXPolarity: true, randomYPolarity: false);
            Assert.Multiple(() =>
            {
                Assert.That(vector.X, Is.Positive.Or.Negative);
                Assert.That(vector.Y, Is.Positive);
            });

            vector = GameDefs.GetRandomVector(random, randomXPolarity: false, randomYPolarity: true);
            Assert.Multiple(() =>
            {
                Assert.That(vector.X, Is.Positive);
                Assert.That(vector.Y, Is.Positive.Or.Negative);
            });

            vector = GameDefs.GetRandomVector(random, Direction.Left);
            Assert.Multiple(() =>
            {
                Assert.That(vector.X, Is.Positive);
                Assert.That(vector.Y, Is.Positive.Or.Negative);
            });

            vector = GameDefs.GetRandomVector(random, Direction.Right);
            Assert.Multiple(() =>
            {
                Assert.That(vector.X, Is.Negative);
                Assert.That(vector.Y, Is.Positive.Or.Negative);
            });

            vector = GameDefs.GetRandomVector(random, Direction.Top);
            Assert.Multiple(() =>
            {
                Assert.That(vector.X, Is.Positive.Or.Negative);
                Assert.That(vector.Y, Is.Positive);
            });

            vector = GameDefs.GetRandomVector(random, Direction.Bottom);
            Assert.Multiple(() =>
            {
                Assert.That(vector.X, Is.Positive.Or.Negative);
                Assert.That(vector.Y, Is.Negative);
            });
        }

        /// <summary>
        /// Assert that the correct rotation Matrix for a given angle is returned by GetRotationMatrix
        /// </summary>
        [Test]
        public void GetRotationMatrixTest()
        {
            Matrix matrix = GameDefs.GetRotationMatrix(0);

            Assert.That(matrix, Is.EqualTo(Matrix.Identity));

            matrix = GameDefs.GetRotationMatrix(180);
            Matrix comparisonMatrix = new(-0.5984600690578581, -0.8011526357338304, 0.8011526357338304, -0.5984600690578581, 0, 0);
            Assert.That(matrix, Is.EqualTo(comparisonMatrix));
        }

        /// <summary>
        /// Assert that the random polarities generated are either positive or negative
        /// </summary>
        [Test]
        public void GetRandomPolarityTest()
        {
            Assert.That(GameDefs.GetRandomPolarity(random), Is.Positive.Or.Negative);
        }

        /// <summary>
        /// Assert that the random rotation-rate generator returns values within correct bounds
        /// </summary>
        [Test]
        public void GetRandomRotationRate()
        {
            Assert.That(GameDefs.GetRandomRotationRate(random), Is.
                GreaterThanOrEqualTo(-GameDefs.MAX_ROTATION_RATE).And.
                LessThanOrEqualTo(GameDefs.MAX_ROTATION_RATE));
        }

        /// <summary>
        /// Assert that a Projectile Fired from a Player has the expected rotation angle and Tag value
        /// </summary>
        [Test]
        public void Player_FireTest()
        {
            player.Fire(projectile, 0);
            Assert.Multiple(() =>
            {
                Assert.That(projectile.Rotation.Angle, Is.EqualTo(0 - 180));
                Assert.That(projectile.Tag, Is.EqualTo(player.Tag));
            });
        }

        /// <summary>
        /// Assert that a Projectile Fired from an Enemy has the expected rotation angle
        /// </summary>
        [Test]
        public void Enemy_FireTest()
        {
            largeEnemy.Fire(projectile, 0);
            Assert.That(projectile.Rotation.Angle, Is.EqualTo(0 - 180));
        }
    }
}
