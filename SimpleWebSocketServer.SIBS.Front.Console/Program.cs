using SimpleWebSocketServer.SIBS.Front.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SimpleWebSocketServer.SIBS.Front.Enums.Enums;

namespace SimpleWebSocketServer.SIBS.Front.Console
{
    internal class Program
    {
        #region "Constants"

        private const string _Address = "wss://192.168.40.104:10005";
        private const string _MessageEnterQToStopCommand = "Enter 'q' to stop the application";
        private const string _MessageErrorErrorOccurred = "Error occurred";
        private const string _MessageErrorProcessingRequest = "Error processing request";
        private const string _MessagePressAnyKeyToExit = "Press any key to exit...";
        private const string _MessageStoppingTheServer = "Disconnecting...";
        private const string _MessageTheFollowingCommandsAreAvailable = "The following commands are available:";
        private const string _MessageEnterCode = "Enter code or 'q' to stop: ";
        private const string _MessageEnterTerminalID = "Please enter the terminal ID to link to the front (or 'q' to cancel):";
        private const string _MessageInvalidInput = "Invalid input";
        private const string _ClientId = "22a17e95-0f30-4f0d-86c6-c84e9e519a9c";
        private const int _TerminalID = 2060668540;

        #endregion

        private static TerminalClient TerminalClient { get; set; }
        public static int TerminalClient_RefundReqResponseReceived { get; }

        private static readonly ManualResetEvent statusEventReceived = new ManualResetEvent(false);
        /// <summary>
        private static PaymentData LastPaymentData = null;
        /// </summary>
        /// <param name="args"></param>

        static void Main(string[] args)
        {
            try
            {
                System.Console.WriteLine(_MessageEnterQToStopCommand);

                // Start the WebSocket server asynchronously
                TerminalClient = new TerminalClient(_Address, Guid.Parse(_ClientId), _TerminalID);

                // Front events
                //TerminalClient.ClientConnectedResponseReceived += TerminalClient_ClientConnectedResponseReceived;
                TerminalClient.RegisterFrontResponseReceived += TerminalClient_RegisterFrontResponseReceived;
                TerminalClient.ListTerminalsResponseReceived += TerminalClient_ListTerminalsResponseReceived;
                TerminalClient.LinqTerminalToFrontResponseReceived += TerminalClient_LinqTerminalToFrontResponseReceived;

                // Terminal events
                TerminalClient.ProcessPaymentReqReceived += TerminalClient_ProcessPaymentReqReceived;
                TerminalClient.EventNotificationReceived += TerminalClient_EventNotificationReceived;
                TerminalClient.HeartbeatNotificationReceived += TerminalClient_HeartbeatNotificationReceived;
                TerminalClient.ReceiptNotificationReceived += TerminalClient_ReceiptNotificationReceived;
                TerminalClient.PairingNotificationReceived += TerminalClient_PairingNotificationReceived;
                TerminalClient.PairingReqReceived += TerminalClient_PairingReqReceived;
                TerminalClient.ErrorNotificationReceived += TerminalClient_ErrorNotificationReceived;
                TerminalClient.RefundReqReceived += TerminalClient_RefundReqReceived;
                TerminalClient.CommunicationsReqReceived += TerminalClient_CommunicationsReqReceived;
                TerminalClient.GetMerchantDataReqReceived += TerminalClient_GetMerchantDataReqReceived;
                TerminalClient.SetMerchantDataReqReceived += TerminalClient_SetMerchantDataReqReceived;
                TerminalClient.ConfigTerminalReqReceived += TerminalClient_ConfigTerminalReqReceived;
                TerminalClient.CustomerDataResponseReceived += TerminalClient_CustomerDataResponseReceived;
                TerminalClient.LoyaltyInquiryResponseReceived += TerminalClient_LoyaltyInquiryResponseReceived;
                TerminalClient.PendingReversalsReqReceived += TerminalClient_PendingReversalsReqReceived;
                TerminalClient.DeletePendingReversalsReqReceived += TerminalClient_DeletePendingReversalsReqReceived;
                TerminalClient.ReconciliationReqReceived += TerminalClient_ReconciliationReqReceived;
                TerminalClient.TerminalStatusReqResponseReceived += TerminalClient_TerminalStatusReqResponseReceived;

                ListenForUserInput().Wait();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorErrorOccurred}: {ex.Message}");
            }

            System.Console.WriteLine(_MessagePressAnyKeyToExit);
            System.Console.ReadKey();
        }

        #region "Private Methods"

