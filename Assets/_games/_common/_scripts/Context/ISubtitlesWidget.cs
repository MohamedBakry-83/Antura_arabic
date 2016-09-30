﻿
namespace EA4S
{
    public interface ISubtitlesWidget
    {
        void DisplaySentence(TextID text, float enterDuration = 2, bool showSpeaker = false, System.Action onSentenceCompleted = null);
        void Clear();
    }
}
