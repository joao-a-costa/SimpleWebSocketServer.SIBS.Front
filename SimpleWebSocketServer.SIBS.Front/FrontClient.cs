using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleWebSocketServer.SIBS.Front.Models;
using static SimpleWebSocketServer.SIBS.Front.Enums.Enums;

namespace SimpleWebSocketServer.SIBS.Front
{
    public class TerminalClient
    {
        #region "Constants"

        private const string _MessageEnterJSONCommand = "Enter JSON command:";
        private const string _MessageErrorProcessingRequest = "Error processing request";
        private const string _MessageEnterAmount = "Enter amount: ";
        private const string _MessageInvalidInput = "Invalid input";
        private const string _MessageUsingLastSuccessfullTranscationDataForRefund = "Using last successfull transcation data for refund";
        private const string _MessageEnterIBANOrEmptyToUseLastOne = "Enter IBAN or 'enter' to use last one: ";

        #endregion

        public TerminalClient(string address, Guid clientId, int terminalId)
        {
            Address = address;
            ClientId = clientId;
            TerminalId = terminalId;            
        }

        private string Address { get; set; }
        private ClientWebSocket Socket { get; set; }
        public Guid ClientId { get; set; }
        public int TerminalId { get; set; }
        public bool IsConnected => Socket?.State == WebSocketState.Open;

        public delegate void RegisterFrontResponseReceivedEventHandler(object sender, RegisterFrontResponse reqResponse);
        public event RegisterFrontResponseReceivedEventHandler RegisterFrontResponseReceived;
        public delegate void ListTerminalsResponseReceivedEventHandler(object sender, ListTerminalsResponse reqResponse);
        public event ListTerminalsResponseReceivedEventHandler ListTerminalsResponseReceived;
        public delegate void LinqTerminalToFrontResponseReceivedEventHandler(object sender, LinqTerminalToFrontResponse reqResponse);
        public event LinqTerminalToFrontResponseReceivedEventHandler LinqTerminalToFrontResponseReceived;

        public delegate void ClientConnectedEventHandler(object sender, Guid clientId, string message);
        public delegate void ClientDisconnectedEventHandler(object sender, EventArgs e);
        public delegate void TerminalStatusReqResponseReceivedEventHandler(object sender, TerminalStatusReqResponse reqResponse);
        public delegate void SetAuthCredentialsReqResponseEventHandler(object sender, SetAuthCredentialsReqResponse reqResponse);
        public delegate void ProcessPaymentReqResponseEventHandler(object sender, ProcessPaymentReqResponse reqResponse);
        public delegate void EventNotificationEventHandler(object sender, EventNotification reqResponse);
        public delegate void HeartbeatNotificationEventHandler(object sender, HeartbeatNotification reqResponse);
        public delegate void ReceiptNotificationEventHandler(object sender, ReceiptNotification reqResponse);
        public delegate void PairingReqEventHandler(object sender, PairingReqResponse reqResponse);
        public delegate void ReconciliationReqEventHandler(object sender, ReconciliationReqResponse reqResponse);
        public delegate void PairingNotificationEventHandler(object sender, PairingNotification reqResponse);
        public delegate void ErrorNotificationEventHandler(object sender, ErrorNotification reqResponse);
        public delegate void RefundReqResponseEventHandler(object sender, RefundReqResponse reqResponse);
        public delegate void CommunicationsReqResponseEventHandler(CommunicationsReqResponse reqResponse);
        public delegate void GetMerchantDataReqResponseEventHandler(GetMerchantDataReqResponse reqResponse);
        public delegate void SetMerchantDataReqResponseEventHandler(SetMerchantDataReqResponse reqResponse);
        public delegate void ConfigTerminalReqResponseEventHandler(ConfigTerminalReqResponse reqResponse);
        public delegate void InstallCertificateMessageEventHandler(object sender, string e);
        public delegate void CustomerDataResponseEventHandler(CustomerDataResponse reqResponse);
        public delegate void LoyaltyInquiryResponseEventHandler(LoyaltyInquiryResponse reqResponse);
        public delegate void PendingReversalsReqResponseEventHandler(PendingReversalsReqResponse reqResponse);
        public delegate void DeletePendingReversalsReqResponseEventHandler(DeletePendingReversalsReqResponse reqResponse);
        public delegate void FrontRegisteredEventHandler(object sender, Guid clientId);
        public delegate void TerminalDisconnectedEventHandler(TerminalDisconnected reqResponse);

