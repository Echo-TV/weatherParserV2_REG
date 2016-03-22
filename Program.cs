using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Net;
using System.Xml;
using System.Xml.XPath;
using System.Globalization;

namespace consolegw2
{
    class Program
    {
        static void Swap<T>(ref T x, ref T y)
        {
            T tempVariable = x;
            x = y;
            y = tempVariable;
        }

        static void ConsoleExceptionAlert(string caution, Exception exception)
        {
            ConsoleColor currentConsoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(caution);
            Console.WriteLine(exception.Message);
            Console.WriteLine();
            Console.WriteLine("Sorry, application will be closed!");
            Console.WriteLine("Please, press ENTER.");
            Console.ForegroundColor = currentConsoleColor;
            Console.ReadLine();
        }

        static void ConsoleErrorAlert(string caution)
        {
            ConsoleColor currentConsoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(caution);
            Console.WriteLine();
            Console.WriteLine("Sorry, application will be closed!");
            Console.WriteLine("Please, press ENTER.");
            Console.ForegroundColor = currentConsoleColor;
            Console.ReadLine();
        }

        static void ConsoleWarningAlert(string caution)
        {
            ConsoleColor currentConsoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(caution);
            Console.WriteLine("Please, press ENTER.");
            Console.ForegroundColor = currentConsoleColor;
            Console.ReadLine();
        }
        static void ConsoleWarningAlert(List <string> caution)
        {
            ConsoleColor currentConsoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (string caution_part in caution) Console.WriteLine(caution);
            Console.WriteLine("Please, press ENTER.");
            Console.ForegroundColor = currentConsoleColor;
            Console.ReadLine();
        }

        static string MakeTemperatureString(string temperature, int left_shift, int right_shift)
        {
            string from_t = string.Empty;
            string to_t = string.Empty;

            int t = Convert.ToInt32(temperature);
            int t1 = t - left_shift;
            int t2 = t + right_shift;

            //Rule +5
            if (t1 > 0)
            {
                from_t = "+" + t1;
            }
            else
            {
                from_t = t1.ToString();
            }

            //Rule +5
            if (t2 > 0)
            {
                to_t = "+" + t2;
            }
            else
            {
                to_t = t2.ToString();
            }

            //Rule 0...-5/-3...-5
            if (t1 < 0 && t2 <= 0) Swap(ref from_t, ref to_t);

            return from_t + "°..." + to_t + "°";
        }

        static string MakeTemperatureString(string from_temperature, string to_temperature, int left_shift, int right_shift)
        {
            int maxRange = 4;
            if (Math.Abs(Convert.ToInt16(from_temperature) - Convert.ToInt16(to_temperature)) == 1)
            {
                from_temperature = Convert.ToString(Math.Min(Convert.ToInt16(from_temperature), Convert.ToInt16(to_temperature)) - left_shift);
                to_temperature = Convert.ToString(Math.Max(Convert.ToInt16(from_temperature), Convert.ToInt16(to_temperature)) + 1 + right_shift);
            }
            else if (Math.Abs(Convert.ToInt16(from_temperature) - Convert.ToInt16(to_temperature)) > maxRange)
            {
                decimal t = 0;
                t = (Convert.ToInt16(from_temperature) + Convert.ToInt16(to_temperature)) / 2;
                from_temperature = (Math.Ceiling(t - maxRange / 2) - left_shift).ToString();
                to_temperature = (Math.Ceiling(t + maxRange / 2) + right_shift).ToString();
            }
            else
            {
                from_temperature = Convert.ToString(Convert.ToInt16(from_temperature) - left_shift);
                to_temperature = Convert.ToString(Convert.ToInt16(to_temperature) + right_shift);
            }

            // Rule +5
            if (Convert.ToInt16(from_temperature) > 0)
            {
                from_temperature = "+" + from_temperature;
            }

            // Rule +5
            if (Convert.ToInt16(to_temperature) > 0)
            {
                to_temperature = "+" + to_temperature;
            }

            // Rule 0...-5/-3...-5
            if (Convert.ToInt16(from_temperature) < 0 && Convert.ToInt16(to_temperature) <= 0) Swap(ref from_temperature, ref to_temperature);

            return from_temperature + "°..." + to_temperature + "°";
        }

