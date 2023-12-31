// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using JetBrains.Annotations;
using osu.Game.Collections;
using osu.Game.Database;

namespace osu.Game.Overlays.Music
{
    public class FilterCriteria
    {
        /// <summary>
        /// The search text.
        /// </summary>
        public string SearchText;

        /// <summary>
        /// The collection to filter beatmaps from.
        /// </summary>
        [CanBeNull]
        public Live<BeatmapCollection> Collection;
    }
}
