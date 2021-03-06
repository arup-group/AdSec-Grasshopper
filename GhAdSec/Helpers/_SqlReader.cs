using System;
using System.Linq;
using System.Drawing;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using AdSecGH.Parameters;
using System.Data.SQLite;
using System.Data;

namespace AdSecGH.Helpers
{
    /// <summary>
    /// Class containing functions to interface with SQLite db files.
    /// </summary>
    public class SqlReader
    {
        /// <summary>
        /// Method to set up a SQLite Connection to a specified .db3 file.
        /// Will return a SQLite connection to the aforementioned .db3 file database.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static SQLiteConnection Connection(string filePath)
        {
            return new SQLiteConnection($"URI=file:{filePath};mode=ReadOnly");
        }

        /// <summary>
        /// Get catalogue data from SQLite file (.db3). The method returns a tuple with:
        /// Item1 = list of catalogue name (string)
        /// where first item will be "All"
        /// Item2 = list of catalogue number (int)
        /// where first item will be "-1" representing All
        /// </summary>
        /// <param name="filePath">Path to SecLib.db3</param>
        /// <returns></returns>
        public static Tuple<List<string>, List<int>> GetCataloguesDataFromSQLite(string filePath)
        {
            // Create empty lists to work on:
            List<string> catNames = new List<string>();
            List<int> catNumber = new List<int>();

            using (var db = Connection(filePath))
            {
                db.Open();
                SQLiteCommand cmd = db.CreateCommand();
                cmd.CommandText = @"Select CAT_NAME || ' -- ' || CAT_NUM as CAT_NAME from Catalogues";

                cmd.CommandType = CommandType.Text;
                SQLiteDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    // get data
                    string sqlData = Convert.ToString(r["CAT_NAME"]);

                    // split text string
                    // example: British -- 2
                    catNames.Add(sqlData.Split(new string[] { " -- " }, StringSplitOptions.None)[0]);
                    catNumber.Add(Int32.Parse(sqlData.Split(new string[] { " -- " }, StringSplitOptions.None)[1]));
                }
                db.Close();
            }
            catNames.Insert(0, "All");
            catNumber.Insert(0, -1);
            return new Tuple<List<string>, List<int>>(catNames, catNumber);
        }

