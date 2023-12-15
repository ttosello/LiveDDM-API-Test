
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;


namespace LMCApiTest
{
    internal class Program
    {
        static HttpClient _Client = null!;
        static VendorAuthenticationToken? _VendorToken;


        static async Task Main(string[] args)
        {
            //
            // init the http client and set the token
            //
            await InitHttpClient();
            //
            // get patients
            //
            var pats = await GetPatients();
            //
            // get todays appointments
            //
            var apps = await GetAppointments(DateTime.Now.Date);
            

            foreach (var app in apps)
            {
                Console.WriteLine($"{app.Patient.ToString()}\t{app.AppointmentDateTime.ToString("hh:mm tt")}\t{app.Provider}");
            }

            Console.ReadKey();

        }

        static async Task InitHttpClient()
        {
            var baseUri = new Uri("https://liveddmmanagementclient.azurewebsites.net");
           

            _Client = new HttpClient();
            _Client.BaseAddress = baseUri;

            //
            // authenticate using your vendor Api key
            //
            var apiKey = "{your_api_key}";
            var uri = $"api/authentication/authenticatevendor/{apiKey}";
            //
            // get your Jwt token
            //
            var response = await _Client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            _VendorToken = JsonSerializer.Deserialize<VendorAuthenticationToken>(content);

            if (_VendorToken is null)
                throw new Exception("Vendor token is null.");

            _Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _VendorToken.Token);

        }
        static async Task<List<PatientInfo>> GetPatients()
        {

            //
            // define the endpoint
            //
            var uri = $"api/liveddm/patients/getpatientinfo/0/20";
            //
            // get the response Json object
            //
            var response = await _Client.GetAsync(uri);
            var json = await response.Content.ReadAsStringAsync();

            var patients = JsonSerializer.Deserialize<List<PatientInfo>>(json);

            return patients;
        }

        static async Task<ICollection<AppointmentInfo>> GetAppointments(DateTime appointmentDate)
        {

            //
            // define the endpoint
            //
            var uri = $"api/liveddm/appointments/getappointmentsbydate/{appointmentDate.ToString("MM-dd-yyyy")}";

            //
            // get the response Json object
            //
            var response = await _Client.GetAsync(uri);
            var json = await response.Content.ReadAsStringAsync();


            var apps = JsonSerializer.Deserialize<List<AppointmentInfo>>(json);
            return apps;
        }

        static async Task<int> AddOrUpdateAppointment(AppointmentInfo appointmentInfo)
        {

            //
            // define the endpoint
            //
            var uri = $"api/liveddm/appointments/AddOrUpdateAppointment";

            //
            // get the response Json object
            //
            var response = await _Client.PostAsJsonAsync<AppointmentInfo>(uri, appointmentInfo);
            var json = await response.Content.ReadAsStringAsync();


            var appId = JsonSerializer.Deserialize<int>(json);
            return appId;
        }

    }


    internal class VendorAuthenticationToken
    {
        [JsonPropertyName("vendorName")]
        public string VendorName { get; set; } = null!;

        [JsonPropertyName("token")]
        public string Token { get; set; } = null!;

        [JsonPropertyName("expiresOn")]
        public DateTime ExpiresOn { get; set; }
    }
    enum GenderTypes
    {
        NotSpecified, Male, Female, Other
    }
    internal class PatientInfo
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("birthDate")]
        public DateTime BirthDate { get; set; }

        [JsonPropertyName("gender")]
        public GenderTypes Gender { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("cellNumber")]
        public string CellNumber { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("street1")]
        public string Street1 { get; set; }

        [JsonPropertyName("street2")]
        public string Street2 { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("province")]
        public string Province { get; set; }

        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }

        [JsonPropertyName("rfmId")]
        public int RfmId { get; set; }

        [JsonPropertyName("responsibleFamilyMember")]
        public string ResponsibleFamilyMember { get; set; }

        public override string ToString()
        {
            return $"{FirstName} {LastName}";
        }


    }

    internal class AppointmentInfo
    {
        [JsonPropertyName("isHere")]
        public bool IsHere { get; set; }

        [JsonPropertyName("units")]
        public int Units { get; set; }

        [JsonPropertyName("isConfirmed")]
        public bool IsConfirmed { get; set; }

        [JsonPropertyName("isComplete")]
        public bool IsComplete { get; set; }

        [JsonPropertyName("appointmentId")]
        public int AppointmentId { get; set; }

        [JsonPropertyName("patientId")]
        public int PatientId { get; set; }

        [JsonPropertyName("patient")]
        public PatientInfo? Patient { get; set; } = null!;

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("appointmentColor")]
        public int AppointmentColor { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
        [JsonPropertyName("time")]
        public DateTime Time { get; set; }

        [JsonPropertyName("provider")]
        public string? Provider { get; set; }

        [JsonPropertyName("providerID")]
        public int ProviderID { get; set; }

        [JsonPropertyName("procedureCategory")]
        public string? ProcedureCategory { get; set; }

        [JsonPropertyName("procedureCategoryId")]
        public int ProcedureCategoryId { get; set; }
            [JsonIgnore]
            public DateTime AppointmentDateTime
            {
                get
                {
                    return Date.AddTicks(Time.TimeOfDay.Ticks);
                }
            }
        }
}