        public event TerminalStatusReqResponseReceivedEventHandler TerminalStatusReqResponseReceived;
        public event ProcessPaymentReqResponseEventHandler ProcessPaymentReqReceived;
        public event EventNotificationEventHandler EventNotificationReceived;
        public event HeartbeatNotificationEventHandler HeartbeatNotificationReceived;
        public event ReceiptNotificationEventHandler ReceiptNotificationReceived;
        public event PairingReqEventHandler PairingReqReceived;
        public event ReconciliationReqEventHandler ReconciliationReqReceived;
        public event PairingNotificationEventHandler PairingNotificationReceived;
        public event ErrorNotificationEventHandler ErrorNotificationReceived;
        public event RefundReqResponseEventHandler RefundReqReceived;
        public event CommunicationsReqResponseEventHandler CommunicationsReqReceived;
        public event GetMerchantDataReqResponseEventHandler GetMerchantDataReqReceived;
        public event SetMerchantDataReqResponseEventHandler SetMerchantDataReqReceived;
        public event ConfigTerminalReqResponseEventHandler ConfigTerminalReqReceived;
        public event CustomerDataResponseEventHandler CustomerDataResponseReceived;
        public event LoyaltyInquiryResponseEventHandler LoyaltyInquiryResponseReceived;
        public event PendingReversalsReqResponseEventHandler PendingReversalsReqReceived;
        public event DeletePendingReversalsReqResponseEventHandler DeletePendingReversalsReqReceived;
        public event TerminalDisconnectedEventHandler TerminalDisconnectedReceived;


        #region "Private"

        /// <summary>
        /// Sends a message to the WebSocket server.
        /// </summary>
        /// <param name="data">The message to send.</param>
        /// <returns>The result of the send operation.</returns>
        private async Task<SendResult> Send(string data)
        {
            if (!IsConnected)
                return new SendResult { Success = false, StatusCode = StatusCode.NOTCONNECTED };

            var bytes = Encoding.UTF8.GetBytes(data);
            await Socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

            return new SendResult { Success = true, StatusCode = StatusCode.OK };
        }

        /// <summary>
        /// Loop to receive incoming WebSocket messages.
        /// </summary>
        private async Task ReceiveLoop()
        {
            var buffer = new byte[4096];

            while (Socket.State == WebSocketState.Open)
            {
                var result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                    //OnClientDisconnected();
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ProcessMessage(message);
                    //OnMessageReceived(message);
                }
            }
        }