        static void Main(string[] args)
        {
            //MakeTemperatureString Tests
            //Console.WriteLine(MakeTemperatureString("2", 1, 1));
            //Console.WriteLine(MakeTemperatureString("2", 3, 0));
            //Console.WriteLine(MakeTemperatureString("1", "8", 0, 0));
            //Console.WriteLine(MakeTemperatureString("1", "8", 2, -1));
            //Console.ReadLine();
            //return;

            //CONSOLE APPLICATION SETTINGS
            Console.ForegroundColor = ConsoleColor.White;

            //READ CONFIGURATION FILE 
            string CONFIG_FILE = "Settings.ini";
            string line = string.Empty;
            int equalPosition;
            string section = string.Empty;
            string key;
            string value;
            List<string> errorMessagesList = new List<string>();

            Dictionary<string, string> iniFileValues = new Dictionary<string, string>();

            if (File.Exists(CONFIG_FILE))
            {
                Console.WriteLine("Read configuration file ...");

                int i = 0;
                StreamReader configFile = new StreamReader(CONFIG_FILE);
                while (!configFile.EndOfStream)
                {
                    do
                    {
                        line = configFile.ReadLine().Trim();
                        i++;

                    } while (line == string.Empty && !configFile.EndOfStream);

                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        section = line.Substring(1, line.Length - 2);
                        Console.WriteLine("{0}: [{1}]", i, section);
                    }
                    else if (section == string.Empty)
                    {
                        errorMessagesList.Clear();
                        errorMessagesList.Add(i + ": " + line);
                        errorMessagesList.Add("Syntax error: section header not found.");
                        ConsoleWarningAlert(errorMessagesList);
                    }
                    else if (line != string.Empty)
                    {
                        equalPosition = line.IndexOf("=");
                        switch (equalPosition)
                        {
                            case -1:
                                errorMessagesList.Clear();
                                errorMessagesList.Add(i + ": " + line);
                                errorMessagesList.Add("Syntax error: equal sign is skiped.");
                                ConsoleWarningAlert(errorMessagesList);
                                break;

                            case 0:
                                errorMessagesList.Clear();
                                errorMessagesList.Add(i + ": " + line);
                                errorMessagesList.Add("Syntax error: key is skiped.");
                                ConsoleWarningAlert(errorMessagesList);
                                break;

                            default:
                                key = line.Substring(0, equalPosition).Trim();
                                value = line.Substring(equalPosition + 1, line.Length - equalPosition - 1).Trim();
                                Console.WriteLine("{0}: {1} = {2}", i, key, value);
                                try
                                {
                                    iniFileValues.Add(section + "." + key, value);
                                }
                                catch (Exception exception)
                                {
                                    ConsoleWarningAlert(exception.Message);
                                }
                                break;
                        }
                    }
                }
            }
            else
            {
                ConsoleErrorAlert(CONFIG_FILE + " not found.");
                return;
            }
            Console.WriteLine("{0} reading is finished.", CONFIG_FILE);
            Console.WriteLine();


            //APPLY SETTINGS
            int NUMBER_OF_DAYS;
            string FILE_NAME_PREFIX;
            string OUTPUT_DIRECTORY;
            string DICTIONARY_FILE_NAME;

            Console.WriteLine("Apply settings ...");
            string errorMessage = "";
            try
            {
                errorMessage = "'[GLOBAL] Number of days'";
                NUMBER_OF_DAYS = Convert.ToInt16(iniFileValues["GLOBAL.Number of days"]);
                errorMessage = "'[WDATA] File name prefix'";
                FILE_NAME_PREFIX = iniFileValues["WDATA.File name prefix"];
                errorMessage = "'[WDATA] Output Path'";
                OUTPUT_DIRECTORY = iniFileValues["WDATA.Output path"];
                errorMessage = "'[DICTIONARY] File name'";
                DICTIONARY_FILE_NAME = iniFileValues["DICTIONARY.File name"];
            }

            catch
            {
                errorMessage += " setting error!";
                ConsoleErrorAlert(errorMessage);
                return;
            }

            Console.WriteLine("Settings apply successfuly.");
            Console.WriteLine();



            //READ CONFIGURATION FILE 
            Dictionary<string, string> NominativeToGenetive = new Dictionary<string, string>()
            {
                {"январь", "января"},
                {"февраль", "февраля"},
                {"март", "марта"},
                {"апрель", "апреля"},
                {"май", "мая"},
                {"июнь", "июня"},
                {"июль", "июля"},
                {"август", "августа"},
                {"сентябрь", "сентября"},
                {"октябрь", "октября"},
                {"ноябрь", "ноября"},
                {"декабрь", "декабря"}
            };

            //DICTIONARIES CREATION
            Dictionary<string, string> dictionary_wind_direction = new Dictionary<string, string>();
            Dictionary<string, string> dictionary_wind_direction_charaster = new Dictionary<string, string>();
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            Dictionary<string, string> dictionary_day = new Dictionary<string, string>();
            Dictionary<string, string> dictionary_night = new Dictionary<string, string>();

            string wind_direction_charaster = string.Empty;
            string wind_direction = string.Empty;
            string user_defenition = string.Empty;
            string day_charaster = string.Empty;
            string night_charaster = string.Empty;
            string space = string.Empty;

