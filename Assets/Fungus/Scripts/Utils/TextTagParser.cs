// This code is part of the Fungus library (http://fungusgames.com) maintained by Chris Gregan (http://twitter.com/gofungus).
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Fungus
{
    /// <summary>
    /// Parses a string for special Fungus text tags.
    /// </summary>
    public class TextTagParser : ITextTagParser
    {
        public static string GetTagHelp()
        {
            return "" +
                "\t{b} Bold Text {/b}\n" + 
                "\t{i} Italic Text {/i}\n" +
                "\t{color=red} Color Text (color){/color}\n" +
                "\t{size=30} Text size {/size}\n" +
                "\n" +
                "\t{s}, {s=60} Writing speed (chars per sec){/s}\n" +
                "\t{w}, {w=0.5} Wait (seconds)\n" +
                "\t{wi} Wait for input\n" +
                "\t{wc} Wait for input and clear\n" +
                "\t{wp}, {wp=0.5} Wait on punctuation (seconds){/wp}\n" +
                "\t{c} Clear\n" +
                "\t{x} Exit, advance to the next command without waiting for input\n" +
                "\n" +
                "\t{vpunch=10,0.5} Vertically punch screen (intensity,time)\n" +
                "\t{hpunch=10,0.5} Horizontally punch screen (intensity,time)\n" +
                "\t{punch=10,0.5} Punch screen (intensity,time)\n" +
                "\t{flash=0.5} Flash screen (duration)\n" +
                "\n" +
                "\t{audio=AudioObjectName} Play Audio Once\n" +
                "\t{audioloop=AudioObjectName} Play Audio Loop\n" +
                "\t{audiopause=AudioObjectName} Pause Audio\n" +
                "\t{audiostop=AudioObjectName} Stop Audio\n" +
                "\n" +
                "\t{m=MessageName} Broadcast message\n" +
                "\t{$VarName} Substitute variable";
        }

        protected virtual void AddWordsToken(List<TextTagToken> tokenList, string words)
        {
            TextTagToken token = new TextTagToken();
            token.type = TextTagToken.TokenType.Words;
            token.paramList = new List<string>(); 
            token.paramList.Add(words);
            tokenList.Add(token);
        }
        
        protected virtual void AddTagToken(List<TextTagToken> tokenList, string tagText)
        {
            if (tagText.Length < 3 ||
                tagText.Substring(0,1) != "{" ||
                tagText.Substring(tagText.Length - 1,1) != "}")
            {
                return;
            }
            
            string tag = tagText.Substring(1, tagText.Length - 2);
            
            var type = TextTagToken.TokenType.Invalid;
            List<string> parameters = ExtractParameters(tag);
            
            if (tag == "b")
            {
                type = TextTagToken.TokenType.BoldStart;
            }
            else if (tag == "/b")
            {
                type = TextTagToken.TokenType.BoldEnd;
            }
            else if (tag == "i")
            {
                type = TextTagToken.TokenType.ItalicStart;
            }
            else if (tag == "/i")
            {
                type = TextTagToken.TokenType.ItalicEnd;
            }
            else if (tag.StartsWith("color="))
            {
                type = TextTagToken.TokenType.ColorStart;
            }
            else if (tag == "/color")
            {
                type = TextTagToken.TokenType.ColorEnd;
            }
            else if (tag.StartsWith("size="))
            {
                type = TextTagToken.TokenType.SizeStart;
            }
            else if (tag == "/size")
            {
                type = TextTagToken.TokenType.SizeEnd;
            }
            else if (tag == "wi")
            {
                type = TextTagToken.TokenType.WaitForInputNoClear;
            }
            if (tag == "wc")
            {
                type = TextTagToken.TokenType.WaitForInputAndClear;
            }
            else if (tag.StartsWith("wp="))
            {
                type = TextTagToken.TokenType.WaitOnPunctuationStart;
            }
            else if (tag == "wp")
            {
                type = TextTagToken.TokenType.WaitOnPunctuationStart;
            }
            else if (tag == "/wp")
            {
                type = TextTagToken.TokenType.WaitOnPunctuationEnd;
            }
            else if (tag.StartsWith("w="))
            {
                type = TextTagToken.TokenType.Wait;
            }
            else if (tag == "w")
            {
                type = TextTagToken.TokenType.Wait;
            }
            else if (tag == "c")
            {
                type = TextTagToken.TokenType.Clear;
            }
            else if (tag.StartsWith("s="))
            {
                type = TextTagToken.TokenType.SpeedStart;
            }
            else if (tag == "s")
            {
                type = TextTagToken.TokenType.SpeedStart;
            }
            else if (tag == "/s")
            {
                type = TextTagToken.TokenType.SpeedEnd;
            }
            else if (tag == "x")
            {
                type = TextTagToken.TokenType.Exit;
            }
            else if (tag.StartsWith("m="))
            {
                type = TextTagToken.TokenType.Message;
            }
            else if (tag.StartsWith("vpunch") ||
                     tag.StartsWith("vpunch="))
            {
                type = TextTagToken.TokenType.VerticalPunch;
            }
            else if (tag.StartsWith("hpunch") ||
                     tag.StartsWith("hpunch="))
            {
                type = TextTagToken.TokenType.HorizontalPunch;
            }
            else if (tag.StartsWith("punch") ||
                     tag.StartsWith("punch="))
            {
                type = TextTagToken.TokenType.Punch;
            }
            else if (tag.StartsWith("flash") ||
                     tag.StartsWith("flash="))
            {
                type = TextTagToken.TokenType.Flash;
            }
            else if (tag.StartsWith("audio="))
            {
                type = TextTagToken.TokenType.Audio;
            }
            else if (tag.StartsWith("audioloop="))
            {
                type = TextTagToken.TokenType.AudioLoop;
            }
            else if (tag.StartsWith("audiopause="))
            {
                type = TextTagToken.TokenType.AudioPause;
            }
            else if (tag.StartsWith("audiostop="))
            {
                type = TextTagToken.TokenType.AudioStop;
            }
            
            if (type != TextTagToken.TokenType.Invalid)
            {
                TextTagToken token = new TextTagToken();
                token.type = type;
                token.paramList = parameters;           
                tokenList.Add(token);
            }
            else
            {
                Debug.LogWarning("Invalid text tag " + tag);
            }
        }

        protected virtual List<string> ExtractParameters(string input)
        {
            List<string> paramsList = new List<string>();
            int index = input.IndexOf('=');
            if (index == -1)
            {
                return paramsList;
            }

            string paramsStr = input.Substring(index + 1);
            var splits = paramsStr.Split(',');
            foreach (var p in splits) 
            {
                paramsList.Add(p.Trim());
            }
            return paramsList;
        }

        #region ITextTagParser implementation

        public virtual List<TextTagToken> Tokenize(string storyText)
        {
            List<TextTagToken> tokens = new List<TextTagToken>();

            string pattern = @"\{.*?\}";
            Regex myRegex = new Regex(pattern);

            Match m = myRegex.Match(storyText);   // m is the first match

            int position = 0;
            while (m.Success)
            {
                // Get bit leading up to tag
                string preText = storyText.Substring(position, m.Index - position);
                string tagText = m.Value;

                if (preText != "")
                {
                    AddWordsToken(tokens, preText);
                }
                AddTagToken(tokens, tagText);

                position = m.Index + tagText.Length;
                m = m.NextMatch();
            }

            if (position < storyText.Length)
            {
                string postText = storyText.Substring(position, storyText.Length - position);
                if (postText.Length > 0)
                {
                    AddWordsToken(tokens, postText);
                }
            }

            // Remove all leading whitespace & newlines after a {c} or {wc} tag
            // These characters are usually added for legibility when editing, but are not 
            // desireable when viewing the text in game.
            bool trimLeading = false;
            foreach (TextTagToken token in tokens)
            {
                if (trimLeading &&
                    token.type == TextTagToken.TokenType.Words)
                {
                    token.paramList[0] = token.paramList[0].TrimStart(' ', '\t', '\r', '\n');
                }

                if (token.type == TextTagToken.TokenType.Clear || 
                    token.type == TextTagToken.TokenType.WaitForInputAndClear)
                {
                    trimLeading = true;
                }
                else
                {
                    trimLeading = false;
                }
            }

            return tokens;
        }

        #endregion
    }    
}