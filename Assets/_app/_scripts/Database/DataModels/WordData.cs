﻿using System;
using UnityEngine;

namespace EA4S.Db
{

    [Serializable]
    public class WordData : IData
    {
        public string Id;
        public WordDataKind Kind;
        public WordDataCategory Category;
        public string Arabic;
        public string[] Letters;
        public LetterSymbol[] Symbols; //TODO
        public int Difficulty;
        public int Drawing;

        public int NumberOfLetters { get { return Letters.Length; } }

        public string GetId()
        {
            return Id;
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}",
                Id,
                Kind,
                Category,
                Arabic
                );
        }

    }

    [Serializable]
    public struct LetterSymbol
    {
        public string LetterId;
        public string SymbolId;
    }
}