            try
            {
                Console.WriteLine("Read dictionary file ...");
                Console.WriteLine();

                StreamReader dictionaryFile = new StreamReader(DICTIONARY_FILE_NAME);

                //WIND_CHAPTER
                Console.WriteLine("Wind chapter");
                for (int i = 1; i < 9; i++)
                {
                    //KEY
                    key = string.Empty;
                    if (dictionaryFile.Peek() > -1)
                    {
                        key = dictionaryFile.ReadLine();
                    }
                    else
                    {
                        dictionaryFile.Dispose();
                        errorMessage = "Dictionary file syntax error. Chapter 'Wind'. Key[" + i + "] not found!";
                        ConsoleErrorAlert(errorMessage);
                        return;
                    }
                    if (key.Length == 0)
                    {
                        dictionaryFile.Dispose();
                        errorMessage = "Dictionary file syntax error. Chapter 'Wind'. Key[" + i + "] not found!";
                        ConsoleErrorAlert(errorMessage);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Key[{0}] is {1}", i, key);
                    }


                    //WIND DIRECTION
                    if (dictionaryFile.Peek() > -1)
                    {
                        wind_direction = dictionaryFile.ReadLine();
                    }
                    else
                    {
                        dictionaryFile.Dispose();
                        errorMessage = "Dictionary file syntax error. Chapter 'Wind'. Description value[" + i + "] not found!";
                        ConsoleErrorAlert(errorMessage);
                        return;
                    }
                    if (wind_direction.Length == 0)
                    {
                        dictionaryFile.Dispose();
                        errorMessage = "Dictionary file syntax error. Chapter 'Wind'. Description value[" + i + "] not found!";
                        ConsoleErrorAlert(errorMessage);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Description value[{0}] is {1}", i, wind_direction);
                    }

                    //WIND DIRECTION CHARASTER
                    if (dictionaryFile.Peek() > -1)
                    {
                        wind_direction_charaster = dictionaryFile.ReadLine();
                    }
                    else
                    {
                        dictionaryFile.Dispose();
                        errorMessage = "Dictionary file syntax error. Chapter 'Wind'. Charaster value[" + i + "] not found!";
                        ConsoleErrorAlert(errorMessage);
                        return;
                    }
                    if (wind_direction_charaster.Length == 0)
                    {
                        dictionaryFile.Dispose();
                        errorMessage = "Dictionary file syntax error. Chapter 'Wind'. Charaster value[" + i + "] not found!";
                        ConsoleErrorAlert(errorMessage);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Charaster value[{0}] is {1}", i, wind_direction_charaster);
                    }

                    //SPACE
                    if (dictionaryFile.Peek() > -1)
                    {
                        space = dictionaryFile.ReadLine().Trim();
                        if (space.Length != 0)
                        {
                            dictionaryFile.Dispose();
                            errorMessage = "Dictionary file syntax error. Space isn`t exist!";
                            ConsoleErrorAlert(errorMessage);
                            return;
                        }
                    }

                    dictionary_wind_direction.Add(key, wind_direction);
                    dictionary_wind_direction_charaster.Add(key, wind_direction_charaster);
                }

                //WEATHER_TYPE_CHAPTER
                while (!dictionaryFile.EndOfStream)
                {
                    //KEY
                    key = dictionaryFile.ReadLine();

                    //USER_DEFENITION
                    if (dictionaryFile.Peek() > -1)
                        user_defenition = dictionaryFile.ReadLine();
                    else
                    {
                        dictionaryFile.Dispose();
                        errorMessage = "Dictionary file syntax error. User defenition of key '" + key + "' not found!";
                        ConsoleErrorAlert(errorMessage);
                        return;
                    }

                    //DAY_CHARASTER
                    if (dictionaryFile.Peek() > -1)
                        day_charaster = dictionaryFile.ReadLine();
                    else
                    {
                        dictionaryFile.Dispose();
                        errorMessage = "Dictionary file syntax error. Day charaster of key '" + key + "' not found!";
                        ConsoleErrorAlert(errorMessage);
                        return;
                    }

                    //NIGHT_CHARASTER
                    if (dictionaryFile.Peek() > -1)
                        night_charaster = dictionaryFile.ReadLine();
                    else
                    {
                        dictionaryFile.Dispose();
                        errorMessage = "Dictionary file syntax error. Night charaster of key '" + key + "' not found!";
                        ConsoleErrorAlert(errorMessage);
                        return;
                    }

                    //SPACE
                    if (dictionaryFile.Peek() > -1)
                    {
                        space = dictionaryFile.ReadLine().Trim();
                        if (space.Length != 0)
                        {
                            dictionaryFile.Dispose();
                            errorMessage = "Dictionary file syntax error!";
                            ConsoleErrorAlert(errorMessage);
                            return;
                        }
                    }

                    dictionary.Add(key, user_defenition);
                    dictionary_day.Add(key, day_charaster);
                    dictionary_night.Add(key, night_charaster);

                    Console.WriteLine(key);
                    Console.WriteLine(user_defenition);
                    Console.WriteLine(day_charaster);
                    Console.WriteLine(night_charaster);
                    Console.WriteLine();
                }
            }
            catch (Exception exception)
            {
                ConsoleExceptionAlert("Dictionary creation error:", exception);
                return;
            }