        /// <summary>
        /// Processes the incoming message.
        /// </summary>
        /// <param name="message">The message to process.</param>
        private void ProcessMessage(string message)
        {
            // Try to parse the message as a TerminalStatusReqResponse object
            var resultTerminal = JsonConvert.DeserializeObject<TerminalStatusReqResponse>(message);

            if (resultTerminal != null)
            {

                switch (resultTerminal.Type)
                {
                    case RequestType.CLIENT_CONNECTED_RESPONSE:
                        var response = JsonConvert.DeserializeObject<ClientConnectedResponse>(message);
                        //ClientId = response.ClientId;
                        //ClientConnectedResponseReceived?.Invoke(this, response);
                        break;
                    case RequestType.STATUS_RESPONSE:
                        OnTerminalStatusReqResponseReceived(JsonConvert.DeserializeObject<TerminalStatusReqResponse>(message));
                        break;
                    case RequestType.REGISTER_FRONT_RESPONSE:
                        RegisterFrontResponseReceived?.Invoke(this, JsonConvert.DeserializeObject<RegisterFrontResponse>(message));
                        break;
                    case RequestType.LIST_TERMINALS_RESPONSE:
                        ListTerminalsResponseReceived?.Invoke(this, JsonConvert.DeserializeObject<ListTerminalsResponse>(message));
                        break;
                    case RequestType.LINQ_TERMINAL_TO_FRONT_RESPONSE:
                        LinqTerminalToFrontResponseReceived?.Invoke(this, JsonConvert.DeserializeObject<LinqTerminalToFrontResponse>(message));
                        break;

                    case RequestType.EVENT_NOTIFICATION:
                        OnEventNotificationReceived(JsonConvert
                            .DeserializeObject<EventNotification>(message));
                        break;
                    case RequestType.PROCESS_PAYMENT_RESPONSE:
                        OnProcessPaymentReqReceived(JsonConvert
                            .DeserializeObject<ProcessPaymentReqResponse>(message));
                        break;
                    case RequestType.ERROR_NOTIFICATION:
                        OnErrorNotificationReceived(JsonConvert
                            .DeserializeObject<ErrorNotification>(message));
                        break;
                    case RequestType.HEARTBEAT_NOTIFICATION:
                        OnHeartbeatNotificationReceived(JsonConvert
                            .DeserializeObject<HeartbeatNotification>(message));
                        break;
                    case RequestType.RECEIPT_NOTIFICATION:
                        OnReceiptNotificationReceived(JsonConvert
                            .DeserializeObject<ReceiptNotification>(message));
                        break;
                    case RequestType.PAIRING_RESPONSE:
                        OnPairingReqReceived(JsonConvert
                            .DeserializeObject<PairingReqResponse>(message));
                        break;
                    case RequestType.PAIRING_NOTIFICATION:
                        OnPairingNotificationReceived(JsonConvert
                            .DeserializeObject<PairingNotification>(message));
                        break;
                    case RequestType.RECONCILIATION_RESPONSE:
                        OnReconciliationReqReceived(JsonConvert
                            .DeserializeObject<ReconciliationReqResponse>(message));
                        break;
                    case RequestType.REFUND_RESPONSE:
                        OnRefundReqReceived(JsonConvert
                            .DeserializeObject<RefundReqResponse>(message));
                        break;
                    case RequestType.COMMUNICATIONS_RESPONSE:
                        OnCommunicationsReqReceived(JsonConvert
                            .DeserializeObject<CommunicationsReqResponse>(message));
                        break;
                    case RequestType.GET_MERCHANT_DATA_RESPONSE:
                        OnGetMerchantDataReqReceived(JsonConvert
                            .DeserializeObject<GetMerchantDataReqResponse>(message));
                        break;
                    case RequestType.SET_MERCHANT_DATA_RESPONSE:
                        OnSetMerchantDataReqReceived(JsonConvert
                            .DeserializeObject<SetMerchantDataReqResponse>(message));
                        break;
                    case RequestType.CONFIG_TERMINAL_RESPONSE:
                        OnConfigTerminalReqReceived(JsonConvert
                            .DeserializeObject<ConfigTerminalReqResponse>(message));
                        break;
                    case RequestType.CUSTOMER_DATA_RESPONSE:
                        OnCustomerDataResponseReceived(JsonConvert
                            .DeserializeObject<CustomerDataResponse>(message));
                        break;
                    case RequestType.LOYALTY_INQUIRY_RESPONSE:
                        OnLoyaltyInquiryResponseReceived(JsonConvert
                            .DeserializeObject<LoyaltyInquiryResponse>(message));
                        break;
                    case RequestType.PENDING_REVERSALS_RESPONSE:
                        OnPendingReversalsReqResponseReceived(JsonConvert
                            .DeserializeObject<PendingReversalsReqResponse>(message));
                        break;
                    case RequestType.TERMINAL_DISCONNECTED:
                        OnTerminalDisconnectedReceived(JsonConvert
                            .DeserializeObject<TerminalDisconnected>(message));
                        break;
                    case RequestType.DELETE_PENDING_REVERSALS_RESPONSE:
                        OnDeletePendingReversalsReqResponseReceived(JsonConvert
                            .DeserializeObject<DeletePendingReversalsReqResponse>(message));
                        break;
                    case RequestType.NEW_TERMINAL_CONNECTED_REQUEST:
                        var newTerminalConnectedReq = JsonConvert.DeserializeObject<NewTerminalConnectedReq>(message);

                        if (newTerminalConnectedReq.TerminalId == TerminalId)
                            LinqTerminalToFrontRequest(ClientId, TerminalId).Wait();
                        break;
                    case RequestType.LINQ_TERMINAL_TO_FRONT_REQUEST:
                        var linqTerminalToFrontReq = JsonConvert.DeserializeObject<LinqTerminalToFrontReq>(message);
                        LinqTerminalToFrontRequest(linqTerminalToFrontReq.Front, linqTerminalToFrontReq.Terminal).Wait();
                        break;
                    default:
                        //Log(_MessageReceivedUnknownMessage);
                        break;
                }
            }
        }

