using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Net.Mail;
using System.Configuration;

namespace EDWFileHandler
{
    public class Utility
    {

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public static HeaderFileLinePositions oHeaderFileLinePositions;   // Header File Line Positions
        public static DataFileLinePositions oDataFileLinePositions;       // Data File Line Positions
        public static TrailerFileLinePositions oTrailerFileLinePositions; // Trailer File Line Positions
        public static SIRAXDataFileLinePositions oSIRAXDataFileLinePositions;       // Data File Line Positions



        public static bool InitInputFileConfigData(FileType enFileType, string sFileHeaderTrailerConfigXMLFile, string sFileDataConfigXMLFile, string delivType = "")
        {
            Logger.Info("Initiating load Configs");
            DataTable oDataTable_FileHeaderTrailerCfg = ReadXMLToDataTable(sFileHeaderTrailerConfigXMLFile);
            DataTable oDataTable_FileDataCfg = ReadXMLToDataTable(sFileDataConfigXMLFile);

            InitFileHeaderTrailerCfg(enFileType, oDataTable_FileHeaderTrailerCfg);

            if (PAIConst.FileTypeToLoad == FileType.SRX)
            {
                if (delivType != "PAX")
                    InitSIRAXFileDataCfg(enFileType, oDataTable_FileDataCfg);
                else if(delivType == "PAX")
                    InitSIRAXFileDataCfgNoDoc(enFileType, oDataTable_FileDataCfg);
            }
            else
                InitFileDataCfg(enFileType, oDataTable_FileDataCfg);
            Logger.Info("Load Configs successful.");
            return true;

        }
        private static DataTable ReadXMLToDataTable(string sXMLFilePathAndName)
        {
            DataSet oDataSet = new DataSet();
            oDataSet.ReadXml(sXMLFilePathAndName);
            DataTable oDataTable = oDataSet.Tables[0];
            return oDataTable;
        }

        private static bool InitFileHeaderTrailerCfg(FileType enFileType, DataTable dtFileLinePosFormat)
        {

            try
            {
                LoadPosition(ref oHeaderFileLinePositions.RecordRowType, "Header", "RecordRowType", dtFileLinePosFormat);
                LoadPosition(ref oHeaderFileLinePositions.SendingSystem, "Header", "SendingSystem", dtFileLinePosFormat);
                LoadPosition(ref oHeaderFileLinePositions.Supplier, "Header", "Supplier", dtFileLinePosFormat);
                LoadPosition(ref oHeaderFileLinePositions.DeliveryDate, "Header", "DeliveryDate", dtFileLinePosFormat);
                LoadPosition(ref oHeaderFileLinePositions.TypeOfDelivery, "Header", "TypeOfDelivery", dtFileLinePosFormat);
                LoadPosition(ref oHeaderFileLinePositions.DeliverySeqNo, "Header", "DeliverySeqNo", dtFileLinePosFormat);

                // Trailer               

                LoadPosition(ref oTrailerFileLinePositions.RecordRowType, "Trailer", "RecordRowType", dtFileLinePosFormat);
                LoadPosition(ref oTrailerFileLinePositions.SendingSystem, "Trailer", "SendingSystem", dtFileLinePosFormat);
                LoadPosition(ref oTrailerFileLinePositions.Supplier, "Trailer", "Supplier", dtFileLinePosFormat);
                LoadPosition(ref oTrailerFileLinePositions.DeliveryDate, "Trailer", "DeliveryDate", dtFileLinePosFormat);
                LoadPosition(ref oTrailerFileLinePositions.TypeOfDelivery, "Trailer", "TypeOfDelivery", dtFileLinePosFormat);
                LoadPosition(ref oTrailerFileLinePositions.DeliverySeqNo, "Trailer", "DeliverySeqNo", dtFileLinePosFormat);
                LoadPosition(ref oTrailerFileLinePositions.NoOfRecords, "Trailer", "NoOfRecords", dtFileLinePosFormat);


                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }
        }

