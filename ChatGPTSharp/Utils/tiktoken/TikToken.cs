﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace ChatGPTSharp.Utils.tiktoken
{
    public class TikToken
    {
        public static TikToken EncodingForModel(string modelName)
        {
            var setting = EncodingSettingModel.GetEncodingSetting(modelName);
            return new TikToken(setting);
        }

        public static Regex SpecialTokenRegex(HashSet<string> tokens)
        {
            var inner = string.Join("|", tokens.Select(Regex.Escape));
            return new Regex($"({inner})");
        }


        private CoreBPE _corePBE;

        private EncodingSettingModel _setting;

        public TikToken(EncodingSettingModel setting)
        {
            _corePBE = new CoreBPE(setting.MergeableRanks, setting.SpecialTokens, setting.PatStr);
            _setting = setting;
        }

        public HashSet<string> SpecialTokensSet()
        {
            return new HashSet<string>(_setting.SpecialTokens.Keys);
        }

        public List<int> Encode(string text, object allowedSpecial = null, object disallowedSpecial = null)
        {
            if (disallowedSpecial == null)
            {
                disallowedSpecial = "all";
            }

            var allowedSpecialSet = allowedSpecial.Equals("all") ? SpecialTokensSet() : new HashSet<string>((IEnumerable<string>)allowedSpecial);
            var disallowedSpecialSet = disallowedSpecial.Equals("all") ? new HashSet<string>(SpecialTokensSet().Except(allowedSpecialSet)) : new HashSet<string>((IEnumerable<string>)disallowedSpecial);

            if (disallowedSpecialSet.Count() > 0)
            {
                var specialTokenRegex = SpecialTokenRegex(disallowedSpecialSet);
                var match = specialTokenRegex.Match(text);
                if (match.Success)
                {
                    throw new Exception(match.Value);
                }
            }

            return _corePBE.EncodeNative(text, allowedSpecialSet).Item1;
        }


        public string Decode(List<int> tokens)
        {
            var ret = _corePBE.DecodeNative(tokens.ToArray());
            string str = Encoding.UTF8.GetString(ret.ToArray());
            return str;
        }





    }
}