        #endregion

        #region "Public"

        public async Task<Tuple<bool, Exception>> Connect()
        {
            var res = true;
            Exception exception = null;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            try
            {
                Socket = new ClientWebSocket();

                //TODO: Remove this line in production code
                System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;

                if (!IsConnected)
                {

                    await Socket.ConnectAsync(new Uri(Address), cts.Token);

                    _ = Task.Run(ReceiveLoop);
                }
            }
            catch (Exception ex)
            {
                res = false;
                exception = ex;
            }
            finally
            {
                cts?.Dispose();
            }

            return new Tuple<bool, Exception>(res, exception);
        }

        public async Task Disconnect()
        {
            if (Socket.State == WebSocketState.Open)
            {
                await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
            }
        }

        #region "Commands"

        /// <summary>
        /// Sends a front register request.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<SendResult> SendRegisterFrontRequest(Guid clientId)
        {
            SendResult sendResult = null;

            try
            {
                var request = new RegisterFrontReq { Front = clientId };
                sendResult = await Send(JsonConvert.SerializeObject(request));
            }
            catch (Exception ex)
            {
                var message = $"{_MessageErrorProcessingRequest}: {ex.Message}";
                Console.WriteLine(message);
                sendResult = new SendResult { Success = false, StatusCode = StatusCode.ERROR, Message = message, Exception = ex };
            }

            return sendResult;
        }

