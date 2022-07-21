using RestSharpApi.Data;
using RestSharpApi.Hooks;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using TechTalk.SpecFlow;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using NUnit.Framework;
using Bookings;
using Notification;
using User;
using Video;
using System.Collections.Generic;

namespace RestSharpApi.Steps
{
    [Binding]
	///<summary>
	/// Class to interact with the BookingsApi
	///</summary>
    public class BookingsApiSteps : RestSharpHooks

    {
        private static BookingsApi BookingApiService;
        protected static ScenarioContext _scenarioContext;
        private static readonly string HearingScenarionKey = "Hearing";

        [When(@"I check BookingsApi health")]
        public static void WhenICheckBookingsApiHealth()
        {
            throw new PendingStepException();
        }

        public BookingsApiSteps(ScenarioContext scenarioContext)
        {
            var _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GetServiceToServiceToken());
             BookingApiService = new BookingsApi(_client);
            BookingApiService.BaseUrl = config.bookingsapi;
            _scenarioContext = scenarioContext;
        }

        [When(@"I book a hearing")]
        public static async Task WhenIBookAHearing()
        {

            await BookAHearing();
            await ConfirmAHearing();
        }
		
        public static async Task BookAHearing()
        {
            _logger.Info("MK is Booking a hearing");
            BookNewHearingRequest _request = AddHearingData();
            HearingDetailsResponse _response = new HearingDetailsResponse();
            try
            {
                _logger.Info("making request");
                _response = await BookingApiService.BookNewHearingAsync(_request);
                _logger.Info($"response is {_response.Id}");

            }
            catch (Bookings.ApiException<Bookings.ProblemDetails> e)
            {
                _logger.Info(e.Message);
                Bookings.ProblemDetails pd = e.Result;
                _logger.Info(pd.AdditionalProperties);
                foreach (var it in pd.AdditionalProperties)
                    _logger.Info(it.ToString);
                _logger.Info($" exception type {e.GetType().Name}");
                throw;
            }
            catch (Exception e)
            {
                _logger.Info(e.Message);
                _logger.Info($" exception type {e.GetType().Name}");
                throw;
            }
            var _hearing = _scenarioContext.Get<Hearing>(HearingScenarionKey);
            _hearing.HearingId = _response.Id;
            _logger.Info($"Hearing: Response id {_response.Id}, status is{_response.Status}");
            _logger.Info(_response.ToString());
            _logger.Info($"Booking ended");
        }

        private static BookNewHearingRequest AddHearingData()
        {
            Hearing _hearing = new Hearing
            {
                Audio_recording_required = false,
                Scheduled_date_time = DateTime.Now.AddMinutes(1),
                CaseType = "Civil",
                HearingTypeName = "Civil Enforcement",
                Hearing_venue_name = "Birmingham Civil and Family Justice Centre",
                ScheduledDuration = 60
            };
			
			//Add a case to the hearing
            Case _case = new Case
            {
                Is_lead_case = true,
                Name = "Case aaa",
                Number = "AA/AAA111"
            };
            _hearing.AddCase(_case);
			
			//Add a participant to the hearing
            Participant _participant = new Participant
            {
                
                UserName = "auto_aw_Judge01@hearings.reform.hmcts.net",
                CaseRoleName = "Judge",
                HearingRoleName = "Judge",
                ContactEmail = "auto_aw_Judge01@hearings.reform.hmcts.net",
                LastName = "Judge_01",
                FirstName = "auto_aw.",
                DisplayName = "auto_aw_Judge01"

            };			
            _hearing.AddParticipant(_participant);
			
            _scenarioContext.Add(HearingScenarionKey, _hearing);
            BookNewHearingRequest _request = new BookNewHearingRequest
            {
                Audio_recording_required = _hearing.Audio_recording_required,
                Scheduled_date_time = _hearing.Scheduled_date_time,
                Hearing_venue_name = _hearing.Hearing_venue_name,
                Case_type_name = _hearing.CaseType,
                Hearing_type_name = _hearing.HearingTypeName,
                Scheduled_duration = _hearing.ScheduledDuration
            };
			
            CaseRequest _request2 = new CaseRequest { Is_lead_case = _case.Is_lead_case, Name = _case.Name, Number = _case.Number};
            var cases = new List<CaseRequest> { _request2 };
            _request.Cases = cases;
            Bookings.ParticipantRequest _request3 = new Bookings.ParticipantRequest
            {
                Username = _participant.UserName,
                Case_role_name = _participant.CaseRoleName,
                Hearing_role_name = _participant.HearingRoleName,
                Contact_email = _participant.ContactEmail,
                Last_name = _participant.LastName,
                First_name = _participant.FirstName,
                Display_name = _participant.DisplayName
            };
            var participantList = new List<Bookings.ParticipantRequest> { _request3 };
            _request.Participants = participantList;
            return _request;
        }