            Console.WriteLine("Dictionary is ready.");
            Console.WriteLine();

            //ARRAY_DATA_CHECK

            Console.WriteLine("Data check ...");

            int tryNumber;
            bool isDownloaded = false;
            DateTime todayDate = DateTime.Now.Date;

            WebClient webClient = new WebClient();

            tryNumber = 1;

            Console.WriteLine("27730.xml");

            if (!File.Exists("27730.xml") || (File.GetLastWriteTime("27730.xml").Date != todayDate))
            {
                Console.WriteLine("Download:");

                while (!isDownloaded)
                {
                    Console.Write("Try #{0} ...", tryNumber);

                    try
                    {
                        webClient.DownloadFile("http://export.yandex.ru/weather-ng/forecasts/27730.xml", "27730.xml");
                        isDownloaded = true;
                        Console.WriteLine(" success");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(" failed");
                        Console.WriteLine(e.Message);
                        isDownloaded = false;
                    }
                    tryNumber++;
                }
                isDownloaded = false;
            }
            else
            {
                Console.WriteLine("File exist.");
            }
            Console.WriteLine();



            string today = DateTime.Today.ToString("D", CultureInfo.CreateSpecificCulture("ru-RU"));

            string spaceMarker = string.Empty;

            string dayDateNumber = string.Empty;
            string dayDateMonth = string.Empty;

            string dayMarker = @"////ДЕНЬ";
            string day_temperature = string.Empty;
            string day_temperature_region = string.Empty;
            string day_weather_type = string.Empty;
            string day_weather_type_charaster = string.Empty;
            string day_wind_direction = string.Empty;
            string day_wind_direction_charaster = string.Empty;
            string day_wind_speed = string.Empty;
            int day_pressure = 0;
            int day_humidity = 0;

            string eveningMarker = @"////ВЕЧЕР";
            string evening_temperature = string.Empty;
            string evening_temperature_region = string.Empty;
            string evening_weather_type = string.Empty;
            string evening_weather_type_charaster = string.Empty;
            string evening_wind_direction = string.Empty;
            string evening_wind_direction_charaster = string.Empty;
            string evening_wind_speed = string.Empty;
            int evening_pressure = 0;
            int evening_humidity = 0;


            string nightDateNumber = string.Empty;
            string nightDateMonth = string.Empty;

            string nightMarker = @"////НОЧЬ";
            string night_temperature = string.Empty;
            string night_temperature_region = string.Empty;
            string night_weather_type = string.Empty;
            string night_weather_type_charaster = string.Empty;
            string night_wind_direction = string.Empty;
            string night_wind_direction_charaster = string.Empty;
            string night_wind_speed = string.Empty;
            int night_pressure = 0;
            int night_humidity = 0;

            string morningMarker = @"////УТРО";
            string morning_temperature = string.Empty;
            string morning_temperature_region = string.Empty;
            string morning_weather_type = string.Empty;
            string morning_weather_type_charaster = string.Empty;
            string morning_wind_direction = string.Empty;
            string morning_wind_direction_charaster = string.Empty;
            string morning_wind_speed = string.Empty;
            int morning_pressure = 0;
            int morning_humidity = 0;


            XmlDocument xmldocument = new XmlDocument();
            xmldocument.Load("27730.xml");
            XmlElement xmlroot = xmldocument.DocumentElement;

            XmlNodeList daynodes = xmlroot.GetElementsByTagName("day");

