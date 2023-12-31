// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Game.Tournament.Screens.Showcase;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneShowcaseScreen : TournamentTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new ShowcaseScreen());
        }
    }
}