        protected static string GetServiceToServiceToken()
        {
            AuthenticationResult result;
            var credential = new ClientCredential(config.clientid, config._clientSecret);
            var authContext = new AuthenticationContext($"{config._authority}{config._tenetid}");
            try
            {
                result = authContext.AcquireTokenAsync(config.bookingsapiResourceId, credential).Result;
            }
            catch (AdalException)
            {
                throw new UnauthorizedAccessException();
            }
            return result.AccessToken;
        }

        public static async Task ConfirmAHearing()
        {
            var _hearing = _scenarioContext.Get<Hearing>(HearingScenarionKey);
            var hearingId = _hearing.HearingId;
            UpdateBookingStatusRequest request = new UpdateBookingStatusRequest();
            request.Status = UpdateBookingStatus.Created;
            request.Updated_by = "automation_admin@hearings.reform.hmcts.net";
            await BookingApiService.UpdateBookingStatusAsync(hearingId, request);
        }

    [When(@"I get case Types")]
    public static async Task WhenIGetCaseTypes()
    {
        _logger.Info("Looking for Case Types");
            _logger.Info("making request");
            var cases = await BookingApiService.GetCaseTypesAsync(System.Threading.CancellationToken.None);
                foreach (var caseType in cases)
                {
                    _logger.Info($" Case found {caseType.Name}, {caseType.Id}, {caseType.GetType()}");
                }
        _logger.Info($"Case Type List found");
    }

        [When(@"I get venues")]
        public static async Task WhenIGetVenues()
        {
            _logger.Info("Looking for Venues");
            var venues = await BookingApiService.GetHearingVenuesAsync(System.Threading.CancellationToken.None);
            _logger.Info($"Venues found {venues}");
            foreach (var venue in venues)
                _logger.Info($"venue id {venue.Id} is {venue.Name}");
        }

        [When(@"I ask for judges with the name ""([^""]*)""")]
        public static async Task WhenIAskForJudgesWithTheName(string judgename)
        {
            _logger.Info($"Looking for Judges starting with {judgename}");
            SearchTermRequest searchTermRequest = new SearchTermRequest();
            searchTermRequest.Term = judgename;
            var judges = await BookingApiService.PostJudiciaryPersonBySearchTermAsync(searchTermRequest);
            _logger.Info($"Judges found {judges}");
            foreach (var judge in judges)
                _logger.Info($"judge id {judge.Id} is {judge.Username}, {judge.Title}, {judge.First_name}, {judge.Last_name}");
        }

        [When(@"I get anonymous data")]
        public static async Task WhenIGetAnonymousData()
        {
            _logger.Info("Looking for Anonymous data");
            AnonymisationDataResponse bigList;
            try
            {
             bigList = await BookingApiService.GetAnonymisationDataAsync();

            }
            catch (ApiException)
            {

                throw;
            }
            foreach (var big in bigList.Usernames)
            {
                _logger.Info($"found user {big}");
            }
            foreach (var big in bigList.Hearing_ids)
            {
                _logger.Info($"found hearing {big}");
                await GetHearings(big);
            }
        }

        public static async Task GetHearings(Guid hearingId)
        {
            _logger.Info($"Looking for Hearing data for Hearing {hearingId.ToString()}");
            StringBuilder sb2 = new StringBuilder();
            var _client2 = new HttpClient();
            _client2.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client2.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GetServiceToServiceToken());
            BookingsApi BookingApiService2 = new BookingsApi(_client2);
            BookingApiService2.BaseUrl = config.bookingsapi;
            try
            {
            var f = await BookingApiService2.GetHearingDetailsByIdAsync(hearingId);
            foreach (var participant in f.Participants)
            {
                _logger.Info($"looking at participant {participant.Username}, {participant.GetType()}, {participant.First_name} {participant.Last_name}");
                sb2.Append($"participant {participant.Username}, {participant.GetType()}, {participant.First_name} {participant.Last_name}");
            }
            }
            catch (Exception)
            {
                throw;
            }
            return;
        }
    }

}

