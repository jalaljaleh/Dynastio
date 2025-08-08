TextMatching Utility
This README presents usage examples and expected outputs for the TextMatching class.

1. Normalize
Console.WriteLine("café".Normalize());
// Output: cafe

Console.WriteLine("  Résumé  ".Normalize());
// Output: resume



2. LevenshteinDistance
// Classic edit distance between "kitten" and "sitting"
int d1 = "kitten".LevenshteinDistance("sitting");
Console.WriteLine(d1);
// Output: 3

// Between identical strings
Console.WriteLine("hello".LevenshteinDistance("hello"));
// Output: 0



3. Damerau–Levenshtein
// Transposition of "a" and "c"
int d2 = "ca".DamerauLevenshtein("ac");
Console.WriteLine(d2);
// Output: 1

// Same as classic when no transpositions
Console.WriteLine("kitten".DamerauLevenshtein("sitting"));
// Output: 3



4. Jaro Similarity
double j1 = "MARTHA".Jaro("MARHTA");
Console.WriteLine(j1.ToString("F6"));
// Output: 0.944444

double j2 = "DIXON".Jaro("DICKSONX");
Console.WriteLine(j2.ToString("F6"));
// Output: 0.767857



5. Jaro–Winkler Similarity
double jw1 = "MARTHA".JaroWinkler("MARHTA");
Console.WriteLine(jw1.ToString("F6"));
// Output: 0.961111

double jw2 = "DIXON".JaroWinkler("DICKSONX");
Console.WriteLine(jw2.ToString("F6"));
// Output: 0.813333



6. Soundex (US-English)
Console.WriteLine("Washington".Soundex());
// Output: W252

Console.WriteLine("Pfister".Soundex());
// Output: P236



7. IsMatch (threshold & algorithm)
// Using default Jaro–Winkler, threshold 0.8
bool m1 = "kitten".IsMatch("sitting", threshold: 0.5);
Console.WriteLine(m1);
// Output: True  (similarity ≈ 0.571)

// Using Damerau–Levenshtein
bool m2 = "ca".IsMatch("ac", threshold: 1.0, algorithm: MatchAlgorithm.Damerau);
Console.WriteLine(m2);
// Output: True  (distance=1, maxLen=2, similarity=0.5)



8. GetSimilarity (0…1 scale)
double sim1 = "kitten".GetSimilarity("sitting");
Console.WriteLine(sim1.ToString("F3"));
// Output: 0.571  (1 − 3/7)

double sim2 = "MARTHA".GetSimilarity("MARHTA", MatchAlgorithm.Jaro);
Console.WriteLine(sim2.ToString("F6"));
// Output: 0.944444



9. FindBestMatch in a Collection
var candidates = new[] { "apple", "apply", "ape" };
var best = candidates.FindBestMatch("appel");

// best.Match == "apple"
// best.Score ≈ 0.933333

Console.WriteLine($"{best.Match} ({best.Score:F3})");
// Output: apple (0.933)



10. FuzzyContains (Approximate Substring)
bool f1 = "abracadabra".FuzzyContains("cada", maxDistance: 1);
Console.WriteLine(f1);
// Output: True

bool f2 = "hello world".FuzzyContains("worlds", maxDistance: 1);
Console.WriteLine(f2);
// Output: True  ("world" → "worlds" edits: +1)