        /// <summary>
        /// Sends a terminal status request.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendTerminalStatusRequest()
        {
            try
            {
                var request = new TerminalStatusReq();
                await Send(JsonConvert.SerializeObject(request));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a list terminals request.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendListTerminalRequest()
        {
            try
            {
                var request = new ListTerminalsReq();
                await Send(JsonConvert.SerializeObject(request));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a reconciliation request.
        /// </summary>
        /// <param name="req">The request object</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendReconciliationRequest(ReconciliationReq req)
        {
            try
            {
                await Send(JsonConvert.SerializeObject(req));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a reconciliation request.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendReconciliationRequest()
        {
            try
            {
                string input = string.Empty;

                System.Console.Write(_MessageEnterIBANOrEmptyToUseLastOne);

                // Read user input synchronously
                input = System.Console.ReadLine();

                var reconciliationReq = new ReconciliationReq() { Iban = input };
                await SendReconciliationRequest(reconciliationReq);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a communication request.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendCommunicationRequest()
        {
            try
            {
                var communicationsReq = new CommunicationsReq();
                await Send(JsonConvert.SerializeObject(communicationsReq));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a get merchant data request.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendGetMerchantDataRequest(GetMerchantDataReq getMerchantDataReq)
        {
            try
            {
                await Send(JsonConvert.SerializeObject(getMerchantDataReq));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a set merchant data request.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendSetMerchantDataRequest(MerchantData merchantData)
        {
            try
            {
                await Send(JsonConvert.SerializeObject(new SetMerchantDataReq()
                {
                    AcceptorAddress = merchantData.AcceptorAddress,
                    AcceptorLocation = merchantData.AcceptorLocation,
                    AcceptorName = merchantData.AcceptorName,
                    FiscalNumber = merchantData.FiscalNumber,
                    MerchantName = merchantData.MerchantName,
                    TerminalName = merchantData.TerminalName
                }));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a process payment request.
        /// </summary>
        /// <param name="req"> The request object</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendProcessPaymentRequest(ProcessPaymentReq req)
        {
            try
            {
                await Send(JsonConvert.SerializeObject(req));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a process payment request.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendProcessPaymentRequest()
        {
            try
            {
                double amount = 0;
                var invalidAmount = true;

                while (invalidAmount)
                {
                    System.Console.Write(_MessageEnterAmount);

                    // Read user input synchronously
                    string input = System.Console.ReadLine();

                    if (double.TryParse(input, out amount))
                        invalidAmount = false;

                    if (invalidAmount)
                        System.Console.WriteLine(_MessageInvalidInput);
                }

                var processPaymentRequest = new ProcessPaymentReq { AmountData = new AmountData { Amount = amount } };
                await SendProcessPaymentRequest(processPaymentRequest);

                //statusEventReceived.Set();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a refund payment request.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendRefundPaymentRequest(RefundReq req)
        {
            try
            {
                await Send(JsonConvert.SerializeObject(req));

                //statusEventReceived.Set();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a refund payment request.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendRefundPaymentRequest(PaymentData paymentData)
        {
            try
            {
                System.Console.WriteLine(_MessageUsingLastSuccessfullTranscationDataForRefund);

                var processPaymentRequest = new RefundReq
                {
                    Amount = paymentData.Amount,
                    OriginalTransactionData = new OriginalTransactionData
                    {
                        PaymentType = paymentData.PaymentType,
                        ServerDateTime = paymentData.ServerDateTime,
                        SdkId = paymentData.SdkId,
                        ServerId = paymentData.ServerId,
                        TransactionType = paymentData.TransactionType
                    }
                };
                await SendRefundPaymentRequest(processPaymentRequest);

                //statusEventReceived.Set();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a pairing code request.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendPairingRequest()
        {
            try
            {
                var pairingReq = new PairingReq();
                await Send(JsonConvert.SerializeObject(pairingReq));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a pairing code request cancel
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendPairingRequestCancel(PairingReq req)
        {
            try
            {
                await Send(JsonConvert.SerializeObject(req));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a pairing code request cancel
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendPairingRequestCancel()
        {
            try
            {
                var pairingReq = new PairingReq() { PairingStep = PairingStep.CANCEL_PAIRING };
                await SendPairingRequestCancel(pairingReq);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a pairing code request code to validate
        /// </summary>
        /// <param name="code">The code to validate</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendPairingRequestCode(PairingReq req)
        {
            try
            {
                await Send(JsonConvert.SerializeObject(req));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a pairing code request code to validate
        /// </summary>
        /// <param name="code">The code to validate</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendPairingRequestCode(string code)
        {
            try
            {
                var pairingReq = new PairingReq() { PairingCode = code, PairingStep = PairingStep.VALIDATE_PAIRING_CODE };
                await SendPairingRequestCode(pairingReq);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a set merchant data request.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendConfigTerminalRequest(ConfigTerminalReq configTerminalReq)
        {
            try
            {
                await Send(JsonConvert.SerializeObject(configTerminalReq));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a customer data request.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendCustomerDataReq(CustomerDataReq customerDataReq)
        {
            try
            {
                await Send(JsonConvert.SerializeObject(customerDataReq));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a loyalty inquiry request.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendLoyaltyInquiryReq(LoyaltyInquiryReq loyaltyInquiryReq)
        {
            try
            {
                await Send(JsonConvert.SerializeObject(loyaltyInquiryReq));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a pending reversals request.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendPendingReversalsRequest(PendingReversalsReq req)
        {
            try
            {
                await Send(JsonConvert.SerializeObject(req));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a delete pending reversals request.
        /// </summary>
        /// <param name="req">The request object</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendDeletePendingReversalsReqRequest(DeletePendingReversalsReq req)
        {
            try
            {
                await Send(JsonConvert.SerializeObject(req));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        public async Task LinqTerminalToFrontRequest(int terminaId)
        {
            try
            {
                var linqTerminalToFrontResponse = new LinqTerminalToFrontReq
                {
                    Front = ClientId,
                    Terminal = terminaId,
                };

                await Send(JsonConvert.SerializeObject(linqTerminalToFrontResponse));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        public async Task LinqTerminalToFrontRequest(Guid clientId, int terminaId)
        {
            try
            {
                var linqTerminalToFrontResponse = new LinqTerminalToFrontReq
                {
                    Front = clientId,
                    Terminal = terminaId,
                };

                await Send(JsonConvert.SerializeObject(linqTerminalToFrontResponse));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        #endregion

        #endregion

        #region "Events"

        /// <summary>
        /// OnTerminalStatusReqResponseReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnTerminalStatusReqResponseReceived(TerminalStatusReqResponse reqResponse)
        {
            TerminalStatusReqResponseReceived?.Invoke(this, reqResponse);
        }

        /// <summary>
        /// OnHeartbeatNotificationReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnHeartbeatNotificationReceived(HeartbeatNotification reqResponse)
        {
            HeartbeatNotificationReceived?.Invoke(this, reqResponse);
        }

        /// <summary>
        /// OnErrorNotificationReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnErrorNotificationReceived(ErrorNotification reqResponse)
        {
            ErrorNotificationReceived?.Invoke(this, reqResponse);
        }

        /// <summary>
        /// OnReceiptNotificationReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnReceiptNotificationReceived(ReceiptNotification reqResponse)
        {
            ReceiptNotificationReceived?.Invoke(this, reqResponse);
        }

        /// <summary>
        /// OnPairingReqReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnPairingReqReceived(PairingReqResponse reqResponse)
        {
            PairingReqReceived?.Invoke(this, reqResponse);
        }

        /// <summary>
        /// OnPairingNotificationReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnPairingNotificationReceived(PairingNotification reqResponse)
        {
            PairingNotificationReceived?.Invoke(this, reqResponse);
        }

        /// <summary>
        /// OnReconciliationReqReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnReconciliationReqReceived(ReconciliationReqResponse reqResponse)
        {
            ReconciliationReqReceived?.Invoke(this, reqResponse);
        }

        /// <summary>
        /// OnRefundReqReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnRefundReqReceived(RefundReqResponse reqResponse)
        {
            RefundReqReceived?.Invoke(this, reqResponse);
        }

        /// <summary>
        /// OnCommunicationsReqReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnCommunicationsReqReceived(CommunicationsReqResponse reqResponse)
        {
            CommunicationsReqReceived?.Invoke(reqResponse);
        }

        /// <summary>
        /// OnGetMerchantDataReqReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnGetMerchantDataReqReceived(GetMerchantDataReqResponse reqResponse)
        {
            GetMerchantDataReqReceived?.Invoke(reqResponse);
        }

        /// <summary>
        /// OnSetMerchantDataReqReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnSetMerchantDataReqReceived(SetMerchantDataReqResponse reqResponse)
        {
            SetMerchantDataReqReceived?.Invoke(reqResponse);
        }

        /// <summary>
        /// OnConfigTerminalReqReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnConfigTerminalReqReceived(ConfigTerminalReqResponse reqResponse)
        {
            ConfigTerminalReqReceived?.Invoke(reqResponse);
        }

        /// <summary>
        /// OnCustomerDataResponseReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnCustomerDataResponseReceived(CustomerDataResponse reqResponse)
        {
            CustomerDataResponseReceived?.Invoke(reqResponse);
        }

        /// <summary>
        /// OnCustomerDataResponseReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnLoyaltyInquiryResponseReceived(LoyaltyInquiryResponse reqResponse)
        {
            LoyaltyInquiryResponseReceived?.Invoke(reqResponse);
        }

        /// <summary>
        /// OnPendingReversalsReqResponseReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnPendingReversalsReqResponseReceived(PendingReversalsReqResponse reqResponse)
        {
            PendingReversalsReqReceived?.Invoke(reqResponse);
        }

        /// <summary>
        /// OnDeletePendingReversalsReqResponseReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnDeletePendingReversalsReqResponseReceived(DeletePendingReversalsReqResponse reqResponse)
        {
            DeletePendingReversalsReqReceived?.Invoke(reqResponse);
        }

        /// <summary>
        /// OnProcessPaymentReqReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnProcessPaymentReqReceived(ProcessPaymentReqResponse reqResponse)
        {
            ProcessPaymentReqReceived?.Invoke(this, reqResponse);
        }

        /// <summary>
        /// OnEventNotificationReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnEventNotificationReceived(EventNotification reqResponse)
        {
            EventNotificationReceived?.Invoke(this, reqResponse);
        }

        /// <summary>
        /// OnPendingReversalsReqResponseReceived event handler
        /// </summary>
        /// <param name="reqResponse">The response</param>
        private void OnTerminalDisconnectedReceived(TerminalDisconnected reqResponse)
        {
            TerminalDisconnectedReceived?.Invoke(reqResponse);
        }

        #endregion
    }
}