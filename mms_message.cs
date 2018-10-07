﻿
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace wp2droidMsg
{
    public class MmsPart
    {
        private int m_nSeq;
        private string m_sCt;
        private string m_sName;
        private string m_sChset;
        private string m_sCd;
        private string m_sFn;
        private string m_sCid;
        private string m_sCl;
        private string m_sCtt_s;
        private string m_sCtt_t;
        private string m_sText;
        private string m_sData; //optional
        private int m_nSef_type;

        public MmsPart()
        {
        }

        public string Text => m_sText;

        struct ContentTypeInfo
        {
            public string sCid;
            public string sExt;

            public ContentTypeInfo(string sCid, string sExt)
            {
                this.sCid = sCid;
                this.sExt = sExt;
            }
        }

        private static Dictionary<string, ContentTypeInfo> s_mpContentTypeInfo =
            new Dictionary<string, ContentTypeInfo>
            {
                {"application/smil", new ContentTypeInfo("smil", "xml")},
                {"image/jpeg", new ContentTypeInfo("image", "jpg")},
                {"image/png", new ContentTypeInfo("image", "png")},
                {"image/gif", new ContentTypeInfo("image", "gif")},
                {"text/x-vCard", new ContentTypeInfo("vCard", "xml")},
                {"text/plain", new ContentTypeInfo("text", "txt")}
            };

        static ContentTypeInfo GetContentTypeInfo(string sContentType)
        {
            if (s_mpContentTypeInfo.ContainsKey(sContentType))
                return s_mpContentTypeInfo[sContentType];

            return new ContentTypeInfo("unknown", "");
        }

        /*----------------------------------------------------------------------------
        	%%Function: CreateFromWpAttachment
        	%%Qualified: wp2droidMsg.MmsPart.CreateFromWpAttachment
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public static MmsPart CreateFromWpAttachment(WpMessageAttachment att, ref int seq, int locationIndex)
        {
            MmsPart mmsp = new MmsPart();

            if (att.ContentType == "application/smil")
            {
                mmsp.m_nSeq = -1;
            }
            else
            {
                mmsp.m_nSeq = seq++;
            }

            mmsp.m_sCt = att.ContentType;
            mmsp.m_sName = null;
            mmsp.m_sCd = null;
            mmsp.m_sFn = null;
            ContentTypeInfo cti = GetContentTypeInfo(mmsp.m_sCt);

            if (mmsp.m_sCt == "text/plain")
            {
                mmsp.m_sChset = MmsMessage.s_CHARSET_UTF8.ToString();
                Decoder d = Encoding.Unicode.GetDecoder();

                byte[] rgb = Convert.FromBase64String(att.Data);
                string s = System.Text.Encoding.Unicode.GetString(rgb);

                mmsp.m_sText = s;
                mmsp.m_sData = null;
            }
            else
            {
                mmsp.m_sChset = null;
                mmsp.m_sData = att.Data;
            }

            mmsp.m_sCid = $"<{cti.sCid}>";
            mmsp.m_sCl = $"{cti.sCid}{locationIndex:D5}.{cti.sExt}";
            mmsp.m_sCtt_s = null;
            mmsp.m_sCtt_t = null;

            return mmsp;
        }

        public static void ParseDroidPartAttribute(XmlReader xr, MmsPart part)
        {
            switch (xr.Name)
            {
                case "seq": part.m_nSeq = XmlIO.ReadGenericIntElement(xr, "seq") ?? 0; break;
                case "ct": part.m_sCt = XmlIO.ReadGenericStringElement(xr, "ct"); break;
                case "name": part.m_sName = XmlIO.ReadGenericNullableStringElement(xr, "name"); break;
                case "chset": part.m_sChset= XmlIO.ReadGenericNullableStringElement(xr, "chset"); break;
                case "cd": part.m_sCd = XmlIO.ReadGenericNullableStringElement(xr, "cd"); break;
                case "fn": part.m_sFn = XmlIO.ReadGenericNullableStringElement(xr, "fn"); break;
                case "cid": part.m_sCid = XmlIO.ReadGenericNullableStringElement(xr, "cid"); break;
                case "cl": part.m_sCl = XmlIO.ReadGenericNullableStringElement(xr, "cl"); break;
                case "ctt_s": part.m_sCtt_s = XmlIO.ReadGenericNullableStringElement(xr, "ctt_s"); break;
                case "ctt_t": part.m_sCtt_t = XmlIO.ReadGenericNullableStringElement(xr, "ctt_t"); break;
                case "text": part.m_sText = XmlIO.ReadGenericNullableStringElement(xr, "text"); break;
                case "sef_type": part.m_nSef_type = XmlIO.ReadGenericIntElement(xr, "sef_type") ?? 0; break;
                case "data": part.m_sData = XmlIO.ReadGenericNullableStringElement(xr, "data"); break;
            }
        }

        public static MmsPart CreateFromDroidXml(XmlReader xr)
        {
            return XmlReadTemplates<MmsPart>.ParseSingleElementWithAttributes(xr, "part", ParseDroidPartAttribute);
        }
        

        /*----------------------------------------------------------------------------
        	%%Function: CreatePartsFromDroidXml
        	%%Qualified: wp2droidMsg.MmsPart.CreatePartsFromDroidXml
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public static List<MmsPart> CreatePartsFromDroidXml(XmlReader xr)
        {
            return XmlReadTemplates<MmsPart>.ReadListOfSingleElements(xr, "parts", "part", CreateFromDroidXml);
        }

        public void WriteToDroidXml(XmlWriter xw)
        {
            xw.WriteStartElement("part");
            xw.WriteAttributeString("seq", m_nSeq.ToString());

            xw.WriteAttributeString("ct", MmsMessage.Nullable(m_sCt));   // converted
            xw.WriteAttributeString("name", MmsMessage.Nullable(m_sName));   // converted
            xw.WriteAttributeString("chset", MmsMessage.Nullable(m_sChset));   // converted
            xw.WriteAttributeString("cd", MmsMessage.Nullable(m_sCd));   // converted
            xw.WriteAttributeString("fn", MmsMessage.Nullable(m_sFn));   // converted
            xw.WriteAttributeString("cid", MmsMessage.Nullable(m_sCid));   // converted
            xw.WriteAttributeString("cl", MmsMessage.Nullable(m_sCl));   // converted
            xw.WriteAttributeString("ctt_s", MmsMessage.Nullable(m_sCtt_s));   // converted
            xw.WriteAttributeString("ctt_t", MmsMessage.Nullable(m_sCtt_t));   // converted
            xw.WriteAttributeString("text", MmsMessage.Nullable(m_sText));   // converted
            xw.WriteAttributeString("sef_type", m_nSef_type.ToString());   // converted
            if (m_sData != null)
                xw.WriteAttributeString("data", MmsMessage.Nullable(m_sData));   // converted

            xw.WriteEndElement();
        }
    }

    public class MmsAddress
    {
        // there is no schema for this published, this is just observed from a backup file
        private string m_sAddress;
        private int m_nType;
        private int m_nCharset;

        // from PduHeaders class:
        public static int s_AddressTypeFrom = 137;
        public static int s_AddressTypeBCC = 129;
        public static int s_AddressTypeCC = 130;
        public static int s_AddressTypeTo = 151;

        public string Address => m_sAddress;

        public MmsAddress()
        {
        }

        public MmsAddress(string sAddress, int nType, int nCharset)
        {
            m_sAddress = sAddress;
            m_nType = nType;
            m_nCharset = nCharset;
        }

        public static void ParseDroidMmsAttribute(XmlReader xr, MmsAddress addr)
        {
            switch (xr.Name)
            {
                case "address":
                    addr.m_sAddress = XmlIO.ReadGenericNullableStringElement(xr, "address");
                    break;
                case "type":
                    addr.m_nType = XmlIO.ReadGenericIntElement(xr, "type") ?? 0;
                    break;
                case "charset":
                    addr.m_nCharset = XmlIO.ReadGenericIntElement(xr, "charset") ?? 0;
                    break;
            }
        }

        /*----------------------------------------------------------------------------
        	%%Function: CreateAddressesFromDroidXml
        	%%Qualified: wp2droidMsg.MmsAddress.CreateAddressesFromDroidXml
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public static List<MmsAddress> CreateAddressesFromDroidXml(XmlReader xr)
        {
            return XmlReadTemplates<MmsAddress>.ReadListOfSingleElements(xr, "addrs", "addr",
                ((reader) =>
                    XmlReadTemplates<MmsAddress>.ParseSingleElementWithAttributes(reader, "addr",
                        ParseDroidMmsAttribute)));
        }

        public void WriteToDroidXml(XmlWriter xw)
        {
            xw.WriteStartElement("addr");
            xw.WriteAttributeString("address", MmsMessage.Nullable(m_sAddress));
            xw.WriteAttributeString("charset", m_nCharset.ToString());
            xw.WriteAttributeString("type", m_nType.ToString());
            xw.WriteEndElement();
        }
    }

    public class MmsMessage
    {
        #region Member Variables
#pragma warning disable CS0649 // never assigned, always default
        private List<MmsAddress> m_plAddresses;
        private List<MmsPart> m_plParts;

        // only from the observed file (a backup from an android phone using syntech's message backup
        private int m_nSpam_report;
        private string m_sMsg_id;
        private string m_sApp_id;
        private string m_sFrom_address;
        private string m_sSub_id; // subject id
        private int m_nReserved;
        private int m_nUsing_mode;
        private int m_nRr_st;
        private int m_nFavorite;
        private int m_nHidden;
        private int m_nDeletable;
        private int m_nD_rpt_st; // delivery report status
        private int m_nCallback_set;
        private string m_sDevice_name;
        private int m_nSim_slot;
        private string m_sCreator;
        private string m_sSim_imsi;
        private int m_nSafe_message;
        private int m_nSecret_mode;

        // from the schema http://synctech.com.au/wp-content/uploads/2018/01/sms.xsd_.txt AND observed file
        // comments about what the fields mean inferred from https://www.programcreek.com/java-api-examples/?code=ivanovpv/darksms/darksms-master/psm/src/main/java/ru/ivanovpv/gorets/psm/mms/pdu/PduPersister.java
        private ulong m_ulDate; // required, date
        private string m_sCt_t; // required, content type
        private string m_sCt_l; // required, content location
        private string m_sCt_cls; // required, content class
        private int m_nMsg_box; // required, message box
        private string m_sAddress; // required, concatenation of addresses
        private string m_sSub_cs; // required, subject charset
        private string m_sRetr_st; // required, retrieve status
        private string m_sD_tm; // required, delivery time
        private string m_sExp; // required, expiry
        private int m_nLocked; // required
        private string m_sM_id; // required, message id
        private string m_sRetr_txt; // required, retrieve text
        private int m_nDate_sent; // required, date sent
        private int m_nRead; // required, read
        private string m_sRpt_a; // required, report allowed
        private int? m_nPri; // required, priority
        private string m_sResp_txt; // required, response text
        private int m_nD_rpt; // required, delivery report
        private int m_nType; // required, message type
        private int? m_nRr; // required, read report
        private string m_sSub; // optional, subject
        private string m_sRead_status; // required, read status
        private int m_nSeen;
        private string m_sResp_st; // required
        private int? m_nText_only; // optional
        private string m_sSt; // required, status
        private string m_sRetr_txt_cs; // required, retreive text charset
        private string m_sTr_id; // required, transation id
        private string m_sM_cls; // required, message class
        private int m_nV; // required, version
        private string m_sM_size; // required, message size
        private string m_sReadable_date; // optional
        private string m_sContact_name; // optional
#pragma warning restore CS0649 // never assigned, always default

        // charsets from https://www.iana.org/assignments/character-sets/character-sets.xhtml

        // ReSharper disable UnusedMember.Global
        public static int s_CHARSET_USASCII = 3;
        public static int s_CHARSET_UTF8 = 106;

        // from https://kernel.googlesource.com/pub/scm/network/ofono/mmsd/+/b93b6037b7b0b3964c28a1c0721f6726e7c1cf21/src/mmsutil.h
        public static int s_MMS_MESSAGE_TYPE_SEND_REQ = 128;
        public static int s_MMS_MESSAGE_TYPE_SEND_CONF = 129;
        public static int s_MMS_MESSAGE_TYPE_NOTIFICATION_IND = 130;
        public static int s_MMS_MESSAGE_TYPE_NOTIFYRESP_IND = 131;
        public static int s_MMS_MESSAGE_TYPE_RETRIEVE_CONF = 132;
        public static int s_MMS_MESSAGE_TYPE_ACKNOWLEDGE_IND = 133;
        public static int s_MMS_MESSAGE_TYPE_DELIVERY_IND = 134;

        public static int s_MMS_MESSAGE_RSP_STATUS_OK = 128;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_UNSUPPORTED_MESSAGE = 136;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_TRANS_FAILURE = 192;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_TRANS_NETWORK_PROBLEM = 195;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_PERM_FAILURE = 224;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_PERM_SERVICE_DENIED = 225;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_PERM_MESSAGE_FORMAT_CORRUPT = 226;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_PERM_SENDING_ADDRESS_UNRESOLVED = 227;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_PERM_CONTENT_NOT_ACCEPTED = 229;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_PERM_LACK_OF_PREPAID = 235;

        public static int s_MMS_MESSAGE_VERSION_1_0 = 0x90;
        public static int s_MMS_MESSAGE_VERSION_1_1 = 0x91;
        public static int s_MMS_MESSAGE_VERSION_1_2 = 0x92;
        public static int s_MMS_MESSAGE_VERSION_1_3 = 0x93;

        public static int s_MMS_EXPIRY_DEFAULT = 604800;
        // ReSharper restore UnusedMember.Global

        public static int s_MESSAGEBOX_RECEIVED = 1;
        public static int s_MESSAGEBOX_SENT = 2;

        private static char[] c_rgchAscii = new char[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u',
            'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
            'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };
        #endregion

        public string Date => ReadableDateFromWindowsTimestamp(SmsMessage.MsecWinFromSecondsUnix(m_ulDate));

        #region Constructors
        public string Text
        {
            get
            {
                if (m_plParts != null && m_plParts.Count > 0)
                {
                    foreach (MmsPart part in m_plParts)
                    {
                        if (part.Text != null)
                            return part.Text;
                    }
                }

                return "<null>";
            }
        }
        public string To => m_sAddress;

        /*----------------------------------------------------------------------------
        	%%Function: MmsMessage
        	%%Qualified: wp2droidMsg.MmsMessage.MmsMessage
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public MmsMessage()
        {
            m_sCt_t = "application/vnd.wap.multipart.related";
            m_sCreator = "com.github.rlittletht.wp2droidMsg";
            m_sM_cls = "personal";
            m_nPri = 129;
            m_sSub_id = "-1";
            m_nD_rpt = 129;
            m_nRr = 129;
            m_nSeen = 1;
            m_nV = 18; // why 18? observed was 16
            m_sMsg_id = "0";
            m_sApp_id = "0";
            m_nText_only = 0;
        }

        /*----------------------------------------------------------------------------
        	%%Function: CreateFromWpMmsMessage
        	%%Qualified: wp2droidMsg.MmsMessage.CreateFromWpMmsMessage
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public static MmsMessage CreateFromWpMmsMessage(WpMessage wpm, int seqNo)
        {
            MmsMessage mms = new MmsMessage();

            // first, convert the addresses
            mms.m_plAddresses = new List<MmsAddress>();
            // first, add an address for the sender

            if (wpm.Recipients != null)
                foreach (string s in wpm.Recipients)
                    mms.m_plAddresses.Add(new MmsAddress(s, MmsAddress.s_AddressTypeTo, s_CHARSET_UTF8));

            if (wpm.Incoming) // type 1
            {
                mms.m_plAddresses.Add(new MmsAddress(wpm.Sender, MmsAddress.s_AddressTypeFrom, s_CHARSET_UTF8));
                mms.m_nType = s_MMS_MESSAGE_TYPE_RETRIEVE_CONF;
                mms.m_nMsg_box = s_MESSAGEBOX_RECEIVED;
                mms.m_sExp = "null";
                mms.m_sResp_st = "null";
                mms.m_sAddress = wpm.Sender;
            }
            else
            {
                // if its not incoming, then the sender is ourselves
                mms.m_plAddresses.Add(new MmsAddress("insert-address-token", MmsAddress.s_AddressTypeFrom, s_CHARSET_UTF8));
                mms.m_nType = s_MMS_MESSAGE_TYPE_SEND_REQ;
                mms.m_nMsg_box = s_MESSAGEBOX_SENT;
                mms.m_sExp = "604800"; // 7 days
                mms.m_sResp_st = s_MMS_MESSAGE_RSP_STATUS_OK.ToString();
                mms.m_sAddress = AddressBuildFromAddresses(mms.m_plAddresses);
            }

            mms.m_ulDate = SmsMessage.SecondsUnixFromMsecWin(wpm.LocalTimestamp);
            mms.m_nRead = wpm.Read ? 1 : 0;
            mms.m_sM_id = CreateRandomCharacterString(c_rgchAscii, 26);
            mms.m_sTr_id = CreateRandomCharacterString(c_rgchAscii, 12);

            mms.m_sReadable_date = ReadableDateFromWindowsTimestamp(wpm.LocalTimestamp);

            // now convert the parts...
            foreach (WpMessageAttachment att in wpm.Attachments)
            {
                int seqPart = 0;
                MmsPart part = MmsPart.CreateFromWpAttachment(att, ref seqPart, seqNo);

                if (mms.m_plParts == null)
                    mms.m_plParts = new List<MmsPart>();

                mms.m_plParts.Add(part);
            }

            return mms;
        }
        #endregion

        /*----------------------------------------------------------------------------
        	%%Function: AddressBuildFromAddresses
        	%%Qualified: wp2droidMsg.MmsMessage.AddressBuildFromAddresses
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        static string AddressBuildFromAddresses(List<MmsAddress> pladdr)
        {
            StringBuilder sb = new StringBuilder();
            foreach (MmsAddress mma in pladdr)
            {
                if (sb.Length > 0)
                    sb.Append("~");
                sb.Append(mma.Address);
            }

            return sb.ToString();
        }


        public static string Nullable(string s)
        {
            return s ?? "null";
        }

        public static string Nullable(int? n)
        {
            return n == null ? "null" : n.ToString();
        }

        public static string Nullable(long? n)
        {
            return n == null ? "null" : n.ToString();
        }

        public static string Nullable(ulong? n)
        {
            return n == null ? "null" : n.ToString();
        }

        public static void ParseDroidMmsAttribute(XmlReader xr, MmsMessage mms)
        {

            switch (xr.Name)
            {
                case "date": mms.m_ulDate = XmlIO.ReadGenericUInt64Element(xr, "date") ?? 0;  break;
                case "spam_report": mms.m_nSpam_report = XmlIO.ReadGenericIntElement(xr, "spam_report") ?? 0; break;
                case "ct_t": mms.m_sCt_t = XmlIO.ReadGenericNullableStringElement(xr, "ct_t"); break;
                case "msg_box": mms.m_nMsg_box = XmlIO.ReadGenericIntElement(xr, "msg_box") ?? 0; break;
                case "address": mms.m_sAddress = XmlIO.ReadGenericStringElement(xr, "address"); break;
                case "sub_cs": mms.m_sSub_cs = XmlIO.ReadGenericNullableStringElement(xr, "sub_cs"); break;
                case "retr_st": mms.m_sRetr_st = XmlIO.ReadGenericNullableStringElement(xr, "retr_st"); break;
                case "d_tm": mms.m_sD_tm = XmlIO.ReadGenericNullableStringElement(xr, "d_tm"); break;
                case "exp": mms.m_sExp = XmlIO.ReadGenericNullableStringElement(xr, "exp"); break;
                case "msg_id": mms.m_sMsg_id = XmlIO.ReadGenericNullableStringElement(xr, "msg_id"); break;
                case "app_id": mms.m_sApp_id = XmlIO.ReadGenericNullableStringElement(xr, "app_id"); break;
                case "from_address": mms.m_sFrom_address = XmlIO.ReadGenericNullableStringElement(xr, "from_address"); break;
                case "m_id": mms.m_sM_id = XmlIO.ReadGenericNullableStringElement(xr, "m_id"); break;
                case "retr_txt": mms.m_sRetr_txt = XmlIO.ReadGenericNullableStringElement(xr, "retr_txt"); break;
                case "date_sent": mms.m_nDate_sent = XmlIO.ReadGenericIntElement(xr, "date_sent") ?? 0; break;
                case "rpt_a": mms.m_sRpt_a = XmlIO.ReadGenericNullableStringElement(xr, "rpt_a"); break;
                case "ct_cls": mms.m_sCt_cls = XmlIO.ReadGenericNullableStringElement(xr, "ct_cls"); break;
                case "sub_id": mms.m_sSub_id = XmlIO.ReadGenericNullableStringElement(xr, "sub_id"); break;
                case "resp_txt": mms.m_sResp_txt = XmlIO.ReadGenericNullableStringElement(xr, "resp_txt"); break;
                case "ct_l": mms.m_sCt_l = XmlIO.ReadGenericNullableStringElement(xr, "ct_l"); break;
                case "reserved": mms.m_nReserved = XmlIO.ReadGenericIntElement(xr, "reserved") ?? 0; break;
                case "m_type": mms.m_nType = XmlIO.ReadGenericIntElement(xr, "m_type") ?? 0; break;
                case "sub": mms.m_sSub = XmlIO.ReadGenericNullableStringElement(xr, "sub"); break;
                case "read_status": mms.m_sRead_status = XmlIO.ReadGenericNullableStringElement(xr, "read_status"); break;
                case "device_name": mms.m_sDevice_name = XmlIO.ReadGenericNullableStringElement(xr, "device_name"); break;
                case "resp_st": mms.m_sResp_st = XmlIO.ReadGenericNullableStringElement(xr, "resp_st"); break;
                case "st": mms.m_sSt = XmlIO.ReadGenericNullableStringElement(xr, "st"); break;
                case "retr_txt_cs": mms.m_sRetr_txt_cs = XmlIO.ReadGenericNullableStringElement(xr, "retr_txt_cs"); break;
                case "creator": mms.m_sCreator = XmlIO.ReadGenericNullableStringElement(xr, "creator"); break;
                case "m_size": mms.m_sM_size = XmlIO.ReadGenericNullableStringElement(xr, "m_size"); break;
                case "sim_imsi": mms.m_sSim_imsi = XmlIO.ReadGenericNullableStringElement(xr, "sim_imsi"); break;
                case "tr_id": mms.m_sTr_id = XmlIO.ReadGenericNullableStringElement(xr, "tr_id"); break;
                case "m_cls": mms.m_sM_cls = XmlIO.ReadGenericNullableStringElement(xr, "m_cls"); break;
                case "readable_date": mms.m_sReadable_date = XmlIO.ReadGenericNullableStringElement(xr, "readable_date"); break;
                case "contact_name": mms.m_sContact_name = XmlIO.ReadGenericNullableStringElement(xr, "contact_name"); break;
                case "locked": mms.m_nLocked = XmlIO.ReadGenericIntElement(xr, "locked") ?? 0; break;
                case "read": mms.m_nRead = XmlIO.ReadGenericIntElement(xr, "read") ?? 0; break;
                case "pri": mms.m_nPri = XmlIO.ReadGenericIntElement(xr, "pri"); break;
                case "d_rpt": mms.m_nD_rpt = XmlIO.ReadGenericIntElement(xr, "d_rpt") ?? 0; break;
                case "using_mode": mms.m_nUsing_mode = XmlIO.ReadGenericIntElement(xr, "using_mode") ?? 0; break;
                case "rr_st": mms.m_nRr_st = XmlIO.ReadGenericIntElement(xr, "rr_st") ?? 0; break;
                case "favorite": mms.m_nFavorite = XmlIO.ReadGenericIntElement(xr, "favorite") ?? 0; break;
                case "rr": mms.m_nRr = XmlIO.ReadGenericIntElement(xr, "rr"); break;
                case "hidden": mms.m_nHidden = XmlIO.ReadGenericIntElement(xr, "hidden") ?? 0; break;
                case "deletable": mms.m_nDeletable = XmlIO.ReadGenericIntElement(xr, "deletable") ?? 0; break;
                case "d_rpt_st": mms.m_nD_rpt_st = XmlIO.ReadGenericIntElement(xr, "d_rpt_st") ?? 0; break;
                case "callback_set": mms.m_nCallback_set = XmlIO.ReadGenericIntElement(xr, "callback_set") ?? 0; break;
                case "seen": mms.m_nSeen = XmlIO.ReadGenericIntElement(xr, "seen") ?? 0; break;
                case "text_only": mms.m_nText_only = XmlIO.ReadGenericIntElement(xr, "text_only") ?? 0; break;
                case "sim_slot": mms.m_nSim_slot = XmlIO.ReadGenericIntElement(xr, "sim_slot") ?? 0; break;
                case "v": mms.m_nV = XmlIO.ReadGenericIntElement(xr, "v") ?? 0; break;
                case "safe_message": mms.m_nSafe_message = XmlIO.ReadGenericIntElement(xr, "safe_message") ?? 0; break;
                case "secret_mode": mms.m_nSecret_mode = XmlIO.ReadGenericIntElement(xr, "secret_mode") ?? 0; break;
            }
        }

        static void ParseMmsMessageElement(XmlReader xr, MmsMessage mms)
        {
            switch (xr.Name)
            {
                case "parts":
                    mms.m_plParts = MmsPart.CreatePartsFromDroidXml(xr);
                    break;
                case "addrs":
                    mms.m_plAddresses = MmsAddress.CreateAddressesFromDroidXml(xr);
                    break;
                default:
                    throw new Exception($"unknown element {xr.Name} under mms message");
            }

        }

        public static MmsMessage CreateFromDroidXmlReader(XmlReader xr)
        {
            MmsMessage mms = new MmsMessage();

            if (xr.Name != "mms")
                throw new Exception("not at the correct node");

            bool fEmptyMmsElement = xr.IsEmptyElement;

            if (!XmlIO.Read(xr))
                throw new Exception("nothing to read");

            while (true)
            {
                XmlIO.SkipNonContent(xr);
                XmlNodeType nt = xr.NodeType;

                // PUT MMS children here
                if (nt == XmlNodeType.Element)
                {
                    ParseMmsMessageElement(xr, mms);
                    continue;
                }


                if (nt == XmlNodeType.EndElement)
                {
                    if (xr.Name != "mms")
                        throw new Exception("unmatched sms element");

                    xr.ReadEndElement();
                    break;
                }

                if (xr.NodeType != XmlNodeType.Attribute)
                    throw new Exception("unexpected non attribute on <sms> element");

                while (true)
                {
                    // consume all the attributes
                    ParseDroidMmsAttribute(xr, mms);
                    if (!xr.MoveToNextAttribute())
                    {
                        if (fEmptyMmsElement)
                        {
                            xr.Read();  // get past the attribute
                            return mms;
                        }

                        break; // continue till we find the end sms element
                    }

                    // otherwise just continue...
                }

                if (!XmlIO.Read(xr))
                    throw new Exception("never encountered end sms element");
            }

            return mms;
        }

        public void WriteToDroidXml(XmlWriter xw)
        {
            xw.WriteStartElement("mms");
            xw.WriteAttributeString("date", m_ulDate.ToString());   // converted
            xw.WriteAttributeString("spam_report", m_nSpam_report.ToString()); // not present, always 0
            xw.WriteAttributeString("ct_t", Nullable(m_sCt_t)); // static in both
            xw.WriteAttributeString("msg_box", m_nMsg_box.ToString()); // converted
            xw.WriteAttributeString("address", Nullable(m_sAddress)); // converted
            xw.WriteAttributeString("sub_cs", Nullable(m_sSub_cs)); // static always null
            xw.WriteAttributeString("retr_st", Nullable(m_sRetr_st)); // static always null
            xw.WriteAttributeString("d_tm", Nullable(m_sD_tm));  // static always null
            xw.WriteAttributeString("exp", Nullable(m_sExp)); // converted
            xw.WriteAttributeString("locked", m_nLocked.ToString());  // static always 0
            xw.WriteAttributeString("msg_id", Nullable(m_sMsg_id));  // static always 0
            xw.WriteAttributeString("app_id", Nullable(m_sApp_id)); // static always 0
            xw.WriteAttributeString("from_address", Nullable(m_sFrom_address)); // static always null
            xw.WriteAttributeString("m_id", Nullable(m_sM_id)); // converted
            xw.WriteAttributeString("retr_txt", Nullable(m_sRetr_txt)); // static always null
            xw.WriteAttributeString("date_sent", Nullable(m_nDate_sent)); // static always nul0l
            xw.WriteAttributeString("read", Nullable(m_nRead)); // converted
            xw.WriteAttributeString("rpt_a", Nullable(m_sRpt_a)); // static always null
            xw.WriteAttributeString("ct_cls", Nullable(m_sCt_cls)); // static always null
            xw.WriteAttributeString("pri", Nullable(m_nPri)); // static always 129 | null
            xw.WriteAttributeString("sub_id", Nullable(m_sSub_id)); // static always -1
            xw.WriteAttributeString("resp_txt", Nullable(m_sResp_txt)); // static always null
            xw.WriteAttributeString("ct_l", Nullable(m_sCt_l)); // not null in observed, but no source for conversion. null?
            xw.WriteAttributeString("d_rpt", Nullable(m_nD_rpt)); // static always 129
            xw.WriteAttributeString("reserved", Nullable(m_nReserved));  // static always null
            xw.WriteAttributeString("using_mode", Nullable(m_nUsing_mode));  // static always 0
            xw.WriteAttributeString("rr_st", Nullable(m_nRr_st));  // static always 0
            xw.WriteAttributeString("m_type", Nullable(m_nType)); // converted
            xw.WriteAttributeString("favorite", Nullable(m_nFavorite)); // static always 0
            xw.WriteAttributeString("rr", Nullable(m_nRr)); // static always 129 or null, don't know why
            xw.WriteAttributeString("sub", Nullable(m_sSub)); // static always null
            xw.WriteAttributeString("hidden", Nullable(m_nHidden)); // static always 0
            xw.WriteAttributeString("deletable", Nullable(m_nDeletable)); // static always 0
            xw.WriteAttributeString("read_status", Nullable(m_sRead_status));  // static always null
            xw.WriteAttributeString("d_rpt_st", Nullable(m_nD_rpt_st));  // static always 0
            xw.WriteAttributeString("callback_set", Nullable(m_nCallback_set)); // static always 0
            xw.WriteAttributeString("seen", Nullable(m_nSeen)); // static always 1 except for unseen messages; should be 1
            xw.WriteAttributeString("device_name", Nullable(m_sDevice_name)); // static always null
            xw.WriteAttributeString("resp_st", Nullable(m_sResp_st)); // converted
            xw.WriteAttributeString("text_only", Nullable(m_nText_only)); // static in convert (0), but calculated in backup
            xw.WriteAttributeString("sim_slot", Nullable(m_nSim_slot)); // static always 0
            xw.WriteAttributeString("st", Nullable(m_sSt)); // static always null
            xw.WriteAttributeString("retr_txt_cs", Nullable(m_sRetr_txt_cs));  // static always null
            xw.WriteAttributeString("creator", Nullable(m_sCreator)); // converted
            xw.WriteAttributeString("m_size", Nullable(m_sM_size)); // null in convert, but calculated in backup
            xw.WriteAttributeString("sim_imsi", Nullable(m_sSim_imsi)); // static always null
            xw.WriteAttributeString("safe_message", Nullable(m_nSafe_message)); // static always null
            xw.WriteAttributeString("tr_id", Nullable(m_sTr_id)); // converted/generated
            xw.WriteAttributeString("m_cls", Nullable(m_sM_cls)); // static, always "personal" (except 4 occurrences of null)
            xw.WriteAttributeString("v", Nullable(m_nV)); // v=18 from convert app; v=16 from observed backup
            xw.WriteAttributeString("secret_mode", Nullable(m_nSecret_mode));   // static always null
            xw.WriteAttributeString("readable_date", Nullable(m_sReadable_date)); // converted
            xw.WriteAttributeString("contact_name", Nullable(m_sContact_name)); // generated on backup, not present in converter

            if (m_plParts != null && m_plParts.Count > 0)
            {
                xw.WriteStartElement("parts");
                foreach (MmsPart part in m_plParts)
                {
                    part.WriteToDroidXml(xw);
                }

                xw.WriteEndElement();
            }

            xw.WriteStartElement("addrs");
            foreach (MmsAddress addr in m_plAddresses)
            {
                addr.WriteToDroidXml(xw);
            }

            xw.WriteEndElement();

            xw.WriteEndElement();
        }


        public static string ReadableDateFromWindowsTimestamp(ulong ulTimestamp)
        {
            DateTime dttm = DateTime.FromFileTime((long)ulTimestamp);
            return dttm.ToString("MMM d, yyyy HH:mm:ss");
        }
        #region TESTS
        [TestCase("", "")]
        [TestCase("1234", "1234")]
        [TestCase("1234|", "1234~")]
        [TestCase("1234|4321", "1234~4321")]
        [TestCase("|1234", "1234")]
        [Test]
        public static void TestAddressBuildFromAddresses(string sIn, string sExpected)
        {
            string[] rgs = sIn.Split('|');
            List<MmsAddress> pladdr = new List<MmsAddress>();

            foreach (string s in rgs)
                pladdr.Add(new MmsAddress(s, 0, 0));

            Assert.AreEqual(sExpected, AddressBuildFromAddresses(pladdr));
        }
        //mms_template = '<mms text_only="0" ct_t="application/vnd.wap.multipart.related" using_mode="0" msg_box="{msg_box}" secret_mode="0" v="18" ct_cls="null" retr_txt_cs="null" d_rpt_st="0" favorite="0" deletable="0" sim_imsi="null" st="null" creator="com.github.matteocontrini.sms-wp-to-android" tr_id="{tr_id}" sim_slot="0" read="{read}" m_id="{m_id}" callback_set="0" m_type="{m_type}" locked="0" retr_txt="null" resp_txt="null" rr_st="0" safe_message="0" retr_st="null" reserved="0" msg_id="0" hidden="0" sub="null" seen="1" rr="129" ct_l="null" from_address="null" m_size="null" exp="{exp}" sub_cs="null" sub_id="-1" app_id="0" resp_st="{resp_st}" date="{date:.0f}" date_sent="0" pri="129" address="{address}" d_rpt="129" d_tm="null" read_status="null" device_name="null" spam_report="0" rpt_a="null" m_cls="personal" readable_date="{readable_date}"><parts>{parts}</parts><addrs>{addrs}</addrs></mms>'
        //mms_part_template = '<part seq="{seq}" ct="{ct}" name="null" chset="{chset}" cd="null" fn="null" cid="&lt;{cid}&gt;" cl="{cl}" ctt_s="null" ctt_t="null" text="{text}" {data} />'
        //mms_addr_template = '<addr address="{address}" type="{type}" charset="{charset}" />'

        //  <mms date="1536330850000" spam_report="0" ct_t="application/vnd.wap.multipart.related" msg_box="1" address="+14254956002~+14259226528" sub_cs="null" retr_st="null" d_tm="null" exp="null" locked="0" msg_id="0" app_id="0" from_address="null"
        // m_id="090714341050003100016" retr_txt="null" date_sent="0" read="1" rpt_a="null" ct_cls="null" pri="129" sub_id="-1" resp_txt="null" ct_l="http://166.216.198.5:8013/0907143410500031000160001" d_rpt="129" reserved="0" using_mode="0"
        // rr_st="0" m_type="132" favorite="0" rr="129" sub="null" hidden="0" deletable="0" read_status="null" d_rpt_st="0" callback_set="0" seen="1" device_name="null" resp_st="null"
        // text_only="1" sim_slot="0" st="null" retr_txt_cs="null"
        // creator="com.samsung.android.messaging" m_size="283" sim_imsi="null" safe_message="0" tr_id="AB0907143410500031000160001" m_cls="personal" v="16" secret_mode="0" readable_date="Sep 7, 2018 07:34:10" contact_name="Eleanor Little, Tonya Henry">


        [TestCase(131777186860194900UL, "Aug 2, 2018 14:24:46")]
        [Test]
        public static void TestReadableDateFromWindowsTimestamp(ulong ulIn, string sExpected)
        {
            Assert.AreEqual(sExpected, ReadableDateFromWindowsTimestamp(ulIn));
        }
        static string CreateRandomCharacterString(char[] rgchChoose, int cRequired)
        {
            int nMax = rgchChoose.Length - 1;
            StringBuilder sb = new StringBuilder(cRequired);
            Random rnd = new Random();

            while (cRequired-- > 0)
                sb.Append(rgchChoose[rnd.Next(nMax)]);

            return sb.ToString();
        }

        [TestCase("a", 1, "a")]
        [TestCase("a", 2, "aa")]
        [Test]
        public static void TestCreateRandomCharacterString(string sDomain, int cRequired, string sExpected)
        {
            char[] rgch = sDomain.ToCharArray();

            string sActual = CreateRandomCharacterString(rgch, cRequired);
            Assert.AreEqual(sExpected, sActual);
        }
        #endregion
    }
}