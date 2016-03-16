using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
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

            return from_t + "..." + to_t + "°C";
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
                t = (Convert.ToInt16(from_temperature) + Convert.ToInt16(to_temperature))/ 2;
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

            return from_temperature + "..." + to_temperature + "°C";
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
            Dictionary<string, string> NominativeToGenetive = new Dictionary<string, string>()
            {
                { "январь","января"},
                { "февраль","февраля"},
                { "март","марта"},
                { "апрель","апреля"},
                { "май","мая"},
                { "июнь","июня"},
                { "июль","июля"},
                { "август","августа"},
                { "сентябрь","сентября"},
                { "октябрь","октября"},
                { "ноябрь","ноября"},
                { "декабрь","декабря"}
            };

            int numberOfDays;
            string outputPath;
            string outputFilenameSample;

            try
            {
                StreamReader configFile = new StreamReader("weatherParserV2_REG.config");
                numberOfDays = Convert.ToInt32(configFile.ReadLine());
                outputPath = configFile.ReadLine();
                outputFilenameSample = configFile.ReadLine();
                Console.WriteLine("Read configuration file successfully.");
                Console.WriteLine();
            }
            catch (Exception exception)
            {
                ConsoleExceptionAlert("Configuration file error:", exception);
                return;
            }

            //DICTIONARIES CREATION
            Dictionary<string, string> dictionary_wind_direction = new Dictionary<string, string>();
            Dictionary<string, string> dictionary_wind_direction_charaster = new Dictionary<string, string>();
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            Dictionary<string, string> dictionary_day = new Dictionary<string, string>();
            Dictionary<string, string> dictionary_night = new Dictionary<string, string>();

            try
            {
                Console.WriteLine("Read dictionary file ...");
                Console.WriteLine();

                StreamReader dictionaryFile = new StreamReader("region.dict");
                string key = string.Empty;
                string wind_direction_charaster = string.Empty;
                string wind_direction = string.Empty;
                string user_defenition = string.Empty;
                string day_charaster = string.Empty;
                string night_charaster = string.Empty;
                string space = string.Empty;

                //WIND_CHAPTER
                Console.WriteLine("Wind chapter");
                for(int i=1; i<9; i++)
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
                        string errorMessage = "Dictionary file syntax error. Chapter 'Wind'. Key[" + i + "] not found!";
                        ConsoleErrorAlert(errorMessage);
                        return;
                    }
                    if (key.Length == 0)
                    {
                        dictionaryFile.Dispose();
                        string errorMessage = "Dictionary file syntax error. Chapter 'Wind'. Key[" + i + "] not found!";
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
                        string errorMessage = "Dictionary file syntax error. Chapter 'Wind'. Description value[" + i + "] not found!";
                        ConsoleErrorAlert(errorMessage);
                        return;
                    }
                    if (wind_direction.Length == 0)
                    {
                        dictionaryFile.Dispose();
                        string errorMessage = "Dictionary file syntax error. Chapter 'Wind'. Description value[" + i + "] not found!";
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
                        string errorMessage = "Dictionary file syntax error. Chapter 'Wind'. Charaster value[" + i + "] not found!";
                        ConsoleErrorAlert(errorMessage);
                        return;
                    }
                    if (wind_direction_charaster.Length == 0)
                    {
                        dictionaryFile.Dispose();
                        string errorMessage = "Dictionary file syntax error. Chapter 'Wind'. Charaster value[" + i + "] not found!";
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
                            string errorMessage = "Dictionary file syntax error. Space isn`t exist!";
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
                        string errorMessage = "Dictionary file syntax error. User defenition of key '" + key + "' not found!";
                        ConsoleErrorAlert(errorMessage);
                        return;
                    }

                    //DAY_CHARASTER
                    if (dictionaryFile.Peek() > -1)
                        day_charaster = dictionaryFile.ReadLine();
                    else
                    {
                        dictionaryFile.Dispose();
                        string errorMessage = "Dictionary file syntax error. Day charaster of key '" + key + "' not found!";
                        ConsoleErrorAlert(errorMessage);
                        return;
                    }

                    //NIGHT_CHARASTER
                    if (dictionaryFile.Peek() > -1)
                        night_charaster = dictionaryFile.ReadLine();
                    else
                    {
                        dictionaryFile.Dispose();
                        string errorMessage = "Dictionary file syntax error. Night charaster of key '" + key + "' not found!";
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
                            string errorMessage = "Dictionary file syntax error!";
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

            string today = DateTime.Today.ToString("D", CultureInfo.CreateSpecificCulture("ru-RU"));

            string dayDateNumber = string.Empty;
            string dayDateMonth = string.Empty;

            string day_temperature = string.Empty;
            string day_temperature_region = string.Empty;
            string day_weather_type = string.Empty;
            string day_weather_type_charaster = string.Empty;
            string day_wind_direction = string.Empty;
            string day_wind_direction_charaster = string.Empty;
            string day_wind_speed = string.Empty;
            int day_pressure = 0;
            int day_humidity = 0;

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

            string night_temperature = string.Empty;
            string night_temperature_region = string.Empty;
            string night_weather_type = string.Empty;
            string night_weather_type_charaster = string.Empty;
            string night_wind_direction = string.Empty;
            string night_wind_direction_charaster = string.Empty;
            string night_wind_speed = string.Empty;
            int night_pressure = 0;
            int night_humidity = 0;

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
            string filename = @"http://export.yandex.ru/weather-ng/forecasts/27730.xml";
            xmldocument.Load(filename);
            XmlElement xmlroot = xmldocument.DocumentElement;

            XmlNodeList daynodes = xmlroot.GetElementsByTagName("day");

            for (int dayCounter = 0; dayCounter < numberOfDays; dayCounter++)
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

                bool isUsed = false;                

                foreach (XmlElement daynode in daynodes)
                {
                    XmlNodeList day_partnodes = daynode.GetElementsByTagName("day_part");
                    string date = DateTime.ParseExact(daynode.Attributes["date"].Value, "yyyy-MM-dd", null).ToString("D", CultureInfo.CreateSpecificCulture("ru-RU"));

                    if (date == day)
                    {
                        isUsed = true;

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
                                day_humidity =  Convert.ToInt32(day_partnode["humidity"].InnerText);
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

                if (isUsed)
                {
                    weather_information_array.Add(dayDateNumber);
                    weather_information_array.Add(dayDateMonth);

                    weather_information_array.Add(day_weather_type_charaster);
                    weather_information_array.Add(day_temperature);
                    weather_information_array.Add(day_weather_type);
                    weather_information_array.Add(day_temperature_region);
                    weather_information_array.Add(day_wind_direction_charaster);
                    weather_information_array.Add(day_wind_direction);
                    weather_information_array.Add(day_wind_speed);
                    weather_information_array.Add(day_humidity + "%");
                    weather_information_array.Add(day_pressure + " мм");

                    weather_information_array.Add(evening_weather_type_charaster);
                    weather_information_array.Add(evening_temperature);
                    weather_information_array.Add(evening_weather_type);
                    weather_information_array.Add(evening_temperature_region);
                    weather_information_array.Add(evening_wind_direction_charaster);
                    weather_information_array.Add(evening_wind_direction);
                    weather_information_array.Add(evening_wind_speed);
                    weather_information_array.Add(evening_humidity + "%");
                    weather_information_array.Add(evening_pressure + " мм");

                    weather_information_array.Add(nightDateNumber);
                    weather_information_array.Add(dayDateMonth);

                    weather_information_array.Add(night_weather_type_charaster);
                    weather_information_array.Add(night_temperature);
                    weather_information_array.Add(night_weather_type);
                    weather_information_array.Add(night_temperature_region);
                    weather_information_array.Add(night_wind_direction_charaster);
                    weather_information_array.Add(night_wind_direction);
                    weather_information_array.Add(night_wind_speed);
                    weather_information_array.Add(night_humidity + "%");
                    weather_information_array.Add(night_pressure + " мм");

                    weather_information_array.Add(morning_weather_type_charaster);
                    weather_information_array.Add(morning_temperature);
                    weather_information_array.Add(morning_weather_type);
                    weather_information_array.Add(morning_temperature_region);
                    weather_information_array.Add(morning_wind_direction_charaster);
                    weather_information_array.Add(morning_wind_direction);
                    weather_information_array.Add(morning_wind_speed);
                    weather_information_array.Add(morning_humidity + "%");
                    weather_information_array.Add(morning_pressure + " мм");

                    ////////////////////////////
                    Console.WriteLine(dayDateNumber);
                    Console.WriteLine(dayDateMonth);

                    Console.WriteLine(day_weather_type_charaster);
                    Console.WriteLine(day_temperature);
                    Console.WriteLine(day_weather_type);
                    Console.WriteLine(day_temperature_region);
                    Console.WriteLine(day_wind_direction_charaster);
                    Console.WriteLine(day_wind_direction);
                    Console.WriteLine(day_wind_speed);
                    Console.WriteLine(day_humidity + "%");
                    Console.WriteLine(day_pressure + "мм");

                    Console.WriteLine(evening_weather_type_charaster);
                    Console.WriteLine(evening_temperature);
                    Console.WriteLine(evening_weather_type);
                    Console.WriteLine(evening_temperature_region);
                    Console.WriteLine(evening_wind_direction_charaster);
                    Console.WriteLine(evening_wind_direction);
                    Console.WriteLine(evening_wind_speed);
                    Console.WriteLine(evening_humidity + "%");
                    Console.WriteLine(evening_pressure + "мм");

                    Console.WriteLine(nightDateNumber);
                    Console.WriteLine(dayDateMonth);

                    Console.WriteLine(night_weather_type_charaster);
                    Console.WriteLine(night_temperature);
                    Console.WriteLine(night_weather_type);
                    Console.WriteLine(night_temperature_region);
                    Console.WriteLine(night_wind_direction_charaster);
                    Console.WriteLine(night_wind_direction);
                    Console.WriteLine(night_wind_speed);
                    Console.WriteLine(night_humidity + "%");
                    Console.WriteLine(night_pressure + "мм");

                    Console.WriteLine(morning_weather_type_charaster);
                    Console.WriteLine(morning_temperature);
                    Console.WriteLine(morning_weather_type);
                    Console.WriteLine(morning_temperature_region);
                    Console.WriteLine(morning_wind_direction_charaster);
                    Console.WriteLine(morning_wind_direction);
                    Console.WriteLine(morning_wind_speed);
                    Console.WriteLine(morning_humidity + "%");
                    Console.WriteLine(morning_pressure + "мм");
                }
                else
                {
                    weather_information_array.Add(dayDateNumber);
                    weather_information_array.Add(dayDateMonth);

                    weather_information_array.Add("B");
                    weather_information_array.Add("Not found");
                    weather_information_array.Add("Not found");
                    weather_information_array.Add("Not found");
                    weather_information_array.Add("^");
                    weather_information_array.Add("xx");
                    weather_information_array.Add("xxx");
                    weather_information_array.Add("xxx");
                    weather_information_array.Add("xxxxxx");

                    weather_information_array.Add("B");
                    weather_information_array.Add("Not found");
                    weather_information_array.Add("Not found");
                    weather_information_array.Add("Not found");
                    weather_information_array.Add("^");
                    weather_information_array.Add("xx");
                    weather_information_array.Add("xxx");
                    weather_information_array.Add("xxx");
                    weather_information_array.Add("xxxxxx");

                    weather_information_array.Add(nightDateNumber);
                    weather_information_array.Add(dayDateMonth);

                    weather_information_array.Add("B");
                    weather_information_array.Add("Not found");
                    weather_information_array.Add("Not found");
                    weather_information_array.Add("Not found");
                    weather_information_array.Add("^");
                    weather_information_array.Add("xx");
                    weather_information_array.Add("xxx");
                    weather_information_array.Add("xxx");
                    weather_information_array.Add("xxxxxx");

                    weather_information_array.Add("B");
                    weather_information_array.Add("Not found");
                    weather_information_array.Add("Not found");
                    weather_information_array.Add("Not found");
                    weather_information_array.Add("^");
                    weather_information_array.Add("xx");
                    weather_information_array.Add("xxx");
                    weather_information_array.Add("xxx");
                    weather_information_array.Add("xxxxxx");

                    string caution = "Please, attention. " + dayDateNumber + " " + dayDateMonth + " data not found!";
                    ConsoleWarningAlert(caution);
 
                }

                string file = Path.Combine(outputPath, outputFilenameSample + " " + day + "rwdat");
                System.IO.File.WriteAllLines(file, weather_information_array, System.Text.Encoding.UTF8);

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










