using UnityEngine;

namespace BallroomLegends.Floor
{
    /// <summary>The colors of one couple: her dress, his suit, and both heads of hair.</summary>
    public struct CoupleLook
    {
        public Color Dress;
        public Color Suit;
        public Color LeaderHair;
        public Color FollowerHair;

        private static readonly Color[] Dresses =
        {
            new Color(0.86f, 0.20f, 0.35f), // ruby
            new Color(0.22f, 0.45f, 0.90f), // royal blue
            new Color(0.55f, 0.30f, 0.80f), // violet
            new Color(0.10f, 0.65f, 0.60f), // teal
            new Color(0.95f, 0.55f, 0.20f), // tangerine
            new Color(0.95f, 0.95f, 0.97f), // white
            new Color(0.95f, 0.45f, 0.70f), // pink
            new Color(0.30f, 0.75f, 0.35f), // emerald
            new Color(0.75f, 0.85f, 0.95f), // ice blue
            new Color(0.60f, 0.10f, 0.15f)  // wine
        };

        private static readonly Color[] Hair =
        {
            new Color(0.10f, 0.07f, 0.05f), // black
            new Color(0.30f, 0.18f, 0.10f), // dark brown
            new Color(0.55f, 0.35f, 0.18f), // light brown
            new Color(0.85f, 0.70f, 0.40f), // blonde
            new Color(0.60f, 0.25f, 0.10f)  // auburn
        };

        private static readonly Color TailSuit = new Color(0.08f, 0.08f, 0.12f);
        private static readonly Color LatinShirt = new Color(0.14f, 0.12f, 0.16f);

        /// <summary>A random look. Standard men wear tailsuits; Latin men dark shirts.</summary>
        public static CoupleLook Random(System.Random random, bool latin)
        {
            return new CoupleLook
            {
                Dress = Dresses[random.Next(Dresses.Length)],
                Suit = latin ? LatinShirt : TailSuit,
                LeaderHair = Hair[random.Next(Hair.Length)],
                FollowerHair = Hair[random.Next(Hair.Length)]
            };
        }

        /// <summary>The player's couple: a gold dress, so you can always spot yourselves.</summary>
        public static CoupleLook Player(bool latin)
        {
            return new CoupleLook
            {
                Dress = new Color(0.96f, 0.80f, 0.42f),
                Suit = latin ? LatinShirt : TailSuit,
                LeaderHair = Hair[1],
                FollowerHair = Hair[3]
            };
        }
    }
}