        /// <summary>
        /// Get section type data from SQLite file (.db3). The method returns a tuple with:
        /// Item1 = list of type name (string)
        /// where first item will be "All"
        /// Item2 = list of type number (int)
        /// where first item will be "-1" representing All
        /// </summary>
        /// <param name="catalogue_number">Catalogue number to get section types from. Input -1 in first item of the input list to get all types</param>
        /// <param name="filePath">Path to SecLib.db3</param>
        /// <param name="inclSuperseeded">True if you want to include superseeded items</param>
        /// <returns></returns>
        public static Tuple<List<string>, List<int>> GetTypesDataFromSQLite(int catalogue_number, string filePath, bool inclSuperseeded = false)
        {
            // Create empty lists to work on:
            List<string> typeNames = new List<string>();
            List<int> typeNumber = new List<int>();

            // get Catalogue numbers if input is -1 (All catalogues)
            List<int> catNumbers = new List<int>();
            if (catalogue_number == -1)
            {
                Tuple<List<string>, List<int>> catalogueData = GetCataloguesDataFromSQLite(filePath);
                catNumbers = catalogueData.Item2;
                catNumbers.RemoveAt(0); // remove -1 from beginning of list
            }
            else
                catNumbers.Add(catalogue_number);

            using (var db = Connection(filePath))
            {
                for (int i = 0; i < catNumbers.Count; i++)
                {
                    int cat = catNumbers[i];

                    db.Open();
                    SQLiteCommand cmd = db.CreateCommand();
                    if (inclSuperseeded)
                        cmd.CommandText = $"Select TYPE_NAME || ' -- ' || TYPE_NUM as TYPE_NAME from Types where TYPE_CAT_NUM = {cat}";
                    else
                        cmd.CommandText = $"Select TYPE_NAME || ' -- ' || TYPE_NUM as TYPE_NAME from Types where TYPE_CAT_NUM = {cat} and not (TYPE_SUPERSEDED = True or TYPE_SUPERSEDED = TRUE or TYPE_SUPERSEDED = 1)";
                    cmd.CommandType = CommandType.Text;
                    SQLiteDataReader r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        // get data
                        string sqlData = Convert.ToString(r["TYPE_NAME"]);

                        // split text string
                        // example: Universal Beams -- 51
                        typeNames.Add(sqlData.Split(new string[] { " -- " }, StringSplitOptions.None)[0]);
                        typeNumber.Add(Int32.Parse(sqlData.Split(new string[] { " -- " }, StringSplitOptions.None)[1]));
                    }
                    db.Close();
                }
            }
            typeNames.Insert(0, "All");
            typeNumber.Insert(0, -1);
            return new Tuple<List<string>, List<int>>(typeNames, typeNumber);
        }


        /// <summary>
        /// Get a list of section profile strings from SQLite file (.db3). The method returns a string that includes type abbriviation as accepted by GSA. 
        /// </summary>
        /// <param name="type_numbers">List of types to get sections from</param>
        /// <param name="filePath">Path to SecLib.db3</param>
        /// <param name="inclSuperseeded">True if you want to include superseeded items</param>
        /// <returns></returns>
        public static List<string> GetSectionsDataFromSQLite(List<int> type_numbers, string filePath, bool inclSuperseeded = false)
        {
            // Create empty list to work on:
            List<string> section = new List<string>();

            List<int> types = new List<int>();
            if (type_numbers[0] == -1)
            {
                Tuple<List<string>, List<int>> typeData = GetTypesDataFromSQLite(-1, filePath, inclSuperseeded);
                types = typeData.Item2;
                types.RemoveAt(0); // remove -1 from beginning of list
            }
            else
                types = type_numbers;

            using (var db = Connection(filePath))
            {
                // get section name
                for (int i = 0; i < types.Count; i++)
                {
                    int type = types[i];
                    db.Open();
                    SQLiteCommand cmd = db.CreateCommand();

                    if (inclSuperseeded)
                        cmd.CommandText = $"Select Types.TYPE_ABR || ' ' || SECT_NAME || ' -- ' || SECT_DATE_ADDED as SECT_NAME from Sect INNER JOIN Types ON Sect.SECT_TYPE_NUM = Types.TYPE_NUM where SECT_TYPE_NUM = {type} ORDER BY SECT_AREA";
                    else
                        cmd.CommandText = $"Select Types.TYPE_ABR || ' ' || SECT_NAME as SECT_NAME from Sect INNER JOIN Types ON Sect.SECT_TYPE_NUM = Types.TYPE_NUM where SECT_TYPE_NUM = {type} and not (SECT_SUPERSEDED = True or SECT_SUPERSEDED = TRUE or SECT_SUPERSEDED = 1) ORDER BY SECT_AREA";

                    cmd.CommandType = CommandType.Text;
                    SQLiteDataReader r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        if (inclSuperseeded)
                        {
                            string full = Convert.ToString(r["SECT_NAME"]);
                            // BSI-IPE IPEAA80 -- 2017-09-01 00:00:00.000
                            string profile = full.Split(new string[] { " -- " }, StringSplitOptions.None)[0];
                            string date = full.Split(new string[] { " -- " }, StringSplitOptions.None)[1];
                            date = date.Replace("-", "");
                            date = date.Substring(0, 8);
                            section.Add(profile + " " + date);
                        }
                        else
                        {
                            string profile = Convert.ToString(r["SECT_NAME"]);
                            // BSI-IPE IPEAA80                           
                            section.Add(profile);
                        }

                    }
                    db.Close();
                }
            }

            //section.Insert(0, "All");

            return section;
        }
    }
}