        private static bool InitFileDataCfg(FileType enFileType, DataTable dtFileLinePosFormat)
        {
            try
            {
                LoadPosition(ref oDataFileLinePositions.ArrivalStation, "Data", "ArrivalStation", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.AutomaticCheckInInd, "Data", "AutomaticCheckInInd", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.BookingClass, "Data", "BookingClass", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.CardNumber, "Data", "CardNumber", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.CardType, "Data", "CardType", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.DepartureDateLocal, "Data", "DepartureDateLocal", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.DepartureStation, "Data", "DepartureStation", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.DocumentCouponNo, "Data", "DocumentCouponNo", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.DocumentNumber, "Data", "DocumentNumber", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.FlightNumber, "Data", "FlightNumber", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.FlightSuffix, "Data", "FlightSuffix", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.MarketingFlightNumber, "Data", "MarketingFlightNumber", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.PassengerName, "Data", "PassengerName", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.PassengerNote, "Data", "PassengerNote", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.PNRNumber, "Data", "PNRNumber", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.ServiceClass, "Data", "ServiceClass", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.FareBasis, "Data", "FareBasis", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.CompanyConsumptionType, "Data", "CompanyConsumptionType", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.CompanyIdentification, "Data", "CompanyIdentification", dtFileLinePosFormat);
                LoadPosition(ref oDataFileLinePositions.TicketlessCode, "Data", "TicketlessCode", dtFileLinePosFormat);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }


            return true;
        }

