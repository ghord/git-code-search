using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCodeSearch.Utilities
{
    public static class LineCounter
    {
        public static int GetCharacterIndex(string content, int line, int column)
        {
            var span = content.AsSpan();
            int currentColumn = 1;
            int currentLine = 1;

            for(int i=0; i<span.Length; i++)
            {
                if (currentColumn == column && currentLine == line)
                    return i;

                if (span[i] == '\r' && i < span.Length && span[i] == '\n')
                {
                    currentColumn = 1;
                    currentLine++;
                    i++;
                }
                else if(span[i] == '\n')
                {
                    currentColumn = 1;
                    currentLine++;
                }
                else
                {
                    currentColumn++;
                }

                
            }

            return -1;
        }
    }
}
