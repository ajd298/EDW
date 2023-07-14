using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using static EDWFileHandler.Utility;


namespace EDWFileHandler
{
    public class FormatValidator
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static bool IsValidDateSt(String date)
        {
            try
            {
                DateTime dt = DateTime.ParseExact(date, "yyyyMMdd", null);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool IsNumeric(string value)
        {
            //Create a regular expression
            Regex re = new Regex("[^0-9]");
            if (re.IsMatch(value))
            {
                return false; //value has non numeric characters 
            }
            return true;
        }

        public static bool HandleValidateFieldFormat(string fileType, ref String fieldVal, int requiredType, LinePosData linePosData, String headerLine, int lineNo)
        {
            bool isValid = true;

            String orgFieldVal = Utility.GetStringValue(linePosData, headerLine);
            fieldVal = orgFieldVal;
            requiredType = EvalRequiredType(fileType, linePosData.iRequired, linePosData.sCondMandForFileType);
            isValid &= ValidateFieldFormat(fileType, ref fieldVal, requiredType, linePosData, lineNo);
            if (isValid)
                isValid &= EvalRequiredValueRange(fileType, fieldVal, orgFieldVal, requiredType, linePosData.sValidValues, linePosData.iFileDataType, linePosData.iCodeDataType, linePosData, lineNo);
            return isValid;
        }

        private static int EvalRequiredType(String actualFileType, int required, String CondMandForFileType)
        {
            // TEST CONDITIONAL MANDATORY
            if (required == RequiredField.COND_OPTIONAL)
            {
                if (CondMandForFileType.Length > 0)
                {
                    if (CondMandForFileType.IndexOf(actualFileType) >= 0) // CondMandForFileType could be "02" or "02,03" and so on.
                        return RequiredField.MANDATORY;
                }
            }
            return required;
        }

        public static bool ValidateFieldFormat(String fileType, ref String fieldVal, int requiredType, LinePosData linePosData, int lineNo)
        {
            try
            {
                //int lineNo, pos, len;
                // int respCode = PAIConst.RespCodes.MANDATORY_FIELDS_INVALID_FORMAT_OR_MISSING;
                String fieldValTemp = "";
                int fileDataType = linePosData.iFileDataType;
                int codeDataType = linePosData.iCodeDataType;
                switch (fileDataType)
                {
                    case FileDataType.ALPHA_NUMERIC:
                        //---------------------------------- 
                        // BASIC ALPHANUMERIC VALIDATION
                        //---------------------------------- 
                        // " A2"  => "A2"   OK ACCEPTED 
                        // "ABC"  => "ABC"  OK ACCEPTED 
                        // "   "  => ""     OK ACCEPTED
                        // " 01"  => "01"   OK ACCEPTED 
                        // "000"  => "000"  OK ACCEPTED 
                        // "001"  => "001"  OK ACCEPTED 
                        // "123"  => "123"  OK ACCEPTED  
                        fieldValTemp = fieldVal.Trim();
                        if ((linePosData.sName == "MemberFirstName") || (linePosData.sName == "MemberLastName"))
                        {
                            ; // TO DO: add test that validates Å,Ä,Ö and other country specific characters. All characters are allowed as long...
                        }
                        else if (!IsAlphaNumericWithSpecCharSt(fieldValTemp))
                        { // Special character found
                            if (requiredType == RequiredField.MANDATORY)
                            {
                                if (codeDataType == CodeDataType.STRING)
                                {
                                    // fieldVal = "-"; keep oroginal value.
                                }
                                else if (codeDataType == CodeDataType.DATETIME)
                                {
                                    fieldVal = PAIConst.NULL_DATE; // "18000101";
                                }
                                else // INT, DECIMAL, LONG
                                {
                                    fieldVal = "0";
                                }

                                if (fieldValTemp == "")
                                    return true; // we accept "" as numeric 0, "null" Datetime! (e.g. for response codes.) 
                                else
                                {
                                    //    LogFieldError(fileType, lineNo, linePosData,
                                    //                  String.Format("Non Alphanumeric value '{0}' detected (file datatype: AN, PAI datatype: String).", fieldValTemp),
                                    //                  respCode, ref eRec, ref log);
                                    return false;
                                }
                            }
                            else
                            {
                                if (codeDataType == CodeDataType.STRING)
                                {
                                    // fieldVal = "-"; keep oroginal value.
                                }
                                else if (codeDataType == CodeDataType.DATETIME)
                                {
                                    fieldVal = PAIConst.NULL_DATE; // "18000101";
                                }
                                else // INT, DECIMAL, LONG
                                {
                                    fieldVal = "0";
                                }
                            }
                        }

                        //----------------------------------------------- 
                        // Alphanumeric c# specific DATATYPE VALIDATION
                        //-----------------------------------------------  
                        switch (codeDataType)
                        {
                            case CodeDataType.STRING:
                                if ((requiredType == RequiredField.MANDATORY) && (fieldVal.Trim() == ""))
                                {
                                    //LogFieldError(fileType, lineNo, linePosData,
                                    //              String.Format("Missing mandatory value '{0}' (file datatype: AN, PAI datatype: String).", fieldVal),
                                    //              respCode, ref eRec, ref log);
                                    fieldVal = "-";
                                    return false;
                                }
                                break;
                            case CodeDataType.INT:
                            case CodeDataType.LONG:
                                if (requiredType == RequiredField.MANDATORY)
                                {
                                    if (!IsNumericSt(fieldVal.Trim()))
                                    { // Is not possible to convert to int - NOT ACCEPTED 
                                        //LogFieldError(fileType, lineNo, linePosData,
                                        //              String.Format("Non Numeric Mandatory value '{0}' detected (file datatype: AN, PAI datatype: int).", fieldVal),
                                        //              respCode, ref eRec, ref log);

                                        fieldVal = "0";
                                        return false;
                                    }
                                    else
                                        fieldVal = fieldVal.Trim();
                                }
                                else if (fieldVal.Trim() == "")
                                {
                                    fieldVal = "0"; // Accept this, do not log and reset to 0
                                }
                                else
                                {
                                    if (!IsNumericSt(fieldVal.Trim()))
                                    { // Is not possible to convert to int - NOT ACCEPTED 
                                        //LogFieldError(fileType, lineNo, linePosData,
                                        //              String.Format("Non Numeric non Mandatory value '{0}' detected (file datatype: AN, PAI datatype: int).", fieldVal),
                                        //              respCode, ref eRec, ref log);

                                        fieldVal = "0";
                                        return false;
                                    }
                                    else
                                        fieldVal = fieldVal.Trim();
                                }
                                break;
                            case CodeDataType.DECIMAL:
                                if (requiredType == RequiredField.MANDATORY)
                                {
                                    if (!IsDecimalSt(fieldVal.Trim()))
                                    { // Is not possible to convert to decimal - NOT ACCEPTED 
                                        //LogFieldError(fileType, lineNo, linePosData,
                                        //              String.Format("Non Decimal Mandatory value '{0}' detected (file datatype: AN, PAI datatype: decimal).", fieldVal),
                                        //              respCode, ref eRec, ref log);

                                        fieldVal = "0";
                                        return false;
                                    }
                                    else
                                        fieldVal = fieldVal.Trim();
                                }
                                else if (fieldVal.Trim() == "")
                                {
                                    fieldVal = "0"; // Accept this, do not log and reset to 0
                                }
                                else
                                {
                                    if (!IsDecimalSt(fieldVal.Trim()))
                                    { // Is not possible to convert to int - NOT ACCEPTED 
                                        //LogFieldError(fileType, lineNo, linePosData,
                                        //              String.Format("Non Decimal non Mandatory value '{0}' detected (file datatype: AN, PAI datatype: decimal).", fieldVal),
                                        //              respCode, ref eRec, ref log);

                                        fieldVal = "0";
                                        return false;
                                    }
                                    else
                                        fieldVal = fieldVal.Trim();
                                }
                                break;
                        }
                        break;

                    case FileDataType.NUMERIC:
                        //----------------------------- 
                        // BASIC NUMERIC VALIDATION
                        //----------------------------- 
                        // " A2"  => "A2"  INVALID 
                        // "ABC"  => "ABC" INVALID 
                        // "   "  => 0     UNEXPECTED ACCEPTED
                        // " 01"  => 1     UNEXPECTED ACCEPTED 
                        // "000"  => 0     OK ACCEPTED
                        // "001"  => 1     OK ACCEPTED 
                        // "123"  => 123   OK ACCEPTED 
                        fieldValTemp = fieldVal.Trim();
                        if (!IsNumericSt(fieldValTemp))
                        { // Non numeric value found - NOT ACCEPTED

                            if (requiredType == RequiredField.MANDATORY)
                            {
                                if (codeDataType == CodeDataType.STRING)
                                {
                                    // fieldVal = "-"; keep oroginal value.
                                }
                                else if (codeDataType == CodeDataType.DATETIME)
                                {
                                    fieldVal = PAIConst.NULL_DATE; // "18000101"; 
                                }
                                else // INT, DECIMAL, LONG
                                {
                                    fieldVal = "0";
                                }

                                if (fieldValTemp == "")
                                    return true; // we accept "" as numeric 0, "null" Datetime! (e.g. response codes.) 
                                else
                                {
                                    //LogFieldError(fileType, lineNo, linePosData,
                                    //              String.Format("Non Numeric value '{0}' detected.", fieldValTemp),
                                    //              respCode, ref eRec, ref log);
                                    return false;
                                }
                            }
                            else
                            {
                                if (codeDataType == CodeDataType.STRING)
                                {
                                    // fieldVal = "-"; keep oroginal value.
                                }
                                else if (codeDataType == CodeDataType.DATETIME)
                                {
                                    fieldVal = PAIConst.NULL_DATE; // "18000101";
                                }
                                else // INT, DECIMAL
                                {
                                    fieldVal = "0";
                                }
                                return true;
                            }
                        }
                        else
                        {
                            //------------------------------------------ 
                            // Numeric c# specific DATATYPE VALIDATION
                            //------------------------------------------ 
                            switch (codeDataType)
                            {
                                case CodeDataType.STRING:  // tested before
                                    if ((requiredType == RequiredField.MANDATORY) && (fieldVal.Trim() == ""))
                                    {
                                        //LogFieldError(fileType, lineNo, linePosData,
                                        //              String.Format("Missing mandatory value '{0}' (file datatype: N, PAI datatype: String).", fieldVal),
                                        //              respCode, ref eRec, ref log);

                                        fieldVal = "-";
                                        return false;
                                    }
                                    break;
                                case CodeDataType.DATETIME:
                                    if (requiredType == RequiredField.MANDATORY)
                                    {
                                        if (!IsValidDateSt(fieldVal))
                                        { // Is not possible to convert to DateTime - NOT ACCEPTED
                                            //LogFieldError(fileType, lineNo, linePosData,
                                            //              String.Format("Invalid Date value '{0}' detected.", fieldVal),
                                            //              respCode, ref eRec, ref log);

                                            fieldVal = PAIConst.NULL_DATE; // "18000101";
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        if (!IsValidDateSt(fieldVal))
                                        {
                                            fieldVal = PAIConst.NULL_DATE; // "18000101";
                                        }
                                    }
                                    break;
                                case CodeDataType.INT:
                                case CodeDataType.LONG:
                                    if (requiredType == RequiredField.MANDATORY)
                                    {
                                        // This below was tested above by the line "if (!Validation.IsNumericSt(fieldVal.Trim()))".
                                        //if (!Validation.IsNumericSt(fieldVal.Trim()))
                                        //{ // Is not possible to convert to int - NOT ACCEPTED 
                                        //    LogFieldError(fileType, lineNo, linePosData,
                                        //                  String.Format("Non Numeric Mandatory value '{0}' detected (file datatype: N, PAI datatype: int)."),
                                        //                  respCode, ref eRec, ref log);

                                        //    fieldVal = "0";
                                        //    return false;
                                        //}
                                        //else
                                        fieldVal = fieldVal.Trim();
                                    }
                                    else if (fieldVal.Trim() == "")
                                    {
                                        fieldVal = "0"; // Accept this, do not log and reset to 0
                                    }
                                    else
                                    {
                                        if (!IsNumericSt(fieldVal.Trim()))
                                        { // Is not possible to convert to int - NOT ACCEPTED 
                                            //LogFieldError(fileType, lineNo, linePosData,
                                            //              String.Format("Non Numeric non Mandatory value '{0}' detected (file datatype: N, PAI datatype: int).", fieldVal),
                                            //              respCode, ref eRec, ref log);

                                            fieldVal = "0";
                                            return false;
                                        }
                                        else
                                            fieldVal = fieldVal.Trim();
                                    }
                                    break;
                                    //case CodeDataType.DECIMAL:  // tested before

                                    //    break;
                            }
                        }
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                String errMsg = String.Format(" In ValidateFieldFormat(), lineNo:{'0'}, FieldName:{'1'}, Start:{'2'}, Length:{'3'}, FileType:{'4'}, FieldVal:{'5'}, RequiredType:{'6'}.", lineNo, linePosData.sName, linePosData.iStart, linePosData.iLength, fileType, fieldVal, requiredType);
                // PAI2Logger.LogToFile_Exception(errMsg, ex);
                return false;
            }
        }
        private static bool EvalRequiredValueRange(String fileType, String fieldVal, String orgFieldVal, int requiredType, String validValues, int fileDataType, int codeDataType, LinePosData linePosData, int lineNo)
        {
            String errMsg = "";

            // TEST VALID VALUES
            bool isValid = true;

            try
            {
                if (validValues != "")
                {
                    if (validValues.IndexOf('-') >= 0)
                    {
                        if (validValues.IndexOf(',') >= 0)
                        { // Possible: "+,-" 
                            if ((validValues.IndexOf('+') > 0) && (validValues.IndexOf('-') > 0))
                            { // special case: "+,-" 
                                if ((fieldVal == "+") || (fieldVal == "-"))
                                {
                                    isValid = TestExpectedNotFound(fieldVal, orgFieldVal, fileDataType, requiredType, ref errMsg);
                                    if (!isValid)
                                    {
                                        errMsg += String.Format(" [ValidValues:{0}]", validValues);
                                        //LoadCommon.LogFieldError(fileType, lineNo, linePosData, errMsg, respCode, ref eRec, ref log);
                                    }
                                }
                            }
                        }
                        else
                        { // "A-Z", "1-120"
                            if (validValues == "A-Z")
                            {
                                if (IsAtoZ(fieldVal))
                                { // Not found - not accepted if mandatory 
                                    isValid = TestExpectedNotFound(fieldVal, orgFieldVal, fileDataType, requiredType, ref errMsg);
                                    if (!isValid)
                                    {
                                        errMsg += String.Format(" [ValidValues:{0}]", validValues);
                                        //  LoadCommon.LogFieldError(fileType, lineNo, linePosData, errMsg, respCode, ref eRec, ref log);
                                    }
                                }
                            }
                            else
                            { // "1-120"
                                if (codeDataType == CodeDataType.INT)
                                {
                                    String[] strArray = validValues.Split('-');
                                    if (strArray.Length == 3)
                                    { // expected!
                                        int from = Convert.ToInt32(strArray[0]);
                                        int to = Convert.ToInt32(strArray[2]);
                                        int val = Convert.ToInt32(fieldVal);
                                        if ((val < from) && (val > to))
                                        { // not within - not accepted if mandatory 
                                            isValid = TestExpectedNotFound(fieldVal, orgFieldVal, fileDataType, requiredType, ref errMsg);
                                            if (!isValid)
                                            {
                                                errMsg += String.Format(" [ValidValues:{0}]", validValues);
                                                // LoadCommon.LogFieldError(fileType, lineNo, linePosData, errMsg, respCode, ref eRec, ref log);
                                            }
                                        }
                                    }
                                }
                                else
                                { // UNEXPECTED FATAL! PAI config missmatch!
                                    errMsg = "Very unexpected fatal error. PAI config datatype missmatch in the file(s): AccrualDataFileMap.xml, AccrualHeaderTrailerFileMap.xml, BillingDataFileMap.xml, BillingHeaderTrailerFileMap.xml, RetroRequestDataFileMap.xml or RetroRequestHeaderTrailerFileMap.xml";
                                    // PAI2Logger.LogToFile_Fatal(errMsg + "In EvalRequiredValueRange().");
                                    // LoadCommon.LogFieldError(fileType, lineNo, linePosData, errMsg, respCode, ref eRec, ref log);
                                    isValid = false;
                                }
                            }
                        }
                    }
                    else
                    { // "02", "Y,N", "01,02"
                        if (fileDataType == FileDataType.NUMERIC)
                        {
                            if (fieldVal.Length < linePosData.iLength)
                                fieldVal = fieldVal.PadLeft(linePosData.iLength, '0');
                        }
                        if (validValues.IndexOf(fieldVal) < 0)
                        { // Not found - not accepted if mandatory 
                            isValid = TestExpectedNotFound(fieldVal, orgFieldVal, fileDataType, requiredType, ref errMsg);
                            if (!isValid)
                            {
                                errMsg += String.Format(" [ValidValues:{0}]", validValues);
                                //  LoadCommon.LogFieldError(fileType, lineNo, linePosData, errMsg, respCode, ref eRec, ref log);
                            }
                        }
                    }
                }
                return isValid;
            }
            catch (Exception ex)
            {
                errMsg = String.Format(" In EvalRequiredValueRange(), lineNo:{'0'}, FieldName:{'1'}, Start:{'2'}, Length:{'3'}, FileType:{'4'}, FieldVal:{'5'}, RequiredType:{'6'}.", lineNo, linePosData.sName, linePosData.iStart, linePosData.iLength, fileType, fieldVal, requiredType);
                // PAI2Logger.LogToFile_Exception(errMsg, ex);
                return false;
            }
        }
        private static bool TestExpectedNotFound(string fieldVal, String orgFieldVal, int fileDataType, int requiredType, ref String errMsg)
        {
            if (requiredType == RequiredField.MANDATORY)
            { // This is not accepted if mandatory. out of range 7
                errMsg = String.Format("Unexpected value '{0}' detected for Mandatory field.", orgFieldVal);
                return false;
            }
            else
            { // Not mandatory, "  " (AN) and 0 (N) accepted.
                if (fileDataType != FileDataType.ALPHA_NUMERIC)
                { // "  " is accepted.
                    if (fieldVal.Trim() != "")
                    {
                        if (IsNumericSt(fieldVal.Trim()))
                        {
                            if (Convert.ToInt64(fieldVal.Trim()) != 0) // PAI accepts "000" (== "   ") for AN!
                            {
                                errMsg = String.Format("Unexpected value '{0}' detected for Optional field (file datatype: AN).", orgFieldVal);
                                return false;
                            }
                        }
                    }
                }
                //else // if (fileDataType == FileDataType.NUMERIC) 
                //{ // Only 0 is accepted
                //    if (Convert.ToInt64(fieldVal) != 0) // (just convert, numeric test was performed before) 
                //    {
                //        errMsg = String.Format("Unexpected value '{0}' detected for Optional field (file datatype: N).", orgFieldVal);
                //        return false;
                //    }
                //}
                else if (fileDataType == FileDataType.NUMERIC)
                {// Only 0 is accepted
                    if (Convert.ToInt64(fieldVal) != 0)
                    {
                        errMsg = String.Format("Unexpected value '{0}' detected for Optional field (file datatype: N).", orgFieldVal);
                        return false;
                    }
                }

            }
            //isValid = false; // out of range 9
            //LoadCommon.LogFieldError(fileType, lineNo, linePosData,
            //   String.Format("Required value '{0}' Mandatory out of range 9 detected.", fieldVal),
            //   respCode, ref eRec, ref log);
            return true;
        }
        public static bool IsNumericSt(string value)
        {
            if (value != null)
            {
                if (value != "")
                {
                    Regex re = new Regex("[^0-9]");
                    if (re.IsMatch(value))
                    {
                        return false; //value has non numeric characters 
                    }
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }
        public static bool IsAtoZ(string value)
        {
            
            if (Regex.IsMatch(value, @"^[A-Z]+$"))
            {
                return false;
            }
            return true;
        }

        public static bool IsAlphaNumericWithSpecCharSt(string value)
        {
            //Create a regular expression
            Regex re = new Regex("[^0-9A-Za-z_\\/\\s\\.\\,\\+\\-\\@]");
            if (re.IsMatch(value))
            {
                return false; // value has non alphabetic / non space characters
            }
            return true;
        }
        public static bool IsDecimalSt(string value)
        {
            if (value != null)
            {
                if (value != "")
                {
                    Regex re = new Regex("[^0-9.]");
                    if (re.IsMatch(value))
                    {
                        return false; // value has non numeric and non decimal characters
                    }
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public static bool ValidateFileAndRecCnt(String fullFileName, ref int recordCount, ref string trailerLine)
        {
            Logger.Info("Validate Header, Trailer and data");
            System.IO.StreamReader oStreamReader = new System.IO.StreamReader(fullFileName);
            try
            {
               
                String sFileLine;
                String sRecordType;
                bool bHeaderExists = false;
                //bool bDataRecExists = false;
                bool bTrailerExists = false;

                Int32 iCounter = 0;                
                sFileLine = oStreamReader.ReadLine();                            
                sRecordType = sFileLine.Substring(0, 1);

                while (sFileLine != null)
                {
                    if (sFileLine.Length > 2) // avoid last (or additional) empty line exception where length is 1-2
                    {
                        sRecordType = sFileLine.Substring(0, 1);
                        if (sRecordType == PAIConst.cs_ACC_FLT_HEADER_RECORD)
                        {
                            
                            bHeaderExists = true;
                        }
                        else if (sRecordType == PAIConst.cs_ACC_FLT_DATA_RECORD) // Check for Data Records only
                        {
                            iCounter++;
                            //bDataRecExists = true;
                        }
                        else if (sRecordType == PAIConst.cs_ACC_FLT_TRAILER_RECORD) // Check for Trailer Record only
                        {
                            trailerLine = sFileLine;
                            bTrailerExists = true;
                        }
                    }                

                    sFileLine = oStreamReader.ReadLine();
                }



                if (bHeaderExists == false)
                {
                    Logger.Error(String.Format("File load Header not found (fullFileName: '{0}') in Validation.ValidateFileAndRecCnt()", fullFileName));
                }
                //else if (bDataRecExists == false)
                //{
                //    PAI2Logger.LogToFile_Error(String.Format("File load DataRecords not found (fullFileName: '{0}') in Validation.ValidateFileAndRecCnt()", fullFileName));
                //    errRecord.ErrorString = "DataRecords Not Found.";
                //    reportLog.Sections[1].ErrorRecords.Add(errRecord);
                //}
                else if (bTrailerExists == false)
                {
                    Logger.Error(String.Format("File load Trailer not found (fullFileName: '{0}') in Validation.ValidateFileAndRecCnt()", fullFileName));
                }

                if (bTrailerExists)
                {
                    recordCount = iCounter;
                }

                //if ((iRecCount == iCounter) && (bHeaderExists == true) && (bDataRecExists == true) && (bTrailerExists == true))
                if ((iCounter == 0) || (bHeaderExists == false) || (bTrailerExists == false))
                {
                    if (iCounter == 0 && (bHeaderExists == true) && (bTrailerExists == true))
                        Logger.Error(String.Format("Record count is 0 for (fullFileName: '{0}') in Validation.ValidateFileAndRecCnt()", fullFileName));
                    else
                        return false;

                  

                    return true;
                }

                return true;
            }
            catch(Exception ex)
            {
                Logger.Fatal(ex);
                throw;
            }
            finally
            {
                oStreamReader.Close();
            }
        }
    }


}
