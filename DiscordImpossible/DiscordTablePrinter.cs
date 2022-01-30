using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordImpossible
{
    public static class DiscordTablePrinter
    {
        public static string PrintAsDiscordTable(this DataTable dataTable)
        {
            /*
             +---------+---------------+---------------------+---------------------+--------------------------------------+
             | Channel | Author        | Expiry Time         | Original Message ID | Response Message ID                  |
             +---------+---------------+---------------------+---------------------+--------------------------------------+
             | 1       | cmjchrisjones | 25/01/2022 01:55:22 | 935352066468110408  | 37b5efda-dfbc-47aa-9e02-7f227b4f0f1c |
             +---------+---------------+---------------------+---------------------+--------------------------------------+


             */

            var sb = new StringBuilder();
            int col1Width = 0;
            int col2Width = 0;
            int col3Width = 0;
            int col4Width = 0;
            int col5Width = 0;
            int col6Width = 0;

            // find the longest text in each column so we can like everything up

            foreach (DataRow row in dataTable.Rows)
            {
                if (row["Channel"].ToString().Length > col1Width)
                {
                    col1Width = row["Channel"].ToString().Length;
                }
                if (row["Author"].ToString().Length > col2Width)
                {
                    col2Width = row["Author"].ToString().Length;
                }
                if (row["Expiry Time"].ToString().Length > col3Width)
                {
                    col3Width = row["Expiry Time"].ToString().Length;
                }
                if (row["Original Message ID"].ToString().Length > col4Width)
                {
                    col4Width = row["Original Message ID"].ToString().Length;
                }
                if (row["Response Message ID"].ToString().Length > col6Width)
                {
                    col6Width = row["Response Message ID"].ToString().Length;
                }

                if (row["Original Message ID"].ToString().Length > col5Width)
                {
                    col5Width = row["Original Message ID"].ToString().Length;
                }
            }

            // Print the column headings
            sb.AppendLine(
                $"+{"-".RepeatCharacter(col1Width + 2)}" +
                    $"+{"-".RepeatCharacter(col2Width + 2)}" +
                    $"+{"-".RepeatCharacter(col3Width + 2)}" +
                    $"+{"-".RepeatCharacter(col4Width + 2)}" +
                    $"+{"-".RepeatCharacter(col5Width + 2)}" +
                    $"+{"-".RepeatCharacter(col6Width + 2)} +");

            // add the items
            foreach(DataRow row in dataTable.Rows)
            {
                sb.AppendLine(
                    $"| {row["Channel"].ToString()}  |" +
                        $"  {row["Author"].ToString()}  |" +
                        $"  {row["Expiry Time"].ToString()}  |" +
                        $"  {row["Original Message ID"].ToString()}  |" +
                        $"  {row["Response Message ID"].ToString()}  |" +
                        $"  {row["Tracking GUID"].ToString()}  |");
            }


            sb.AppendLine(
                $"+{"-".RepeatCharacter(col1Width + 2)}" +
                    $"+{"-".RepeatCharacter(col2Width + 2)}" +
                    $"+{"-".RepeatCharacter(col3Width + 2)}" +
                    $"+{"-".RepeatCharacter(col4Width + 2)}" +
                    $"+{"-".RepeatCharacter(col5Width + 2)}" +
                    $"+{"-".RepeatCharacter(col6Width + 2)} +");

            // return the string

            return sb.ToString();
        }

        public static string RepeatCharacter(this string s, int chars)
        {
            var sb = new StringBuilder();
            for(int i = 0; i < chars; i++)
            {
                sb.Append(s);
            }
            return sb.ToString();
        }
    }
}