        private static bool InitSIRAXFileDataCfg(FileType enFileType, DataTable dtFileLinePosFormat)
        {
            try
            {
                LoadPosition(ref oSIRAXDataFileLinePositions.ArrivalStation, "Data", "ArrivalStation", dtFileLinePosFormat);
                //// LoadPosition(ref oSIRAXDataFileLinePositions.AutomaticCheckInInd, "Data", "AutomaticCheckInInd", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.BookingClass, "Data", "BookingClass", dtFileLinePosFormat);
                // LoadPosition(ref oSIRAXDataFileLinePositions.CardNumber, "Data", "CardNumber", dtFileLinePosFormat);
                // LoadPosition(ref oSIRAXDataFileLinePositions.CardType, "Data", "CardType", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.DepartureDateLocal, "Data", "DepartureDateLocal", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.DepartureStation, "Data", "DepartureStation", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.DocumentCouponNo, "Data", "DocumentCouponNo", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.DocumentNumber, "Data", "DocumentNumber", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.FlightNumber, "Data", "FlightNumber", dtFileLinePosFormat);
                // LoadPosition(ref oSIRAXDataFileLinePositions.FlightSuffix, "Data", "FlightSuffix", dtFileLinePosFormat);
                //LoadPosition(ref oSIRAXDataFileLinePositions.MarketingFlightNumber, "Data", "MarketingFlightNumber", dtFileLinePosFormat);
                //  LoadPosition(ref oSIRAXDataFileLinePositions.PassengerName, "Data", "PassengerName", dtFileLinePosFormat);
                // LoadPosition(ref oSIRAXDataFileLinePositions.PassengerNote, "Data", "PassengerNote", dtFileLinePosFormat);
                //LoadPosition(ref oSIRAXDataFileLinePositions.PNRNumber, "Data", "PNRNumber", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.ServiceClass, "Data", "ServiceClass", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.FareBasis, "Data", "FareBasis", dtFileLinePosFormat);
                //LoadPosition(ref oSIRAXDataFileLinePositions.CompanyConsumptionType, "Data", "CompanyConsumptionType", dtFileLinePosFormat);
                // LoadPosition(ref oSIRAXDataFileLinePositions.CompanyIdentification, "Data", "CompanyIdentification", dtFileLinePosFormat);
                // LoadPosition(ref oSIRAXDataFileLinePositions.TicketlessCode, "Data", "TicketlessCode", dtFileLinePosFormat);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }


            return true;
        }

        private static bool InitSIRAXFileDataCfgNoDoc(FileType enFileType, DataTable dtFileLinePosFormat)
        {
            try
            {
                LoadPosition(ref oSIRAXDataFileLinePositions.ArrivalStation, "Data", "ArrivalStation", dtFileLinePosFormat);
                //// LoadPosition(ref oSIRAXDataFileLinePositions.AutomaticCheckInInd, "Data", "AutomaticCheckInInd", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.BookingClass, "Data", "BookingClass", dtFileLinePosFormat);
                // LoadPosition(ref oSIRAXDataFileLinePositions.CardNumber, "Data", "CardNumber", dtFileLinePosFormat);
                // LoadPosition(ref oSIRAXDataFileLinePositions.CardType, "Data", "CardType", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.DepartureDateLocal, "Data", "DepartureDateLocal", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.DepartureStation, "Data", "DepartureStation", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.DocumentCouponNo, "Data", "DocumentCouponNo", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.DocumentNumber, "Data", "DocumentNumber", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.FlightNumber, "Data", "FlightNumber", dtFileLinePosFormat);
                // LoadPosition(ref oSIRAXDataFileLinePositions.FlightSuffix, "Data", "FlightSuffix", dtFileLinePosFormat);
                //LoadPosition(ref oSIRAXDataFileLinePositions.MarketingFlightNumber, "Data", "MarketingFlightNumber", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.PassengerName, "Data", "PassengerName", dtFileLinePosFormat);
                // LoadPosition(ref oSIRAXDataFileLinePositions.PassengerNote, "Data", "PassengerNote", dtFileLinePosFormat);
                //LoadPosition(ref oSIRAXDataFileLinePositions.PNRNumber, "Data", "PNRNumber", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.ServiceClass, "Data", "ServiceClass", dtFileLinePosFormat);
                LoadPosition(ref oSIRAXDataFileLinePositions.FareBasis, "Data", "FareBasis", dtFileLinePosFormat);
                //LoadPosition(ref oSIRAXDataFileLinePositions.CompanyConsumptionType, "Data", "CompanyConsumptionType", dtFileLinePosFormat);
                // LoadPosition(ref oSIRAXDataFileLinePositions.CompanyIdentification, "Data", "CompanyIdentification", dtFileLinePosFormat);
                // LoadPosition(ref oSIRAXDataFileLinePositions.TicketlessCode, "Data", "TicketlessCode", dtFileLinePosFormat);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }


            return true;
        }
        private static void LoadPosition(ref LinePosData oLinePosData, String sRowType, String sFieldId, DataTable dtFileLinePosFormat)
        {
            DataRow oDataRow;
            // Example: "RowType = 'Header' AND Name = 'RecordRowType'"
            String sSelect = "RowType = '" + sRowType + "' AND Name = '" + sFieldId + "'";
            oDataRow = dtFileLinePosFormat.Select(sSelect)[0];

            oLinePosData.sName = oDataRow["Name"].ToString();

            if (oLinePosData.sName == "CheckinSourceCode_AO")
                oLinePosData.sName = oLinePosData.sName;

            oLinePosData.iStart = int.Parse(oDataRow["Start"].ToString());
            oLinePosData.iLength = int.Parse(oDataRow["Length"].ToString());
            //oLinePosData.iStop = oLinePosData.iLength + 1 - oLinePosData.iStart;
            oLinePosData.iStop = oLinePosData.iStart + oLinePosData.iLength - 1;

            String sTemp = oDataRow["FileDataType"].ToString();
            if (sTemp.ToUpper() == "N")
                oLinePosData.iFileDataType = FileDataType.NUMERIC;
            else // if(sTemp.ToUpper() == "AN")
                oLinePosData.iFileDataType = FileDataType.ALPHA_NUMERIC;

            sTemp = oDataRow["RowType"].ToString();
            if (sTemp.ToUpper() == "HEADER")
                oLinePosData.FileRowType = FileRowType.HEADER;
            else if (sTemp.ToUpper() == "DATA")
                oLinePosData.FileRowType = FileRowType.DATA;
            else // if (sTemp.ToUpper() == "TRAILER")
                oLinePosData.FileRowType = FileRowType.TRAILER;

            sTemp = oDataRow["CodeDataType"].ToString();
            if (sTemp.ToUpper() == "INT")
                oLinePosData.iCodeDataType = CodeDataType.INT;
            else if (sTemp.ToUpper() == "DATETIME")
                oLinePosData.iCodeDataType = CodeDataType.DATETIME;
            else if (sTemp.ToUpper() == "DECIMAL")
                oLinePosData.iCodeDataType = CodeDataType.DECIMAL;
            else if (sTemp.ToUpper() == "LONG")
                oLinePosData.iCodeDataType = CodeDataType.LONG;
            else // if((sTemp.ToUpper() == "STRING")
                oLinePosData.iCodeDataType = CodeDataType.STRING;

            sTemp = oDataRow["Required"].ToString();
            if (sTemp.ToUpper() == "O")
                oLinePosData.iRequired = RequiredField.OPTIONAL;
            else if (sTemp.ToUpper() == "C")
                oLinePosData.iRequired = RequiredField.COND_OPTIONAL;
            else //if(sTemp.ToUpper() == "M")
                oLinePosData.iRequired = RequiredField.MANDATORY;

            oLinePosData.sValidValues = oDataRow["ValidValues"].ToString();
            oLinePosData.sCondMandForFileType = oDataRow["CondMandForFileType"].ToString();
            oLinePosData.iSerial = int.Parse(oDataRow["Serial"].ToString());
        }

        public static int GetIntValueConvertEmptyStringToZero(LinePosData oLinePosData, String sFileLine)
        {
            int iStartPosition = oLinePosData.iStart - 1;
            int iStopPosition = oLinePosData.iStop - 1;
            int iLength = oLinePosData.iLength;
            int iLineLength = sFileLine.Length;
            String sInt = "";

            if (iLineLength >= (iStartPosition + iLength))
                sInt = sFileLine.Substring(iStartPosition, iLength).Trim();
            else if (iLineLength > iStartPosition)
                sInt = sFileLine.Substring(iStartPosition, iLineLength - iStartPosition).Trim();
            else
                return -1;

            if (sInt.Length > 0)
            {
                if (FormatValidator.IsNumeric(sInt))
                    return int.Parse(sInt);
                else// Unexpected (could be logged) Alphanumeric, this should be handled by the format validator.
                    return 0;
            }
            else // String with spaces ("  ") interprets as 0 - unexpected, but acceted 
                return 0;
        }

        public static String GetStringValue(LinePosData oLinePosData, String sFileLine)
        {
            int iStartPosition = oLinePosData.iStart - 1;
            int iStopPosition = oLinePosData.iStop - 1;
            int iLength = oLinePosData.iLength;
            int iLineLength = sFileLine.Length;

            if (iLineLength >= (iStartPosition + iLength))
                return sFileLine.Substring(iStartPosition, iLength).Trim();
            else if (iLineLength > iStartPosition)
                return sFileLine.Substring(iStartPosition, iLineLength - iStartPosition).Trim();
            else
                return "";
        }

        public static DateTime GetDateTimeValue(LinePosData oLinePosData, String sFileLine)
        {
            int iStartPosition = oLinePosData.iStart - 1;
            int iStopPosition = oLinePosData.iStop - 1;
            int iLength = oLinePosData.iLength;
            int iLineLength = sFileLine.Length;
            String sDate = "";
            DateTime oDate = new DateTime(1800, 01, 01);

            try
            {
                if (iLineLength >= (iStartPosition + iLength))
                    sDate = sFileLine.Substring(iStartPosition, iLength).Trim();
                else if (iLineLength > iStartPosition)
                    sDate = sFileLine.Substring(iStartPosition, iLineLength - iStartPosition).Trim();
                else
                    return new DateTime(1800, 01, 01);

                if (sDate == "00000000")
                    sDate = sDate;

                if (sDate.Length > 0)
                {
                    if (FormatValidator.IsValidDateSt(sDate))
                    {
                        //oDate = new DateTime(int.Parse(sDate.Substring(0, 4)),
                        //        int.Parse(sDate.Substring(4, 2)), int.Parse(sDate.Substring(6, 2)));
                        oDate = DateTime.ParseExact(sDate, "yyyyMMdd", null);
                    }
                    else // Unexpected (could be logged) Alphanumeric, this should be handled by the format validator.
                        return new DateTime(1800, 01, 01);
                }
                else // String with spaces ("  ") interprets as null date - unexpected, but acceted 
                    return new DateTime(1800, 01, 01);
            }
            catch (Exception ex)
            {
                //PAI2Logger.LogToFile_Exception(String.Format("In GetDateTimeValue(), date:'{0}'", sDate), ex);
            }
            return oDate;
        }




        public static void Send(MailMessage message)
        {
            try
            {
                string host = ConfigurationManager.AppSettings["MailServer"];
                string port = ConfigurationManager.AppSettings["MailServerPort"];
                string sender = ConfigurationManager.AppSettings["SenderMailAddress"];
                string receivers = ConfigurationManager.AppSettings["ReceiverMailAddress"];
                message.To.Add(receivers);
                message.IsBodyHtml = true;
                SmtpClient client = new SmtpClient(host, Convert.ToInt32(port));
                message.From = new MailAddress(sender);
                client.Timeout = 2000;
                client.Send(message);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

    }
}
