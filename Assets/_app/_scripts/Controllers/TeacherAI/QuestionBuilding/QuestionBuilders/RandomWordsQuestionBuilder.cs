﻿using EA4S.Teacher;
using System.Collections.Generic;

namespace EA4S
{

    public class RandomWordsQuestionBuilder : IQuestionBuilder
    {
        private int nPacks;
        private int nCorrect;
        private int nWrong;
        private bool firstCorrectIsQuestion;
        private Db.WordDataCategory category;
        private bool drawingNeeded;

        public RandomWordsQuestionBuilder(int nPacks, int nCorrect = 1,  int nWrong = 0, bool firstCorrectIsQuestion = false, Db.WordDataCategory category = Db.WordDataCategory.None, bool drawingNeeded = false)
        {
            this.nPacks = nPacks;
            this.nCorrect = nCorrect;
            this.nWrong = nWrong;
            this.firstCorrectIsQuestion = firstCorrectIsQuestion;
            this.category = category;
            this.drawingNeeded = drawingNeeded;
        }

        // Pack history state
        private List<string> previousPacksIDs = new List<string>();
        // @todo: handle this private PackListHistory history = PackListHistory.SkipPacks;

        public List<QuestionPackData> CreateAllQuestionPacks()
        {
            previousPacksIDs.Clear();

            List<QuestionPackData> packs = new List<QuestionPackData>();
            for (int pack_i = 0; pack_i < nPacks; pack_i++)
            {
                var pack = CreateSingleQuestionPackData();
                packs.Add(pack);
            }

            return packs;
        }

        private QuestionPackData CreateSingleQuestionPackData()
        {
            var teacher = AppManager.Instance.Teacher;

            var correctWords = teacher.wordAI.SelectWords(
                () => teacher.wordHelper.GetWordsByCategory(category, drawingNeeded), 
                    new SelectionParameters(SelectionSeverity.AsManyAsPossible, nCorrect, 
                        packListHistory: PackListHistory.ForceAllDifferent, filteringIds:previousPacksIDs)
                );
            previousPacksIDs.AddRange(correctWords.ConvertAll(x => x.GetId()).ToArray());

            var wrongWords = teacher.wordAI.SelectWords(
                () => teacher.wordHelper.GetWordsNotIn(correctWords.ToArray()), 
                    new SelectionParameters(SelectionSeverity.AsManyAsPossible, nWrong)
                );

            var question = firstCorrectIsQuestion ? correctWords[0] : null;

            // Debug
            if (ConfigAI.verboseTeacher){
                string debugString = "--------- TEACHER: question pack result ---------";
                debugString += "\nCorrect Words: " + correctWords.Count;
                foreach (var l in correctWords) debugString += " " + l;
                debugString += "\nWrong Words: " + wrongWords.Count;
                foreach (var l in wrongWords) debugString += " " + l;
                UnityEngine.Debug.Log(debugString);
            }

            return QuestionPackData.Create(question, correctWords, wrongWords);
        }

    }
}