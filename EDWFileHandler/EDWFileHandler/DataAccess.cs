using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;


namespace EDWFileHandler
{
    public class DataAccess
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public bool insertDatatransfer(DATATRANSFEREDWFLT dataTransfer)
        {
            try
            {
                using (var context = new PAIEntities())
                {

                    context.DATATRANSFEREDWFLT.Add(dataTransfer);
                    context.SaveChanges();
                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }

        }

        public bool insertDatatransfer(DATATRANSFERSIRAXIN dataTransfer, ref int DelivNoLocal)
        {
            try
            {
                using (var context = new PAIEntities())
                {

                    context.DATATRANSFERSIRAXIN.Add(dataTransfer);
                    context.SaveChanges();
                    DelivNoLocal = dataTransfer.LOCAL_DELIVNO;
                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }

        }

        public bool insertDatatransfer(DATATRANSFEREDWDCS dataTransfer)
        {
            try
            {
                using (var context = new PAIEntities())
                {

                    context.DATATRANSFEREDWDCS.Add(dataTransfer);
                    context.SaveChanges();
                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }

        }

        public bool insertDatatransfer(DATATRANSFEREDWRES dataTransfer)
        {
            try
            {
                using (var context = new PAIEntities())
                {

                    context.DATATRANSFEREDWRES.Add(dataTransfer);
                    context.SaveChanges();
                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }

        }
        public bool insertData(List<AIRTRACKFTLPFS> airtrackList)
        {
            try
            {
                using (var context = new PAIEntities())
                {

                    context.AIRTRACKFTLPFS.AddRange(airtrackList);

                    context.SaveChanges();

                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while saving FTL data to DB");
                return false;
            }

        }
        public bool insertData(List<AIRTRACK> airtrackList)
        {
            try
            {
                using (var context = new PAIEntities())
                {

                    context.AIRTRACK.AddRange(airtrackList);

                    context.SaveChanges();

                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }

        }

        public bool insertData(List<AIRTRACKDCSPAL> airtrackList)
        {
            try
            {
                using (var context = new PAIEntities())
                {

                    context.AIRTRACKDCSPAL.AddRange(airtrackList);

                    context.SaveChanges();
                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }

        }

        public bool insertData(List<AIRTRACKRES> airtrackList)
        {
            try
            {
                using (var context = new PAIEntities())
                {
                    context.AIRTRACKRES.AddRange(airtrackList);

                    context.SaveChanges();
                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }

        }

        public long insertData(AIRTRACKRES airtrackRes)
        {
            try
            {
                using (var context = new PAIEntities())
                {
                    AIRTRACKRES atkRes = new AIRTRACKRES();
                    context.AIRTRACKRES.Add(airtrackRes);

                    context.SaveChanges();
                    return airtrackRes.AIRTRACK_FTL_PFS_ID;
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return -1;
            }

        }
        public bool updateDataTransferTotRec(DATATRANSFEREDWFLT dataTransfer)
        {
            try
            {
                using (var context = new PAIEntities())
                {
                    DATATRANSFEREDWFLT dtf = new DATATRANSFEREDWFLT();
                    dtf = context.DATATRANSFEREDWFLT.Where(d => d.DELIVNO == dataTransfer.DELIVNO).First();
                    dtf.TOTAL_REC = dataTransfer.TOTAL_REC;
                    dtf.NOOF_REC_RJCTD = dataTransfer.NOOF_REC_RJCTD;
                    dtf.STATUS = dataTransfer.STATUS;
                    dtf.LST_UPDTD = DateTime.Now;
                    context.SaveChanges();

                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }

        }

        public bool updateDataTransferTotRec(DATATRANSFERAIRTRACK dataTransfer)
        {
            try
            {
                using (var context = new PAIEntities())
                {
                    DATATRANSFERAIRTRACK dtf = new DATATRANSFERAIRTRACK();
                    dtf = context.DATATRANSFERAIRTRACK.Where(d => d.DELIVNO == dataTransfer.DELIVNO).FirstOrDefault();
                    dtf.TOTAL_REC = dataTransfer.TOTAL_REC;
                    dtf.STATUS = dataTransfer.STATUS;
                    dtf.LST_UPDTD = DateTime.Now;
                    context.SaveChanges();

                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }

        }

        public bool updateDataTransferTotRec(DATATRANSFERSIRAXIN dataTransfer)
        {
            try
            {
                using (var context = new PAIEntities())
                {
                    DATATRANSFERSIRAXIN dts = new DATATRANSFERSIRAXIN();
                    dts = context.DATATRANSFERSIRAXIN.Where(d => d.LOCAL_DELIVNO == dataTransfer.LOCAL_DELIVNO).FirstOrDefault();
                    dts.TOTAL_REC = dataTransfer.TOTAL_REC;
                    dts.NOOF_REC_RJCTD = dataTransfer.NOOF_REC_RJCTD;
                    dts.STATUS = dataTransfer.STATUS;
                    dts.LST_UPDTD = DateTime.Now;
                    context.SaveChanges();

                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }

        }

        public bool updateDataTransferTotRec(DATATRANSFEREDWDCS dataTransfer)
        {
            try
            {
                using (var context = new PAIEntities())
                {
                    DATATRANSFEREDWDCS dtf = new DATATRANSFEREDWDCS();
                    dtf = context.DATATRANSFEREDWDCS.Where(d => d.DELIVNO == dataTransfer.DELIVNO).First();
                    dtf.TOTAL_REC = dataTransfer.TOTAL_REC;
                    dtf.NOOF_REC_RJCTD = dataTransfer.NOOF_REC_RJCTD;
                    dtf.STATUS = dataTransfer.STATUS;
                    dtf.LST_UPDTD = DateTime.Now;
                    context.SaveChanges();

                    return true;
                }

            }

            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }

        }

        public bool updateDataTransferTotRec(DATATRANSFEREDWRES dataTransfer)
        {
            try
            {
                using (var context = new PAIEntities())
                {
                    DATATRANSFEREDWRES dtf = new DATATRANSFEREDWRES();
                    dtf = context.DATATRANSFEREDWRES.Where(d => d.DELIVNO == dataTransfer.DELIVNO).First();
                    dtf.TOTAL_REC = dataTransfer.TOTAL_REC;
                    dtf.NOOF_REC_RJCTD = dataTransfer.NOOF_REC_RJCTD;
                    dtf.STATUS = dataTransfer.STATUS;
                    dtf.LST_UPDTD = DateTime.Now;

                    context.SaveChanges();

                    return true;
                }

            }

            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }

        }


        public bool insertDatatransfer(DATATRANSFERAIRTRACK dataTransfer)
        {
            try
            {
                int id = 0;
                using (var context = new PAIEntities())
                {

                    id = context.DATATRANSFERAIRTRACK.Add(dataTransfer).DELIVNO;
                    context.SaveChanges();
                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }

        }
        public bool checkDuplicateAirtrack(AIRTRACK airtrack, bool dayChange)
        {
            try
            {
                using (var context = new PAIEntities())
                {
                    AIRTRACK atk = new AIRTRACK();
                    if (dayChange)
                    {
                        //DateTime depDate = airtrack.DATOP ?? DateTime.Now;
                        atk = context.AIRTRACK.Where(d => d.CCID == airtrack.CCID && d.CCTYPME == airtrack.CCTYPME && d.STNON == airtrack.STNON && d.STNOFF == airtrack.STNOFF && d.DATOP >= airtrack.DATOP.AddDays(-1) && d.DATOP <= airtrack.DATOP.AddDays(1)).FirstOrDefault(); //add dep date  
                    }
                    else
                        atk = context.AIRTRACK.Where(d => d.CCID == airtrack.CCID && d.CCTYPME == airtrack.CCTYPME && d.STNON == airtrack.STNON && d.STNOFF == airtrack.STNOFF && d.DATOP == airtrack.DATOP).FirstOrDefault(); //add dep date  


                    if (atk != null)
                    {
                        if (String.IsNullOrEmpty(atk.FBCODE) && !String.IsNullOrEmpty(airtrack.FBCODE))
                            return false;
                        else
                            return true;
                    }
                         
                    return false;

                }

            }

            catch (Exception ex)
            {
                Logger.Fatal(ex);
                throw;
            }

        }

        public bool checkDuplicateAirtrackCheckIn(string DocNo, string StnOn, string StnOff, string FltNo, DateTime Datop, string Ccid, string Cctypme, bool dayChg)
        {
            try
            {
                using (var context = new PAIEntities())
                {
                    AIRTRACK atk = new AIRTRACK();
                    //atk = context.AIRTRACKs.Where(d => d.DOCNO == DocNo && d.STNON == StnOn && d.STNOFF == StnOff && d.DATOP == Datop).FirstOrDefault(); //add dep date
                    if (!dayChg)
                        atk = context.AIRTRACK.Where(d => d.STNON == StnOn && d.STNOFF == StnOff && d.DATOP == Datop && d.CCID == Ccid && d.CCTYPME == Cctypme).FirstOrDefault(); //add dep date  
                    else
                    {
                        atk = context.AIRTRACK.Where(d => d.STNON == StnOn && d.STNOFF == StnOff && d.DATOP == Datop && d.CCID == Ccid && d.FLTNO == FltNo).FirstOrDefault();
                        if (atk == null)
                        {
                            DateTime depDate = Datop.AddDays(-1);
                            atk = context.AIRTRACK.Where(d => d.STNON == StnOn && d.STNOFF == StnOff && d.DATOP == depDate && d.CCID == Ccid && d.FLTNO == FltNo).FirstOrDefault();
                        }
                        if (atk == null)
                        {
                            DateTime depDate = Datop.AddDays(1);
                            atk = context.AIRTRACK.Where(d => d.STNON == StnOn && d.STNOFF == StnOff && d.DATOP == depDate && d.CCID == Ccid && d.FLTNO == FltNo).FirstOrDefault();
                        }

                    }
                    if (atk != null)
                        return true;

                    return false;

                }

            }

            catch (Exception ex)
            {
                Logger.Fatal(ex);
                throw;
            }

        }

        public AIRTRACK checkDuplicateAirtrackRes(string DocNo, string StnOn, string StnOff, string FltNo, DateTime Datop, string Ccid, string Cctypme, bool dayChg)
        {
            try
            {
                using (var context = new PAIEntities())
                {
                    AIRTRACK atk = new AIRTRACK();
                    //atk = context.AIRTRACKs.Where(d => d.DOCNO == DocNo && d.STNON == StnOn && d.STNOFF == StnOff && d.DATOP == Datop).FirstOrDefault(); //add dep date
                    if (!dayChg)
                        atk = context.AIRTRACK.Where(d => d.STNON == StnOn && d.STNOFF == StnOff && d.DATOP == Datop && d.CCID == Ccid && d.FLTNO == FltNo && d.CCTYPME == Cctypme).FirstOrDefault(); //add dep date  
                    else
                    {
                        atk = context.AIRTRACK.Where(d => d.STNON == StnOn && d.STNOFF == StnOff && d.DATOP == Datop && d.CCID == Ccid && d.FLTNO == FltNo && d.CCTYPME == Cctypme).FirstOrDefault();
                        if (atk == null)
                        {
                            DateTime depDate = Datop.AddDays(-1);
                            atk = context.AIRTRACK.Where(d => d.STNON == StnOn && d.STNOFF == StnOff && d.DATOP == depDate && d.CCID == Ccid && d.CCTYPME == Cctypme).FirstOrDefault();
                        }
                        if (atk == null)
                        {
                            DateTime depDate = Datop.AddDays(1);
                            atk = context.AIRTRACK.Where(d => d.STNON == StnOn && d.STNOFF == StnOff && d.DATOP == depDate && d.CCID == Ccid && d.FLTNO == FltNo).FirstOrDefault();
                        }

                    }

                    return atk;

                }

            }

            catch (Exception ex)
            {
                Logger.Fatal(ex);
                throw;
            }

        }
        public bool updateAirtrackResId(long airtrackId, long airtrackResId)
        {
            try
            {
                using (var context = new PAIEntities())
                {
                    AIRTRACK atk = new AIRTRACK();
                    atk = context.AIRTRACK.Where(d => d.AIRTRACKID == airtrackId).First();
                    atk.AIRTRACKRESID = airtrackResId;                 
                    atk.UPDTD_ON = DateTime.Now;
                    context.SaveChanges();

                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }

        }

        public bool checkDuplicateReservation(string DocNo, string StnOn, string StnOff, string FltNo, DateTime Datop, string Ccid, string Cctypme, bool dayChg)
        {
            try
            {
                using (var context = new PAIEntities())
                {

                    //atk = context.AIRTRACKs.Where(d => d.DOCNO == DocNo && d.STNON == StnOn && d.STNOFF == StnOff && d.DATOP == Datop).FirstOrDefault(); //add dep date

                    //var airtrackRes = context.RetreiveReservationRecords(Ccid, StnOn, StnOff, Datop, DocNo, FltNo);

                    //if (airtrackRes != null)
                    //    return true;

                    return false;

                }

            }

            catch (Exception ex)
            {
                Logger.Fatal(ex);
                throw;
            }

        }

        public bool checkDayChangeFlag(DateTime? departureDate, string cityOn, string cityOff)
        {
            try
            {
                bool dayChangeFlag = false;
                using (var context = new PAIEntities())
                {
                    System.Data.Entity.Core.Objects.ObjectParameter isDayChanged = new System.Data.Entity.Core.Objects.ObjectParameter("DAY_CHANGED", typeof(string));

                    context.CheckDayChanged(cityOn, cityOff, departureDate, isDayChanged);

                    if (Convert.ToString(isDayChanged.Value) == "Y")
                        dayChangeFlag = true;

                    return dayChangeFlag;
                }

            }

            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }

        }

        public bool GetStationDetails(string originStn, string destStn, ref string depCity, ref string arrCity)
        {
            try
            {
                System.Data.Entity.Core.Objects.ObjectParameter DepartureCity = new System.Data.Entity.Core.Objects.ObjectParameter("DepCity", typeof(string));
                System.Data.Entity.Core.Objects.ObjectParameter ArrivalCity = new System.Data.Entity.Core.Objects.ObjectParameter("ArrCity", typeof(string));
                System.Data.Entity.Core.Objects.ObjectParameter FlightType = new System.Data.Entity.Core.Objects.ObjectParameter("fltType", typeof(string));
                bool isIntercontinental = false;
                string fltType = "";
                using (var context = new PAIEntities())
                {
                    context.GetFlightDetailsSIRAX(originStn, destStn, FlightType, DepartureCity, ArrivalCity);
                    depCity = Convert.ToString(DepartureCity.Value);
                    arrCity = Convert.ToString(ArrivalCity.Value);
                    fltType = Convert.ToString(FlightType.Value);
                    if (fltType == "IC")
                        isIntercontinental = true;


                    return isIntercontinental;
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                throw;
            }

        }

        public List<AIRTRACKRES> GetResRecords(string docNo, DateTime? departureDate, bool dayChg, bool missingData)
        {
            try
            {
                List<AIRTRACKRES> airtrackResList = new List<AIRTRACKRES>();
                using (var context = new PAIEntities())
                {
                    if (!missingData)
                    {
                        airtrackResList = context.GetResSRXMatchedRecords(dayChg, docNo, departureDate, missingData).Select(d => new AIRTRACKRES
                        {
                            AIRTRACK_FTL_PFS_ID = d.AIRTRACK_FTL_PFS_ID,
                            FLTNO = d.FLTNO,
                            FLTSUF = d.FLTSUF,
                            DATOP = d.DATOP,
                            CCID = d.CCID,
                            CCTYPME = d.CCTYPME,
                            STNON = d.STNON,
                            DOCNO = d.DOCNO,
                            SERVCL = d.SERVCL,
                            RECLOC = d.RECLOC,
                            PSGRNT = d.PSGRNT,
                            PAXNM = d.PAXNM,

                            STNOFF = d.STNOFF,
                            BKGCLS = d.BKGCLS,
                            MKTFLTNO = d.MKTFLTNO,
                            INSERTED_ON = d.INSERTED_ON,
                            PROCESSED_STATUS = d.PROCESSED_STATUS,
                            DTEDWRESID = d.DTEDWRESID,
                            FBCODE = d.FBCODE,
                            COUPNO = d.COUPNO,
                            RESP_CODE = d.RESP_CODE,
                            CMPID = d.CMPID,
                            CMPCNSPNTYPE = d.CMPCNSPNTYPE,
                            AUTOCHECKIN = d.AUTOCHECKIN,
                            TICKETLESSCODE = d.TICKETLESSCODE
                        }).ToList();
                    }
                    else
                    {
                        airtrackResList = context.GetResSRXMatchedRecords(dayChg, docNo, departureDate, missingData).Select(d => new AIRTRACKRES
                        {
                            AIRTRACK_FTL_PFS_ID = d.AIRTRACK_FTL_PFS_ID,
                            FLTNO = d.FLTNO,
                            FLTSUF = d.FLTSUF,
                            DATOP = d.DATOP,
                            CCID = d.CCID,
                            CCTYPME = d.CCTYPME,
                            STNON = d.STNON,
                            DOCNO = d.DOCNO,
                            SERVCL = d.SERVCL,
                            RECLOC = d.RECLOC,
                            PSGRNT = d.PSGRNT,
                            PAXNM = d.PAXNM,

                            STNOFF = d.STNOFF,
                            BKGCLS = d.BKGCLS,
                            MKTFLTNO = d.MKTFLTNO,
                            INSERTED_ON = d.INSERTED_ON,
                            PROCESSED_STATUS = d.PROCESSED_STATUS,
                            DTEDWRESID = d.DTEDWRESID,
                            FBCODE = d.FBCODE,
                            COUPNO = d.COUPNO,
                            RESP_CODE = d.RESP_CODE,
                            CMPID = d.CMPID,
                            CMPCNSPNTYPE = d.CMPCNSPNTYPE,
                            AUTOCHECKIN = d.AUTOCHECKIN,
                            TICKETLESSCODE = d.TICKETLESSCODE
                        }).ToList();
                    }
                    return airtrackResList;
                }

            }

            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return null;
            }
        }

        public AIRTRACKRES GetResRecords( DateTime? departureDate, string StnOn, string StnOff, string fltNo ,string PaxName, bool dayChg, bool missingData)
        {
            try
            {
                AIRTRACKRES airtrackRes = new AIRTRACKRES();
                using (var context = new PAIEntities())
                {
                    if (!missingData)
                    {
                        airtrackRes = context.GetResSRXMatchedRecordsNoDoc(dayChg,PaxName, departureDate,StnOn,StnOff,fltNo,missingData).Select(d => new AIRTRACKRES
                        {
                            AIRTRACK_FTL_PFS_ID = d.AIRTRACK_FTL_PFS_ID,
                            FLTNO = d.FLTNO,
                            FLTSUF = d.FLTSUF,
                            DATOP = d.DATOP,
                            CCID = d.CCID,
                            CCTYPME = d.CCTYPME,
                            STNON = d.STNON,
                            DOCNO = d.DOCNO,
                            SERVCL = d.SERVCL,
                            RECLOC = d.RECLOC,
                            PSGRNT = d.PSGRNT,
                            PAXNM = d.PAXNM,

                            STNOFF = d.STNOFF,
                            BKGCLS = d.BKGCLS,
                            MKTFLTNO = d.MKTFLTNO,
                            INSERTED_ON = d.INSERTED_ON,
                            PROCESSED_STATUS = d.PROCESSED_STATUS,
                            DTEDWRESID = d.DTEDWRESID,
                            FBCODE = d.FBCODE,
                            COUPNO = d.COUPNO,
                            RESP_CODE = d.RESP_CODE,
                            CMPID = d.CMPID,
                            CMPCNSPNTYPE = d.CMPCNSPNTYPE,
                            AUTOCHECKIN = d.AUTOCHECKIN,
                            TICKETLESSCODE = d.TICKETLESSCODE
                        }).FirstOrDefault();
                    }
                    else
                    {
                        airtrackRes= context.GetResSRXMatchedRecordsNoDoc(dayChg, PaxName, departureDate, StnOn, StnOff, fltNo, missingData).Select(d => new AIRTRACKRES
                        {
                            AIRTRACK_FTL_PFS_ID = d.AIRTRACK_FTL_PFS_ID,
                            FLTNO = d.FLTNO,
                            FLTSUF = d.FLTSUF,
                            DATOP = d.DATOP,
                            CCID = d.CCID,
                            CCTYPME = d.CCTYPME,
                            STNON = d.STNON,
                            DOCNO = d.DOCNO,
                            SERVCL = d.SERVCL,
                            RECLOC = d.RECLOC,
                            PSGRNT = d.PSGRNT,
                            PAXNM = d.PAXNM,

                            STNOFF = d.STNOFF,
                            BKGCLS = d.BKGCLS,
                            MKTFLTNO = d.MKTFLTNO,
                            INSERTED_ON = d.INSERTED_ON,
                            PROCESSED_STATUS = d.PROCESSED_STATUS,
                            DTEDWRESID = d.DTEDWRESID,
                            FBCODE = d.FBCODE,
                            COUPNO = d.COUPNO,
                            RESP_CODE = d.RESP_CODE,
                            CMPID = d.CMPID,
                            CMPCNSPNTYPE = d.CMPCNSPNTYPE,
                            AUTOCHECKIN = d.AUTOCHECKIN,
                            TICKETLESSCODE = d.TICKETLESSCODE
                        }).FirstOrDefault();
                    }
                    return airtrackRes;
                }

            }

            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return null;
            }
        }

        public bool updateResStatus(List<long> airtrackIds)
        {
            try
            {
                using (var context = new PAIEntities())
                {

                    var atr = context.AIRTRACKRES.Where(d => airtrackIds.Contains(d.AIRTRACK_FTL_PFS_ID)).ToList();
                    atr.ForEach(ar => ar.PROCESSED_STATUS = ResStatus.ProcessedFromSIRAX);

                    context.SaveChanges();

                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }

        }



        public bool CheckDSN(string delvNo)
        {
            try
            {

                using (var context = new PAIEntities())
                {
                    if (PAIConst.FileTypeToLoad == FileType.FTL)
                    {
                        DATATRANSFEREDWFLT dataTrasfer = new DATATRANSFEREDWFLT();
                        int dsn = Convert.ToInt32(delvNo);
                        dataTrasfer = context.DATATRANSFEREDWFLT.Where(p => p.DELIVNO == dsn && p.STATUS != (int)DatatransferState.UNDEFINED).FirstOrDefault();
                        if (dataTrasfer != null)
                        {
                            return false;
                        }
                    }
                    if (PAIConst.FileTypeToLoad == FileType.DCS)
                    {
                        DATATRANSFEREDWDCS dataTrasfer = new DATATRANSFEREDWDCS();
                        int dsn = Convert.ToInt32(delvNo);
                        dataTrasfer = context.DATATRANSFEREDWDCS.Where(p => p.DELIVNO == dsn && p.STATUS != (int)DatatransferState.UNDEFINED).FirstOrDefault();
                        if (dataTrasfer != null)
                        {
                            return false;
                        }
                    }
                    if (PAIConst.FileTypeToLoad == FileType.RES)
                    {
                        DATATRANSFEREDWRES dataTrasfer = new DATATRANSFEREDWRES();
                        int dsn = Convert.ToInt32(delvNo);
                        dataTrasfer = context.DATATRANSFEREDWRES.Where(p => p.DELIVNO == dsn && p.STATUS != (int)DatatransferState.UNDEFINED).FirstOrDefault();
                        if (dataTrasfer != null)
                        {
                            return false;
                        }
                    }

                    if (PAIConst.FileTypeToLoad == FileType.SRX)
                    {
                        DATATRANSFERSIRAXIN dataTrasfer = new DATATRANSFERSIRAXIN();
                        int dsn = Convert.ToInt32(delvNo);
                        dataTrasfer = context.DATATRANSFERSIRAXIN.Where(p => p.DELIVNO == dsn && p.TYPDELIV=="PAX").FirstOrDefault();
                        if (dataTrasfer != null)
                        {
                            return false;
                        }
                    }

                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                throw;
            }

        }

        public int LoadSuperView()
        {
            try
            {
                using (var context_RV = new EDWREVEXPEntities())
                using (var context_FP = new EDWTESTEntities())
                {


                    // List<FLOWN_PAI> fpList = context_FP.FLOWN_PAI.Where(s=>s.FLOWN_SEG_DEP_DATE<= DateTime.Now.AddDays(-4))


                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
            return 1;
        }

        public bool loadAirtrack()
        {
            try
            {
                Logger.Info("Load Airtrack start, file type {0}", PAIConst.FileTypeToLoad);
                using (var context = new PAIEntities())
                {
                    context.Database.CommandTimeout = 500;
                    if (PAIConst.FileTypeToLoad == FileType.DCS)
                        context.LoadAirtrack();
                    else if (PAIConst.FileTypeToLoad == FileType.FTL)
                        context.LoadAirtrackFromFTL();

                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }

            return true;
        }


        public DateTime? GetLastLoadedResDate()
        {

            DateTime? DATE_OF_INSERT = DateTime.MinValue;
            try
            {
                Logger.Info("Load Airtrack start, file type {0}", PAIConst.FileTypeToLoad);
                using (var context = new PAIEntities())
                {

                    DATATRANSFEREDWRES dtRes = new DATATRANSFEREDWRES();
                    dtRes = context.DATATRANSFEREDWRES.OrderByDescending(s => s.DATE_OF_INSERT).FirstOrDefault();
                    if (dtRes != null)
                    {
                        DATE_OF_INSERT = dtRes.DATE_OF_INSERT;
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return DATE_OF_INSERT;
            }

            return DATE_OF_INSERT;
        }

        public DateTime? GetLastLoadedFTLDate()
        {

            DateTime? DATE_OF_INSERT = DateTime.MinValue;
            try
            {
                Logger.Info("Load Airtrack start, file type {0}", PAIConst.FileTypeToLoad);
                using (var context = new PAIEntities())
                {

                    DATATRANSFEREDWFLT dtRes = new DATATRANSFEREDWFLT();

                    dtRes = context.DATATRANSFEREDWFLT.OrderByDescending(s => s.DATE_OF_INSERT).FirstOrDefault();
                    //DATE_OF_INSERT = dtRes.DATE_OF_INSERT;
                    if (dtRes != null)
                    {
                        DATE_OF_INSERT = dtRes.DATE_OF_INSERT;
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return DATE_OF_INSERT;
            }

            return DATE_OF_INSERT;
        }
    }
}
