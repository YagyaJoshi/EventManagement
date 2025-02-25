using Newtonsoft.Json;

namespace EventManagement.Utilities.Payment.Mastercard
{
    public class _3ds
    {
        public string acsEci { get; set; }
        public string authenticationToken { get; set; }
        public string transactionId { get; set; }
    }

    public class _3ds1
    {
        public string paResStatus { get; set; }
        public string veResEnrolled { get; set; }
    }

    public class Acquirer
    {
        public string merchantId { get; set; }
        public int? batch { get; set; }
        public string date { get; set; }
        public string id { get; set; }
        public string settlementDate { get; set; }
        public string timeZone { get; set; }
        public string transactionId { get; set; }
    }

    public class Address
    {
        public string city { get; set; }
        public string country { get; set; }
        public string postcodeZip { get; set; }
        public string stateProvince { get; set; }
        public string street { get; set; }
    }

    public class Authentication
    {
        [JsonProperty("3ds")]
        public _3ds _3ds { get; set; }

        [JsonProperty("3ds1")]
        public _3ds1 _3ds1 { get; set; }
        public string acceptVersions { get; set; }
        public double amount { get; set; }
        public string channel { get; set; }
        public string payerInteraction { get; set; }
        public string purpose { get; set; }
        public Redirect redirect { get; set; }
        public DateTime time { get; set; }
        public string version { get; set; }
        public string transactionId { get; set; }
    }

    public class AuthorizationResponse
    {
        public string cardLevelIndicator { get; set; }
        public string commercialCard { get; set; }
        public string commercialCardIndicator { get; set; }
        public string marketSpecificData { get; set; }
        public string posData { get; set; }
        public string posEntryMode { get; set; }
        public string processingCode { get; set; }
        public string responseCode { get; set; }
        public string returnAci { get; set; }
        public string stan { get; set; }
        public string transactionIdentifier { get; set; }
        public string validationCode { get; set; }
    }

    public class Billing
    {
        public Address address { get; set; }
    }

    public class Card
    {
        public string brand { get; set; }
        public Expiry expiry { get; set; }
        public string fundingMethod { get; set; }
        public string nameOnCard { get; set; }
        public string number { get; set; }
        public string scheme { get; set; }
        public string storedOnFile { get; set; }
    }

    public class Chargeback
    {
        public int amount { get; set; }
        public string currency { get; set; }
    }

    public class Device
    {
        public string browser { get; set; }
        public string ipAddress { get; set; }
    }

    public class Expiry
    {
        public string month { get; set; }
        public string year { get; set; }
    }

    public class OrderDetails
    {
        public double amount { get; set; }
        public string authenticationStatus { get; set; }
        public Chargeback chargeback { get; set; }
        public DateTime creationTime { get; set; }
        public string currency { get; set; }
        public string description { get; set; }
        public string id { get; set; }
        public DateTime lastUpdatedTime { get; set; }
        public double merchantAmount { get; set; }
        public string merchantCategoryCode { get; set; }
        public string merchantCurrency { get; set; }
        public string status { get; set; }
        public double totalAuthorizedAmount { get; set; }
        public double totalCapturedAmount { get; set; }
        public double totalDisbursedAmount { get; set; }
        public double totalRefundedAmount { get; set; }
        public ValueTransfer valueTransfer { get; set; }

        public Custom custom { get; set; }
    }

    public class Custom
    {
        public long oldGuestId {  get; set; }
        public long? newGuestId { get; set; }
        public long orderId { get; set; }
        public string? passportFirstName { get; set; }

        public string? passportLastName { get; set; }
        public string? passportNumber { get; set; }

        public string? organizationId { get; set; }

        public string? walletAmount { get; set; }

        public string? userId {  get; set; }
        public string? penaltyAmount { get; set; }

        public string? actualAmount { get; set; }

    }

    public class Provided
    {
        public Card card { get; set; }
    }

    public class Redirect
    {
        public string domainName { get; set; }
    }

    public class Response
    {
        public string gatewayCode { get; set; }
        public string gatewayRecommendation { get; set; }
        public string acquirerCode { get; set; }
        public string acquirerMessage { get; set; }
    }

    public class OrderDetailsResponse
    {
        [JsonProperty("3dsAcsEci")]
        public string _3dsAcsEci { get; set; }
        public double amount { get; set; }
        public Authentication authentication { get; set; }
        public string authenticationStatus { get; set; }
        public string authenticationVersion { get; set; }
        public Billing billing { get; set; }
        public Chargeback chargeback { get; set; }
        public DateTime creationTime { get; set; }
        public string currency { get; set; }
        public string description { get; set; }
        public Device device { get; set; }
        public string id { get; set; }
        public DateTime lastUpdatedTime { get; set; }
        public string merchant { get; set; }
        public double merchantAmount { get; set; }
        public string merchantCategoryCode { get; set; }
        public string merchantCurrency { get; set; }
        public string result { get; set; }
        public SourceOfFunds sourceOfFunds { get; set; }
        public string status { get; set; }
        public double totalAuthorizedAmount { get; set; }
        public double totalCapturedAmount { get; set; }
        public double totalDisbursedAmount { get; set; }
        public double totalRefundedAmount { get; set; }
        public List<Transaction> transaction { get; set; }
    }

    public class SourceOfFunds
    {
        public Provided provided { get; set; }
        public string type { get; set; }
    }

    public class Transaction
    {
        public Authentication authentication { get; set; }
        public Billing billing { get; set; }
        public Device device { get; set; }
        public string merchant { get; set; }
        public OrderDetails order { get; set; }
        public Response response { get; set; }
        public string result { get; set; }
        public SourceOfFunds sourceOfFunds { get; set; }
        public DateTime timeOfLastUpdate { get; set; }
        public DateTime timeOfRecord { get; set; }
        public Transaction transaction { get; set; }
        public string version { get; set; }
        public AuthorizationResponse authorizationResponse { get; set; }
        public string gatewayEntryPoint { get; set; }
        public Acquirer acquirer { get; set; }
        public double amount { get; set; }
        public string authenticationStatus { get; set; }
        public string currency { get; set; }
        public string id { get; set; }
        public string stan { get; set; }
        public string type { get; set; }
        public string authorizationCode { get; set; }
        public string receipt { get; set; }
        public string source { get; set; }
        public string terminal { get; set; }
    }

    public class ValueTransfer
    {
        public string accountType { get; set; }
    }

}
