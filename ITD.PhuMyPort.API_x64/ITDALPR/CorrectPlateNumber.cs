using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace ITD.PhuMyPort.API.ITDALPR
{
public class CorrectPlateNumber
    {
        #region Fields

        private string _mRegisteredPlateNumber;
        private string _mImprovedPlateNumber;
        private string _mRecognationPlateNumber;
        private int _mTimeInNanoSeconds = 0;
        private List<string> _mListSeri = new List<string>();
        private Dictionary<string, string> _mListKeyMap = new Dictionary<string, string>();

        private List<string> normalSerials = new List<string>();

        #endregion

        #region Properties

        public string ImprovedPlateNumber
        {
            get { return _mImprovedPlateNumber; }
        }

        public int TimeInNanoSeconds
        {
            get { return _mTimeInNanoSeconds; }
        }

        #endregion

        #region Constructors

        public CorrectPlateNumber()
        {
            //process now
            InitResources();
            normalSerials.AddRange(new string[] { "LD", "NG", "QT", "NN", "DA" });
        }

        #endregion

        #region Methods

        public void CorrectNumber(string registeredPlateNumber, string recognationPlateNumber)
        {
            try
            {
                //Stopwatch st = new Stopwatch();
                //st.Start();
                _mRegisteredPlateNumber = string.IsNullOrEmpty(registeredPlateNumber) ? "" : registeredPlateNumber.ToUpper();
                _mRecognationPlateNumber = string.IsNullOrEmpty(recognationPlateNumber) ? "" : recognationPlateNumber.ToUpper();
                _mRecognationPlateNumber = _mRecognationPlateNumber.Replace("Å", "A");
                _mRecognationPlateNumber = _mRecognationPlateNumber.Replace("Š", "S");
                _mImprovedPlateNumber = ExtractDifferent(_mRegisteredPlateNumber, _mRecognationPlateNumber);
                //st.Stop();
                //_mTimeInNanoSeconds = (int) st.ElapsedMilliseconds; // * 1000000);
            }
            catch (Exception ex)
            {
                //TKUtils.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name,
                //    ex.ToString());
                _mImprovedPlateNumber = recognationPlateNumber;
            }
        }

        //init character key map to correctlly process
        private void InitResources()
        {
            XmlDocument xmlDoc;
            if (File.Exists("PlateCheckRule.xml"))
            {
                //loading here
                xmlDoc = new XmlDocument();
                xmlDoc.Load("PlateCheckRule.xml");
                foreach (XmlNode child in xmlDoc.GetElementsByTagName("dictionaryrule"))
                {
                    if (child.Attributes["name"].Value == "Seri")
                    {
                        foreach (XmlNode child1 in child.SelectNodes("code"))
                        {
                            _mListSeri.Add(child1.Attributes["value"].Value);
                        }
                        break;
                    }
                }
            }
            else
            {
                _mListSeri.AddRange(new string[]
                {
                    "LD", "NG", "QT", "NN", "AA", "AB", "AC", "AD", "AT", "AA", "AP", "BBB", "BC", "BH", "BK", "BL", "BT",
                    "BP", "BS", "BV", "HA", "HB", "HC", "HD", "HE", "HT", "HQ", "HN", "HH", "KA", "KB", "KC", "KD", "KV",
                    "KP", "KH", "KK", "KT", "KN", "PA", "PP", "PK", "PT", "PQ", "PX", "PC", "HL", "QA", "QK", "QP", "QB",
                    "QH", "TC", "TH", "TK", "TT", "TM", "TN", "DB", "ND", "CH", "VB", "VK", "VT"
                });
            }
            if (File.Exists("CharacterMapping.xml"))
            {
                //loading here
                xmlDoc = new XmlDocument();
                xmlDoc.Load("CharacterMapping.xml");
                foreach (XmlNode child in xmlDoc.GetElementsByTagName("key"))
                {
                    _mListKeyMap.Add(child.Attributes["value"].Value, child.InnerText);
                }
            }
            else
            {
                _mListKeyMap.Add("0", "D,U,Q");
                _mListKeyMap.Add("1", "L,J,T,I");
                _mListKeyMap.Add("2", "Z");
                _mListKeyMap.Add("3", "E");
                _mListKeyMap.Add("4", "A");
                _mListKeyMap.Add("5", "S");
                _mListKeyMap.Add("6", "C,G,O,B");
                _mListKeyMap.Add("7", "T,Y");
                _mListKeyMap.Add("8", "B,A,O");
                _mListKeyMap.Add("9", "B");
                _mListKeyMap.Add("A", "4");
                _mListKeyMap.Add("B", "8,6,0");
                _mListKeyMap.Add("C", "6");
                _mListKeyMap.Add("D", "0");
                _mListKeyMap.Add("E", "3");
                _mListKeyMap.Add("F", "5");
                _mListKeyMap.Add("G", "6");
                _mListKeyMap.Add("H", "4,7,1");
                _mListKeyMap.Add("I", "1");
                _mListKeyMap.Add("J", "1");
                _mListKeyMap.Add("O", "0,6,9,8");
                _mListKeyMap.Add("S", "5");
                _mListKeyMap.Add("T", "7");
                _mListKeyMap.Add("U", "0");
                _mListKeyMap.Add("Z", "2");
            }
        }

        //check recogplate with Viet Nam licence plate number format and correct basic issues //3 issues
        private int MatchTemplate(ref string inputPlate)
        {
            int iR = 0; //default passed
            Regex rg = new Regex("[1-9][0-9][A-Z]{1,2}[0-9]{4,5}\\d"); //Viet Nam licence plate number
            Regex rg2 = new Regex("\\d[A-Z]{2,2}\\d"); //checking seri //type 3
            Regex rg3 = new Regex("[0-9A-Z]{1,4}[0-9]{4,6}\\d"); //last number failed //type 1
            Regex rg4 = new Regex("[1-9][0-9][A-Z]{1,2}[0-9]{4,6}"); //province code failed //type 2
            Regex rg5 = new Regex("[1-9][A-Z]{1,4}[0-9]{4,6}"); // sai 2 ky tu
            Match match = rg.Match(inputPlate);
            if (match.Success)
            {
                Match match2 = rg2.Match(inputPlate);
                if (match2.Success)
                {
                    if (_mListSeri.Contains(inputPlate.Substring(2, 2)))
                    {
                        iR = 0;
                    }
                    else
                    {
                        iR = 3;
                    }
                }
            }
            else
            {
                //failed   
                if (rg3.Match(inputPlate).Success)
                {
                    iR = 0;
                    if (rg4.Match(inputPlate).Success)
                    {
                        iR = 0;
                    }
                    else
                    {
                        iR = 2;
                    }
                }
                else
                    iR = 1;
                if (iR > 0)// && string.IsNullOrEmpty(_mRegisteredPlateNumber)) //retry here
                {
                    char[] chartemp = inputPlate.ToCharArray();
                    if (rg5.Match(inputPlate).Success)
                    {
                        if ('A' <= inputPlate[1] && 'Z' >= inputPlate[1])
                        {
                            string s = inputPlate[1].ToString();
                            if (_mListKeyMap.Keys.Contains(s))
                                chartemp[1] = _mListKeyMap[s][0];
                        }
                    }

                    for (int i = 4; i < inputPlate.Length; i++)
                    {
                        if ('A' <= inputPlate[i] && 'Z' >= inputPlate[i])
                        {
                            string s = inputPlate[i].ToString();
                            if (_mListKeyMap.Keys.Contains(s))
                                chartemp[i] = _mListKeyMap[s][0];
                            else
                                chartemp[i] = '0';
                        }
                    }

                    if (chartemp.Length > 0)
                        inputPlate = new string(chartemp);


                    if (inputPlate.Length > 3 && !_mListSeri.Contains(inputPlate.Substring(2, 2)))
                    {
                        if ('A' <= inputPlate[3] && 'Z' >= inputPlate[3])
                        {
                            string s = inputPlate[3].ToString();
                            if (_mListKeyMap.Keys.Contains(s))
                                chartemp[3] = _mListKeyMap[s][0];
                        }

                        if ('A' >= inputPlate[2] && 'Z' <= inputPlate[2])// ky tu thu 2 la ky tu so
                        {
                            string s = inputPlate[2].ToString();
                            if (_mListKeyMap.Keys.Contains(s))
                                chartemp[2] = _mListKeyMap[s][0];
                        }

                        if (chartemp.Length > 0)
                            inputPlate = new string(chartemp);
                    }
                }
            }
            return iR;
        }
        bool compareSubSame(Dictionary<int, string> sameSubs, Dictionary<int, string> sameSubs1)
        {
            if (sameSubs.Count == sameSubs1.Count)
            {
                var subSort = verifySameSub(sameSubs);
                var subSort2 = verifySameSub(sameSubs1);
                for (int i = 0; i < subSort.ToList().Count; i++)
                {
                    if ((subSort.ToList()[i].Key != subSort2.ToList()[i].Key) || (subSort.ToList()[i].Value != subSort2.ToList()[i].Value))
                    {
                        //if (i == subSort.ToList().Count - 1 && (subSort.ToList()[i].Value.EndsWith(subSort2.ToList()[i].Value) || subSort2.ToList()[i].Value.EndsWith(subSort.ToList()[i].Value)))
                        //continue;
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        List<KeyValuePair<int, string>> verifySameSub(Dictionary<int, string> sameSubs)
        {
        a:
            var subSort = from pair in sameSubs
                          orderby pair.Key
                          select pair;
            int or = 0;
            for (int i = 0; i < subSort.ToList().Count; i++)
            {
                if (or != 0 && or >= subSort.ToList()[i].Key)//falied 
                {
                    if (subSort.ToList()[i].Key <= or)
                    {
                        var newKey = subSort.ToList()[i].Key + 1 + (or - subSort.ToList()[i].Key);
                        if (!sameSubs.ContainsKey(newKey))
                            sameSubs.Add(newKey, subSort.ToList()[i].Value.Substring(1, subSort.ToList()[i].Value.Length - 1));
                        sameSubs.Remove(subSort.ToList()[i].Key);
                        goto a;
                    }
                    break;
                }
                else
                {
                    or = subSort.ToList()[i].Key + sameSubs[subSort.ToList()[i].Key].Length - 1;
                }
            }
            return subSort.ToList();
        }
        //extrac different postion in registerd number and recognation number
        public string ExtractDifferent(string registeredNumber, string recognationNumber)
        {
            string result = recognationNumber;
            //same size
            int iR = MatchTemplate(ref recognationNumber);

            if (string.IsNullOrEmpty(registeredNumber))
            {
                return recognationNumber;
            }
            Dictionary<int, string> sameSubs = getSubs(recognationNumber, registeredNumber);//recog with regis
            Dictionary<int, string> sameSubs1 = getSubs(registeredNumber, recognationNumber);//regis with recog

            if (compareSubSame(sameSubs, sameSubs1))
            {
            a:
                var subSort = from pair in sameSubs
                              orderby pair.Key
                              select pair;
                //verify order
                int or = 0; bool invalid = false;
                for (int i = 0; i < subSort.ToList().Count; i++)
                {
                    if (or != 0 && or >= subSort.ToList()[i].Key)//falied 
                    {
                        if (subSort.ToList()[i].Key <= or)
                        {
                            var newKey = subSort.ToList()[i].Key + 1 + (or - subSort.ToList()[i].Key);
                            if (!sameSubs.ContainsKey(newKey))
                                sameSubs.Add(newKey, subSort.ToList()[i].Value.Substring(1, subSort.ToList()[i].Value.Length - 1));
                            sameSubs.Remove(subSort.ToList()[i].Key);
                            goto a;
                        }
                        invalid = true;
                        break;
                    }
                    else
                    {
                        or = subSort.ToList()[i].Key + sameSubs[subSort.ToList()[i].Key].Length - 1;
                    }
                }
                //verify order in regis
                var subSort2 = from pair in sameSubs1
                               orderby pair.Key
                               select pair;
            b:
                int or1 = 0;
                for (int i = 0; i < subSort2.ToList().Count; i++)
                {
                    if (or1 != 0 && or1 >= subSort2.ToList()[i].Key)//falied 
                    {
                        if (subSort2.ToList()[i].Key <= or1)
                        {
                            var newKey = subSort2.ToList()[i].Key + 1 + (or1 - subSort2.ToList()[i].Key);
                            if (!sameSubs1.ContainsKey(newKey))
                                sameSubs1.Add(newKey, subSort2.ToList()[i].Value.Substring(1, subSort2.ToList()[i].Value.Length - 1));
                            sameSubs1.Remove(subSort2.ToList()[i].Key);
                            goto b;
                        }
                        invalid = true;
                        break;
                    }
                    else
                    {
                        or1 = subSort2.ToList()[i].Key + sameSubs1[subSort2.ToList()[i].Key].Length - 1;
                    }
                }

                if (!invalid)
                {
                    var listSubs = from pair in sameSubs
                                   orderby pair.Value.Length descending
                                   select pair;
                    Dictionary<int, int> dicSubs = new Dictionary<int, int>();
                    foreach (KeyValuePair<int, string> item in listSubs)
                    {
                        dicSubs.Add(item.Key, item.Value.Length);
                    }

                    int iCount1 = 0, iCount2 = 0, iCount3 = 0, iCount4 = 0, iCount5 = 0, iCount6 = 0;

                    bool bPassed = false;
                    foreach (KeyValuePair<int, int> item in dicSubs)
                    {
                        //passed length
                        //regis length 9
                        if (item.Value > 5 && item.Value >= (registeredNumber.Length * 9 / 10) && registeredNumber.Length == 9 && recognationNumber.Length >= 8)
                        {
                            bPassed = true;
                            break;
                        }
                        //regis length 8
                        if (item.Value > 4 && item.Value >= (registeredNumber.Length * 9 / 10) && registeredNumber.Length == 8 && recognationNumber.Length >= 7)
                        {
                            bPassed = true;
                            break;
                        }
                        //regis length 7
                        if (item.Value > 3 && item.Value >= (registeredNumber.Length * 9 / 10) && registeredNumber.Length == 7 && recognationNumber.Length >= 6)
                        {
                            bPassed = true;
                            break;
                        }
                        switch (item.Value)
                        {
                            case 6:
                                iCount6 += 1;
                                break; //length=6
                            case 5:
                                iCount5 += 1;
                                break; //length=5
                            case 4:
                                iCount4 += 1;
                                break; //length=4
                            case 3:
                                iCount3 += 1;
                                break; //length=3
                            case 2:
                                iCount2 += 1;
                                break; //length=2
                            case 1:
                                iCount1 += 1;
                                break; //length=1
                        }
                        if ((iCount6 >= 1 && (iCount1 >= 1 || iCount2 >= 1)) ||
                            (iCount5 >= 1 && (iCount3 >= 1 || iCount2 >= 1 || iCount1 >= 2)) || //length=5
                            (iCount4 >= 2 || (iCount4 >= 1 && iCount3 >= 1)) ||
                            (iCount4 >= 1 && iCount2 >= 1 && dicSubs.Count >= 3 && registeredNumber.Length == 7) || //length =4 and length=2 and length 3
                            (iCount4 >= 1 && iCount3 >= 1) ||
                           //(iCount4 >= 1 && (iCount2 >= 1 || dicSubs.Count >= 2)) ||//leng
                           //(iCount3 >= 2) || (iCount3 == 1 && dicSubs.Keys.Count >= 3) || //length =3
                           (iCount3 >= 2 && (dicSubs.Keys.Count >= 3 || registeredNumber.Length == 7)) || //length =3
                                                                                                          // (iCount2 >= 2) || (iCount2 == 2 && dicSubs.Keys.Count >= 4) || //length =2
                            (iCount2 >= 3 && dicSubs.Keys.Count >= 4)) //length =2
                                                                       //(iCount1 >= registeredNumber.Length / 2 + 1)) //length =1
                        {
                            bPassed = true;
                            break;
                        }
                    }
                    if (bPassed)
                        result = registeredNumber; //passed to replace
                }
            }
            return result;
        }
        //get list of subs string same postion in to string 
        private Dictionary<int, string> getSubs(string recognationNumber, string registeredNumber)
        {
            Dictionary<int, string> listSubs = new Dictionary<int, string>();
            // Avoid full length.
            for (int length = 1; length <= recognationNumber.Length; length++)
            {
                // End index is tricky.
                for (int start = 0; start <= recognationNumber.Length - length; start++)
                {
                    string substring = recognationNumber.Substring(start, length);
                    if (registeredNumber.Contains(substring))
                    {
                        int index = registeredNumber.IndexOf(substring);
                        int indexCurrent = start;
                        int lastindex = registeredNumber.LastIndexOf(substring);
                        List<int> removeIndex = new List<int>();
                        for (int i = 0; i < listSubs.Keys.Count; i++)
                        {
                            int key = listSubs.Keys.ElementAt(i);
                            string sub = listSubs.Values.ElementAt(i);
                            if (key >= index && ((sub.Length + key - 1) <= (substring.Length + index - 1)) &&
                                sub.Length < substring.Length)
                            {
                                removeIndex.Add(key);
                            }
                        }
                        foreach (int key in removeIndex)
                        {
                            listSubs.Remove(key);
                        }
                        if (!listSubs.Keys.Contains(index))
                            listSubs.Add(index, substring);
                        else if (lastindex != index && !listSubs.Keys.Contains(lastindex))
                        {
                            listSubs.Add(lastindex, substring);
                        }
                    }
                }
            }
            return listSubs;
        }
        public int OptimizeRecogPlate(ref string inputPlate)
        {
            int ret = -1;
            Regex formatNormal1char = new Regex("^[1-9][0-9][A-Z]{1,1}[0-9]{4,5}\\d"); // format bien so dan su co 2 ky tu chu (length = 7,8)
            Regex formatNormal1_1char = new Regex("^[1-9][0-9][A-Z]{1,1}[0-9]{1,4}\\d"); // format bien so dan su co 2 ky tu chu (length = 7,8), bi thieu cac ky tu cuoi
            Regex formatNormal1_2char = new Regex("[A-Z]{1,1}[0-9]{1,4}\\d"); // format bien so dan su co 2 ky tu chu (length = 7,8), thieu mot so ky tu cuoi, hoac 1,2 ky tu dau
            Regex formatNormal2char = new Regex("^[1-9][0-9][A-Z]{2,2}[0-9]{4,5}\\d"); // format bien so dan su co 2 ky tu chu(length = 9)
            Regex formatNormal2_2char = new Regex("^[1-9][0-9][A-Z]{2,2}[0-9]{2,4}\\d"); // format bien so dan su co 2 ky tu chu(length = 9), nhung bi thieu mot so ky tu 1->3 ky tu cuoi
            Regex formatNormal2_3char = new Regex("[A-Z]{2,2}[0-9]{2,4}\\d"); // format bien so dan su co 2 ky tu chu(length = 9), nhung bi thieu mot so ky tu 1->3 ky tu cuoi, va 1 hoac 2 ky tu dau
            Regex formatNational = new Regex("^[1-9][0-9]{4,4}(NN|QT|NG){1,1}[0-9]{2,2}");

            char[] chartemp = inputPlate.ToCharArray();

            if (inputPlate.Length == 9)
            {
                Match match = formatNormal2char.Match(inputPlate);
                if (match.Success)
                {
                    ret = 0;
                }
                else if (formatNational.IsMatch(inputPlate))
                {
                    ret = 0;
                }
                else
                {
                    // Fixed 2 ky tu truoc seri
                    for (int i = 0; i < 2; i++)
                    {
                        if ('A' <= inputPlate[i] && 'Z' >= inputPlate[i])
                        {
                            string s = inputPlate[i].ToString();
                            if (_mListKeyMap.Keys.Contains(s))
                                chartemp[i] = _mListKeyMap[s][0];
                            else
                                chartemp[i] = '1';
                        }
                    }
                    // Fixed cac ky tu sau seri
                    for (int i = 4; i < inputPlate.Length; i++)
                    {
                        if ('A' <= inputPlate[i] && 'Z' >= inputPlate[i])
                        {
                            string s = inputPlate[i].ToString();
                            if (_mListKeyMap.Keys.Contains(s))
                                chartemp[i] = _mListKeyMap[s][0];
                            else
                                chartemp[i] = '0';
                        }
                    }
                    if (!normalSerials.Contains(inputPlate.Substring(2, 2)))
                    {
                        if (inputPlate[2] == 'L' || inputPlate[3] == 'D')
                        {
                            chartemp[2] = 'L';
                            chartemp[3] = 'D';
                        }
                        else if (inputPlate[2] == 'N' || inputPlate[3] == 'N')
                        {
                            chartemp[2] = 'N';
                            chartemp[3] = 'N';
                        }
                        else if (inputPlate[2] == 'Q' || inputPlate[3] == 'T')
                        {
                            chartemp[2] = 'Q';
                            chartemp[3] = 'T';
                        }
                        else if (inputPlate[2] == 'N' || inputPlate[3] == 'G')
                        {
                            chartemp[2] = 'N';
                            chartemp[3] = 'G';
                        }
                        else if (inputPlate[2] == 'D' || inputPlate[3] == 'A')
                        {
                            chartemp[2] = 'D';
                            chartemp[3] = 'A';
                        }
                        else
                        {
                            for (int i = 2; i < 4; i++)
                            {
                                if ('0' <= inputPlate[i] && '9' >= inputPlate[i])
                                {
                                    string s = inputPlate[i].ToString();
                                    if (_mListKeyMap.Keys.Contains(s))
                                        chartemp[i] = _mListKeyMap[s][0];
                                    else
                                        chartemp[i] = 'L';
                                }
                            }
                        }
                    }

                    ret = 2;
                }
            }
            else if (inputPlate.Length == 8 || inputPlate.Length == 7)
            {
                Match match = formatNormal1char.Match(inputPlate);
                if (match.Success)
                {
                    ret = 0;
                }
                else if (formatNormal2_2char.IsMatch(inputPlate))
                {
                    ret = 1;
                }
                else
                {
                    // Fixed 2 ky tu truoc seri
                    for (int i = 0; i < 2; i++)
                    {
                        if ('A' <= inputPlate[i] && 'Z' >= inputPlate[i])
                        {
                            string s = inputPlate[i].ToString();
                            if (_mListKeyMap.Keys.Contains(s))
                                chartemp[i] = _mListKeyMap[s][0];
                            else
                                chartemp[i] = '1';
                        }
                    }


                    // Edit ky tu thu 3
                    if (inputPlate[2] >= '0' && inputPlate[2] <= '9')
                    {
                        string s = inputPlate[2].ToString();
                        if (_mListKeyMap.Keys.Contains(s))
                            chartemp[2] = _mListKeyMap[s][0];
                        else
                            chartemp[2] = 'A';
                    }

                    // Fixed cac ky tu sau seri
                    int offset = 3;
                    if ('A' <= inputPlate[3] && 'Z' >= inputPlate[3]) // bien so co 9 ky tu ma bi mat ky tu cuoi
                    {
                        offset = 3;
                    }
                    else
                    {
                        offset = 4;
                    }

                    for (int i = offset; i < inputPlate.Length; i++)
                    {
                        if ('A' <= inputPlate[i] && 'Z' >= inputPlate[i])
                        {
                            string s = inputPlate[i].ToString();
                            if (_mListKeyMap.Keys.Contains(s))
                                chartemp[i] = _mListKeyMap[s][0];
                            else
                                chartemp[i] = '0';
                        }
                    }

                    ret = 2;
                }
            }
            else if (inputPlate.Length < 7)
            {
                if (formatNormal1_1char.IsMatch(inputPlate))
                {
                    ret = 1;
                }
                else
                {

                }
            }

            if (chartemp.Length > 0)
                inputPlate = new string(chartemp);
            return ret;
        }
        /// <summary>
        /// Chon nhung ket qua gan giong bien so xe
        /// </summary>
        /// <param name="listResult"></param>
        /// <returns></returns>
        public List<string> SelectRecogResult(List<string> listResult)
        {
            if (listResult == null || listResult.Count == 0)
            {
                return listResult;
            }

            // 5 ky ty I,1 lien tiep thi remove
            Regex rg1 = new Regex("[1,I]{5,}");

            List<string> ls = listResult.Where(s => rg1.IsMatch(s)).ToList();
            foreach (var l in ls)
            {
                listResult.Remove(l);
            }

            //remove 2 ky tu II
            ls = listResult.Where(s => s.Contains("II")).ToList();
            foreach (var l in ls)
            {
                listResult.Remove(l);
            }

            // remove cac ket qua co nhieu hon 3 ky tu chu
            ls.Clear();
            foreach (var l in listResult)
            {
                int num = getNumofChar(l);
                if (num > 3)
                {
                    ls.Add(l);
                }
            }

            foreach (var l in ls)
            {
                listResult.Remove(l);
            }

            // remove cac ket qua length <6 va > 9
            ls.Clear();
            foreach (var l in listResult)
            {
                if (l.Length < 6 || l.Length > 9)
                {
                    ls.Add(l);
                }
            }

            foreach (var l in ls)
            {
                listResult.Remove(l);
            }

            return listResult;

        }
        private int getNumofChar(string plate)
        {
            if (string.IsNullOrEmpty(plate))
            {
                return 0;
            }

            int ret = 0;

            for (int i = 0; i < plate.Length; i++)
            {
                if ('A' <= plate[i] && 'Z' >= plate[i])
                {
                    ret++;
                }
            }

            return ret;
        }
        #endregion
    }
}
