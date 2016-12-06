using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PokemonGameInfo.Helpers
{
    public class Utils
    {
        public static double Similarity(string s1, string s2)
        {
            string Longer = s1, Shorter = s2;
            if (s1.Length < s2.Length)
            {
                Longer = s2;
                Shorter = s1;
            }

            int LongerLength = Longer.Length;
            if (LongerLength == 0)
            {
                return 1.0;
            }

            return (LongerLength - EditDistance(Longer, Shorter)) / (double)LongerLength;
        }

        public static int EditDistance(string S1, string S2)
        {
            S1 = S1.ToLower();
            S2 = S2.ToLower();

            int[] Costs = new int[S2.Length + 1];
            for (int i = 0; i <= S1.Length; i++)
            {
                int LastValue = i;
                for (int j = 0; j <= S2.Length; j++)
                {
                    if (i == 0)
                        Costs[j] = j;
                    else
                    {
                        if (j > 0)
                        {
                            int NewValue = Costs[j - 1];

                            if (S1[i - 1] != S2[j - 1])
                                NewValue = Math.Min(Math.Min(NewValue, LastValue),
                                    Costs[j]) + 1;
                            Costs[j - 1] = LastValue;
                            LastValue = NewValue;
                        }
                    }
                }
                if (i > 0)
                    Costs[S2.Length] = LastValue;
            }
            return Costs[S2.Length];
        }

        public static Dictionary<int, double> FindPokemonSimilarities(string Pkmn, int qtd)
        {
            Dictionary<int, double> Scores = new Dictionary<int, double>();
            foreach (int pos in Constants.PokemonNames.Keys)
            {
                string Name;
                if(Constants.PokemonNames.TryGetValue(pos,out Name))
                {
                    Scores.Add(pos, Similarity(Name, Pkmn)); 
                } 
            }
            
            var sortedDict = from entry in Scores orderby entry.Value descending select entry;
            if (qtd == 0) qtd = sortedDict.Count();
            Scores = sortedDict.Take(qtd).ToDictionary<KeyValuePair<int, double>, int, double>(pair => pair.Key, pair => pair.Value);
            return Scores;
        }
    }
}