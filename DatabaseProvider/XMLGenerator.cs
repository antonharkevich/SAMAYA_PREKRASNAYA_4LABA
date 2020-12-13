using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{

    interface IXMLGenerator
    {
        string ConvertTableToXMLString(string tableName, List<List<KeyValuePair<string, object>>> rows);
    }
    class XMLGenerator : IXMLGenerator
    {
        public string ConvertTableToXMLString(string table, List<List<KeyValuePair<string, object>>> rows)
        {
            StringBuilder Stringbuilder = new StringBuilder();

            Stringbuilder.Append($"<{table}>\n");

            foreach (var row in rows)
            {
                Stringbuilder.Append($"<tablerow ");

                foreach (var pair in row)
                {
                    Stringbuilder.Append($"{pair.Key}=\"{pair.Value.ToString()}\" ");
                }

                Stringbuilder.Append($"/>\n");
            }

            Stringbuilder.Append($"<{table}/>");

            return Stringbuilder.ToString();
        }
    }
}