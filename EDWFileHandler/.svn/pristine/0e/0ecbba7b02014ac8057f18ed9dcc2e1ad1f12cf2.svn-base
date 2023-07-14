using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDWFileHandler
{
    public class PAIConst
    {
        public const int NO_OF_RECORDS_TO_RETRIEVE = 500;
        public const int NO_OF_RECORDS_TO_SAVE = 500;
        public const int NO_OF_PROCESSED_ITEMS_UPDATE = 100;
        public const int NO_OF_ROWS_TO_SAVE_TO_DATABASE = 1000;
        public const String FILE_FILLER_CHARACTER = " ";
        public const String PAI_SENDING_SYSTEM = "SK"; // sending system is always 'SAS' for PAI. Info from Arne/customer 080218 pst
        public const String FFP_PROGRAM_NAME = "SK";
        public const String PAI_MAIL_SENDER_ADDRESS = "donotreplytosas@sas.se";
        public const String PAI_FILEREAD_ENCODING = "ISO-8859-15";   // We are using "ISO-8859-15" encoding that is an updated version of ISO-8859-1 to be able to handle swedish (and other european) characters.
        public const String NULL_DATE = "18000101";
        public const String DATETOTIME_yyyy_MM_dd_hh_mm_ss = "yyyy-MM-dd hh:mm:ss";
        public const String INSTALL_PACKAGE_VSS_VERSION = "Vss version: V170, 2014-06-26";//Previous version:"Vss version: V169, 2014-05-13"; Please update this constant when building BATCH deployment msi package. Format: "Vss version: V7, 2008-04-16" 
        public const String RETRO_RESPONSE_BATCH_USER_NAME = "WEBTSTUSR9";
        public const String OWNER_AIRLINE_EBMS_HIS_LINE = "SK"; // Using this in CreateEbHisLine.cs
        public const String ANY_SEAT_AWARD_IDENTIFIER = "AASKA"; //Used to check the Award ticket

        public const String cs_ACC_FLT_DATA_RECORD = "2";
        public const String cs_ACC_FLT_HEADER_RECORD = "1";
        public const String cs_ACC_FLT_TRAILER_RECORD = "3";
        public const int DuplicateAirtrack = 12;
        public const int ResponseCodeMandFieldsMissing = 13;
        public const int ResponseCodeDuplicteRecord = 14;
        public const int ReservationNotFound = 15;
        public const int ErrorinMatching = 16;
        public const int OutsideHoldingPeriod = 17;        
        public const int WFOnStar = 20;
        public const int ReservationProcessed = 21;
        public const int CheckInDataInitialProcessedStatus= 1;
        public static FileType FileTypeToLoad;
    }
    public static class FileNames // Global UCR Shortname constants
    {
        public const String sFTL_HEADER_CONFIG_FILE = "FTLHeaderTrailerFileMap.xml";
        public const String sFTL_DATA_CONFIG_FILE = "FTLDataFileMap.xml";
        public const String sDCS_HEADER_CONFIG_FILE = "DCSHeaderTrailerFileMap.xml";
        public const String sDCS_DATA_CONFIG_FILE = "DCSDataFileMap.xml";
        public const String sRES_HEADER_CONFIG_FILE = "RESHeaderTrailerFileMap.xml";
        public const String sRES_DATA_CONFIG_FILE = "RESDataFileMap.xml";
        public const String sSRX_HEADER_CONFIG_FILE = "SIRAXINHeaderTrailerFileMap.xml";
        public const String sSRX_DATA_CONFIG_FILE = "SIRAXINDataFileMap.xml";
        public const String sSRX_NO_DOC_HEADER_CONFIG_FILE = "SIRAXINHeaderTrailerFileMapNoDoc.xml";
        public const String sSRX_NO_DOC_DATA_CONFIG_FILE = "SIRAXINDataFileMapNoDoc.xml";
    }

    public static class ResStatus // Global UCR Shortname constants
    {
        public const int ProcessedFromSIRAX = 4;
        public const int Processed = 2;

    }

    public static class AirtrackStatus // Global UCR Shortname constants
    {
        public const int InitialStatus = 0;
        public const int LoadedState = 1;
    }
    public static class AirtrackDatatranferStatus // Global UCR Shortname constants
    {
        public const int InitialStatus = 0;
        public const int Processed = 1;
    }


    public struct LinePosData
    {
        public string sName;
        public int iStart;
        public int iLength;
        public int iStop;
        public int FileRowType;
        public int iFileDataType;
        public int iCodeDataType;
        public int iRequired;
        public string sValidValues; // "01,02,03" -> one of, "A-Z" or "1-120" -> from to.  
        public string sCondMandForFileType; // Conditionally Mandandatory For File Type. ex.: "02" - mandatory for Handback 
        public int iSerial; // identifier of the field
    }

    public struct HeaderFileLinePositions
    {
        public LinePosData RecordRowType;
        public LinePosData SendingSystem;
        public LinePosData Supplier;
        public LinePosData DeliveryDate;
        public LinePosData TypeOfDelivery;
        public LinePosData DeliverySeqNo;
        public LinePosData NoOfRecords;

    }

  

    public struct DataFileLinePositions
    {
        public LinePosData RecordRowType;
        public LinePosData FlightNumber;
        public LinePosData FlightSuffix;
        public LinePosData DepartureDateLocal;                    // _RO Retro Only 
        public LinePosData DepartureStation;
        public LinePosData CardType;
        public LinePosData CardNumber;
        public LinePosData DocumentNumber;
        public LinePosData AutomaticCheckInInd;      
        public LinePosData PassengerName;
        public LinePosData ServiceClass;                          // _AO Accrual Only 
        public LinePosData PNRNumber;
        public LinePosData ArrivalStation;                     // _AO Accrual Only 
        public LinePosData BookingClass;
        public LinePosData PassengerNote;
        public LinePosData IATAAgentNo;
        public LinePosData MarketingFlightNumber;
        public LinePosData DocumentCouponNo;
        public LinePosData FareBasis;
        public LinePosData CompanyConsumptionType;
        public LinePosData CompanyIdentification;
        public LinePosData TicketlessCode;




    }

    public struct SIRAXDataFileLinePositions
    {
        public LinePosData RecordRowType;
        public LinePosData FlightNumber;
        //public LinePosData FlightSuffix;
        public LinePosData DepartureDateLocal;                    // _RO Retro Only 
        public LinePosData DepartureStation;
        //public LinePosData CardType;
        //public LinePosData CardNumber;
        public LinePosData DocumentNumber;
        //public LinePosData AutomaticCheckInInd;
        public LinePosData PassengerName;
        public LinePosData ServiceClass;                      
        //public LinePosData PNRNumber;
        public LinePosData ArrivalStation;                 
        public LinePosData BookingClass;
        //public LinePosData PassengerNote;
        //public LinePosData IATAAgentNo;
        //public LinePosData MarketingFlightNumber;
        public LinePosData DocumentCouponNo;
        public LinePosData FareBasis;
        //public LinePosData CompanyConsumptionType;
        //public LinePosData CompanyIdentification;
        //public LinePosData TicketlessCode;
        public LinePosData Filler;




    }

    public struct TrailerFileLinePositions
    {
        public LinePosData RecordRowType;
        public LinePosData SendingSystem;
        public LinePosData Supplier;
        public LinePosData DeliveryDate;
        public LinePosData TypeOfDelivery;
        public LinePosData DeliverySeqNo;
        public LinePosData NoOfRecords;

    }

    public enum FileType : int
    {
        FTL = 7,//PFS
        DCS = 8, //PAL
        RES = 9,//RES
        SRX = 10,//SIRAX
        DEF = 11,//Default

    }

    public enum DatatransferState : int
    {
        UNDEFINED = 0,
        LOADED = 1,      

    }


    public class FileDataType
    {
        public const int ALPHA_NUMERIC = 1;    // "AN"
        public const int NUMERIC = 2;          // "N"
    }

    public class FileRowType
    {
        public const int HEADER = 1;
        public const int DATA = 2;
        public const int TRAILER = 3;
    }

    public class CodeDataType
    {
        public const int INT = 1;             // "int"
        public const int STRING = 2;          // "String"
        public const int DATETIME = 3;        // "DateTime"
        public const int DECIMAL = 4;         // "Decimal"
        public const int LONG = 5;            // "long"
    }

    public class RequiredField
    {
        public const int MANDATORY = 1;       // "M"
        public const int OPTIONAL = 2;        // "O"
        public const int COND_OPTIONAL = 3;   // "C"
    }


}
