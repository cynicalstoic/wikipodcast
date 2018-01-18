using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace wikicast
{
    class Program
    {
        static void Main(string[] args)
        {
            string workingdirectory = "c:\\users\\swaddy\\Desktop\\wikicast\\";
            string outputdirectory = "c:\\users\\swaddy\\Desktop\\wikicast\\output";
            string fileLocation = "c:\\users\\swaddy\\Desktop\\wikicast\\text.html";
            string dataFile = "c:\\users\\swaddy\\Desktop\\wikicast\\data.log";
            string newsFile = "c:\\users\\swaddy\\Desktop\\wikicast\\dayNews.html";
            string newsTextPath = "c:\\users\\swaddy\\Desktop\\wikicast\\";
            string newsTextFileName = "-NewsText.txt";
            
            string url = "https://en.wikipedia.org/wiki/Portal:Current_events";
            string prev1date = FormatDate(-1);
            string prev2date = FormatDate(-2);
            string newsTextFile = newsTextPath+prev1date+newsTextFileName;
            string HTMLRaw = GetWebContents(url);
            string dayNews = "";
            News yesterdayNews = new News(prev1date, workingdirectory, outputdirectory);
            WriteFile(fileLocation, HTMLRaw,false);
            WriteFile(dataFile, "Yesterday: "+ prev1date, false);
            WriteFile(dataFile, "Two days ago: "+prev2date, true);
            dayNews = GetDayNews(HTMLRaw, prev1date, prev2date);
            WriteFile(newsFile, dayNews,false);
            WriteFile(newsTextFile, SimplifyNews(dayNews),false);
        }
        static string GetWebContents(string url)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            byte[] raw = wc.DownloadData(url);

            string webData = System.Text.Encoding.UTF8.GetString(raw);
            return webData;
        }
        static void WriteFile (string fl, string cont, bool append)
        {
            if (append)
                File.AppendAllText(fl, "\r\n"+cont);
            else
                File.WriteAllText(fl, cont);
        }
        static string FormatDate(int i)
        {
            var cDate = DateTime.Today.AddDays(i);
            return cDate.ToString("yyyy_MMMM_dd");
        }
        static string GetDayNews(string html, string newsStartDate, string newsEndDate)
        {
            int startpos = html.IndexOf("id=\"" + newsStartDate);
            int endpos = html.IndexOf("id=\"" + newsEndDate);
            string curtext = html.Substring(startpos, endpos - startpos);
            return curtext.Substring(curtext.IndexOf("<"));
            //return "id = \"" + newsStartDate+"\r\nstartpos:\t" + startpos.ToString() + "\r\nendpos:\t" + endpos.ToString();
        }
        static string SimplifyNews(string cont)
        {
            string news = "";
            string[] array = cont.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string newssource = "";
            string contentline = "";
            foreach (string s in array)
            {

                switch(GetLineType(s))
                {
                    case "Top":
                        {
                            contentline = StripTagsRegex(s);
                            news = news + "\r\n" + contentline;
                            break;
                        }
                    case "Header":
                        {
                            contentline = StripTagsRegex(s);
                            news = news + "\r\n\r\nIn " + contentline;
                            break;
                        }
                    case "Brief":
                        {
                            newssource = GetNewsSource(s);
                            contentline = StripSource(StripTagsRegex(s));
                            if(contentline.IndexOf(" ()") > -1)
                            {
                                contentline = contentline.Substring(0, contentline.Length - 3);
                            }
                            news = news + "\r\n  " + newssource + contentline;
                            break;
                        }
                    case "Junk":
                        {
                            break;
                        }
                }
  /*              if (GetLineType(s)=="Top")
                {
                    news = news + "\r\n[@In]@" + StripTagsRegex(s);
                }
                else if (s.IndexOf("<li>") != -1)
                {
                    news = news + "\r\n" + StripTagsRegex(s);
                }
   */
            }
            return news;
        }
        static string GetLineType(string source)
        {
            if (source.IndexOf("<div role=\"heading\"") != -1)
            {
                if (source.IndexOf("Current events of") != -1)
                    return "Top";
                else
                    return "Header";
            }
            else if (source.IndexOf("<li>") != -1)
            {
                return "Brief";
            }
            else return "Style";
                

            //Current events of
        }
        static string StripTagsRegex(string source)
        {
            return Regex.Replace(source.Replace("&#160;", " "), "<.*?>", string.Empty);
        }
        static string GetNewsSource(string source)
        {
            string pattern = @"(?<=\().+?(?=\))";
            Match result = Regex.Match(source, pattern);
            string returnval = "";
            if (result.Success)
            {

                returnval = "  From "+ StripTagsRegex(result.Value)+":";
            }
            
            return returnval;
        }

        static string StripSource(string source)
        {
            string pattern = @"(?<=\().+?(?=\))";
            return Regex.Replace(source, pattern, string.Empty);
        }
    }
}
