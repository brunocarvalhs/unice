using System.Collections.Generic;
using System.Linq;

namespace Unice
{
    /// <summary>A small match for the gameplay tests: the house map, a fixed seed, one human ("h1") and some bots, host side.</summary>
    public static class MatchFixture
    {
        const int TestSeed = 1234;

        public static Game NewMatch(int bots = 3)
        {
            Map.Load(MapHouse.Build());
            GameRandom.Seed(TestSeed);
            var game = new Game { isHost = true, botCount = bots, totalRounds = 5 };
            game.NewMatch(new List<Game.Human> { new Game.Human { id = "h1", name = "Ana", kit = null, joinTime = 0 } });
            return game;
        }

        /// <summary>Someone who hides: neither the Unice nor the one with the doll.</summary>
        public static Character PlainHider(Game game) => game.characters.First(character => !character.IsUnice && character != game.doll.holder);

        /// <summary>Another hider, not <paramref name="except"/>, not the Unice.</summary>
        public static Character OtherHider(Game game, Character except) => game.characters.First(character => !character.IsUnice && character != except);
    }
}
