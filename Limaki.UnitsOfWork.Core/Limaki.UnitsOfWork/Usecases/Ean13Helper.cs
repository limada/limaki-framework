/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2017 Lytico
 *
 * http://www.limada.org
 * 
 */


using System;
using System.Linq;

namespace Limaki.UnitsOfWork.Usecases {

    public class Ean13Helper {

        public const string FontName = "Code EAN13";

        /// <summary>
        /// converts a barcode-string into a string displayable with font EAN13
        /// </summary>
        /// <returns>string viewable with EAN13-Font</returns>
        /// <param name="barcode">a string with the barcode; should be 12 digits long</param>
        public static string EAN13 (string barcode) {

            if (string.IsNullOrEmpty (barcode))
                return string.Empty;

            barcode = barcode.PadRight (12, '0').Substring (0, 12);
            int i;
            int first;
            int checksum = 0;
            string result = "";
            bool tableA;

            if (barcode.Length == 12 && barcode.All (c => char.IsDigit (c))) {
                // Calculation of the checksum
                for (i = 1; i < 12; i += 2) {
                    checksum += Convert.ToInt32 (barcode.Substring (i, 1));
                }
                checksum *= 3;
                for (i = 0; i < 12; i += 2) {
                    checksum += Convert.ToInt32 (barcode.Substring (i, 1));
                }

                barcode += (10 - checksum % 10) % 10;
                //The first digit is taken just as it is, the second one come from table A
                result = barcode.Substring (0, 1) + (char)(65 + Convert.ToInt32 (barcode.Substring (1, 1)));
                first = Convert.ToInt32 (barcode.Substring (0, 1));
                for (i = 2; i <= 6; i++) {
                    tableA = false;
                    switch (i) {
                        case 2:
                            if (first >= 0 && first <= 3) tableA = true;
                            break;
                        case 3:
                            if (first == 0 || first == 4 || first == 7 || first == 8) tableA = true;
                            break;
                        case 4:
                            if (first == 0 || first == 1 || first == 4 || first == 5 || first == 9) tableA = true;
                            break;
                        case 5:
                            if (first == 0 || first == 2 || first == 5 || first == 6 || first == 7) tableA = true;
                            break;
                        case 6:
                            if (first == 0 || first == 3 || first == 6 || first == 8 || first == 9) tableA = true;
                            break;
                    }

                    if (tableA)
                        result += (char)(65 + Convert.ToInt32 (barcode.Substring (i, 1)));
                    else
                        result += (char)(75 + Convert.ToInt32 (barcode.Substring (i, 1)));
                }
                result += "*"; // Add middle separator

                for (i = 7; i <= 12; i++) {
                    result += (char)(97 + Convert.ToInt32 (barcode.Substring (i, 1)));
                }
                result += "+"; // Add end mark
            }
            return result;
        }

        /// <summary>
        /// a string which give the add-on bar code when it is dispayed with EAN13.TTF font
        /// </summary>
        /// <returns></returns>
        /// <param name="barcode">A 2 or 5 digits length string</param>
        public static string AddOn (string barcode) {

            if (string.IsNullOrEmpty (barcode))
                return string.Empty;

            int i;
            int checksum = 0;
            bool tableA;
            string result = "";


            // Check for 2 or 5 characters And if is digits
            if ((barcode.Length == 2 || barcode.Length == 5) && barcode.All (c => char.IsDigit (c))) {
                // Checksum calculation
                if (barcode.Length == 2)
                    checksum = 10 + Convert.ToInt32 (barcode) % 4;
                else {
                    for (i = 0; i < 5; i += 2) {
                        checksum += Convert.ToInt32 (barcode.Substring (i, 1));
                    }
                    checksum = (checksum * 3 + Convert.ToInt32 (barcode.Substring (1, 1)) * 9 + Convert.ToInt32 (barcode.Substring (3, 1)) * 9) % 10;
                }
                result = "[";

                for (i = 0; i < barcode.Length; i++) {
                    tableA = false;
                    switch (i) {
                        case 0:
                            if ((checksum >= 4 && checksum <= 9) || checksum == 10 || checksum == 11) tableA = true;
                            break;
                        case 1:
                            if (checksum == 1 || checksum == 2 || checksum == 3 || checksum == 5 || checksum == 6
                                || checksum == 9 || checksum == 10 || checksum == 12) tableA = true;
                            break;
                        case 2:
                            if (checksum == 0 || checksum == 2 || checksum == 3 || checksum == 6
                                || checksum == 7 || checksum == 8) tableA = true;
                            break;
                        case 3:
                            if (checksum == 0 || checksum == 1 || checksum == 3 || checksum == 4
                                || checksum == 8 || checksum == 9) tableA = true;
                            break;
                        case 4:
                            if (checksum == 0 || checksum == 1 || checksum == 2 || checksum == 4
                                || checksum == 5 || checksum == 7) tableA = true;
                            break;
                    }
                    if (tableA)
                        result += (char)(65 + Convert.ToInt32 (barcode.Substring (i, 1)));
                    else
                        result += (char)(75 + Convert.ToInt32 (barcode.Substring (i, 1)));

                    if ((barcode.Length == 2 && i == 0) || (barcode.Length == 5 && i < 4)) result += (char)(92); // Add character separator
                }
            }
            return result;
        }
    }
}