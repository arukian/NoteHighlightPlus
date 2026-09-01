using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NoteHighlightAddin
{
    public class LanguageDetectionService
    {
        private sealed class LanguageRule
        {
            public string Tag { get; set; }

            public List<string> StrongPatterns { get; set; }

            public List<string> WeakPatterns { get; set; }
        }

        private readonly List<LanguageRule> _rules;

        public enum LanguageDetectionConfidence
        {
            None,
            Low,
            Medium,
            High
        }

        public class LanguageDetectionResult
        {
            public string Language { get; set; }

            public int Score { get; set; }

            public int SecondBestScore { get; set; }

            public LanguageDetectionConfidence Confidence { get; set; }
        }

        public LanguageDetectionService()
        {
            _rules = new List<LanguageRule>
            {
                new LanguageRule
                {
                    Tag = "python",
                    StrongPatterns = new List<string>
                    {
                        @"^\s*def\s+\w+\s*\(",
                        @"^\s*class\s+\w+\s*[:\(]",
                        @"^\s*from\s+\w+.*\s+import\s+",
                        @"^\s*import\s+\w+",
                        @"^\s*if\s+.*:\s*$",
                        @"^\s*elif\s+.*:\s*$",
                        @"^\s*else\s*:\s*$",
                        @"^\s*for\s+.*\s+in\s+.*:\s*$",
                        @"^\s*while\s+.*:\s*$"
                    },
                    WeakPatterns = new List<string>
                    {
                        @"\bprint\s*\(",
                        @"\bNone\b",
                        @"\bTrue\b",
                        @"\bFalse\b",
                        @"\blambda\b",
                        @"\byield\b"
                    }
                },

                new LanguageRule
                {
                    Tag = "js",
                    StrongPatterns = new List<string>
                    {
                        @"\b(const|let|var)\s+\w+\s*=",
                        @"\bfunction\s+\w+\s*\(",
                        @"=>",
                        @"\bconsole\.log\s*\(",
                        @"\brequire\s*\(",
                        @"\bmodule\.exports\b"
                    },
                    WeakPatterns = new List<string>
                    {
                        @"\bundefined\b",
                        @"\bnull\b",
                        @"\basync\b",
                        @"\bawait\b",
                        @"\bPromise\b"
                    }
                },

                new LanguageRule
                {
                    Tag = "java",
                    StrongPatterns = new List<string>
                    {
                        @"\bpublic\s+static\s+void\s+main\s*\(",
                        @"\bSystem\.out\.println\s*\(",
                        @"\bpublic\s+class\s+\w+",
                        @"\bprivate\s+\w+\s+\w+\s*[;=]",
                        @"\bprotected\s+\w+\s+\w+\s*[;=]"
                    },
                    WeakPatterns = new List<string>
                    {
                        @"\bnew\s+\w+\s*\(",
                        @"\bextends\b",
                        @"\bimplements\b",
                        @"\bString\b",
                        @"\bboolean\b"
                    }
                },

                new LanguageRule
                {
                    Tag = "cs",
                    StrongPatterns = new List<string>
                    {
                        @"\busing\s+System\b",
                        @"\bnamespace\s+\w+",
                        @"\bConsole\.Write(Line)?\s*\(",
                        @"\bpublic\s+class\s+\w+",
                        @"\b(string|int|bool|double|decimal)\s+\w+\s*[=;]"
                    },
                    WeakPatterns = new List<string>
                    {
                        @"\bvar\s+\w+\s*=",
                        @"\basync\b",
                        @"\bawait\b",
                        @"\bget;\s*set;",
                        @"\bnull\b"
                    }
                },

                new LanguageRule
                {
                    Tag = "sql",
                    StrongPatterns = new List<string>
                    {
                    @"^\s*SELECT\b",
                    @"^\s*FROM\b",
                    @"^\s*INSERT\s+INTO\b",
                    @"^\s*UPDATE\b",
                    @"^\s*DELETE\s+FROM\b",
                    @"^\s*CREATE\s+TABLE\b"
                    },
                    WeakPatterns = new List<string>
                    {
                        @"\bWHERE\b",
                        @"\bJOIN\b",
                        @"\bGROUP\s+BY\b",
                        @"\bORDER\s+BY\b",
                        @"\bHAVING\b",
                        @"\bVALUES\b",
                        @"\bSET\b"
                    }
                },

                new LanguageRule
                {
                    Tag = "html",
                    StrongPatterns = new List<string>
                    {
                        @"<!DOCTYPE\s+html",
                        @"<html\b",
                        @"<head\b",
                        @"<body\b",
                        @"</\w+>"
                    },
                    WeakPatterns = new List<string>
                    {
                        @"<div\b",
                        @"<span\b",
                        @"<p\b",
                        @"<a\b",
                        @"class\s*="
                    }
                },

                new LanguageRule
                {
                    Tag = "css",
                    StrongPatterns = new List<string>
                    {
                        @"[.#]?[a-zA-Z][\w\-]*\s*\{",
                        @"@\w+",
                        @"\b(display|position|margin|padding|color|background)\s*:"
                    },
                    WeakPatterns = new List<string>
                    {
                        @"\bpx\b",
                        @"\brem\b",
                        @"\bem\b",
                        @"\bflex\b",
                        @"\bgrid\b"
                    }
                },

                new LanguageRule
                {
                    Tag = "ps1",
                    StrongPatterns = new List<string>
                    {
                        @"\bWrite-Host\b",
                        @"\bGet-\w+\b",
                        @"\bSet-\w+\b",
                        @"\bNew-\w+\b",
                        @"\$\w+\s*="
                    },
                    WeakPatterns = new List<string>
                    {
                        @"\bparam\s*\(",
                        @"\bforeach\s*\(",
                        @"\bWhere-Object\b",
                        @"\bSelect-Object\b"
                    }
                }
            };
        }

        public string Detect(string code)
        {
            LanguageDetectionResult result =
                DetectDetailed(code);

            return result.Language;
        }

        public LanguageDetectionResult DetectDetailed(
            string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return new LanguageDetectionResult
                {
                    Language = null,
                    Score = 0,
                    SecondBestScore = 0,
                    Confidence =
                        LanguageDetectionConfidence.None
                };
            }

            string normalizedCode = Normalize(code);

            Dictionary<string, int> scores =
                new Dictionary<string, int>();

            foreach (LanguageRule rule in _rules)
            {
                int score = 0;

                foreach (string pattern in rule.StrongPatterns)
                {
                    score += CountMatches(
                        normalizedCode,
                        pattern) * 3;
                }

                foreach (string pattern in rule.WeakPatterns)
                {
                    score += CountMatches(
                        normalizedCode,
                        pattern);
                }

                scores[rule.Tag] = score;
            }

            List<KeyValuePair<string, int>> ordered =
                scores
                    .OrderByDescending(
                        pair => pair.Value)
                    .ToList();

            KeyValuePair<string, int> best =
                ordered[0];

            int secondBestScore =
                ordered.Count > 1
                    ? ordered[1].Value
                    : 0;

            if (best.Value <= 0)
            {
                return new LanguageDetectionResult
                {
                    Language = null,
                    Score = 0,
                    SecondBestScore = secondBestScore,
                    Confidence =
                        LanguageDetectionConfidence.None
                };
            }

            int difference =
                best.Value - secondBestScore;

            LanguageDetectionConfidence confidence;

            if (
    (best.Value >= 6 &&
     difference >= 3)
    ||
    (best.Value >= 4 &&
     secondBestScore == 0)
   )
            {
                confidence =
                    LanguageDetectionConfidence.High;
            }
            else if (
                best.Value >= 3 &&
                difference >= 2
                )
            {
                confidence =
                    LanguageDetectionConfidence.Medium;
            }
            else
            {
                confidence =
                    LanguageDetectionConfidence.Low;
            }

            return new LanguageDetectionResult
            {
                Language = best.Key,
                Score = best.Value,
                SecondBestScore = secondBestScore,
                Confidence = confidence
            };
        }

        private static string Normalize(string code)
        {
            return code
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');
        }

        private static int CountMatches(
            string code,
            string pattern)
        {
            return Regex.Matches(
                code,
                pattern,
                RegexOptions.Multiline |
                RegexOptions.IgnoreCase)
                .Count;
        }
    }
}