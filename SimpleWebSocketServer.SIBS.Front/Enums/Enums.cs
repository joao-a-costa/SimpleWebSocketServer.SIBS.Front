using System.ComponentModel;

namespace SimpleWebSocketServer.SIBS.Front.Enums
{
    public class Enums
    {
        public enum StatusCode
        {
            OK = 0,
            NOTCONNECTED = 1,
            ERROR = 500,
        }
        public enum RequestType
        {
            STATUS_REQUEST,
            STATUS_RESPONSE,
            SET_AUTH_CREDENTIAL_REQUEST,
            SET_AUTH_CREDENTIAL_RESPONSE,
            PROCESS_PAYMENT_REQUEST,
            PROCESS_PAYMENT_RESPONSE,
            EVENT_NOTIFICATION,
            ERROR_NOTIFICATION,
            HEARTBEAT_NOTIFICATION,
            TX_REQUEST,
            TX_RESPONSE,
            RECEIPT_NOTIFICATION,
            PAIRING_REQUEST,
            PAIRING_RESPONSE,
            PAIRING_NOTIFICATION,
            RECONCILIATION_REQUEST,
            RECONCILIATION_RESPONSE,
            REFUND_REQUEST,
            REFUND_RESPONSE,
            COMMUNICATIONS_REQUEST,
            COMMUNICATIONS_RESPONSE,
            GET_MERCHANT_DATA_REQUEST,
            GET_MERCHANT_DATA_RESPONSE,
            SET_MERCHANT_DATA_REQUEST,
            SET_MERCHANT_DATA_RESPONSE,
            CONFIG_TERMINAL_REQUEST,
            CONFIG_TERMINAL_RESPONSE,
            CUSTOMER_DATA_REQUEST,
            CUSTOMER_DATA_RESPONSE,
            LOYALTY_INQUIRY_REQUEST,
            LOYALTY_INQUIRY_RESPONSE,
            PENDING_REVERSALS_REQUEST,
            PENDING_REVERSALS_RESPONSE,
            DELETE_PENDING_REVERSALS_REQUEST,
            DELETE_PENDING_REVERSALS_RESPONSE,
            LIST_TERMINALS_REQUEST,
            LIST_TERMINALS_RESPONSE,
            REGISTER_FRONT_REQUEST,
            REGISTER_FRONT_RESPONSE,
            LINQ_TERMINAL_TO_FRONT_REQUEST,
            LINQ_TERMINAL_TO_FRONT_RESPONSE,
            //CLIENT_CONNECTED_REQUEST,
            CLIENT_CONNECTED_RESPONSE,
            NEW_TERMINAL_CONNECTED_REQUEST,
            TERMINAL_DISCONNECTED
        }

        public enum AuthorizationType
        {
            NA,
            GENERAL,
            AFD,
            EVC,
            FUEL
        }

        public enum PairingStep
        {
            GENERATE_PAIRING_CODE,
            VALIDATE_PAIRING_CODE,
            CANCEL_PAIRING
        }

        public enum Version
        {
            V_1
        }

        public enum PrintMode
        {
            [Description("A - Impressão no SmartPOS")]
            MODE_A,
            [Description("B - Talão cliente - Impressão no SmartPOS Talão comerciante - Gestão no ePOS")]
            MODE_B,
            [Description("C - Gestão no ePOS")]
            MODE_C,
        }

        public enum ReceiptFormat
        {
            [Description("TWENTY_COLUMNS")]
            TWENTY_COLUMNS,
            [Description("FORTY_COLUMNS")]
            FORTY_COLUMNS,
        }

        public enum TerminalCommandOptions
        {
            [Description("Connect to server")]
            ConnectToServer = 1,
            [Description("Disconnect from server")]
            DisconnectFromServer = 2,
            [Description("Configure Address")]
            ConfigureAddress = 3,
            [Description("Send process payment request")]
            SendProcessPaymentRequest = 5,
            [Description("Send pairing request")]
            SendPairingRequest = 6,
            [Description("Send pairing request cancel")]
            SendPairingRequestCancel = 7,
            [Description("Send refund payment request")]
            SendRefundPaymentRequest = 8,
            [Description("Send reconciliation request")]
            SendReconciliationRequest = 9,
            [Description("Send communication status request")]
            SendCommunicationStatusRequest = 10,
            [Description("Send get merchant data request")]
            SendGetMerchantDataRequest = 11,
            [Description("Send set merchant data request")]
            SendSetMerchantDataRequest = 12,
            [Description("Send configuration terminal request")]
            SendConfigTerminalRequest = 13,
            [Description("Send customer data request")]
            SendCustomerDataRequest=14,
            [Description("Send loyalty inquiry request")]
            SendLoyaltyInquiryRequest = 15,
            [Description("Send pending reversals request")]
            SendPendingReversalsRequest = 16,
            [Description("Send delete pending reversals request")]
            SendDeletePendingReversalsReqRequest = 17,
            [Description("Send Terminal Status Request")]
            SendTerminalStatusRequest = 18,
            [Description("Send List Terminals Request")]
            SendListTerminalsRequest = 19,
            [Description("Send Linq Terminal to Front Request")]
            SendLinqTerminalToFrontRequest = 20,
            [Description("Show list of commands")]
            ShowListOfCommands = 9998
        }
    }
}