        /// <summary>
        /// Method to log messages to the console
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            System.Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]{message}");
        }

        /// <summary>
        /// Listens for user input and sends the input to the WebSocket server.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        private async static Task ListenForUserInput()
        {
            var serverIsRunning = true;

            while (serverIsRunning)
            {
                ShowListOfCommands();

                // Read user input synchronously
                string input = System.Console.ReadLine();

                // Check if the input is 'q' to stop the server
                if (input.ToLower() == "q")
                {
                    if (TerminalClient != null)
                    {
                        System.Console.WriteLine(_MessageStoppingTheServer);
                        await TerminalClient.Disconnect(); // Stop the WebSocket server
                    }
                    break; // Exit the loop
                }

                // Parse the input to the enum
                if (int.TryParse(input, out int commandValue) && Enum.IsDefined(typeof(TerminalCommandOptions), commandValue))
                {
                    var command = (TerminalCommandOptions)commandValue;

                    Log($"Waiting for response...");

                    switch (command)
                    {
                        case TerminalCommandOptions.ConnectToServer:
                            var res = await TerminalClient.Connect();

                            if (res.Item1)
                            {
                                await TerminalClient.SendRegisterFrontRequest(Guid.Parse(_ClientId));
                                WaitForEvent(statusEventReceived);
                            }
                            else
                                Log(res.Item2.Message);
                            break;
                        //case TerminalCommandOptions.SendRegisterFrontRequest:
                        //    await TerminalClient.SendRegisterFrontRequest(Guid.Parse(_ClientId));
                        //    WaitForEvent(statusEventReceived);
                        //    break;
                        //case TerminalCommandOptions.SendListTerminalsAvailableRequest:
                        //    await TerminalClient.SendListTerminalRequest();
                        //    WaitForEvent(statusEventReceived);
                        //    break;
                        case TerminalCommandOptions.SendProcessPaymentRequest:
                            await TerminalClient.SendProcessPaymentRequest();
                            WaitForEvent(statusEventReceived);
                            break;
                        case TerminalCommandOptions.SendPairingRequest:
                            await TerminalClient.SendPairingRequest();
                            WaitForEvent(statusEventReceived);
                            break;
                        case TerminalCommandOptions.SendPairingRequestCancel:
                            await TerminalClient.SendPairingRequestCancel();
                            WaitForEvent(statusEventReceived);
                            break;
                        case TerminalCommandOptions.SendRefundPaymentRequest:
                            await TerminalClient.SendRefundPaymentRequest(LastPaymentData);
                            WaitForEvent(statusEventReceived);
                            break;
                        case TerminalCommandOptions.SendReconciliationRequest:
                            await TerminalClient.SendReconciliationRequest();
                            WaitForEvent(statusEventReceived);
                            break;
                        case TerminalCommandOptions.SendCommunicationStatusRequest:
                            await TerminalClient.SendCommunicationRequest();
                            WaitForEvent(statusEventReceived);
                            break;
                        case TerminalCommandOptions.SendGetMerchantDataRequest:
                            await TerminalClient.SendGetMerchantDataRequest(new GetMerchantDataReq());
                            WaitForEvent(statusEventReceived);
                            break;
                        case TerminalCommandOptions.SendSetMerchantDataRequest:
                            await TerminalClient.SendSetMerchantDataRequest(new MerchantData());
                            WaitForEvent(statusEventReceived);
                            break;
                        case TerminalCommandOptions.SendConfigTerminalRequest:
                            await TerminalClient.SendConfigTerminalRequest(new ConfigTerminalReq());
                            WaitForEvent(statusEventReceived);
                            break;
                        case TerminalCommandOptions.SendCustomerDataRequest:
                            await TerminalClient.SendCustomerDataReq(new CustomerDataReq());
                            WaitForEvent(statusEventReceived);
                            break;
                        case TerminalCommandOptions.SendLoyaltyInquiryRequest:
                            await TerminalClient.SendLoyaltyInquiryReq(new LoyaltyInquiryReq());
                            WaitForEvent(statusEventReceived);
                            break;
                        case TerminalCommandOptions.SendPendingReversalsRequest:
                            await TerminalClient.SendPendingReversalsRequest(new PendingReversalsReq());
                            WaitForEvent(statusEventReceived);
                            break;
                        case TerminalCommandOptions.SendDeletePendingReversalsReqRequest:
                            await TerminalClient.SendDeletePendingReversalsReqRequest(new DeletePendingReversalsReq());
                            WaitForEvent(statusEventReceived);
                            break;
                        case TerminalCommandOptions.SendTerminalStatusRequest:
                            await TerminalClient.SendTerminalStatusRequest();
                            WaitForEvent(statusEventReceived);
                            break;
                        case TerminalCommandOptions.SendListTerminalsRequest:
                            await TerminalClient.SendListTerminalRequest();
                            WaitForEvent(statusEventReceived);
                            break;
                        case TerminalCommandOptions.SendLinqTerminalToFrontRequest:
                            await SendLinqTerminalToFrontRequest();
                            break;
                        //case TerminalCommandOptions.LinqTerminalToFrontRequest:
                        //    await TerminalClient.LinqTerminalToFrontRequest();
                        //    WaitForEvent(statusEventReceived);
                        //    break;
                        case TerminalCommandOptions.ShowListOfCommands:
                            ShowListOfCommands();
                            break;
                    }
                }
                else
                {
                    System.Console.WriteLine(_MessageInvalidInput);
                    ShowListOfCommands();
                }
            }
        }

        /// <summary>
        /// Shows the list of commands.
        /// </summary>
        private static void ShowListOfCommands()
        {
            System.Console.WriteLine($"{Environment.NewLine}{_MessageTheFollowingCommandsAreAvailable}");

            foreach (TerminalCommandOptions command in Enum.GetValues(typeof(TerminalCommandOptions)))
            {
                System.Console.WriteLine($"   {(int)command} - {Utilities.UtilitiesSibs.GetEnumDescription(command)}");
            }

            System.Console.WriteLine($"{Environment.NewLine}");
        }

        /// <summary>
        /// Event wait handler
        /// </summary>
        /// <param name="eventHandle">The event handle</param>
        /// <param name="actionName">The action name</param>
        private static void WaitForEvent(ManualResetEvent eventHandle)
        {
            eventHandle.WaitOne();
            eventHandle.Reset();
        }

        private static async Task SendLinqTerminalToFrontRequest()
        {
            System.Console.WriteLine(_MessageEnterTerminalID);

            string input = System.Console.ReadLine();

            // Check if the input is 'q' to cancel
            if (input.ToLower() == "q")
                return;

            // Parse the input to an integer
            if (!int.TryParse(input, out int terminalId))
            {
                System.Console.WriteLine(_MessageInvalidInput);
                return;                
            }

            // Send the request to link the terminal to the front
            await TerminalClient.LinqTerminalToFrontRequest(terminalId);
            WaitForEvent(statusEventReceived);
        }

        #endregion

        #region "Events"

        #region "Terminal"

        private static void TerminalClient_DeletePendingReversalsReqReceived(DeletePendingReversalsReqResponse reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        private static void TerminalClient_PendingReversalsReqReceived(PendingReversalsReqResponse reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        private static void TerminalClient_LoyaltyInquiryResponseReceived(LoyaltyInquiryResponse reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        private static void TerminalClient_CustomerDataResponseReceived(CustomerDataResponse reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        private static void TerminalClient_ConfigTerminalReqReceived(ConfigTerminalReqResponse reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        private static void TerminalClient_SetMerchantDataReqReceived(SetMerchantDataReqResponse reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        private static void TerminalClient_GetMerchantDataReqReceived(GetMerchantDataReqResponse reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        private static void TerminalClient_CommunicationsReqReceived(CommunicationsReqResponse reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        private static void TerminalClient_RefundReqReceived(object sender, RefundReqResponse reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        private static void TerminalClient_ErrorNotificationReceived(object sender, ErrorNotification reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        private static void TerminalClient_PairingNotificationReceived(object sender, PairingNotification reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));

            statusEventReceived.Set();
        }

        private static void TerminalClient_PairingReqReceived(object sender, PairingReqResponse reqResponse)
        {
            try
            {
                System.Console.Write(_MessageEnterCode);

                string input = System.Console.ReadLine();
                double code = 0;
                var invalidAmount = true;

                if (double.TryParse(input, out code))
                    invalidAmount = false;

                if (invalidAmount || input == "q")
                    Task.Run(() => TerminalClient.SendPairingRequestCancel()).Wait();
                else
                    Task.Run(() => TerminalClient.SendPairingRequestCode(code.ToString())).Wait();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{_MessageErrorProcessingRequest}: {ex.Message}");
            }
        }

        private static void TerminalClient_ReceiptNotificationReceived(object sender, ReceiptNotification reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        private static void TerminalClient_HeartbeatNotificationReceived(object sender, HeartbeatNotification reqResponse)
        {
            //Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
        }

        private static void TerminalClient_EventNotificationReceived(object sender, EventNotification reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            //statusEventReceived.Set();
        }

        private static void TerminalClient_ProcessPaymentReqReceived(object sender, ProcessPaymentReqResponse reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        private static void TerminalClient_ReconciliationReqReceived(object sender, ReconciliationReqResponse reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        private static void TerminalClient_TerminalStatusReqResponseReceived(object sender, TerminalStatusReqResponse reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        #endregion

        #region "Front"

        //private static void TerminalClient_ClientConnectedResponseReceived(object sender, ClientConnectedResponse reqResponse)
        //{
        //    Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
        //    Task.Run(() => TerminalClient.SendRegisterFrontRequest(Guid.Parse(_ClientId))).Wait();
        //}

        private static void TerminalClient_RegisterFrontResponseReceived(object sender, RegisterFrontResponse reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        private static void TerminalClient_ListTerminalsResponseReceived(object sender, ListTerminalsResponse reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        private static void TerminalClient_LinqTerminalToFrontResponseReceived(object sender, LinqTerminalToFrontResponse reqResponse)
        {
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(reqResponse));
            statusEventReceived.Set();
        }

        #endregion

        #endregion
    }
}
