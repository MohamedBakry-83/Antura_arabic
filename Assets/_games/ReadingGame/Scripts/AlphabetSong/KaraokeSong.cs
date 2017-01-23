﻿using System.Collections.Generic;

namespace EA4S.Minigames.ReadingGame
{
    public class KaraokeSong
    {
        public readonly List<KaraokeSegment> lines = new List<KaraokeSegment>();

        public KaraokeSong(IEnumerable<KaraokeSegment> segments)
        {
            this.lines.AddRange(segments);
        }

        public float GetSegmentsLength()
        {
            float length = 0;
            for (int i = 0, count = lines.Count; i < count; ++i)
                length += lines[i].Length;

            return length;
        }
    }
}