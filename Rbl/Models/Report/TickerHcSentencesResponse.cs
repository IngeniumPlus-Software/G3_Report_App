﻿using Rbl.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rbl.Models.Report
{
    public class TickerHcSentencesResponse
    {
        public static char[] WORD_SEPS = new char[] { '.', '?', '!', ' ', ';', ':', ',' };

        public string Ticker { get; set; }
        public IList<SentenceScorer> Scores { get; set; } = new List<SentenceScorer>();

        public TickerHcSentencesResponse(DfAllRatiosAvailable obj, IDictionary<WordTypesEnum, IList<string>> allImportantWords)
        {
            Ticker = obj.Ticker;
            var sentences = obj.HcSentences.Split("\n\n");
            foreach (var sentence in sentences)
            {
                Scores.Add(new SentenceScorer(sentence, allImportantWords));
            }
        }

        public string[] GetRawHtml(WordTypesEnum type, int count = 4)
        {
            var top = Scores.OrderByDescending(x => x.Scores[type]).Take(count);

            return top.Select(x => x.GetSentenceRawHtml(type)).ToArray();

        }

        public class SentenceScorer
        {
            public string Sentence { get; set; }
            public IDictionary<WordTypesEnum, int> Scores { get; set; } = new Dictionary<WordTypesEnum, int>();
            public IDictionary<WordTypesEnum, string[]> BoldWords { get; set; } = new Dictionary<WordTypesEnum, string[]>();

            public SentenceScorer(string sentence, IDictionary<WordTypesEnum, IList<string>> allImportantWords)
            {
                Sentence = sentence;

                var allWords = sentence.Split(WORD_SEPS);
                foreach (var importantWordsGroup in allImportantWords)
                {
                    var wordType = importantWordsGroup.Key;
                    var boldedWords = new List<string>();
                    var count = 0;
                    foreach (var iWord in importantWordsGroup.Value)
                    {
                        var matchQuery = from word in allWords where word.Equals(iWord, System.StringComparison.CurrentCultureIgnoreCase) select word;
                        var curCount = matchQuery.Count();
                        if (curCount > 0)
                        {
                            if (!boldedWords.Contains(iWord, StringComparer.OrdinalIgnoreCase))
                                boldedWords.Add(iWord);

                            count += curCount;
                        }
                    }

                    BoldWords[wordType] = boldedWords.ToArray();
                    Scores[wordType] = count;
                }
            }

            public string GetSentenceRawHtml(WordTypesEnum wordType, int maxLength = 200)
            {
                var result = Sentence;

                if (Sentence.Length > maxLength)
                {
                    result = result.Substring(0, _GetMaxSubstring(result, maxLength)) + "...";
                }

                foreach (var boldWord in BoldWords[wordType])
                {
                    result = result.Replace(boldWord, $"<b>{boldWord}</b>", System.StringComparison.CurrentCultureIgnoreCase);
                }

                return result;
            }

            private int _GetMaxSubstring(string str, int length)
            {
                var sub = str.Substring(0, length);
                var max = -1;
                foreach(var ws in WORD_SEPS)
                {
                    var c = sub.LastIndexOf(ws);
                    if (c > max)
                        max = c;
                }

                return max;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("{ 'Total Score': ");
                sb.Append(this.Scores.Sum(x => x.Value));

                foreach(var s in Scores)
                {
                    sb.Append(", '");
                    sb.Append(s.Key);
                    sb.Append("': ");
                    sb.Append(s.Value);
                }

                sb.Append(" }");
                return sb.ToString();
            }
        }
    }
}