            for (int dayCounter = 0; dayCounter < NUMBER_OF_DAYS; dayCounter++)
            {
                string day = DateTime.Today.AddDays(dayCounter).ToString("D", CultureInfo.CreateSpecificCulture("ru-RU"));
                string next_day = DateTime.Today.AddDays(dayCounter + 1).ToString("D", CultureInfo.CreateSpecificCulture("ru-RU"));

                dayDateNumber = DateTime.Today.AddDays(dayCounter).Day.ToString();
                dayDateMonth = NominativeToGenetive[(DateTime.Today.AddDays(dayCounter).ToString("MMMM", CultureInfo.CreateSpecificCulture("ru-RU")).ToLower())].ToUpper();

                nightDateNumber = DateTime.Today.AddDays(dayCounter + 1).Day.ToString();
                nightDateMonth = NominativeToGenetive[(DateTime.Today.AddDays(dayCounter + 1).ToString("MMMM", CultureInfo.CreateSpecificCulture("ru-RU")).ToLower())].ToUpper();

                List<string> weather_information_array = new List<string>();

                Console.WriteLine(day);
                Console.WriteLine();

                bool isAvailable = false;

                foreach (XmlElement daynode in daynodes)
                {
                    XmlNodeList day_partnodes = daynode.GetElementsByTagName("day_part");
                    string date = DateTime.ParseExact(daynode.Attributes["date"].Value, "yyyy-MM-dd", null).ToString("D", CultureInfo.CreateSpecificCulture("ru-RU"));

                    if (date == day)
                    {
                        isAvailable = true;

                        foreach (XmlNode day_partnode in day_partnodes)
                        {

                            if (day_partnode.Attributes["type"].Value == "day")
                            {
                                //DAY WEATHER TYPE CHARASTER
                                try
                                {
                                    day_weather_type_charaster = dictionary_day[day_partnode["weather_type"].InnerText];
                                }
                                catch (KeyNotFoundException exception)
                                {
                                    string caution = "Day dictionary error. Yandex service weather type definition is '" + day_partnode["weather_type"].InnerText + "':";
                                    ConsoleExceptionAlert(caution, exception);
                                    return;
                                }

                                //DAY WEATHER TYPE
                                try
                                {
                                    day_weather_type = dictionary[day_partnode["weather_type"].InnerText];
                                }
                                catch (KeyNotFoundException exception)
                                {
                                    string caution = "Dictionary error. Yandex service weather type definition is '" + day_partnode["weather_type"].InnerText + "':";
                                    ConsoleExceptionAlert(caution, exception);
                                    return;
                                }

                                //DAY WIND DIRECTION
                                try
                                {
                                    day_wind_direction = dictionary_wind_direction[day_partnode["wind_direction"].InnerText];
                                    day_wind_direction_charaster = dictionary_wind_direction_charaster[day_partnode["wind_direction"].InnerText];
                                }
                                catch (KeyNotFoundException exception)
                                {
                                    string caution = "Dictionary error. Yandex service wind direction definition is '" + day_partnode["wind_direction"].InnerText + "':";
                                    ConsoleExceptionAlert(caution, exception);
                                    return;
                                }

                                //DAY WIND SPEED
                                day_wind_speed = day_partnode["wind_speed"].InnerText.Replace(".", ",") + " м/с";

                                //DAY HUMIDITY
                                day_humidity = Convert.ToInt32(day_partnode["humidity"].InnerText);
                                //DAY PRESSURE
                                day_pressure = Convert.ToInt32(day_partnode["pressure"].InnerText);

                                //DAY_TEMPERATURE
                                if (day_partnode["temperature"] != null)
                                {
                                    day_temperature = MakeTemperatureString(day_partnode["temperature"].InnerText, 1, 1);
                                    day_temperature_region = MakeTemperatureString(day_partnode["temperature"].InnerText, 3, 0);
                                }
                                else
                                {
                                    day_temperature = MakeTemperatureString(day_partnode["temperature_from"].InnerText, day_partnode["temperature_to"].InnerText, 0, 0);
                                    day_temperature_region = MakeTemperatureString(day_partnode["temperature_from"].InnerText, day_partnode["temperature_to"].InnerText, 2, -1);
                                }
                            }

                            if (day_partnode.Attributes["type"].Value == "evening")
                            {
                                //EVENING WEATHER TYPE CHARASTER
                                try
                                {
                                    evening_weather_type_charaster = dictionary_day[day_partnode["weather_type"].InnerText];
                                }
                                catch (KeyNotFoundException exception)
                                {
                                    string caution = "Day dictionary error. Yandex service weather type definition is '" + day_partnode["weather_type"].InnerText + "':";
                                    ConsoleExceptionAlert(caution, exception);
                                    return;
                                }

                                //EVENING WEATHER TYPE
                                try
                                {
                                    evening_weather_type = dictionary[day_partnode["weather_type"].InnerText];
                                }
                                catch (KeyNotFoundException exception)
                                {
                                    string caution = "Dictionary error. Yandex service weather type definition is '" + day_partnode["weather_type"].InnerText + "':";
                                    ConsoleExceptionAlert(caution, exception);
                                    return;
                                }

                                //EVENING WIND DIRECTION
                                try
                                {
                                    evening_wind_direction = dictionary_wind_direction[day_partnode["wind_direction"].InnerText];
                                    evening_wind_direction_charaster = dictionary_wind_direction_charaster[day_partnode["wind_direction"].InnerText];
                                }
                                catch (KeyNotFoundException exception)
                                {
                                    string caution = "Dictionary error. Yandex service wind direction definition is '" + day_partnode["wind_direction"].InnerText + "':";
                                    ConsoleExceptionAlert(caution, exception);
                                    return;
                                }

                                //EVENING WIND SPEED
                                evening_wind_speed = day_partnode["wind_speed"].InnerText.Replace(".", ",") + " м/с";

                                //EVENING HUMIDITY
                                evening_humidity = Convert.ToInt32(day_partnode["humidity"].InnerText);
                                //EVENING PRESSURE
                                evening_pressure = Convert.ToInt32(day_partnode["pressure"].InnerText);

                                if (day_partnode["temperature"] != null)
                                {
                                    evening_temperature = MakeTemperatureString(day_partnode["temperature"].InnerText, 1, 1);
                                    evening_temperature_region = MakeTemperatureString(day_partnode["temperature"].InnerText, 3, 0);
                                }
                                else
                                {
                                    evening_temperature = MakeTemperatureString(day_partnode["temperature_from"].InnerText, day_partnode["temperature_to"].InnerText, 0, 0);
                                    evening_temperature_region = MakeTemperatureString(day_partnode["temperature_from"].InnerText, day_partnode["temperature_to"].InnerText, 2, -1);
                                }
                            }//if evening

                            if (day_partnode.Attributes["type"].Value == "night")
                            {
                                //NIGHT WEATHER TYPE CHARASTER
                                try
                                {
                                    night_weather_type_charaster = dictionary_day[day_partnode["weather_type"].InnerText];
                                }
                                catch (KeyNotFoundException exception)
                                {
                                    string caution = "Day dictionary error. Yandex service weather type definition is '" + day_partnode["weather_type"].InnerText + "':";
                                    ConsoleExceptionAlert(caution, exception);
                                    return;
                                }

                                //NIGHT WEATHER TYPE
                                try
                                {
                                    night_weather_type = dictionary[day_partnode["weather_type"].InnerText];
                                }
                                catch (KeyNotFoundException exception)
                                {
                                    string caution = "Dictionary error. Yandex service weather type definition is '" + day_partnode["weather_type"].InnerText + "':";
                                    ConsoleExceptionAlert(caution, exception);
                                    return;
                                }

                                //NIGHT WIND DIRECTION
                                try
                                {
                                    night_wind_direction = dictionary_wind_direction[day_partnode["wind_direction"].InnerText];
                                    night_wind_direction_charaster = dictionary_wind_direction_charaster[day_partnode["wind_direction"].InnerText];
                                }
                                catch (KeyNotFoundException exception)
                                {
                                    string caution = "Dictionary error. Yandex service wind direction definition is '" + day_partnode["wind_direction"].InnerText + "':";
                                    ConsoleExceptionAlert(caution, exception);
                                    return;
                                }

                                //NIGHT WIND SPEED
                                night_wind_speed = day_partnode["wind_speed"].InnerText.Replace(".", ",") + " м/с";

                                //NIGHT HUMIDITY
                                night_humidity = Convert.ToInt32(day_partnode["humidity"].InnerText);
                                //NIGHT PRESSURE
                                night_pressure = Convert.ToInt32(day_partnode["pressure"].InnerText);

                                //NIGHT TEMPERATURE        
                                if (day_partnode["temperature"] != null)
                                {
                                    night_temperature = MakeTemperatureString(day_partnode["temperature"].InnerText, 1, 1);
                                    night_temperature_region = MakeTemperatureString(day_partnode["temperature"].InnerText, 3, 0);
                                }
                                else
                                {
                                    night_temperature = MakeTemperatureString(day_partnode["temperature_from"].InnerText, day_partnode["temperature_to"].InnerText, 0, 0);
                                    night_temperature_region = MakeTemperatureString(day_partnode["temperature_from"].InnerText, day_partnode["temperature_to"].InnerText, 2, -1);

                                }
                            }//if night

                        }//foreach day_partnodes
                    }//if date=day

                    if (date == next_day)
                    {
                        foreach (XmlNode day_partnode in day_partnodes)
                        {
                            if (day_partnode.Attributes["type"].Value == "morning")
                            {
                                //NIGHT WEATHER TYPE CHARASTER
                                try
                                {
                                    morning_weather_type_charaster = dictionary_day[day_partnode["weather_type"].InnerText];
                                }
                                catch (KeyNotFoundException exception)
                                {
                                    string caution = "Day dictionary error. Yandex service weather type definition is '" + day_partnode["weather_type"].InnerText + "':";
                                    ConsoleExceptionAlert(caution, exception);
                                    return;
                                }

                                //NIGHT WEATHER TYPE
                                try
                                {
                                    morning_weather_type = dictionary[day_partnode["weather_type"].InnerText];
                                }
                                catch (KeyNotFoundException exception)
                                {
                                    string caution = "Dictionary error. Yandex service weather type definition is '" + day_partnode["weather_type"].InnerText + "':";
                                    ConsoleExceptionAlert(caution, exception);
                                    return;
                                }

                                //NIGHT WIND DIRECTION
                                try
                                {
                                    morning_wind_direction = dictionary_wind_direction[day_partnode["wind_direction"].InnerText];
                                    morning_wind_direction_charaster = dictionary_wind_direction_charaster[day_partnode["wind_direction"].InnerText];
                                }
                                catch (KeyNotFoundException exception)
                                {
                                    string caution = "Dictionary error. Yandex service wind direction definition is '" + day_partnode["wind_direction"].InnerText + "':";
                                    ConsoleExceptionAlert(caution, exception);
                                    return;
                                }

                                //NIGHT WIND SPEED
                                morning_wind_speed = day_partnode["wind_speed"].InnerText.Replace(".", ",") + " м/с";

                                //NIGHT HUMIDITY
                                morning_humidity = Convert.ToInt32(day_partnode["humidity"].InnerText);
                                //NIGHT PRESSURE
                                morning_pressure = Convert.ToInt32(day_partnode["pressure"].InnerText);

                                if (day_partnode["temperature"] != null)
                                {
                                    morning_temperature = MakeTemperatureString(day_partnode["temperature"].InnerText, 1, 1);
                                    morning_temperature_region = MakeTemperatureString(day_partnode["temperature"].InnerText, 3, 0);
                                }
                                else
                                {
                                    morning_temperature = MakeTemperatureString(day_partnode["temperature_from"].InnerText, day_partnode["temperature_to"].InnerText, 0, 0);
                                    morning_temperature_region = MakeTemperatureString(day_partnode["temperature_from"].InnerText, day_partnode["temperature_to"].InnerText, 2, -1);
                                }
                            }
                        }//foreach day_partnodes
                    }//if date=next_day

                }//foreach daynodes

                if (isAvailable)
                {
                    weather_information_array.Add(dayDateNumber);
                    weather_information_array.Add(dayDateMonth);
                    weather_information_array.Add(dayMarker);

                    weather_information_array.Add(day_weather_type_charaster);
                    weather_information_array.Add(day_temperature);
                    weather_information_array.Add(day_weather_type);
                    weather_information_array.Add(day_temperature_region);
                    weather_information_array.Add(day_wind_direction_charaster);
                    weather_information_array.Add(day_wind_direction);
                    weather_information_array.Add(day_wind_speed);
                    weather_information_array.Add(day_humidity + "%");
                    weather_information_array.Add(day_pressure + " мм");
                    weather_information_array.Add(spaceMarker);

                    weather_information_array.Add(eveningMarker);
                    weather_information_array.Add(evening_weather_type_charaster);
                    weather_information_array.Add(evening_temperature);
                    weather_information_array.Add(evening_weather_type);
                    weather_information_array.Add(evening_temperature_region);
                    weather_information_array.Add(evening_wind_direction_charaster);
                    weather_information_array.Add(evening_wind_direction);
                    weather_information_array.Add(evening_wind_speed);
                    weather_information_array.Add(evening_humidity + "%");
                    weather_information_array.Add(evening_pressure + " мм");
                    weather_information_array.Add(spaceMarker);

                    weather_information_array.Add(nightDateNumber);
                    weather_information_array.Add(dayDateMonth);

                    weather_information_array.Add(nightMarker);
                    weather_information_array.Add(night_weather_type_charaster);
                    weather_information_array.Add(night_temperature);
                    weather_information_array.Add(night_weather_type);
                    weather_information_array.Add(night_temperature_region);
                    weather_information_array.Add(night_wind_direction_charaster);
                    weather_information_array.Add(night_wind_direction);
                    weather_information_array.Add(night_wind_speed);
                    weather_information_array.Add(night_humidity + "%");
                    weather_information_array.Add(night_pressure + " мм");
                    weather_information_array.Add(spaceMarker);

                    weather_information_array.Add(morningMarker);
                    weather_information_array.Add(morning_weather_type_charaster);
                    weather_information_array.Add(morning_temperature);
                    weather_information_array.Add(morning_weather_type);
                    weather_information_array.Add(morning_temperature_region);
                    weather_information_array.Add(morning_wind_direction_charaster);
                    weather_information_array.Add(morning_wind_direction);
                    weather_information_array.Add(morning_wind_speed);
                    weather_information_array.Add(morning_humidity + "%");
                    weather_information_array.Add(morning_pressure + " мм");
                    weather_information_array.Add(spaceMarker);

                    ////////////////////////////
                    Console.WriteLine(dayDateNumber);
                    Console.WriteLine(dayDateMonth);

                    Console.WriteLine(dayMarker);
                    Console.WriteLine(day_weather_type_charaster);
                    Console.WriteLine(day_temperature);
                    Console.WriteLine(day_weather_type);
                    Console.WriteLine(day_temperature_region);
                    Console.WriteLine(day_wind_direction_charaster);
                    Console.WriteLine(day_wind_direction);
                    Console.WriteLine(day_wind_speed);
                    Console.WriteLine(day_humidity + "%");
                    Console.WriteLine(day_pressure + " мм");
                    Console.WriteLine(spaceMarker);

                    Console.WriteLine(eveningMarker);
                    Console.WriteLine(evening_weather_type_charaster);
                    Console.WriteLine(evening_temperature);
                    Console.WriteLine(evening_weather_type);
                    Console.WriteLine(evening_temperature_region);
                    Console.WriteLine(evening_wind_direction_charaster);
                    Console.WriteLine(evening_wind_direction);
                    Console.WriteLine(evening_wind_speed);
                    Console.WriteLine(evening_humidity + "%");
                    Console.WriteLine(evening_pressure + " мм");
                    Console.WriteLine(spaceMarker);

                    Console.WriteLine(nightDateNumber);
                    Console.WriteLine(dayDateMonth);

                    Console.WriteLine(nightMarker);
                    Console.WriteLine(night_weather_type_charaster);
                    Console.WriteLine(night_temperature);
                    Console.WriteLine(night_weather_type);
                    Console.WriteLine(night_temperature_region);
                    Console.WriteLine(night_wind_direction_charaster);
                    Console.WriteLine(night_wind_direction);
                    Console.WriteLine(night_wind_speed);
                    Console.WriteLine(night_humidity + "%");
                    Console.WriteLine(night_pressure + " мм");
                    Console.WriteLine(spaceMarker);

                    Console.WriteLine(morningMarker);
                    Console.WriteLine(morning_weather_type_charaster);
                    Console.WriteLine(morning_temperature);
                    Console.WriteLine(morning_weather_type);
                    Console.WriteLine(morning_temperature_region);
                    Console.WriteLine(morning_wind_direction_charaster);
                    Console.WriteLine(morning_wind_direction);
                    Console.WriteLine(morning_wind_speed);
                    Console.WriteLine(morning_humidity + "%");
                    Console.WriteLine(morning_pressure + " мм");
                    Console.WriteLine(spaceMarker);
                }
                else
                {
                    weather_information_array.Add(dayDateNumber);
                    weather_information_array.Add(dayDateMonth);

                    weather_information_array.Add(dayMarker);
                    weather_information_array.Add(")");
                    weather_information_array.Add("N/A");
                    weather_information_array.Add("N/A");
                    weather_information_array.Add("N/A");
                    weather_information_array.Add(")");
                    weather_information_array.Add(string.Empty);
                    weather_information_array.Add("N/A м/с");
                    weather_information_array.Add("N/A %");
                    weather_information_array.Add("N/A мм");
                    weather_information_array.Add(spaceMarker);

                    weather_information_array.Add(eveningMarker);
                    weather_information_array.Add(")");
                    weather_information_array.Add("N/A");
                    weather_information_array.Add("N/A");
                    weather_information_array.Add("N/A");
                    weather_information_array.Add(")");
                    weather_information_array.Add(string.Empty);
                    weather_information_array.Add("N/A м/с");
                    weather_information_array.Add("N/A %");
                    weather_information_array.Add("N/A мм");
                    weather_information_array.Add(spaceMarker);

                    weather_information_array.Add(nightDateNumber);
                    weather_information_array.Add(nightDateMonth);

                    weather_information_array.Add(nightMarker);
                    weather_information_array.Add(")");
                    weather_information_array.Add("N/A");
                    weather_information_array.Add("N/A");
                    weather_information_array.Add("N/A");
                    weather_information_array.Add(")");
                    weather_information_array.Add(string.Empty);
                    weather_information_array.Add("N/A м/с");
                    weather_information_array.Add("N/A %");
                    weather_information_array.Add("N/A мм");
                    weather_information_array.Add(spaceMarker);

                    weather_information_array.Add(morningMarker);
                    weather_information_array.Add(")");
                    weather_information_array.Add("N/A");
                    weather_information_array.Add("N/A");
                    weather_information_array.Add("N/A");
                    weather_information_array.Add(")");
                    weather_information_array.Add(string.Empty);
                    weather_information_array.Add("N/A м/с");
                    weather_information_array.Add("N/A %");
                    weather_information_array.Add("N/A мм");
                    weather_information_array.Add(spaceMarker);

                    string caution = "Please, attention. " + dayDateNumber + " " + dayDateMonth + " data not found!";
                    ConsoleWarningAlert(caution);

                }

                //CHECK OUTPU_DIRECTORY AND WRITE DATA
                if (!Directory.Exists(OUTPUT_DIRECTORY) && OUTPUT_DIRECTORY != string.Empty)
                {
                    try
                    {
                        Directory.CreateDirectory(OUTPUT_DIRECTORY);
                    }
                    catch
                    {
                        errorMessage = "Output directory error (path is '" + OUTPUT_DIRECTORY + "')";
                        ConsoleErrorAlert(errorMessage);
                        return;
                    }
                }
                else
                {
                    string file = Path.Combine(OUTPUT_DIRECTORY, FILE_NAME_PREFIX + " " + day + "rwdat");
                    System.IO.File.WriteAllLines(file, weather_information_array, System.Text.Encoding.UTF8);

                }

            }//dayCounter

            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Thank You for waiting.");
            Console.WriteLine("Task is finished successfuly!");
            Console.WriteLine("Please, press ENTER. Application will be closed.");
            Console.ForegroundColor = currentColor;
            Console.ReadLine();

        }//Main    
    }//Program
}//namespace










